using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderCardUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Image buyerIcon;
    [SerializeField] private TMP_Text buyerNameText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text multiplierText;

    [Header("Timers")]
    [SerializeField] private TMP_Text lifeTimerText;
    [SerializeField] private TMP_Text nextDropTimerText;
    [SerializeField] private Slider lifeTimerSlider;

    [Header("Summary")]
    [SerializeField] private TMP_Text linesSummaryText;
    [SerializeField] private Button expandButton;
    [SerializeField] private GameObject linesDetailsRoot;

    [Header("Line Delivery Buttons")]
    [SerializeField] private Button[] deliverLineButtons;
    [SerializeField] private TMP_Text[] deliverLineLabels;

    [Header("Actions")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Button deliverAllButton;

    private ActiveOrder boundOrder;
    private bool isActive;
    private bool expanded;

    public void BindAvailable(ActiveOrder order)
    {
        boundOrder = order;
        isActive = false;
        expanded = false;
        BindCommon();

        SetButton(acceptButton, OnAccept, true);
        SetButton(rejectButton, OnReject, true);
        SetButton(deliverAllButton, OnDeliverAll, false);
        SetDetailsVisible(false);
    }

    public void BindActive(ActiveOrder order)
    {
        boundOrder = order;
        isActive = true;
        expanded = false;
        BindCommon();

        SetButton(acceptButton, OnAccept, false);
        SetButton(rejectButton, OnReject, false);
        SetButton(deliverAllButton, OnDeliverAll, true);
        SetDetailsVisible(false);
    }

    private void BindCommon()
    {
        if (boundOrder == null)
            return;

        if (buyerIcon != null && boundOrder.Buyer != null && boundOrder.Buyer.BuyerIcon != null)
            buyerIcon.sprite = boundOrder.Buyer.BuyerIcon;

        if (buyerNameText != null)
            buyerNameText.text = boundOrder.Buyer != null ? boundOrder.Buyer.BuyerName : "Buyer";

        if (lifeTimerSlider != null)
        {
            lifeTimerSlider.maxValue = isActive
                ? Mathf.Max(1f, boundOrder.TotalExecutionLifetimeSeconds)
                : Mathf.Max(1f, boundOrder.OfferLifetimeSeconds);
        }

        if (expandButton != null)
        {
            expandButton.onClick.RemoveAllListeners();
            expandButton.onClick.AddListener(ToggleExpanded);
            expandButton.gameObject.SetActive(boundOrder.Lines.Count > 1);
        }

        UpdateSummary();
        UpdateTimer();
        BindLineButtons();
    }

    private void BindLineButtons()
    {
        int lineCount = boundOrder != null ? boundOrder.Lines.Count : 0;

        for (int i = 0; i < deliverLineButtons.Length; i++)
        {
            bool active = isActive && i < lineCount;
            Button btn = deliverLineButtons[i];
            if (btn == null)
                continue;

            btn.gameObject.SetActive(active && expanded);
            btn.onClick.RemoveAllListeners();

            int idx = i;
            if (active)
                btn.onClick.AddListener(() => OnDeliverLine(idx));

            if (deliverLineLabels != null && i < deliverLineLabels.Length && deliverLineLabels[i] != null)
            {
                if (active)
                {
                    ActiveOrderLine line = boundOrder.Lines[i];
                    deliverLineLabels[i].text = line.Item != null
                        ? $"Deliver {line.Item.ItemName}"
                        : "Deliver line";
                }
            }
        }
    }

    private void UpdateSummary()
    {
        if (boundOrder == null || linesSummaryText == null)
            return;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < boundOrder.Lines.Count; i++)
        {
            ActiveOrderLine line = boundOrder.Lines[i];
            if (line == null || line.Item == null)
                continue;

            if (sb.Length > 0)
                sb.Append('\n');

            if (isActive)
                sb.Append($"{line.Item.ItemName}: {line.DeliveredAmount}/{line.RequestedAmount}");
            else
                sb.Append($"{line.Item.ItemName}: x{line.RequestedAmount}");
        }

        linesSummaryText.text = sb.ToString();
    }

    public void UpdateTimer()
    {
        if (boundOrder == null)
            return;

        float rem = Mathf.Max(0f, boundOrder.TimeRemaining);
        int m = Mathf.FloorToInt(rem / 60f);
        int s = Mathf.FloorToInt(rem % 60f);

        if (lifeTimerText != null)
        {
            lifeTimerText.text = isActive
                ? $"Contract ends: {m:D2}:{s:D2}"
                : $"Offer expires: {m:D2}:{s:D2}";
            lifeTimerText.color = rem < 30f ? Color.red : Color.white;
        }

        if (nextDropTimerText != null)
        {
            float toDrop = boundOrder.TimeToNextMultiplierDrop;
            if (!isActive)
            {
                nextDropTimerText.text = "Drop starts after accept";
            }
            else if (toDrop <= 0f || boundOrder.CurrentMultiplier <= 1f)
            {
                nextDropTimerText.text = "Next drop: 1x lock";
            }
            else
            {
                int dm = Mathf.FloorToInt(toDrop / 60f);
                int ds = Mathf.FloorToInt(toDrop % 60f);
                nextDropTimerText.text = $"Next drop: {dm:D2}:{ds:D2}";
            }
        }

        if (lifeTimerSlider != null)
            lifeTimerSlider.value = rem;

        if (rewardText != null)
            rewardText.text = $"{boundOrder.CalculateReward()}";

        if (multiplierText != null)
            multiplierText.text = $"x{boundOrder.CurrentMultiplier:F2}";

        UpdateSummary();
    }

    private void ToggleExpanded()
    {
        expanded = !expanded;
        SetDetailsVisible(expanded);
    }

    private void SetDetailsVisible(bool visible)
    {
        if (linesDetailsRoot != null)
            linesDetailsRoot.SetActive(visible);

        for (int i = 0; i < deliverLineButtons.Length; i++)
        {
            if (deliverLineButtons[i] != null)
            {
                bool canShow = isActive && boundOrder != null && i < boundOrder.Lines.Count;
                deliverLineButtons[i].gameObject.SetActive(visible && canShow);
            }
        }
    }

    private void OnAccept()
    {
        if (boundOrder != null && MarketManager.Instance != null)
            MarketManager.Instance.AcceptOrder(boundOrder);
    }

    private void OnReject()
    {
        if (boundOrder != null && MarketManager.Instance != null)
            MarketManager.Instance.RejectOrder(boundOrder);
    }

    private void OnDeliverAll()
    {
        if (boundOrder != null && MarketManager.Instance != null)
            MarketManager.Instance.DeliverToOrder(boundOrder);
    }

    private void OnDeliverLine(int lineIndex)
    {
        if (boundOrder != null && MarketManager.Instance != null)
            MarketManager.Instance.DeliverOrderLine(boundOrder, lineIndex);
    }

    private static void SetButton(Button button, UnityEngine.Events.UnityAction action, bool active)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(active);
        button.onClick.RemoveAllListeners();
        if (active)
            button.onClick.AddListener(action);
    }
}
