using System.Collections.Generic;
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

    [Header("Summary")]
    [SerializeField] private Transform linesSummaryIconsParent;
    [SerializeField] private TMP_Text singleLineAmountText;
    [SerializeField] private Vector2 singleLineIconSize = new(48f, 48f);
    [SerializeField] private Vector2 multiLineIconSize = new(24f, 24f);
    [SerializeField] private Button expandButton;
    [SerializeField] private GameObject linesDetailsRoot;
    [SerializeField] private Transform linesDetailsParent;
    [SerializeField] private GameObject orderLinePrefab;

    [Header("Actions")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Button deliverAllButton;

    [Header("State Visuals")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Color availableBackgroundColor = Color.white;
    [SerializeField] private Color activeBackgroundColor = new Color(0.94f, 0.98f, 1f, 1f);

    private ActiveOrder boundOrder;
    private bool isActive;
    private bool expanded;
    private readonly List<OrderLineUI> spawnedLineViews = new();
    private readonly List<GameObject> spawnedSummaryIcons = new();

    public void BindAvailable(ActiveOrder order)
    {
        boundOrder = order;
        isActive = false;
        expanded = false;

        BindCommon();
        SetButton(acceptButton, OnAccept, true);
        SetButton(rejectButton, OnReject, true);
        SetButton(deliverAllButton, OnDeliverAll, false);
        ApplyStateVisuals();
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
        ApplyStateVisuals();
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

        if (expandButton != null)
        {
            expandButton.onClick.RemoveAllListeners();
            expandButton.onClick.AddListener(ToggleExpanded);
            expandButton.gameObject.SetActive(boundOrder.Lines.Count > 1);
        }

        RebuildLineViews();
        RebuildSummaryIcons();
        UpdateSummary();
        UpdateTimer();
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
            lifeTimerText.text = $"{m:D2}:{s:D2}";
            lifeTimerText.color = rem < 30f ? Color.red : Color.white;
        }

        if (nextDropTimerText != null)
        {
            float toDrop = boundOrder.TimeToNextMultiplierDrop;
            if (!isActive)
                nextDropTimerText.text = string.Empty;
            else if (toDrop <= 0f || boundOrder.CurrentMultiplier <= 1f)
                nextDropTimerText.text = "1x";
            else
            {
                int dm = Mathf.FloorToInt(toDrop / 60f);
                int ds = Mathf.FloorToInt(toDrop % 60f);
                nextDropTimerText.text = $"{dm:D2}:{ds:D2}";
            }
        }

        if (rewardText != null)
            rewardText.text = $"{boundOrder.CalculateReward()}";

        if (multiplierText != null)
            multiplierText.text = $"x{boundOrder.CurrentMultiplier:F1}";

        UpdateSingleLineAmount();
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        if (boundOrder == null)
            return;

        RefreshLineViews();
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

    private void RebuildLineViews()
    {
        ClearLineViews();

        if (boundOrder == null || orderLinePrefab == null || boundOrder.Lines.Count <= 1)
            return;

        Transform parent = linesDetailsParent != null
            ? linesDetailsParent
            : linesDetailsRoot != null ? linesDetailsRoot.transform : null;

        if (parent == null)
            return;

        for (int i = 0; i < boundOrder.Lines.Count; i++)
        {
            GameObject lineObj = Instantiate(orderLinePrefab, parent);
            OrderLineUI lineView = lineObj.GetComponent<OrderLineUI>();
            if (lineView == null)
                continue;

            int idx = i;
            lineView.Bind(boundOrder.Lines[i], isActive, () => OnDeliverLine(idx));
            spawnedLineViews.Add(lineView);
        }
    }

    private void RebuildSummaryIcons()
    {
        ClearSummaryIcons();
        UpdateSingleLineAmount();

        if (boundOrder == null || linesSummaryIconsParent == null)
            return;

        bool singleLine = boundOrder.Lines.Count == 1;
        Vector2 iconSize = singleLine ? singleLineIconSize : multiLineIconSize;

        for (int i = 0; i < boundOrder.Lines.Count; i++)
        {
            ActiveOrderLine line = boundOrder.Lines[i];
            if (line?.Item == null || line.Item.Icon == null)
                continue;

            GameObject iconObj = new GameObject($"{line.Item.ItemName}_SummaryIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(linesSummaryIconsParent, false);

            var rect = iconObj.GetComponent<RectTransform>();
            rect.sizeDelta = iconSize;

            var layout = iconObj.AddComponent<LayoutElement>();
            layout.preferredWidth = iconSize.x;
            layout.preferredHeight = iconSize.y;

            Image image = iconObj.GetComponent<Image>();
            image.sprite = line.Item.Icon;
            image.preserveAspect = true;

            spawnedSummaryIcons.Add(iconObj);
        }
    }

    private void RefreshLineViews()
    {
        if (boundOrder == null)
            return;

        for (int i = 0; i < spawnedLineViews.Count; i++)
        {
            if (spawnedLineViews[i] == null || i >= boundOrder.Lines.Count)
                continue;

            int idx = i;
            spawnedLineViews[i].Bind(boundOrder.Lines[i], isActive, () => OnDeliverLine(idx));
        }
    }

    private void ClearLineViews()
    {
        for (int i = 0; i < spawnedLineViews.Count; i++)
            if (spawnedLineViews[i] != null)
                Destroy(spawnedLineViews[i].gameObject);

        spawnedLineViews.Clear();
    }

    private void ClearSummaryIcons()
    {
        for (int i = 0; i < spawnedSummaryIcons.Count; i++)
            if (spawnedSummaryIcons[i] != null)
                Destroy(spawnedSummaryIcons[i]);

        spawnedSummaryIcons.Clear();
    }

    private void UpdateSingleLineAmount()
    {
        if (singleLineAmountText == null)
            return;

        if (boundOrder == null || boundOrder.Lines.Count != 1)
        {
            singleLineAmountText.gameObject.SetActive(false);
            return;
        }

        ActiveOrderLine line = boundOrder.Lines[0];
        singleLineAmountText.gameObject.SetActive(true);
        singleLineAmountText.text = $"x{Mathf.Max(1, line.RequestedAmount)}";
    }

    private void ApplyStateVisuals()
    {
        if (cardBackground != null)
            cardBackground.color = isActive ? activeBackgroundColor : availableBackgroundColor;
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
