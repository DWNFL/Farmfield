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

    private void Update()
    {
        Vector3 mousePosition = mouseRaycaster.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.GetCellCenterWorld(gridPosition);
    }
}
