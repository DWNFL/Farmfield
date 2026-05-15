using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketUIController : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject marketPanel;

    [Header("Sections")]
    [SerializeField] private GameObject bazaarContent;
    [SerializeField] private GameObject ordersContent;

    [Header("Bazaar Lists")]
    [SerializeField] private Transform buyListParent;
    [SerializeField] private Transform sellListParent;
    [SerializeField] private GameObject marketSlotPrefab;

    [Header("Bazaar Totals")]
    [SerializeField] private TMP_Text buyTotalText;
    [SerializeField] private TMP_Text sellTotalText;
    [SerializeField] private TMP_Text netTotalText;
    [SerializeField] private TMP_Text dispatchTimerText;
    [SerializeField] private Button dispatchButton;

    [Header("Orders")]
    [SerializeField] private Transform availableOrdersParent;
    [SerializeField] private Transform activeOrdersParent;
    [SerializeField] private GameObject orderCardPrefab;

    [Header("Info")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Dropdown buyerDropdown;

    private bool isOpen;
    private BazaarProfileSO selectedBuyer;

    private readonly List<MarketSlotUI> spawnedBuySlots = new();
    private readonly List<MarketSlotUI> spawnedSellSlots = new();
    private readonly List<OrderCardUI> spawnedOrderCards = new();

    private void Start()
    {
        if (buyerDropdown != null)
            buyerDropdown.interactable = false;

        if (dispatchButton != null)
            dispatchButton.onClick.AddListener(OnDispatch);

        if (marketPanel != null)
            marketPanel.SetActive(false);

        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnCoinsChanged += UpdateCoinsDisplay;

        if (MarketManager.Instance != null)
            MarketManager.Instance.OnMarketDataChanged += RefreshUI;
    }

    private void OnDestroy()
    {
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnCoinsChanged -= UpdateCoinsDisplay;

        if (MarketManager.Instance != null)
            MarketManager.Instance.OnMarketDataChanged -= RefreshUI;
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();

        if (isOpen)
        {
            for (int i = 0; i < spawnedOrderCards.Count; i++)
                if (spawnedOrderCards[i] != null)
                    spawnedOrderCards[i].UpdateTimer();
        }

        if (isOpen)
            UpdateDispatchInfo();
    }

    public void Open()
    {
        if (marketPanel == null)
            return;

        marketPanel.SetActive(true);
        isOpen = true;
        PopulateBuyerDropdown();
        ShowAllSections();
        RefreshUI();
        UpdateCoinsDisplay(PlayerWallet.Instance != null ? PlayerWallet.Instance.Coins : 0);
    }

    public void Close()
    {
        if (marketPanel == null)
            return;

        marketPanel.SetActive(false);
        isOpen = false;
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    private void ShowAllSections()
    {
        if (bazaarContent != null) bazaarContent.SetActive(true);
        if (ordersContent != null) ordersContent.SetActive(true);
    }

    private void PopulateBuyerDropdown()
    {
        if (buyerDropdown == null || MarketManager.Instance == null)
            return;

        buyerDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        BazaarProfileSO bazaar = MarketManager.Instance.BazaarBuyer;
        if (bazaar != null)
            options.Add(new TMP_Dropdown.OptionData(bazaar.BuyerName, bazaar.BuyerIcon, Color.white));

        buyerDropdown.AddOptions(options);
        if (bazaar != null)
        {
            selectedBuyer = bazaar;
            buyerDropdown.value = 0;
        }
    }

    private void OnBuyerSelected(int index)
    {
        _ = index;
        selectedBuyer = MarketManager.Instance != null ? MarketManager.Instance.BazaarBuyer : null;
        RefreshBazaarTab();
    }

    private void RefreshUI()
    {
        if (!isOpen)
            return;

        ShowAllSections();
        RefreshBazaarTab();
        RefreshOrdersTab();
    }

    private void RefreshBazaarTab()
    {
        ClearSlotLists();
        if (MarketManager.Instance == null || marketSlotPrefab == null)
            return;

        if (buyListParent != null)
        {
            var buyItems = MarketManager.Instance.PurchasableItems;
            for (int i = 0; i < buyItems.Count; i++)
            {
                SellableItem item = buyItems[i];
                if (item == null)
                    continue;

                GameObject slotObj = Instantiate(marketSlotPrefab, buyListParent);
                MarketSlotUI slot = slotObj.GetComponent<MarketSlotUI>();
                if (slot == null)
                    continue;

                slot.Bind(
                    item,
                    0,
                    MarketManager.Instance.GetBuyPrice(item),
                    selectedBuyer,
                    MarketSlotUI.SlotMode.Buy,
                    MarketManager.Instance.GetQueuedBuyAmount(item),
                    UpdateDispatchInfo);
                spawnedBuySlots.Add(slot);
            }
        }

        if (sellListParent != null && InventoryManager.Instance != null)
        {
            var sellableItems = InventoryManager.Instance.GetSellableItems();
            for (int i = 0; i < sellableItems.Count; i++)
            {
                var info = sellableItems[i];
                GameObject slotObj = Instantiate(marketSlotPrefab, sellListParent);
                MarketSlotUI slot = slotObj.GetComponent<MarketSlotUI>();
                if (slot == null)
                    continue;

                int price = MarketManager.Instance.GetSellPrice(info.Item, selectedBuyer);
                slot.Bind(
                    info.Item,
                    info.TotalCount,
                    price,
                    selectedBuyer,
                    MarketSlotUI.SlotMode.Sell,
                    MarketManager.Instance.GetQueuedSellAmount(info.Item),
                    UpdateDispatchInfo);
                spawnedSellSlots.Add(slot);
            }
        }

        UpdateDispatchInfo();
    }

    private void UpdateDispatchInfo()
    {
        if (MarketManager.Instance == null)
            return;

        int buy = MarketManager.Instance.GetQueuedBuyTotal();
        int sell = MarketManager.Instance.GetQueuedSellTotal(selectedBuyer);
        int net = sell - buy;

        if (buyTotalText != null) buyTotalText.text = $"-{buy}";
        if (sellTotalText != null) sellTotalText.text = $"+{sell}";
        if (netTotalText != null)
        {
            string sign = net >= 0 ? "+" : string.Empty;
            netTotalText.text = $"{sign}{net}";
        }

        if (dispatchTimerText != null)
        {
            if (MarketManager.Instance.DispatchInProgress)
            {
                int sec = Mathf.CeilToInt(MarketManager.Instance.DispatchTimeRemaining);
                dispatchTimerText.text = $"Truck: {sec}s";
            }
            else
            {
                dispatchTimerText.text = "Truck ready";
            }
        }

        if (dispatchButton != null)
            dispatchButton.interactable = !MarketManager.Instance.DispatchInProgress && MarketManager.Instance.HasQueuedTrade();
    }

    private void OnDispatch()
    {
        if (MarketManager.Instance == null)
            return;

        MarketManager.Instance.DispatchQueuedTrade(selectedBuyer);
    }

    private void RefreshOrdersTab()
    {
        ClearOrderCards();

        if (MarketManager.Instance == null || orderCardPrefab == null)
            return;

        if (availableOrdersParent != null)
        {
            foreach (var order in MarketManager.Instance.AvailableOrders)
            {
                GameObject cardObj = Instantiate(orderCardPrefab, availableOrdersParent);
                OrderCardUI card = cardObj.GetComponent<OrderCardUI>();
                if (card == null)
                    continue;

                card.BindAvailable(order);
                spawnedOrderCards.Add(card);
            }
        }

        if (activeOrdersParent != null)
        {
            foreach (var order in MarketManager.Instance.ActiveOrders)
            {
                GameObject cardObj = Instantiate(orderCardPrefab, activeOrdersParent);
                OrderCardUI card = cardObj.GetComponent<OrderCardUI>();
                if (card == null)
                    continue;

                card.BindActive(order);
                spawnedOrderCards.Add(card);
            }
        }
    }

    private void UpdateCoinsDisplay(int coins)
    {
        if (coinsText != null)
            coinsText.text = $"{coins}";
    }

    private void ClearSlotLists()
    {
        for (int i = 0; i < spawnedBuySlots.Count; i++)
            if (spawnedBuySlots[i] != null)
                Destroy(spawnedBuySlots[i].gameObject);

        for (int i = 0; i < spawnedSellSlots.Count; i++)
            if (spawnedSellSlots[i] != null)
                Destroy(spawnedSellSlots[i].gameObject);

        spawnedBuySlots.Clear();
        spawnedSellSlots.Clear();
    }

    private void ClearOrderCards()
    {
        for (int i = 0; i < spawnedOrderCards.Count; i++)
            if (spawnedOrderCards[i] != null)
                Destroy(spawnedOrderCards[i].gameObject);

        spawnedOrderCards.Clear();
    }
}
