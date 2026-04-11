using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Farming/Plant Growth Config")]
public class PlantGrowthConfig : ScriptableObject
{
    [Serializable]
    public struct GrowthStage
    {
        [Tooltip("Время в секундах, необходимое для перехода на следующую стадию")]
        public float DurationSeconds;
        
        [Tooltip("Префаб или визуал для этой стадии. Если пусто, остается предыдущий.")]
        public GameObject VisualPrefab; 
    }

    [Serializable]
    public struct HarvestRule
    {
        public Item DropItem;
        public int MinAmount;
        public int MaxAmount;
    }

    [Header("Growth Stages")]
    [Tooltip("Основные стадии роста самого куста (например, проросток -> средний -> большой)")]
    public GrowthStage[] GrowthStages;
    
    [Header("Fruit Stages (Optional)")]
    [Tooltip("Стадии роста плодов. Используется только если куст многоразовый (например, помидоры).")]
    public GrowthStage[] FruitStages;

    [Header("Harvest Rules")]
    public HarvestRule HarvestDrop;

    [Header("Regrow Rules")]
    public bool IsRegrowable;
    [Tooltip("Сколько раз можно собрать урожай, прежде чем куст погибнет")]
    public int MaxHarvestsPerPlant = 1;
}
