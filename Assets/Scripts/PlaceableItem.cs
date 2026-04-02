using UnityEngine;

public class PlaceableItem : MonoBehaviour
{
    public GridPlacement gridPlacement;

    void OnMouseDown()
    {
        // Проверяем, что не в режиме размещения
        if (gridPlacement != null && !Input.GetKey(KeyCode.LeftShift))
        {
            DeleteItem();
        }
    }

    void DeleteItem()
    {
        Destroy(gameObject);
        Debug.Log("Объект удален");
    }
}