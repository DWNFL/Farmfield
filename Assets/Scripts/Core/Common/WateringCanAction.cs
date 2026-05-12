using UnityEngine;

[CreateAssetMenu(menuName = "Tool Actions/Watering Can Action")]
public class WateringCanAction : ToolAction
{
    [Header("Settings")]
    public int MaxWater = 100;
    public int CurrentWater = 0;
    public int FillAmount = 10;
    public int WaterUsagePerPlant = 1;

    public override bool Execute(Vector3 worldPosition)
    {
        // 1. Попытка набрать воду из колодца (через Raycast, так как колодец — объект)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Well well = hit.collider.GetComponentInParent<Well>();
            if (well != null)
            {
                if (CurrentWater >= MaxWater)
                {
                    Debug.Log("Лейка уже полная!");
                    return false;
                }

                int space = MaxWater - CurrentWater;
                int toTake = Mathf.Min(FillAmount, space);
                int taken = well.TakeWater(toTake);
                
                CurrentWater += taken;
                Debug.Log($"Лейка наполнена: {CurrentWater}/{MaxWater}");
                return true;
            }
        }

        // 2. Попытка полить растение
        PlacementSystem placementSystem = PlacementSystem.Instance;
        if (placementSystem == null) return false;

        Grid grid = placementSystem.GetGrid();
        if (grid == null) return false;

        Vector3Int gridPosition = grid.WorldToCell(worldPosition);
        PlacedPlantBehaviour plant = placementSystem.GetPlantAtGridPosition(gridPosition);

        if (plant != null)
        {
            if (CurrentWater <= 0)
            {
                Debug.Log("В лейке нет воды!");
                return false;
            }

            if (plant.Water())
            {
                CurrentWater -= WaterUsagePerPlant;
                Debug.Log($"Растение полито. Осталось воды: {CurrentWater}/{MaxWater}");
                return true;
            }
        }

        return false;
    }
}
