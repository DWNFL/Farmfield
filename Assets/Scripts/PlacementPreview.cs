using UnityEngine;

public class PlacementPreview : MonoBehaviour
{
    [SerializeField] 
    private GameObject mouseIndicator;
    [SerializeField]
    private MouseRaycaster mouseRaycaster;

    private void Update()
    {
        Vector3 mousePosition = mouseRaycaster.GetSelectedMapPosition();
        mouseIndicator.transform.position = mousePosition;
    } 
}
