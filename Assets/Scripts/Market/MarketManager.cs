using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [Header("Bazaar")]
    [SerializeField] private BazaarProfileSO bazaarBuyer;

    [Header("Contract Buyers")]
    [SerializeField] private BuyerProfileSO[] availableBuyers;

    [Header("Order Templates")]
    [SerializeField] private OrderDataSO[] orderPool;

    [Header("Order Limits")]
    [SerializeField] private int maxActiveOrders = 3;
    [SerializeField] private int maxAvailableOrders = 6;

    [Header("Bazaar Dispatch")]
    [SerializeField] private float bazaarSellIntervalSeconds = 90f;

    [Header("Buy Catalog")]
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private bool useItemDatabaseForPurchases = true;
    [SerializeField] private bool includeManualPurchasableItems;
    [SerializeField] private SellableItem[] purchasableItems;
    [SerializeField] private float buyPriceMultiplier = 1f;

    private readonly List<ActiveOrder> activeOrders = new();
    private readonly List<ActiveOrder> availableOrders = new();
    private readonly List<SellableItem> resolvedPurchasableItems = new();
    private readonly Dictionary<BuyerProfileSO, float> buyerOrderTimers = new();
    private readonly Dictionary<SellableItem, int> queuedBuy = new();
    private readonly Dictionary<SellableItem, int> queuedSell = new();

    private float nextBazaarSellTime;
    private bool dispatchInProgress;
    private float dispatchTimeRemaining;
    private int reservedSellRevenue;
    private readonly List<(SellableItem item, int amount)> reservedPurchases = new();

    public static MarketManager Instance { get; private set; }

    public BazaarProfileSO BazaarBuyer => GetBazaarBuyer();
    public IReadOnlyList<BuyerProfileSO> AvailableBuyers => availableBuyers;
    public IReadOnlyList<ActiveOrder> ActiveOrders => activeOrders;
    public IReadOnlyList<ActiveOrder> AvailableOrders => availableOrders;
    public IReadOnlyList<SellableItem> PurchasableItems => GetPurchasableItems();

    public float BazaarCooldownRemaining => Mathf.Max(0f, nextBazaarSellTime - Time.time);
    public bool DispatchInProgress => dispatchInProgress;
    public float DispatchTimeRemaining => dispatchInProgress ? Mathf.Max(0f, dispatchTimeRemaining) : 0f;

    public event Action OnMarketDataChanged;
    public event Action<ActiveOrder> OnOrderCompleted;
    public event Action<ActiveOrder> OnOrderExpired;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializeBuyerTimers();
        TryGenerateOrdersForAllBuyers(true);
    }

    private void Update()
    {
        UpdateOrdersLifecycle();
        UpdateOrderGeneration();
        UpdateDispatch();
    }

    public int GetSellPrice(SellableItem item, BazaarProfileSO buyer = null)
    {
        if (item == null)
            return 0;

        BazaarProfileSO targetBuyer = buyer ?? GetDefaultBuyer();
        return targetBuyer == null ? item.Price : targetBuyer.GetPrice(item);
    }

    public int GetBuyPrice(SellableItem item)
    {
        if (item == null)
            return 0;

        return Mathf.Max(1, Mathf.RoundToInt(item.Price * Mathf.Max(0.1f, buyPriceMultiplier)));
    }

    private IReadOnlyList<SellableItem> GetPurchasableItems()
    {
        RefreshPurchaseCatalog();
        return resolvedPurchasableItems;
    }

    private void RefreshPurchaseCatalog()
    {
        resolvedPurchasableItems.Clear();

        bool loadedFromDatabase = false;
        if (useItemDatabaseForPurchases && itemDatabase != null && itemDatabase.Items != null)
        {
            for (int i = 0; i < itemDatabase.Items.Count; i++)
            {
                if (itemDatabase.Items[i] is SeedItem seed)
                    AddPurchasableItem(seed);
            }

            loadedFromDatabase = resolvedPurchasableItems.Count > 0;
        }

        if (!useItemDatabaseForPurchases || includeManualPurchasableItems || !loadedFromDatabase)
            AddManualPurchasableItems();
    }

    private void AddManualPurchasableItems()
    {
        if (purchasableItems == null)
            return;

        for (int i = 0; i < purchasableItems.Length; i++)
            AddPurchasableItem(purchasableItems[i]);
    }

    private void AddPurchasableItem(SellableItem item)
    {
        if (item == null || resolvedPurchasableItems.Contains(item))
            return;

        resolvedPurchasableItems.Add(item);
    }

    public void SetQueuedBuy(SellableItem item, int amount)
    {
        if (item == null)
            return;

        if (amount <= 0) queuedBuy.Remove(item);
        else queuedBuy[item] = amount;
    }

    public void SetQueuedSell(SellableItem item, int amount)
    {
        if (item == null)
            return;

        int capped = Mathf.Max(0, amount);
        if (InventoryManager.Instance != null)
            capped = Mathf.Min(capped, InventoryManager.Instance.CountItem(item));

        if (capped <= 0) queuedSell.Remove(item);
        else queuedSell[item] = capped;
    }

    public int GetQueuedBuyAmount(SellableItem item)
    {
        if (item == null)
            return 0;

        return queuedBuy.TryGetValue(item, out int amount) ? Mathf.Max(0, amount) : 0;
    }

    public int GetQueuedSellAmount(SellableItem item)
    {
        if (item == null)
            return 0;

        return queuedSell.TryGetValue(item, out int amount) ? Mathf.Max(0, amount) : 0;
    }

    public int GetQueuedBuyTotal()
    {
        int total = 0;
        foreach (var kv in queuedBuy)
            total += GetBuyPrice(kv.Key) * Mathf.Max(0, kv.Value);
        return total;
    }

    public int GetQueuedSellTotal(BazaarProfileSO buyer = null)
    {
        int total = 0;
        foreach (var kv in queuedSell)
            total += GetSellPrice(kv.Key, buyer) * Mathf.Max(0, kv.Value);
        return total;
    }

    public int GetQueuedNet(BazaarProfileSO buyer = null) => GetQueuedSellTotal(buyer) - GetQueuedBuyTotal();

    public bool HasQueuedTrade()
    {
        return queuedBuy.Count > 0 || queuedSell.Count > 0;
    }

    public bool DispatchQueuedTrade(BazaarProfileSO buyer)
    {
        if (dispatchInProgress || !CanDirectSell(buyer, out _))
            return false;

        if (InventoryManager.Instance == null || PlayerWallet.Instance == null)
            return false;

        int buyTotal = GetQueuedBuyTotal();
        var sellReservations = new List<(SellableItem item, int amount, int price)>();

        foreach (var kv in queuedSell)
        {
            int available = InventoryManager.Instance.CountItem(kv.Key);
            int amount = Mathf.Min(available, kv.Value);
            if (amount <= 0)
                continue;

            sellReservations.Add((kv.Key, amount, GetSellPrice(kv.Key, buyer)));
        }

        if (buyTotal <= 0 && sellReservations.Count == 0)
            return false;

        if (buyTotal > 0 && !PlayerWallet.Instance.SpendCoins(buyTotal))
            return false;

        reservedSellRevenue = 0;
        for (int i = 0; i < sellReservations.Count; i++)
        {
            var sale = sellReservations[i];
            int removed = InventoryManager.Instance.RemoveItem(sale.item, sale.amount);
            if (removed > 0)
                reservedSellRevenue += sale.price * removed;
        }

        reservedPurchases.Clear();
        foreach (var kv in queuedBuy)
        {
            if (kv.Value > 0)
                reservedPurchases.Add((kv.Key, kv.Value));
        }

        queuedBuy.Clear();
        queuedSell.Clear();

        dispatchInProgress = true;
        dispatchTimeRemaining = Mathf.Max(1f, bazaarSellIntervalSeconds);
        nextBazaarSellTime = Time.time + Mathf.Max(0f, bazaarSellIntervalSeconds);
        OnMarketDataChanged?.Invoke();
        return true;
    }

    public bool CanDirectSell(BazaarProfileSO buyer, out float remainingSeconds)
    {
        remainingSeconds = 0f;
        BazaarProfileSO targetBuyer = buyer ?? GetDefaultBuyer();
        if (!IsBazaarBuyer(targetBuyer))
            return true;

        if (dispatchInProgress)
        {
            remainingSeconds = DispatchTimeRemaining;
            return false;
        }

        remainingSeconds = BazaarCooldownRemaining;
        return remainingSeconds <= 0f;
    }

    public bool AcceptOrder(ActiveOrder order)
    {
        if (order == null || order.IsAccepted || !availableOrders.Contains(order) || activeOrders.Count >= maxActiveOrders)
            return false;

        availableOrders.Remove(order);
        order.Accept();
        activeOrders.Add(order);
        OnMarketDataChanged?.Invoke();
        return true;
    }

    public bool RejectOrder(ActiveOrder order)
    {
        if (order == null)
            return false;

        bool removed = availableOrders.Remove(order);
        if (removed)
            OnMarketDataChanged?.Invoke();

        return removed;
    }

    public int DeliverToOrder(ActiveOrder order)
    {
        if (order == null || !order.IsAccepted || order.IsCompleted || order.IsExpired || InventoryManager.Instance == null)
            return 0;

        int delivered = order.DeliverFromInventory(InventoryManager.Instance);
        if (delivered <= 0)
            return 0;

        if (order.IsCompleted)
            CompleteOrder(order);

        OnMarketDataChanged?.Invoke();
        return delivered;
    }

    public int DeliverOrderLine(ActiveOrder order, int lineIndex)
    {
        if (order == null || !order.IsAccepted || order.IsCompleted || order.IsExpired || InventoryManager.Instance == null)
            return 0;

        int delivered = order.DeliverLineFromInventory(lineIndex, InventoryManager.Instance);
        if (delivered <= 0)
            return 0;

        if (order.IsCompleted)
            CompleteOrder(order);

        OnMarketDataChanged?.Invoke();
        return delivered;
    }

    private void CompleteOrder(ActiveOrder order)
    {
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.AddCoins(order.CalculateReward());

        activeOrders.Remove(order);
        OnOrderCompleted?.Invoke(order);
        OnMarketDataChanged?.Invoke();
    }

    private void UpdateDispatch()
    {
        if (!dispatchInProgress)
            return;

        dispatchTimeRemaining -= Time.deltaTime;
        if (dispatchTimeRemaining > 0f)
            return;

        dispatchInProgress = false;
        dispatchTimeRemaining = 0f;

        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < reservedPurchases.Count; i++)
            {
                var p = reservedPurchases[i];
                if (p.item != null && p.amount > 0)
                    InventoryManager.Instance.AddItem(p.item, p.amount);
            }
        }

        if (PlayerWallet.Instance != null && reservedSellRevenue > 0)
            PlayerWallet.Instance.AddCoins(reservedSellRevenue);

        reservedPurchases.Clear();
        reservedSellRevenue = 0;
        OnMarketDataChanged?.Invoke();
    }

    private void UpdateOrdersLifecycle()
    {
        bool changed = false;

        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            ActiveOrder order = activeOrders[i];
            order.Tick(Time.deltaTime);

            if (order.IsExpired && !order.IsCompleted)
            {
                activeOrders.RemoveAt(i);
                OnOrderExpired?.Invoke(order);
                changed = true;
            }
        }

        for (int i = availableOrders.Count - 1; i >= 0; i--)
        {
            ActiveOrder order = availableOrders[i];
            order.Tick(Time.deltaTime);

            if (order.IsExpired)
            {
                availableOrders.RemoveAt(i);
                OnOrderExpired?.Invoke(order);
                changed = true;
            }
        }

        if (changed)
            OnMarketDataChanged?.Invoke();
    }

    private void UpdateOrderGeneration()
    {
        if (availableBuyers == null)
            return;

        for (int i = 0; i < availableBuyers.Length; i++)
        {
            BuyerProfileSO buyer = availableBuyers[i];
            if (buyer == null)
                continue;

            if (!buyerOrderTimers.TryGetValue(buyer, out float timer))
                timer = Mathf.Max(10f, buyer.OrderIntervalSeconds);

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = Mathf.Max(10f, buyer.OrderIntervalSeconds);
                TryGenerateOrderForBuyer(buyer);
            }

            buyerOrderTimers[buyer] = timer;
        }
    }

    private void InitializeBuyerTimers()
    {
        buyerOrderTimers.Clear();
        if (availableBuyers == null)
            return;

        for (int i = 0; i < availableBuyers.Length; i++)
        {
            BuyerProfileSO buyer = availableBuyers[i];
            if (buyer == null)
                continue;

            float baseTimer = Mathf.Max(10f, buyer.OrderIntervalSeconds);
            buyerOrderTimers[buyer] = UnityEngine.Random.Range(3f, baseTimer);
        }
    }

    private void TryGenerateOrdersForAllBuyers(bool force)
    {
        if (availableBuyers == null)
            return;

        for (int i = 0; i < availableBuyers.Length; i++)
        {
            BuyerProfileSO buyer = availableBuyers[i];
            if (buyer == null)
                continue;

            if (buyer == GetBazaarBuyer())
                continue;

            TryGenerateOrderForBuyer(buyer, force);
        }
    }

    private void TryGenerateOrderForBuyer(BuyerProfileSO buyer, bool ignoreOutstandingLimit = false)
    {
        if (orderPool == null || orderPool.Length == 0 || buyer == null)
            return;

        int outstanding = CountOutstandingOrdersForBuyer(buyer);
        if (!ignoreOutstandingLimit && outstanding >= Mathf.Max(1, buyer.MaxOutstandingOrders))
            return;

        if (availableOrders.Count >= maxAvailableOrders)
            return;

        OrderDataSO template = PickTemplateForBuyer(buyer);
        if (template == null || !template.HasValidLines())
            return;

        List<ActiveOrderLine> lines = template.CreateRuntimeLines();
        if (lines.Count == 0)
            return;

        float startMultiplier = UnityEngine.Random.Range(
            Mathf.Max(1f, buyer.MinInitialOrderMultiplier),
            Mathf.Max(buyer.MinInitialOrderMultiplier, buyer.MaxInitialOrderMultiplier));

        float decaySeconds = template.OverrideLifetime
            ? Mathf.Max(1f, template.OverrideDecaySeconds)
            : Mathf.Max(1f, buyer.MultiplierDecaySeconds);

        float holdSeconds = template.OverrideLifetime
            ? Mathf.Max(1f, template.OverrideOneXHoldSeconds)
            : Mathf.Max(1f, buyer.OneXHoldSeconds);

        float stepSeconds = template.OverrideLifetime
            ? Mathf.Max(1f, template.OverrideMultiplierStepSeconds)
            : Mathf.Max(1f, buyer.MultiplierStepSeconds);

        int baseReward = ResolveBaseReward(template, lines);

        var runtimeOrder = new ActiveOrder(
            buyer,
            lines,
            baseReward,
            startMultiplier,
            decaySeconds,
            holdSeconds,
            stepSeconds,
            Mathf.Max(1f, buyer.OfferLifetimeSeconds));

        availableOrders.Add(runtimeOrder);
        OnMarketDataChanged?.Invoke();
    }

    private int CountOutstandingOrdersForBuyer(BuyerProfileSO buyer)
    {
        int count = 0;
        for (int i = 0; i < availableOrders.Count; i++)
            if (availableOrders[i].Buyer == buyer) count++;

        for (int i = 0; i < activeOrders.Count; i++)
            if (activeOrders[i].Buyer == buyer) count++;

        return count;
    }

    private OrderDataSO PickTemplateForBuyer(BuyerProfileSO buyer)
    {
        List<OrderDataSO> candidates = new();

        for (int i = 0; i < orderPool.Length; i++)
        {
            OrderDataSO template = orderPool[i];
            if (template == null || !template.HasValidLines())
                continue;

            if (template.Buyer != null && template.Buyer != buyer)
                continue;

            candidates.Add(template);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private static int ResolveBaseReward(OrderDataSO template, List<ActiveOrderLine> lines)
    {
        int configured = Mathf.Max(1, template.RewardCoins);
        int itemBased = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            ActiveOrderLine line = lines[i];
            if (line?.Item == null)
                continue;

            itemBased += Mathf.Max(1, line.Item.Price) * Mathf.Max(1, line.RequestedAmount);
        }

        return Mathf.Max(configured, itemBased);
    }

    private bool IsBazaarBuyer(BazaarProfileSO buyer)
    {
        BazaarProfileSO bazaar = GetDefaultBuyer();
        return buyer != null && buyer == bazaar;
    }

    public BazaarProfileSO GetDefaultBuyer()
    {
        return GetBazaarBuyer();
    }

    private BazaarProfileSO GetBazaarBuyer()
    {
        if (bazaarBuyer != null)
            return bazaarBuyer;

        return null;
    }
}
