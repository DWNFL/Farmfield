using System;
using UnityEngine;

[Serializable]
public struct ItemPriceOverride
{
    [Tooltip("Предмет, для которого переопределяется цена")]
    public SellableItem TargetItem;

    [Tooltip("Множитель цены (1.5 = на 50% дороже базовой)")]
    public float PriceMultiplier;

    [Tooltip("Будет ли покупатель вообще брать этот товар")]
    public bool WillBuy;
}
