using UnityEngine;

[CreateAssetMenu(menuName = "Tool Actions/Digging Action")]
public class DiggingAction : ToolAction
{
    public override bool Execute(Vector3 worldPosition)
    {
        PlacementSystem placementSystem = PlacementSystem.Instance;
        if (placementSystem == null)
        {
            Debug.LogError("PlacementSystem не найдена!");
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
        Debug.Log($"Клик на grid позицию: {gridPosition}");

        // Ищем растение на этой клетке
        PlacedPlantBehaviour plant = placementSystem.GetPlantAtGridPosition(gridPosition);

        if (plant != null)
        {
            Debug.Log($"Найдено растение на клетке {gridPosition}, выкапываю...");
            plant.DigUp();
            return true;
        }

        Debug.Log($"Нет растения на клетке {gridPosition}");
        return false;
    }
}