using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketUIController : MonoBehaviour
{
    [Header("Основная панель")]
    [SerializeField] private GameObject marketPanel;

    [Header("Вкладки")]
    [SerializeField] private Button bazaarTabButton;
    [SerializeField] private Button ordersTabButton;
    [SerializeField] private GameObject bazaarContent;
    [SerializeField] private GameObject ordersContent;

    [Header("Базар — список товаров")]
    [SerializeField] private Transform bazaarItemListParent;
    [SerializeField] private GameObject marketSlotPrefab;

    [Header("Заказы — списки")]
    [SerializeField] private Transform availableOrdersParent;
    [SerializeField] private Transform activeOrdersParent;
    [SerializeField] private GameObject orderCardPrefab;

    [Header("Информация")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Dropdown buyerDropdown;

    [Header("Визуал вкладок")]
    [SerializeField] private Color activeTabColor = new Color(1f, 0.85f, 0.4f);
    [SerializeField] private Color inactiveTabColor = new Color(0.6f, 0.6f, 0.6f);

    private bool isOpen;
    private int currentTab; // 0 = bazaar, 1 = orders
    private BuyerProfileSO selectedBuyer;

    private List<MarketSlotUI> spawnedSlots = new();
    private List<OrderCardUI> spawnedOrderCards = new();

    private void Start()
    {
        if (bazaarTabButton != null)
            bazaarTabButton.onClick.AddListener(() => SwitchTab(0));

        if (ordersTabButton != null)
            ordersTabButton.onClick.AddListener(() => SwitchTab(1));

        if (buyerDropdown != null)
            buyerDropdown.onValueChanged.AddListener(OnBuyerSelected);

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
        // Закрытие по Escape
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        // Обновление таймеров заказов пока открыт UI
        if (isOpen && currentTab == 1)
        {
            foreach (var card in spawnedOrderCards)
            {
                if (card != null)
                    card.UpdateTimer();
            }
        }
    }

    // ───────────────────────────────────────────
    // Открытие / Закрытие
    // ───────────────────────────────────────────

    public void Open()
    {
        if (marketPanel == null)
            return;

        marketPanel.SetActive(true);
        isOpen = true;

        PopulateBuyerDropdown();
        SwitchTab(0);
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

    public bool IsOpen => isOpen;

    // ───────────────────────────────────────────
    // Вкладки
    // ───────────────────────────────────────────

    public void SwitchTab(int tabIndex)
    {
        currentTab = tabIndex;

        if (bazaarContent != null) bazaarContent.SetActive(tabIndex == 0);
        if (ordersContent != null) ordersContent.SetActive(tabIndex == 1);

        // Подсветка активной вкладки
        if (bazaarTabButton != null)
        {
            var colors = bazaarTabButton.colors;
            colors.normalColor = tabIndex == 0 ? activeTabColor : inactiveTabColor;
            bazaarTabButton.colors = colors;
        }

        if (ordersTabButton != null)
        {
            var colors = ordersTabButton.colors;
            colors.normalColor = tabIndex == 1 ? activeTabColor : inactiveTabColor;
            ordersTabButton.colors = colors;
        }

        RefreshUI();
    }

    // ───────────────────────────────────────────
    // Dropdown покупателей
    // ───────────────────────────────────────────

    private void PopulateBuyerDropdown()
    {
        if (buyerDropdown == null || MarketManager.Instance == null)
            return;

        buyerDropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();
        var buyers = MarketManager.Instance.AvailableBuyers;

        for (int i = 0; i < buyers.Count; i++)
        {
            options.Add(new TMP_Dropdown.OptionData(buyers[i].BuyerName, buyers[i].BuyerIcon, Color.white));
        }

        buyerDropdown.AddOptions(options);

        if (buyers.Count > 0)
        {
            selectedBuyer = buyers[0];
            buyerDropdown.value = 0;
        }
    }

    private void OnBuyerSelected(int index)
    {
        if (MarketManager.Instance == null)
            return;

        var buyers = MarketManager.Instance.AvailableBuyers;
        if (index >= 0 && index < buyers.Count)
        {
            selectedBuyer = buyers[index];
            RefreshBazaarTab();
        }
    }

    // ───────────────────────────────────────────
    // Обновление UI
    // ───────────────────────────────────────────

    private void RefreshUI()
    {
        if (!isOpen)
            return;

        if (currentTab == 0)
            RefreshBazaarTab();
        else
            RefreshOrdersTab();
    }

    private void RefreshBazaarTab()
    {
        ClearSpawnedSlots();

        if (InventoryManager.Instance == null || marketSlotPrefab == null || bazaarItemListParent == null)
            return;

        var sellableItems = InventoryManager.Instance.GetSellableItems();

        foreach (var info in sellableItems)
        {
            if (selectedBuyer != null && !selectedBuyer.WillBuyItem(info.Item))
                continue;

            GameObject slotObj = Instantiate(marketSlotPrefab, bazaarItemListParent);
            MarketSlotUI slot = slotObj.GetComponent<MarketSlotUI>();

            if (slot != null)
            {
                int price = MarketManager.Instance != null
                    ? MarketManager.Instance.GetSellPrice(info.Item, selectedBuyer)
                    : info.Item.Price;

                slot.Bind(info.Item, info.TotalCount, price, selectedBuyer);
                spawnedSlots.Add(slot);
            }
        }
    }

    private void RefreshOrdersTab()
    {
        ClearSpawnedOrderCards();

        if (MarketManager.Instance == null || orderCardPrefab == null)
            return;

        // Доступные заказы
        if (availableOrdersParent != null)
        {
            foreach (var template in MarketManager.Instance.AvailableOrders)
            {
                GameObject cardObj = Instantiate(orderCardPrefab, availableOrdersParent);
                OrderCardUI card = cardObj.GetComponent<OrderCardUI>();

                if (card != null)
                {
                    card.BindAvailable(template);
                    spawnedOrderCards.Add(card);
                }
            }
        }

        // Активные заказы
        if (activeOrdersParent != null)
        {
            foreach (var order in MarketManager.Instance.ActiveOrders)
            {
                GameObject cardObj = Instantiate(orderCardPrefab, activeOrdersParent);
                OrderCardUI card = cardObj.GetComponent<OrderCardUI>();

                if (card != null)
                {
                    card.BindActive(order);
                    spawnedOrderCards.Add(card);
                }
            }
        }
    }

    private void UpdateCoinsDisplay(int coins)
    {
        if (coinsText != null)
            coinsText.text = $"{coins}";
    }

    private void ClearSpawnedSlots()
    {
        foreach (var slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        spawnedSlots.Clear();
    }

    private void ClearSpawnedOrderCards()
    {
        foreach (var card in spawnedOrderCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        spawnedOrderCards.Clear();
    }
}
