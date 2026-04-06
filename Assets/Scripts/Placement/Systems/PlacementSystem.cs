using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Grid grid;
    [SerializeField] private MouseRaycaster mouseRaycaster;
    [SerializeField] private ObjectDatabaseSO database;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Preview")]
    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private GameObject cellIndicator;
    [SerializeField] private GameObject mouseIndicator;


    private GridData gridData;
    private ObjectData selectedObjectData;
    private Renderer previewRenderer;
    private ItemData lastSelectedItem;

    private List<GameObject> placedGameObjects = new();

    private void Awake()
    {
        gridData = new GridData();
    }

    private void Start()
    {
        StopPlacement();
        gridData = new GridData();
        previewRenderer  = cellIndicator.GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        ItemData currentItem = inventoryManager.GetSelectedItem();

        if (currentItem != lastSelectedItem)
        {
            lastSelectedItem = currentItem;
            HandleSelectedItem(currentItem);
        }

        if (selectedObjectData == null)
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

    private void HandleSelectedItem(ItemData item)
    {
        if (item == null)
        {
            StopPlacement();
            return;
        }
    
        if (item.isPlaceable)
            StartPlacement(item.placementID);
        else
            StopPlacement();
    }

    public void StartPlacement(int placementID)
    {
        StopPlacement();

        if (!database.TryGetObjectById(placementID, out selectedObjectData))
        {
            Debug.LogError($"Object with ID {placementID} was not found in database.");
            return;
        }

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
        selectedObjectData = null;

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
        if (selectedObjectData == null)
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

        GameObject newObject = Instantiate(selectedObjectData.Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);

        placedGameObjects.Add(newObject);

        gridData.AddObjectAt(gridPosition, selectedObjectData.Size, selectedObjectData.ID, placedGameObjects.Count - 1);

        PlaceableObject placeable = newObject.GetComponent<PlaceableObject>();
        if (placeable != null)
        {
            placeable.OnPlaced(gridPosition);
        }

    }

    private bool CheckPlacementValidity(Vector3Int gridPosition)
    {
        return gridData.CanPlaceObjectAt(gridPosition, selectedObjectData.Size);
    }

}