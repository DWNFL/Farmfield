using UnityEngine;

[CreateAssetMenu(menuName = "Items/Tool Item")]
public class ToolItem : Item
{
    [Header("Tool Settings")]
    [Tooltip("Поведение инструмента, которое будет выполняться при применении")]
    public ToolAction ActionBehavior;

    // В будущем сюда можно добавить прочность (Durability), 
    // Тир инструмента (Tier) или затрачиваемую энергию.

    private void OnEnable()
    {
        // Убедитесь, что инструмент не является "placeable"
        PlaceableData = null;
    }
}
