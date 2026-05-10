using System;
using UnityEngine;

[Serializable]
public class OrderRequestLine
{
    public SellableItem Item;
    [Min(1)] public int Amount = 10;

    public int GetAmount()
    {
        return Mathf.Max(1, Amount);
    }
}
