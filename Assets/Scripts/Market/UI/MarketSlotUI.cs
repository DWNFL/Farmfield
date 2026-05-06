using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketSlotUI : MonoBehaviour
{
    public enum SlotMode
    {
        Buy,
        Sell
    }

    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_InputField quantityInput;
    [SerializeField] private Button applyButton;
    [SerializeField] private TMP_Text applyButtonText;

    private SellableItem boundItem;
    private int boundCount;
    private int boundPrice;
    private BuyerProfileSO boundBuyer;
    private SlotMode mode;

    public void Bind(SellableItem item, int count, int pricePerUnit, BuyerProfileSO buyer, SlotMode slotMode)
    {
        boundItem = item;
        boundCount = count;
        boundPrice = pricePerUnit;
        boundBuyer = buyer;
        mode = slotMode;

        if (itemIcon != null && item != null && item.Icon != null)
            itemIcon.sprite = item.Icon;

        if (itemNameText != null)
            itemNameText.text = item != null ? item.ItemName : "Item";

        if (amountText != null)
            amountText.text = slotMode == SlotMode.Sell ? $"x{count}" : "Buy";

        if (priceText != null)
            priceText.text = $"{pricePerUnit}";

        if (quantityInput != null)
            quantityInput.text = "0";

        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(OnApply);
            applyButton.interactable = item != null;
        }

        if (applyButtonText != null)
            applyButtonText.text = slotMode == SlotMode.Buy ? "Queue Buy" : "Queue Sell";
    }

    private void OnApply()
    {
        if (boundItem == null || MarketManager.Instance == null)
            return;

        int amount = ParseInput();
        if (mode == SlotMode.Sell)
            amount = Mathf.Min(amount, boundCount);

        if (mode == SlotMode.Buy)
            MarketManager.Instance.SetQueuedBuy(boundItem, amount);
        else
            MarketManager.Instance.SetQueuedSell(boundItem, amount);
    }

    private int ParseInput()
    {
        if (quantityInput == null)
            return 0;

        if (!int.TryParse(quantityInput.text, out int value))
            return 0;

        return Mathf.Max(0, value);
    }
}
