using UnityEngine;

public class ToolUsageSystem : MonoBehaviour
{
    [SerializeField] private MouseRaycaster mouseRaycaster;
    [SerializeField] private InventoryManager inventoryManager;

    private void Start()
    {
        if (mouseRaycaster != null)
        {
            mouseRaycaster.OnClicked += TryUseTool;
        }
    }

    private void OnDestroy()
    {
        if (mouseRaycaster != null)
        {
            mouseRaycaster.OnClicked -= TryUseTool;
        }
    }

    private void TryUseTool()
    {
        // Проверяем, не над ли UI
        if (mouseRaycaster.IsPointerOverUI())
            return;

        // Получаем выбранный предмет
        Item selectedItem = inventoryManager.GetSelectedItem();
        if (selectedItem == null)
            return;

        // Проверяем, это ли инструмент
        ToolItem toolItem = selectedItem as ToolItem;
        if (toolItem == null || toolItem.ActionBehavior == null)
            return;

        // Получаем позицию клика
        Vector3 clickPosition = mouseRaycaster.GetSelectedMapPosition();

        // Вызываем действие инструмента
        bool success = toolItem.ActionBehavior.Execute(clickPosition);

        if (success)
        {
            Debug.Log($"Инструмент использован успешно: {toolItem.name}");
        }
        else
        {
            Debug.Log($"Инструмент не мог быть использован: {toolItem.name}");
        }
    }
}
