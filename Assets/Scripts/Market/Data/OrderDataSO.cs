using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Market/Order Template")]
public class OrderDataSO : ScriptableObject
{
    [Header("Optional Buyer Restriction")]
    [Tooltip("If set, this template is used only for this buyer")]
    public BuyerProfileSO Buyer;

    [Header("Order Lines")]
    [Tooltip("Main format: one contract can request multiple items")]
    public OrderRequestLine[] RequestedLines;

    [Header("Legacy Single Item (optional fallback)")]
    public SellableItem RequestedItem;
    public bool UseAmountRange = true;
    public int RequestedAmount = 10;
    public int MinRequestedAmount = 10;
    public int MaxRequestedAmount = 30;

    [Header("Reward")]
    [Tooltip("Base reward before dynamic order multiplier")]
    public int RewardCoins = 50;

    [Header("Lifetime Override")]
    [Tooltip("If true, override buyer decay/hold durations for this template")]
    public bool OverrideLifetime;
    public float OverrideDecaySeconds = 90f;
    public float OverrideOneXHoldSeconds = 60f;

    public bool HasValidLines()
    {
        if (RequestedLines != null)
        {
            for (int i = 0; i < RequestedLines.Length; i++)
            {
                if (RequestedLines[i] != null && RequestedLines[i].Item != null)
                    return true;
            }
        }

        return RequestedItem != null;
    }

    public List<ActiveOrderLine> CreateRuntimeLines(BuyerProfileSO buyer)
    {
        var lines = new List<ActiveOrderLine>();

        if (RequestedLines != null)
        {
            for (int i = 0; i < RequestedLines.Length; i++)
            {
                OrderRequestLine line = RequestedLines[i];
                if (line == null || line.Item == null)
                    continue;

                if (buyer != null && !buyer.WillBuyItem(line.Item))
                    continue;

                lines.Add(new ActiveOrderLine(line.Item, line.RollAmount()));
            }
        }

        if (lines.Count == 0 && RequestedItem != null)
        {
            int amount = UseAmountRange
                ? UnityEngine.Random.Range(Mathf.Max(1, MinRequestedAmount), Mathf.Max(MinRequestedAmount, MaxRequestedAmount) + 1)
                : Mathf.Max(1, RequestedAmount);

            lines.Add(new ActiveOrderLine(RequestedItem, amount));
        }

        return lines;
    }
}
