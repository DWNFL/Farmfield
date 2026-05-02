using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderCardUI : MonoBehaviour
{
    [SerializeField] private Image buyerIcon;
    [SerializeField] private TMP_Text buyerNameText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    private OrderDataSO availableTemplate;
    private ActiveOrder activeOrder;
    private bool isActive;

    public void BindAvailable(OrderDataSO template)
    {
        availableTemplate = template;
        activeOrder = null;
        isActive = false;

        SetInfo(template.Buyer, template.RequestedItem, template.RequestedAmount, template.RewardCoins);

        if (timerText != null)
            timerText.text = $"{template.TimeLimitSeconds / 60f:F0} мин";

        if (timerSlider != null) timerSlider.gameObject.SetActive(false);
        if (progressSlider != null) progressSlider.gameObject.SetActive(false);

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnAccept);
        }

        if (actionButtonText != null) actionButtonText.text = "Принять";
    }

    public void BindActive(ActiveOrder order)
    {
        activeOrder = order;
        availableTemplate = null;
        isActive = true;

        var t = order.Template;
        SetInfo(t.Buyer, t.RequestedItem, t.RequestedAmount, order.CalculateReward());

        if (amountText != null)
            amountText.text = $"{order.DeliveredAmount}/{t.RequestedAmount}";

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.maxValue = t.RequestedAmount;
            progressSlider.value = order.DeliveredAmount;
        }

        if (timerSlider != null)
        {
            timerSlider.gameObject.SetActive(true);
            timerSlider.maxValue = t.TimeLimitSeconds;
        }

        UpdateTimer();

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnDeliver);
        }

        if (actionButtonText != null) actionButtonText.text = "Сдать";
    }

    private void SetInfo(BuyerProfileSO buyer, SellableItem item, int amount, int reward)
    {
        if (buyerIcon != null && buyer.BuyerIcon != null) buyerIcon.sprite = buyer.BuyerIcon;
        if (buyerNameText != null) buyerNameText.text = buyer.BuyerName;
        if (itemIcon != null && item.Icon != null) itemIcon.sprite = item.Icon;
        if (itemNameText != null) itemNameText.text = item.ItemName;
        if (amountText != null) amountText.text = $"x{amount}";
        if (rewardText != null) rewardText.text = $"{reward}";
    }

    public void UpdateTimer()
    {
        if (!isActive || activeOrder == null) return;

        float rem = Mathf.Max(0f, activeOrder.TimeRemaining);
        int m = Mathf.FloorToInt(rem / 60f);
        int s = Mathf.FloorToInt(rem % 60f);

        if (timerText != null)
        {
            timerText.text = $"{m:D2}:{s:D2}";
            timerText.color = rem < 30f ? Color.red : Color.white;
        }

        if (timerSlider != null) timerSlider.value = rem;
        if (rewardText != null) rewardText.text = $"{activeOrder.CalculateReward()}";
    }

    private void OnAccept()
    {
        if (availableTemplate != null && MarketManager.Instance != null)
            MarketManager.Instance.AcceptOrder(availableTemplate);
    }

    private void OnDeliver()
    {
        if (activeOrder != null && MarketManager.Instance != null)
            MarketManager.Instance.DeliverToOrder(activeOrder);
    }
}
