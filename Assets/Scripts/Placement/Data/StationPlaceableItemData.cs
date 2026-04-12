using UnityEngine;

[CreateAssetMenu(menuName = "Placement/Station Placeable Data")]
public class StationPlaceableItemData : PlaceableItemData
{
    [field: SerializeField, Header("Station")]
    public StationConfig Config { get; private set; }
}
