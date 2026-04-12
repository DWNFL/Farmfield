using UnityEngine;
using System.Collections.Generic;

public class PlacedStationBehaviour : MonoBehaviour
{
    private StationPlaceableItemData stationData;
    private StationRuntimeState state;

    /// <summary>
    /// Конфиг станка (для UI и внешнего доступа).
    /// </summary>
    public StationConfig Config => stationData != null ? stationData.Config : null;

    /// <summary>
    /// Текущее рантайм-состояние (для UI и сохранения).
    /// </summary>
    public StationRuntimeState State => state;

    // ─────────────── Инициализация ───────────────

    public void Init(StationPlaceableItemData data, StationRuntimeState existingState = null)
    {
        stationData = data;

        if (existingState != null)
        {
            state = existingState;
        }
        else
        {
            state = new StationRuntimeState();
            state.CurrentLevelIndex = 0;
            state.Queue = new List<QueuedRecipeState>();
        }

        Debug.Log($"<color=cyan>Станок «{Config.StationName}» инициализирован (ур. {state.CurrentLevelIndex + 1})</color>");
    }

    // ─────────────── Тик очереди ───────────────

    private void Update()
    {
        if (stationData == null || Config == null) return;
        if (state.Queue.Count == 0) return;

        // Тикаем первый НЕготовый элемент очереди
        for (int i = 0; i < state.Queue.Count; i++)
        {
            QueuedRecipeState entry = state.Queue[i];
            if (entry.IsReady) continue;

            entry.RemainingTime -= Time.deltaTime;

            if (entry.RemainingTime <= 0f)
            {
                entry.RemainingTime = 0f;
                entry.IsReady = true;
                Debug.Log($"<color=yellow>Готово: {entry.Recipe.RecipeName}!</color>");
            }

            break; // Тикаем только один (первый незавершённый)
        }
    }

    // ─────────────── Рецепты ───────────────

    /// <summary>
    /// Возвращает все рецепты, доступные на текущем уровне
    /// (объединяет рецепты всех уровней от 0 до CurrentLevelIndex).
    /// </summary>
    public List<StationRecipeConfig> GetAvailableRecipes()
    {
        var result = new List<StationRecipeConfig>();

        if (Config == null || Config.Levels == null) return result;

        for (int lvl = 0; lvl <= state.CurrentLevelIndex && lvl < Config.Levels.Length; lvl++)
        {
            var levelData = Config.Levels[lvl];
            if (levelData.Recipes != null)
            {
                result.AddRange(levelData.Recipes);
            }
        }

        return result;
    }

