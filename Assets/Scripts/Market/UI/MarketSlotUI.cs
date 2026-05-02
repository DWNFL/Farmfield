using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button sellOneButton;
    [SerializeField] private Button sellAllButton;

    private SellableItem boundItem;
    private int boundCount;
    private int boundPrice;
    private BuyerProfileSO boundBuyer;

    public void Bind(SellableItem item, int count, int pricePerUnit, BuyerProfileSO buyer)
    {
        boundItem = item;
        boundCount = count;
        boundPrice = pricePerUnit;
        boundBuyer = buyer;

        if (itemIcon != null && item.Icon != null)
            itemIcon.sprite = item.Icon;

        if (itemNameText != null)
            itemNameText.text = item.ItemName;

        if (amountText != null)
            amountText.text = $"x{count}";

        if (priceText != null)
            priceText.text = $"{pricePerUnit} \u20bd/шт";

        if (sellOneButton != null)
        {
            sellOneButton.onClick.RemoveAllListeners();
            sellOneButton.onClick.AddListener(OnSellOne);
            sellOneButton.interactable = count > 0;
        }

        if (sellAllButton != null)
        {
            sellAllButton.onClick.RemoveAllListeners();
            sellAllButton.onClick.AddListener(OnSellAll);
            sellAllButton.interactable = count > 0;
        }
    }

    private void OnSellOne()
    {
        if (MarketManager.Instance == null || boundItem == null)
            return;

        MarketManager.Instance.SellItem(boundItem, 1, boundBuyer);
    }

    private void OnSellAll()
    {
        if (MarketManager.Instance == null || boundItem == null)
            return;

        MarketManager.Instance.SellAll(boundItem, boundBuyer);
    }
}
