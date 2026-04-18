using System;
using System.Collections.Generic;
using UnityEngine;

public class GridOccupancyData
{
    Dictionary<Vector3Int, OccupancyData> placedObjects = new();
    
    public void AddObjectAt(Vector3Int gridPosition,
                            Vector2Int objectSize,
                            int itemID,
                            int placedObjectIndex)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        OccupancyData data = new OccupancyData(positionToOccupy, itemID, placedObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
                throw new Exception($"Dictionary already contains this cell position {pos}");
            placedObjects[pos] = data;
        }
    }

    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> returnValues = new();
        for (int x = 0; x < objectSize.x; x++)
        {
            for (int y = 0; y < objectSize.y; y++)
            {
                returnValues.Add(gridPosition + new Vector3Int(x, 0, y));
            }
        }
        return returnValues;
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionsToOccupy = CalculatePositions(gridPosition, objectSize);
        foreach (var pos in positionsToOccupy)
        {
            if (placedObjects.ContainsKey(pos))
               return false;
        }
        return true;
    }

    public void RemoveObjectAt(Vector3Int gridPosition)
    {
        if (!placedObjects.TryGetValue(gridPosition, out OccupancyData data))
            return;

        foreach (var pos in data.occupiedPositions)
        {
            placedObjects.Remove(pos);
        }
    }
}

public struct OccupancyData
{
    public List<Vector3Int> occupiedPositions; 
    public int ItemID { get; private set; }
    public int PlacedObjectIndex { get; private set;}

    public OccupancyData(List<Vector3Int> occupiedPositions, int itemId, int placedObjectIndex)
    {
        this.occupiedPositions = occupiedPositions;
        ItemID = itemId;   
        PlacedObjectIndex = placedObjectIndex;
    }


}
