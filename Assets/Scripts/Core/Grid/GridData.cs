using System.Collections.Generic;
using UnityEngine;

public class GridData
{
    private readonly Dictionary<Vector3Int, PlacementCellData> occupiedCells = new();

    public bool CanPlaceObjectAt(Vector3Int origin, Vector2Int size)
    {
        foreach (var position in CalculatePositions(origin, size))
        {
            if (occupiedCells.ContainsKey(position))
                return false;
        }

        return true;
    }

    public void AddObjectAt(Vector3Int origin, Vector2Int size, int objectID, GameObject placedObject)
    {
        foreach (var position in CalculatePositions(origin, size))
        {
            occupiedCells[position] = new PlacementCellData(objectID, origin, placedObject);
        }
    }

    public void RemoveObjectAt(Vector3Int origin, Vector2Int size)
    {
        foreach (var position in CalculatePositions(origin, size))
        {
            occupiedCells.Remove(position);
        }
    }

    public bool IsCellOccupied(Vector3Int position)
    {
        return occupiedCells.ContainsKey(position);
    }

    public bool TryGetCellData(Vector3Int position, out PlacementCellData cellData)
    {
        return occupiedCells.TryGetValue(position, out cellData);
    }

    private IEnumerable<Vector3Int> CalculatePositions(Vector3Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                yield return origin + new Vector3Int(x, y, 0);
            }
        }
    }
}

public readonly struct PlacementCellData
{
    public int ObjectID { get; }
    public Vector3Int OriginCell { get; }
    public GameObject PlacedObject { get; }

    public PlacementCellData(int objectID, Vector3Int originCell, GameObject placedObject)
    {
        ObjectID = objectID;
        OriginCell = originCell;
        PlacedObject = placedObject;
    }
}