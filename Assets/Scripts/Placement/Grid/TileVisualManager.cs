using System.Collections.Generic;
using UnityEngine;

public class TileVisualManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject soilPrefab; // Префаб коричневой земли
    [SerializeField] private Vector3 offset = new Vector3(0, 0.05f, 0); // Чуть увеличил отступ

    private Dictionary<Vector3Int, GameObject> visualTiles = new();
    private Grid grid;
    private bool isSubscribed = false;

    private void Start()
    {
        grid = GetComponentInParent<Grid>();
        if (grid == null) grid = FindFirstObjectByType<Grid>();
        
        TrySubscribe();
    }

    private void Update()
    {
        // Если при старте система не была готова, пытаемся подключиться в процессе
        if (!isSubscribed)
        {
            TrySubscribe();
        }
    }

    private void TrySubscribe()
    {
        if (GridTileSystem.Instance != null)
        {
            GridTileSystem.Instance.OnTileTypeChanged += HandleTileTypeChanged;
            isSubscribed = true;
            Debug.Log("[TileVisualManager] Успешно подключен к GridTileSystem");
        }
    }

    private void OnDestroy()
    {
        if (isSubscribed && GridTileSystem.Instance != null)
        {
            GridTileSystem.Instance.OnTileTypeChanged -= HandleTileTypeChanged;
        }
    }

    private void HandleTileTypeChanged(Vector3Int position, TileType newType)
    {
        Debug.Log($"[TileVisualManager] Тип клетки {position} изменился на {newType}");

        if (newType == TileType.Soil)
        {
            if (!visualTiles.ContainsKey(position))
            {
                CreateSoilVisual(position);
            }
        }
        else
        {
            if (visualTiles.TryGetValue(position, out GameObject visual))
            {
                Destroy(visual);
                visualTiles.Remove(position);
            }
        }
    }

    private void CreateSoilVisual(Vector3Int position)
    {
        if (soilPrefab == null)
        {
            Debug.LogWarning("[TileVisualManager] Не назначен Soil Prefab в инспекторе!");
            return;
        }
        
        if (grid == null) return;

        Vector3 worldPos = grid.CellToWorld(position) + offset;
        GameObject visual = Instantiate(soilPrefab, worldPos, Quaternion.identity, transform);
        visualTiles[position] = visual;
        
        Debug.Log($"[TileVisualManager] Создан визуал почвы в {worldPos}");
    }
}
