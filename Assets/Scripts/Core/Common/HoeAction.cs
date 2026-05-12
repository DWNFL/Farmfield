using UnityEngine;

[CreateAssetMenu(menuName = "Tool Actions/Hoe Action")]
public class HoeAction : ToolAction
{
    public override bool Execute(Vector3 worldPosition)
    {
        PlacementSystem placementSystem = PlacementSystem.Instance;
        if (placementSystem == null)
        {
            Debug.LogError("PlacementSystem не найдена!");
            return false;
        }

        GridTileSystem tileSystem = GridTileSystem.Instance;
        if (tileSystem == null)
        {
            Debug.LogError("GridTileSystem не найдена!");
            return false;
        }

        Grid grid = placementSystem.GetGrid();
        if (grid == null)
        {
            Debug.LogError("Grid не найдена!");
            return false;
        }

        // Преобразуем мировую позицию в grid позицию
        Vector3Int gridPosition = grid.WorldToCell(worldPosition);
        Debug.Log($"[HoeAction] Попытка использовать мотыгу в {gridPosition} (world: {worldPosition})");
        
        TileType currentType = tileSystem.GetTileType(gridPosition);
        Debug.Log($"[HoeAction] Текущий тип клетки: {currentType}");
        if (currentType == TileType.Ground)
        {
            Debug.Log($"Вспахиваю землю в {gridPosition}");
            tileSystem.SetTileType(gridPosition, TileType.Soil);
            return true;
        }

        Debug.Log($"Клетка {gridPosition} уже является почвой или не может быть вспахана.");
        return false;
    }
}