    /// <summary>
    /// Проверяет, хватает ли ресурсов в инвентаре для данного рецепта.
    /// </summary>
    public bool CanAffordRecipe(StationRecipeConfig recipe)
    {
        if (recipe == null || recipe.Ingredients == null) return false;
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance == null!");
            return false;
        }

        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient.Item == null)
            {
                Debug.LogWarning($"В рецепте «{recipe.RecipeName}» один из ингредиентов не назначен (Item = null)!");
                return false;
            }
            if (!InventoryManager.Instance.HasItems(ingredient.Item, ingredient.Amount))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Текущее кол-во свободных слотов в очереди.
    /// </summary>
    public int GetFreeQueueSlots()
    {
        if (Config == null || Config.Levels == null) return 0;

        int maxSlots = Config.Levels[state.CurrentLevelIndex].QueueSlots;
        return Mathf.Max(0, maxSlots - state.Queue.Count);
    }

    /// <summary>
    /// ТЕСТ: берёт первый доступный рецепт и пытается добавить в очередь.
    /// Используй ПКМ → "TEST: Добавить первый рецепт" в Inspector.
    /// </summary>
    [ContextMenu("TEST: Добавить первый рецепт")]
    private void TestAddFirstRecipe()
    {
        var recipes = GetAvailableRecipes();
        if (recipes.Count == 0)
        {
            Debug.LogWarning("Нет доступных рецептов на текущем уровне!");
            return;
        }

        Debug.Log($"Пробуем рецепт: {recipes[0].RecipeName}");
        TryAddToQueue(recipes[0]);
    }

    /// <summary>
    /// Пытается добавить рецепт в очередь.
    /// Проверяет свободные слоты → проверяет ресурсы → списывает → добавляет.
    /// </summary>
    public bool TryAddToQueue(StationRecipeConfig recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("Рецепт не указан.");
            return false;
        }

        // Проверка слотов
        if (GetFreeQueueSlots() <= 0)
        {
            Debug.Log("Очередь заполнена!");
            return false;
        }

        // Проверка ресурсов
        if (!CanAffordRecipe(recipe))
        {
            Debug.Log("Не хватает ингредиентов!");
            return false;
        }

        // Списание ингредиентов
        foreach (var ingredient in recipe.Ingredients)
        {
            InventoryManager.Instance.RemoveItems(ingredient.Item, ingredient.Amount);
        }

        // Рассчитываем время с учётом множителя уровня
        float timeMultiplier = Config.Levels[state.CurrentLevelIndex].TimeMultiplier;
        float adjustedTime = recipe.ProductionTimeSeconds * timeMultiplier;

        // Добавляем в очередь
        var entry = new QueuedRecipeState
        {
            Recipe = recipe,
            RemainingTime = adjustedTime,
            IsReady = false
        };
        state.Queue.Add(entry);

        Debug.Log($"<color=cyan>В очередь: {recipe.RecipeName} ({adjustedTime:F1} сек)</color>");
        return true;
    }

    // ─────────────── Сбор готового ───────────────

    /// <summary>
    /// Есть ли хоть один готовый предмет в очереди.
    /// </summary>
    public bool HasReadyItems()
    {
        foreach (var entry in state.Queue)
        {
            if (entry.IsReady) return true;
        }
        return false;
    }

    /// <summary>
    /// Забирает первый готовый предмет → выдаёт ResultItem в инвентарь → удаляет из очереди.
    /// </summary>
    [ContextMenu("Забрать готовое (Collect)")]
    public bool CollectReady()
    {
        for (int i = 0; i < state.Queue.Count; i++)
        {
            QueuedRecipeState entry = state.Queue[i];
            if (!entry.IsReady) continue;

            // Выдаём результат
            if (InventoryManager.Instance != null && entry.Recipe.ResultItem != null)
            {
                for (int j = 0; j < entry.Recipe.ResultAmount; j++)
                {
                    InventoryManager.Instance.AddItem(entry.Recipe.ResultItem);
                }
            }

            string itemName = entry.Recipe.ResultItem != null ? entry.Recipe.ResultItem.ItemName : "???";
            Debug.Log($"<color=green>ПОЛУЧЕНО: {entry.Recipe.ResultAmount}x {itemName}</color>");

            state.Queue.RemoveAt(i);
            return true;
        }

        Debug.Log("Нет готовых предметов.");
        return false;
    }

    // ─────────────── Улучшение ───────────────

    /// <summary>
    /// Можно ли улучшить станок (есть ли следующий уровень).
    /// </summary>
    public bool CanUpgrade()
    {
        if (Config == null || Config.Levels == null) return false;
        return state.CurrentLevelIndex < Config.Levels.Length - 1;
    }

    /// <summary>
    /// Стоимость следующего улучшения. Возвращает -1 если улучшение невозможно.
    /// </summary>
    public int GetUpgradeCost()
    {
        if (!CanUpgrade()) return -1;
        return Config.Levels[state.CurrentLevelIndex + 1].UpgradeCost;
    }

    /// <summary>
    /// Пытается улучшить станок.
    /// TODO: Списание валюты — пока только повышает уровень.
    /// </summary>
    [ContextMenu("Улучшить станок (Upgrade)")]
    public bool TryUpgrade()
    {
        if (!CanUpgrade())
        {
            Debug.Log("Станок уже максимального уровня!");
            return false;
        }

        int cost = GetUpgradeCost();
        // TODO: Проверить и списать валюту (монеты) у игрока
        // if (!PlayerWallet.Instance.HasCoins(cost)) return false;
        // PlayerWallet.Instance.SpendCoins(cost);

        state.CurrentLevelIndex++;
        Debug.Log($"<color=magenta>Станок «{Config.StationName}» улучшен до ур. {state.CurrentLevelIndex + 1}!</color>");

        return true;
    }

    // ─────────────── Утилиты для UI ───────────────

    /// <summary>
    /// Текущий уровень станка (1-based, для отображения).
    /// </summary>
    public int CurrentLevel => state.CurrentLevelIndex + 1;

    /// <summary>
    /// Максимальный уровень станка.
    /// </summary>
    public int MaxLevel => Config != null && Config.Levels != null ? Config.Levels.Length : 0;
}
