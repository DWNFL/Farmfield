using System;
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


    private GridOccupancyData gridOccupancyData;
    private PlaceableItemData selectedPlaceableItem;
    private Renderer previewRenderer;
    private Item lastSelectedItem;

    private List<GameObject> placedGameObjects = new();

    private void Awake()
    {
        gridOccupancyData = new GridOccupancyData();
        previewRenderer  = cellIndicator.GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        StopPlacement();
    }

    private void Update()
    {
        Item currentItem = inventoryManager.GetSelectedItem();

        if (currentItem != lastSelectedItem)
        {
            lastSelectedItem = currentItem;
            HandleSelectedItem(currentItem);
        }

        if (selectedPlaceableItem == null)
            return;

        Vector3 mousePosition = mouseRaycaster.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition);
        previewRenderer.material.color = placementValidity ? Color.white : Color.red;

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
    
        if (item.IsPlaceable)
            StartPlacement(item.PlaceableData);
        else
            StopPlacement();
    }

    public void StartPlacement(PlaceableItemData placeableItemData)
    {
        StopPlacement();

        if (placeableItemData == null)
        {
            Debug.LogError($"Placeable data is null.");
            return;
        }

        selectedPlaceableItem = placeableItemData;

        if (gridVisualization != null)
            gridVisualization.SetActive(true);

        if (cellIndicator != null)
            cellIndicator.SetActive(true);

        if (mouseIndicator != null)
            mouseIndicator.SetActive(true);

        mouseRaycaster.OnClicked += PlaceObject;
        mouseRaycaster.OnExit += StopPlacement;
    }

    public void StopPlacement()
    {
        selectedPlaceableItem = null;

        if (gridVisualization != null)
            gridVisualization.SetActive(false);

        if (cellIndicator != null)
            cellIndicator.SetActive(false);

        if (mouseIndicator != null)
            mouseIndicator.SetActive(false);

        if (mouseRaycaster != null)
        {
            mouseRaycaster.OnClicked -= PlaceObject;
            mouseRaycaster.OnExit -= StopPlacement;
        }
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
        if (selectedItem == null || selectedItem != lastSelectedItem)
        {
            HandleSelectedItem(selectedItem);
            return;
        }

        Item consumedItem = inventoryManager.TakeSelectedItem(1);
        if (consumedItem == null)
        {
            return;
        }

        GameObject newObject = Instantiate(selectedPlaceableItem.Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);

        placedGameObjects.Add(newObject);

        gridOccupancyData.AddObjectAt(gridPosition, selectedPlaceableItem.Size, lastSelectedItem.ID, placedGameObjects.Count - 1);
        if (selectedPlaceableItem is PlantPlaceableItemData plantData)
        {
            PlacedPlantBehaviour plantBehaviour = newObject.GetComponent<PlacedPlantBehaviour>();
            if (plantBehaviour == null)
            {
                plantBehaviour = newObject.AddComponent<PlacedPlantBehaviour>();
            }
            plantBehaviour.Init(plantData);
        }

        // КОГДА СДЕЛАЕИ ИНТЕРФЕЙС IPLACEABLE
        // IPlaceable placeable = newObject.GetComponent<IPlaceable>();
        // if (placeable != null)
        // {
        //    placeable.OnPlaced(gridPosition);
        // }

    }

    private bool CheckPlacementValidity(Vector3Int gridPosition)
    {
        return gridOccupancyData.CanPlaceObjectAt(gridPosition, selectedPlaceableItem.Size);
    }

}
