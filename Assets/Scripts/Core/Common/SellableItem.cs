using UnityEngine;

[CreateAssetMenu(menuName = "Items/Sellable Item")]
public class SellableItem : Item
{
    [Header("Trading")]
    public int Price;
}
