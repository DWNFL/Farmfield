using System;
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
    [SerializeField] private TMP_Text unitPriceText;
    [SerializeField] private TMP_InputField quantityInput;
    [SerializeField] private Button applyButton;
    [SerializeField] private TMP_Text applyButtonText;
    [SerializeField] private Button maxButton;

    private SellableItem boundItem;
    private int boundCount;
    private int boundPricePerUnit;
    private SlotMode mode;
    private Action totalsChanged;

    public void Bind(
        SellableItem item,
        int count,
        int pricePerUnit,
        BazaarProfileSO buyer,
        SlotMode slotMode,
        int queuedAmount,
        Action onTotalsChanged)
    {
        _ = buyer;
        boundItem = item;
        boundCount = Mathf.Max(0, count);
        boundPricePerUnit = Mathf.Max(0, pricePerUnit);
        mode = slotMode;
        totalsChanged = onTotalsChanged;

        if (itemIcon != null && item != null && item.Icon != null)
            itemIcon.sprite = item.Icon;

        if (itemNameText != null)
            itemNameText.text = item != null ? item.ItemName : "Item";

        if (amountText != null)
            amountText.text = slotMode == SlotMode.Sell ? $"Have: {boundCount}" : "Buy";

        if (unitPriceText != null)
            unitPriceText.text = $"{boundPricePerUnit} ea";

        if (quantityInput != null)
        {
            quantityInput.onValueChanged.RemoveListener(OnQuantityChanged);
            quantityInput.text = Mathf.Max(0, queuedAmount).ToString();
            quantityInput.onValueChanged.AddListener(OnQuantityChanged);
        }

        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(ApplyQueuedAmount);
            applyButton.interactable = item != null;
        }

        if (applyButtonText != null)
            applyButtonText.text = "Set";

        if (maxButton != null)
        {
            maxButton.onClick.RemoveAllListeners();
            maxButton.gameObject.SetActive(slotMode == SlotMode.Sell);
            maxButton.interactable = item != null && slotMode == SlotMode.Sell && boundCount > 0;
            maxButton.onClick.AddListener(SetMaxSellAmount);
        }

        UpdateCardTotal();
        ApplyQueuedAmount();
    }

    private void OnDestroy()
    {
        if (quantityInput != null)
            quantityInput.onValueChanged.RemoveListener(OnQuantityChanged);
    }

    private void OnQuantityChanged(string _)
    {
        ApplyQueuedAmount();
    }

    private void ApplyQueuedAmount()
    {
        if (boundItem == null || MarketManager.Instance == null)
            return;

        int amount = ParseInput();

        if (mode == SlotMode.Buy)
            MarketManager.Instance.SetQueuedBuy(boundItem, amount);
        else
            MarketManager.Instance.SetQueuedSell(boundItem, amount);

        UpdateCardTotal();
        totalsChanged?.Invoke();
    }

    private void SetMaxSellAmount()
    {
        if (quantityInput != null)
            quantityInput.text = boundCount.ToString();
    }

    private int ParseInput()
    {
        if (quantityInput == null)
            return 0;

        if (!int.TryParse(quantityInput.text, out int value))
            return 0;

        value = Mathf.Max(0, value);
        return mode == SlotMode.Sell ? Mathf.Min(value, boundCount) : value;
    }

    private void UpdateCardTotal()
    {
        if (priceText == null)
            return;

        int total = boundPricePerUnit * ParseInput();
        priceText.text = $"{total}";
    }
}
