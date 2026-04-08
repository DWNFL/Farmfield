using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Sellable Item")]
public class SellableItem : Item
{
    [Header("Trading")]
    public int Price;
}
