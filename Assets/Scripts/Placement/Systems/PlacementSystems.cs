using Unity.VisualScripting;
using UnityEngine;

public class PlacementPreview : MonoBehaviour
{
    [SerializeField] 
    private GameObject mouseIndicator, cellIndicator;
    [SerializeField]
    private MouseRaycaster mouseRaycaster;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private ObjectDatabaseSO database;
    private int selectedObjectIndex = -1;

    [SerializeField]
    private GameObject gridVisualization;

    private void Start()
    {
        StopPlacement();
    }
    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex < 0)
        {
            Debug.LogError($"No ID found {ID}");
            return;
        }
        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);
        mouseRaycaster.OnClicked += PlaceObject;
        mouseRaycaster.OnExit += StopPlacement;
    }

    private void  PlaceObject()
    {
        if (mouseRaycaster.IsPointOverUI())
        {
        Vector3 mousePosition = mouseRaycaster.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
        newObject.transform.position = grid.GetCellCenterWorld(gridPosition);
    
            
        }
    }

    public void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);
        mouseRaycaster.OnClicked -= PlaceObject;
        mouseRaycaster.OnExit -= StopPlacement;
    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
            return;
            
        Vector3 mousePosition = mouseRaycaster.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.GetCellCenterWorld(gridPosition);
    }
}
