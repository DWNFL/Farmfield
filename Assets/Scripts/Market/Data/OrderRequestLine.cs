using System;
using UnityEngine;

[Serializable]
public class OrderRequestLine
{
    public SellableItem Item;
    [Min(1)] public int MinAmount = 10;
    [Min(1)] public int MaxAmount = 30;

    public int RollAmount()
    {
        int min = Mathf.Max(1, MinAmount);
        int max = Mathf.Max(min, MaxAmount);
        return UnityEngine.Random.Range(min, max + 1);
    }
}
