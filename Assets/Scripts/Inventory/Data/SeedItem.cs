using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Seed Item")]
public class SeedItem : SellableItem
{
    // Пока дополнительных полей не нужно, 
    // данные о том, что именно сажать, хранятся в поле PlaceableData, 
    // унаследованном от Item.
}
