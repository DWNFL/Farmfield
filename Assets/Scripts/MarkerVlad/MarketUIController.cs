using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MarketUIController : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject marketPanel;
    public GameObject bazaarContent;
    public GameObject ordersContent;

    [Header("Tabs")]
    public Button bazaarTabButton;
    public Button ordersTabButton;

    [Header("Bazaar Containers")]
    public Transform buyListParent;
    public Transform sellListParent;
    public TextMeshProUGUI buyTotalText;
    public TextMeshProUGUI sellTotalText;
    public TextMeshProUGUI netTotalText;
    public TextMeshProUGUI dispatchTimerText;
    public Button dispatchButton;

    [Header("Orders Containers")]
    public Transform availableOrdersParent;
    public Transform activeOrdersParent;

    [Header("Global Info")]
    public TextMeshProUGUI coinsText;
    public TMP_Dropdown buyerDropdown;

    [Header("Prefabs")]
    public GameObject marketSlotPrefab;
    public GameObject orderCardPrefab;

    private void Start()
    {
        // Настройка вкладок
        bazaarTabButton.onClick.AddListener(() => SwitchTab(true));
        ordersTabButton.onClick.AddListener(() => SwitchTab(false));

        // Скрываем рынок при старте
        marketPanel.SetActive(false);
        SwitchTab(true);
    }

    private void Update()
    {
        // Кнопка М (англ) открывает/закрывает рынок
        if (Input.GetKeyDown(KeyCode.M)) ToggleMarket();

        // ТЕСТОВЫЕ КНОПКИ ДЛЯ СПАВНА
        if (Input.GetKeyDown(KeyCode.K)) SpawnTestItem(true);  // Спавн в покупку
        if (Input.GetKeyDown(KeyCode.L)) SpawnTestItem(false); // Спавн в продажу
    }

    public void ToggleMarket()
    {
        marketPanel.SetActive(!marketPanel.activeSelf);
    }

    public void SwitchTab(bool isBazaar)
    {
        bazaarContent.SetActive(isBazaar);
        ordersContent.SetActive(!isBazaar);
    }

    // Тестовая функция спавна карточки
    void SpawnTestItem(bool isBuy)
    {
        Transform parent = isBuy ? buyListParent : sellListParent;
        GameObject newSlot = Instantiate(marketSlotPrefab, parent);
        MarketSlotUI slotScript = newSlot.GetComponent<MarketSlotUI>();

        if (slotScript != null)
            slotScript.Setup("Пшеница", "50$", "10", !isBuy);
    }
}