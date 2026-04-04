using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Grid grid;
    [SerializeField] private MouseRaycaster mouseRaycaster;
    [SerializeField] private ObjectDatabaseSO objectDatabase;

    [Header("Preview")]
    [SerializeField] private GameObject gridVisualization;
    [SerializeField] private GameObject cellIndicator;
    [SerializeField] private GameObject mouseIndicator;

    private GridData gridData;
    private ObjectData selectedObjectData;

    private void Awake()
    {
        gridData = new GridData();
    }

    private void Start()
    {
        StopPlacement();
    }

    private void Update()
    {
        if (selectedObjectData == null)
            return;

        Vector3 mousePosition = mouseRaycaster.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        if (mouseIndicator != null)
            mouseIndicator.transform.position = mousePosition;

        if (cellIndicator != null)
            cellIndicator.transform.position = grid.GetCellCenterWorld(gridPosition);
    }

    public void StartPlacement(int placementID)
    {
        StopPlacement();

        if (!objectDatabase.TryGetObjectById(placementID, out selectedObjectData))
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

        if (!gridData.CanPlaceObjectAt(gridPosition, selectedObjectData.Size))
        {
            Debug.Log("Cannot place object here: cells are occupied.");
            return;
        }

        GameObject placedObject = Instantiate(selectedObjectData.Prefab);
        placedObject.transform.position = CalculateWorldPosition(gridPosition, selectedObjectData.Size);

        PlaceableObject placeable = placedObject.GetComponent<PlaceableObject>();
        if (placeable != null)
        {
            placeable.OnPlaced(gridPosition);
        }

        gridData.AddObjectAt(gridPosition, selectedObjectData.Size, selectedObjectData.ID, placedObject);
    }

    private Vector3 CalculateWorldPosition(Vector3Int gridPosition, Vector2Int size)
    {
        Vector3 basePosition = grid.GetCellCenterWorld(gridPosition);

        if (size == Vector2Int.one)
            return basePosition;

        Vector3 offset = new Vector3((size.x - 1) * grid.cellSize.x * 0.5f, (size.y - 1) * grid.cellSize.y * 0.5f, 0f);
        return basePosition + offset;
    }
}