using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Grid grid;
    [SerializeField] private MouseRaycaster mouseRaycaster;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Preview")]
    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private GameObject cellIndicator;
    [SerializeField] private GameObject mouseIndicator;

    public static PlacementSystem Instance { get; private set; }

    private GridOccupancyData gridOccupancyData;
    private PlaceableItemData selectedPlaceableItem;
    private Renderer previewRenderer;
    private bool toolCursorMode;

    private readonly List<GameObject> placedGameObjects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gridOccupancyData = new GridOccupancyData();

        if (cellIndicator != null)
        {
            previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();
        }
    }

    private void Start()
    {
        StopPlacement();
    }

    private void Update()
    {
        Item currentItem = inventoryManager != null ? inventoryManager.GetSelectedItem() : null;
        SyncCursorModeWithSelectedItem(currentItem);

        if (selectedPlaceableItem == null && !toolCursorMode)
            return;

        Vector3 mousePosition = mouseRaycaster.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        if (previewRenderer != null)
        {
            if (selectedPlaceableItem != null)
            {
                bool placementValidity = CheckPlacementValidity(gridPosition);
                previewRenderer.material.color = placementValidity ? Color.white : Color.red;
            }
            else
            {
                // Tools still show cursor, but without occupancy validation.
                previewRenderer.material.color = Color.white;
            }
        }

        if (mouseIndicator != null)
            mouseIndicator.transform.position = mousePosition;

        if (cellIndicator != null)
            cellIndicator.transform.position = grid.GetCellCenterWorld(gridPosition);
    }

    private void HandleSelectedItem(Item item)
    {
        if (item == null)
        {
            StopPlacement();
            return;
        }

        if (item is ToolItem)
        {
            StartToolCursorMode();
            return;
        }

        if (item.IsPlaceable)
            StartPlacement(item.PlaceableData);
        else
            StopPlacement();
    }

    private void SyncCursorModeWithSelectedItem(Item item)
    {
        if (item == null)
        {
            if (selectedPlaceableItem != null || toolCursorMode)
                StopPlacement();
            return;
        }

        if (item is ToolItem)
        {
            if (!toolCursorMode || selectedPlaceableItem != null)
                StartToolCursorMode();
            return;
        }

        if (item.IsPlaceable)
        {
            if (toolCursorMode || selectedPlaceableItem != item.PlaceableData)
                StartPlacement(item.PlaceableData);
            return;
        }

        if (selectedPlaceableItem != null || toolCursorMode)
            StopPlacement();
    }

    public void StartPlacement(PlaceableItemData placeableItemData)
    {
        StopPlacement();

        if (placeableItemData == null)
        {
            Debug.LogError("Placeable data is null.");
            return;
        }

        selectedPlaceableItem = placeableItemData;
        ShowPlacementVisuals(true);

        mouseRaycaster.OnClicked += PlaceObject;
        mouseRaycaster.OnExit += StopPlacement;
    }

    private void StartToolCursorMode()
    {
        StopPlacement();
        toolCursorMode = true;
        ShowToolCursorVisuals(true);
    }

    public void StopPlacement()
    {
        selectedPlaceableItem = null;
        toolCursorMode = false;

        ShowPlacementVisuals(false);
        ShowToolCursorVisuals(false);

        if (mouseRaycaster != null)
        {
            mouseRaycaster.OnClicked -= PlaceObject;
            mouseRaycaster.OnExit -= StopPlacement;
        }
    }

    private void ShowPlacementVisuals(bool value)
    {
        if (gridVisualization != null)
            gridVisualization.SetActive(value);

        if (cellIndicator != null)
            cellIndicator.SetActive(value);

        if (mouseIndicator != null)
            mouseIndicator.SetActive(value);
    }

    private void ShowToolCursorVisuals(bool value)
    {
        if (gridVisualization != null)
            gridVisualization.SetActive(false);

        if (cellIndicator != null)
            cellIndicator.SetActive(value);

        if (mouseIndicator != null)
            mouseIndicator.SetActive(value);
    }

    private void PlaceObject()
    {
        if (selectedPlaceableItem == null)
            return;

        if (mouseRaycaster.IsPointerOverUI())
            return;

        Vector3 mousePosition = mouseRaycaster.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition);
        if (!placementValidity)
        {
            Debug.Log("Cannot place object here: cells are occupied.");
            return;
        }

        Item selectedItem = inventoryManager.GetSelectedItem();
        if (selectedItem == null)
        {
            StopPlacement();
            return;
        }

        Item consumedItem = inventoryManager.TakeSelectedItem(1);
        if (consumedItem == null)
            return;

        GameObject newObject = Instantiate(selectedPlaceableItem.Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);

        placedGameObjects.Add(newObject);

        gridOccupancyData.AddObjectAt(gridPosition, selectedPlaceableItem.Size, selectedItem.ID, placedGameObjects.Count - 1);
        if (selectedPlaceableItem is PlantPlaceableItemData plantData)
        {
            PlacedPlantBehaviour plantBehaviour = newObject.GetComponent<PlacedPlantBehaviour>();
            if (plantBehaviour == null)
            {
                plantBehaviour = newObject.AddComponent<PlacedPlantBehaviour>();
            }

            plantBehaviour.Init(plantData);
        }
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition)
    {
        bool occupancyValid = gridOccupancyData.CanPlaceObjectAt(gridPosition, selectedPlaceableItem.Size);
        if (!occupancyValid) return false;

        // Проверка типа плитки для растений
        if (selectedPlaceableItem is PlantPlaceableItemData)
        {
            if (GridTileSystem.Instance != null)
            {
                // Проверяем все клетки, которые будет занимать растение
                // (Для простоты обычно растения 1x1, но поддержим размер)
                for (int x = 0; x < selectedPlaceableItem.Size.x; x++)
                {
                    for (int y = 0; y < selectedPlaceableItem.Size.y; y++)
                    {
                        Vector3Int pos = gridPosition + new Vector3Int(x, 0, y);
                        if (GridTileSystem.Instance.GetTileType(pos) != TileType.Soil)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    public PlacedPlantBehaviour GetPlantAtGridPosition(Vector3Int gridPosition)
    {
        foreach (GameObject obj in placedGameObjects)
        {
            if (obj == null) continue;

            Vector3Int objectGridPosition = grid.WorldToCell(obj.transform.position);
            if (objectGridPosition == gridPosition)
            {
                PlacedPlantBehaviour plant = obj.GetComponent<PlacedPlantBehaviour>();
                if (plant != null)
                    return plant;
            }
        }

        return null;
    }

    public Grid GetGrid()
    {
        return grid;
    }

    public void RemovePlacedObject(GameObject obj)
    {
        if (placedGameObjects.Contains(obj))
        {
            Vector3Int objectGridPosition = grid.WorldToCell(obj.transform.position);
            gridOccupancyData.RemoveObjectAt(objectGridPosition);
            placedGameObjects.Remove(obj);
        }
    }
}
