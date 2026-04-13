using UnityEngine;

public abstract class ToolAction : ScriptableObject
{
    /// <summary>
    /// Базовый метод использования инструмента.
    /// Сигнатура метода может быть расширена (например, на Vector3Int gridPosition или GameObject target)
    /// по мере интеграции с системой взаимодействия.
    /// </summary>
    /// <param name="worldPosition">Позиция в мире, где был применен инструмент.</param>
    /// <returns>Возвращает true, если действие было успешно выполнено.</returns>
    public abstract bool Execute(Vector3 worldPosition);
}
