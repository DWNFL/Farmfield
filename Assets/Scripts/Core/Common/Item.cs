using UnityEngine;

public abstract class Item : ScriptableObject
{
    public int ID;
    public string ItemName;
    public Sprite Icon;
    public bool Stackable = true;
    public int MaxStack = 99;

    [Header("Placement")]
    public PlaceableItemData PlaceableData;

    public bool IsPlaceable => PlaceableData != null;


}
