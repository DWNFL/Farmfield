using System;
using System.Collections.Generic;
using UnityEngine;

public class GridTileSystem : MonoBehaviour
{
    public static GridTileSystem Instance { get; private set; }

    private Dictionary<Vector3Int, TileType> tiles = new();

    public event Action<Vector3Int, TileType> OnTileTypeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetTileType(Vector3Int gridPosition, TileType type)
    {
        tiles[gridPosition] = type;
        OnTileTypeChanged?.Invoke(gridPosition, type);
    }

    public TileType GetTileType(Vector3Int gridPosition)
    {
        if (tiles.TryGetValue(gridPosition, out TileType type))
        {
            return type;
        }
        return TileType.Ground; // Default
    }
}
