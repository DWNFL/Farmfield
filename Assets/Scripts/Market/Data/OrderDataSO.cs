using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Market/Order Template")]
public class OrderDataSO : ScriptableObject
{
    [Header("Optional Buyer Restriction")]
    public BuyerProfileSO Buyer;

    [Header("Order Lines")]
    public OrderRequestLine[] RequestedLines;

    [Header("Legacy fallback")]
    public SellableItem RequestedItem;
    public int RequestedAmount = 10;

    [Header("Reward")]
    public int RewardCoins = 50;

    [Header("Lifetime Override")]
    public bool OverrideLifetime;
    public float OverrideDecaySeconds = 90f;
    public float OverrideMultiplierStepSeconds = 15f;
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

    public List<ActiveOrderLine> CreateRuntimeLines()
    {
        var lines = new List<ActiveOrderLine>();

        if (RequestedLines != null)
        {
            for (int i = 0; i < RequestedLines.Length; i++)
            {
                OrderRequestLine line = RequestedLines[i];
                if (line == null || line.Item == null)
                    continue;

                lines.Add(new ActiveOrderLine(line.Item, line.GetAmount()));
            }
        }

        if (lines.Count == 0 && RequestedItem != null)
        {
            lines.Add(new ActiveOrderLine(RequestedItem, Mathf.Max(1, RequestedAmount)));
        }

        return lines;
    }
}
