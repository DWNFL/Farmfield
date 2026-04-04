using UnityEngine;

public abstract class PlaceableObject : MonoBehaviour
{
    public abstract Vector2Int Size { get; }

    public virtual void OnPlaced(Vector3Int gridPosition)
    {
    }
}