using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Farming/Station Config")]
public class StationConfig : ScriptableObject
{
    [Serializable]
    public class LevelData
    {
        [Header("Уровень")]
        public int UpgradeCost;              // Цена улучшения ДО этого уровня (для 1-го = 0)
        public int QueueSlots = 1;           // Сколько заказов можно поставить в очередь

        [Header("Скорость")]
        [Range(0.1f, 1f)]
        public float TimeMultiplier = 1f;    // 1.0 = норма, 0.8 = на 20% быстрее

        [Header("Рецепты этого уровня")]
        public StationRecipeConfig[] Recipes; // Рецепты, ОТКРЫВАЕМЫЕ на этом уровне
    }

    public string StationName;               // "Мельница", "Печь"
    public Sprite StationIcon;               // Иконка станка для UI (опционально)
    public LevelData[] Levels;               // [Level1, Level2, Level3]
}
