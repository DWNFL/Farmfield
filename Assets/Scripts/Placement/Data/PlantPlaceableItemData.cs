using UnityEngine;

[CreateAssetMenu(menuName = "Placement/Plant Placeable Data")]
public class PlantPlaceableItemData : PlaceableItemData
{
    [field: SerializeField, Header("Farming")]
    public PlantGrowthConfig GrowthConfig { get; private set; }
}
