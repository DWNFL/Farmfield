using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActiveOrderLine
{
    public SellableItem Item;
    public int RequestedAmount;
    public int DeliveredAmount;

    public int RemainingAmount => Mathf.Max(0, RequestedAmount - DeliveredAmount);
    public bool IsCompleted => DeliveredAmount >= RequestedAmount;

    public ActiveOrderLine(SellableItem item, int requestedAmount)
    {
        Item = item;
        RequestedAmount = Mathf.Max(1, requestedAmount);
        DeliveredAmount = 0;
    }
}

public class ActiveOrder
{
    public BuyerProfileSO Buyer { get; }
    public int BaseRewardCoins { get; }
    public float InitialMultiplier { get; }
    public float DecayDurationSeconds { get; }
    public float OneXHoldDurationSeconds { get; }
    public float MultiplierStepSeconds { get; }
    public float OfferLifetimeSeconds { get; }
    public float TotalExecutionLifetimeSeconds { get; }
    public bool IsAccepted { get; private set; }
    public IReadOnlyList<ActiveOrderLine> Lines => lines;

    private readonly List<ActiveOrderLine> lines;
    private float offerTimeRemaining;
    private float executionTimeRemaining;

    public float TimeRemaining => IsAccepted ? executionTimeRemaining : offerTimeRemaining;

    public bool IsCompleted
    {
        get
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].IsCompleted)
                    return false;
            }

            return lines.Count > 0;
        }
    }

    public bool IsExpired => TimeRemaining <= 0f;

    public int RequestedAmountTotal
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < lines.Count; i++)
                sum += lines[i].RequestedAmount;
            return sum;
        }
    }

    public int DeliveredAmountTotal
    {
        get
        {
            int sum = 0;
            for (int i = 0; i < lines.Count; i++)
                sum += lines[i].DeliveredAmount;
            return sum;
        }
    }

    public float CurrentMultiplier
    {
        get
        {
            if (!IsAccepted)
                return InitialMultiplier;

            if (DecayDurationSeconds <= 0f)
                return 1f;

            float elapsed = TotalExecutionLifetimeSeconds - Mathf.Max(0f, executionTimeRemaining);
            float effective = Mathf.Min(DecayDurationSeconds, elapsed);
            int totalSteps = Mathf.Max(1, Mathf.CeilToInt(DecayDurationSeconds / Mathf.Max(1f, MultiplierStepSeconds)));
            int currentStep = Mathf.Clamp(Mathf.FloorToInt(effective / Mathf.Max(1f, MultiplierStepSeconds)), 0, totalSteps);
            float t = currentStep / (float)totalSteps;
            return Mathf.Lerp(InitialMultiplier, 1f, t);
        }
    }

    public float TimeToNextMultiplierDrop
    {
        get
        {
            if (!IsAccepted || CurrentMultiplier <= 1f || DecayDurationSeconds <= 0f)
                return 0f;

            float elapsed = TotalExecutionLifetimeSeconds - Mathf.Max(0f, executionTimeRemaining);
            float step = Mathf.Max(1f, MultiplierStepSeconds);
            float nextStepAt = (Mathf.Floor(elapsed / step) + 1f) * step;
            return Mathf.Clamp(nextStepAt - elapsed, 0f, executionTimeRemaining);
        }
    }

    public ActiveOrder(
        BuyerProfileSO buyer,
        List<ActiveOrderLine> orderLines,
        int baseRewardCoins,
        float initialMultiplier,
        float decayDurationSeconds,
        float oneXHoldDurationSeconds,
        float multiplierStepSeconds,
        float offerLifetimeSeconds)
    {
        Buyer = buyer;
        lines = orderLines ?? new List<ActiveOrderLine>();
        BaseRewardCoins = Mathf.Max(1, baseRewardCoins);
        InitialMultiplier = Mathf.Max(1f, initialMultiplier);
        DecayDurationSeconds = Mathf.Max(0f, decayDurationSeconds);
        OneXHoldDurationSeconds = Mathf.Max(0f, oneXHoldDurationSeconds);
        MultiplierStepSeconds = Mathf.Max(1f, multiplierStepSeconds);
        OfferLifetimeSeconds = Mathf.Max(1f, offerLifetimeSeconds);
        TotalExecutionLifetimeSeconds = DecayDurationSeconds + OneXHoldDurationSeconds;
        offerTimeRemaining = OfferLifetimeSeconds;
        executionTimeRemaining = TotalExecutionLifetimeSeconds;
        IsAccepted = false;
    }

    public void Accept()
    {
        IsAccepted = true;
        executionTimeRemaining = TotalExecutionLifetimeSeconds;
    }

    public void Tick(float deltaTime)
    {
        if (IsAccepted)
            executionTimeRemaining = Mathf.Max(0f, executionTimeRemaining - Mathf.Max(0f, deltaTime));
        else
            offerTimeRemaining = Mathf.Max(0f, offerTimeRemaining - Mathf.Max(0f, deltaTime));
    }

    public int CalculateReward()
    {
        return Mathf.Max(1, Mathf.RoundToInt(BaseRewardCoins * CurrentMultiplier));
    }

    public int DeliverFromInventory(InventoryManager inventory)
    {
        if (inventory == null)
            return 0;

        int deliveredTotal = 0;

        for (int i = 0; i < lines.Count; i++)
            deliveredTotal += DeliverLineFromInventory(i, inventory);

        return deliveredTotal;
    }

    public int DeliverLineFromInventory(int lineIndex, InventoryManager inventory)
    {
        if (inventory == null || lineIndex < 0 || lineIndex >= lines.Count)
            return 0;

        ActiveOrderLine line = lines[lineIndex];
        if (line == null || line.Item == null || line.IsCompleted)
            return 0;

        int available = inventory.CountItem(line.Item);
        int toDeliver = Mathf.Min(available, line.RemainingAmount);
        if (toDeliver <= 0)
            return 0;

        int removed = inventory.RemoveItem(line.Item, toDeliver);
        line.DeliveredAmount += removed;
        return removed;
    }
}
