using System;
using System.Collections.Generic;

[Serializable]
public class QueuedRecipeState
{
    public StationRecipeConfig Recipe;   // Какой рецепт
    public float RemainingTime;          // Сколько осталось (в секундах)
    public bool IsReady;                 // Готов ли к сбору
}

[Serializable]
public class StationRuntimeState
{
    public int CurrentLevelIndex = 0;    // Индекс текущего уровня (0 = первый)
    public List<QueuedRecipeState> Queue = new List<QueuedRecipeState>();
}
