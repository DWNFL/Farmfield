using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [Header("Покупатели")]
    [Tooltip("Список доступных покупателей (первый = базар)")]
    [SerializeField] private BuyerProfileSO[] availableBuyers;

    [Header("Заказы")]
    [Tooltip("Пул шаблонов заказов, из которых генерируются случайные")]
    [SerializeField] private OrderDataSO[] orderPool;

    [Tooltip("Максимум активных заказов одновременно")]
    [SerializeField] private int maxActiveOrders = 3;

    [Tooltip("Максимум доступных (непринятых) заказов")]
    [SerializeField] private int maxAvailableOrders = 3;

    [Tooltip("Интервал генерации новых заказов (секунды)")]
    [SerializeField] private float orderGenerationInterval = 120f;

    private List<ActiveOrder> activeOrders = new();
    private List<OrderDataSO> availableOrders = new();
    private float orderGenerationTimer;

    public static MarketManager Instance { get; private set; }

    public IReadOnlyList<BuyerProfileSO> AvailableBuyers => availableBuyers;
    public IReadOnlyList<ActiveOrder> ActiveOrders => activeOrders;
    public IReadOnlyList<OrderDataSO> AvailableOrders => availableOrders;

    // События для UI
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
        GenerateNewOrders();
    }

    private void Update()
    {
        UpdateActiveOrders();
        UpdateOrderGeneration();
    }

    // ───────────────────────────────────────────
    // Обычная продажа (базар)
    // ───────────────────────────────────────────

    /// <summary>
    /// Получить цену предмета у конкретного покупателя. Если buyer == null, используется базовая цена.
    /// </summary>
    public int GetSellPrice(SellableItem item, BuyerProfileSO buyer = null)
    {
        if (item == null)
            return 0;

        if (buyer == null)
            return item.Price;

        return buyer.GetPrice(item);
    }

    /// <summary>
    /// Продать предмет из инвентаря. Возвращает полученную сумму (0 = провал).
    /// </summary>
    public int SellItem(SellableItem item, int amount, BuyerProfileSO buyer = null)
    {
        if (item == null || amount <= 0)
            return 0;

        if (InventoryManager.Instance == null || PlayerWallet.Instance == null)
            return 0;

        // Проверяем, покупает ли этот покупатель данный товар
        if (buyer != null && !buyer.WillBuyItem(item))
        {
            Debug.Log($"{buyer.BuyerName} не покупает {item.ItemName}.");
            return 0;
        }

        int available = InventoryManager.Instance.CountItem(item);
        int toSell = Mathf.Min(amount, available);

        if (toSell <= 0)
            return 0;

        int pricePerUnit = GetSellPrice(item, buyer);
        int totalPrice = pricePerUnit * toSell;

        int removed = InventoryManager.Instance.RemoveItem(item, toSell);
        if (removed <= 0)
            return 0;

        totalPrice = pricePerUnit * removed;
        PlayerWallet.Instance.AddCoins(totalPrice);

        string buyerName = buyer != null ? buyer.BuyerName : "Базар";
        Debug.Log($"<color=yellow>ПРОДАНО: {removed}x {item.ItemName} → {buyerName} за {totalPrice} монет</color>");

        OnMarketDataChanged?.Invoke();
        return totalPrice;
    }

    /// <summary>
    /// Продать ВСЕ предметы данного типа.
    /// </summary>
    public int SellAll(SellableItem item, BuyerProfileSO buyer = null)
    {
        if (item == null || InventoryManager.Instance == null)
            return 0;

        int count = InventoryManager.Instance.CountItem(item);
        return SellItem(item, count, buyer);
    }

    // ───────────────────────────────────────────
    // Система заказов
    // ───────────────────────────────────────────

    /// <summary>
    /// Принять заказ (переместить из доступных в активные).
    /// </summary>
    public bool AcceptOrder(OrderDataSO template)
    {
        if (template == null)
            return false;

        if (activeOrders.Count >= maxActiveOrders)
        {
            Debug.Log("Максимум активных заказов достигнут!");
            return false;
        }

        availableOrders.Remove(template);

        var order = new ActiveOrder(template);
        activeOrders.Add(order);

        Debug.Log($"<color=cyan>ЗАКАЗ ПРИНЯТ: {template.RequestedAmount}x {template.RequestedItem.ItemName} для {template.Buyer.BuyerName}</color>");

        OnMarketDataChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Сдать товар по активному заказу. Возвращает количество засчитанных единиц.
    /// </summary>
    public int DeliverToOrder(ActiveOrder order, int amount = -1)
    {
        if (order == null || order.IsCompleted || order.IsExpired)
            return 0;

        if (InventoryManager.Instance == null)
            return 0;

        SellableItem item = order.Template.RequestedItem;
        int available = InventoryManager.Instance.CountItem(item);
        int needed = order.RemainingAmount;

        if (amount < 0)
            amount = Mathf.Min(available, needed);
        else
            amount = Mathf.Min(amount, Mathf.Min(available, needed));

        if (amount <= 0)
            return 0;

        int removed = InventoryManager.Instance.RemoveItem(item, amount);
        order.DeliveredAmount += removed;

        Debug.Log($"<color=cyan>СДАНО: {removed}x {item.ItemName} ({order.DeliveredAmount}/{order.Template.RequestedAmount})</color>");

        if (order.IsCompleted)
        {
            CompleteOrder(order);
        }

        OnMarketDataChanged?.Invoke();
        return removed;
    }

    private void CompleteOrder(ActiveOrder order)
    {
        int reward = order.CalculateReward();
        PlayerWallet.Instance.AddCoins(reward);

        Debug.Log($"<color=green>ЗАКАЗ ВЫПОЛНЕН! Награда: {reward} монет (включая бонус за скорость)</color>");

        activeOrders.Remove(order);
        OnOrderCompleted?.Invoke(order);
        OnMarketDataChanged?.Invoke();
    }

    private void UpdateActiveOrders()
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            ActiveOrder order = activeOrders[i];
            order.TimeRemaining -= Time.deltaTime;

            if (order.IsExpired && !order.IsCompleted)
            {
                Debug.Log($"<color=red>ЗАКАЗ ПРОСРОЧЕН: {order.Template.RequestedItem.ItemName} для {order.Template.Buyer.BuyerName}</color>");
                activeOrders.RemoveAt(i);
                OnOrderExpired?.Invoke(order);
                OnMarketDataChanged?.Invoke();
            }
        }
    }

    private void UpdateOrderGeneration()
    {
        if (orderPool == null || orderPool.Length == 0)
            return;

        orderGenerationTimer -= Time.deltaTime;
        if (orderGenerationTimer <= 0f)
        {
            orderGenerationTimer = orderGenerationInterval;
            GenerateNewOrders();
        }
    }

    /// <summary>
    /// Генерация новых заказов из пула шаблонов.
    /// </summary>
    public void GenerateNewOrders()
    {
        if (orderPool == null || orderPool.Length == 0)
            return;

        while (availableOrders.Count < maxAvailableOrders)
        {
            int index = UnityEngine.Random.Range(0, orderPool.Length);
            OrderDataSO template = orderPool[index];

            // Не добавляем дубликаты
            if (availableOrders.Contains(template))
                continue;

            availableOrders.Add(template);
        }

        OnMarketDataChanged?.Invoke();
    }

    /// <summary>
    /// Получить покупателя "Базар" (первый в списке).
    /// </summary>
    public BuyerProfileSO GetDefaultBuyer()
    {
        if (availableBuyers != null && availableBuyers.Length > 0)
            return availableBuyers[0];

        return null;
    }
}
