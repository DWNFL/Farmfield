using UnityEngine;

public class PlacedPlantBehaviour : MonoBehaviour
{
    private PlantPlaceableItemData plantData;
    private PlantRuntimeState state;
    private GameObject currentVisual;

    public void Init(PlantPlaceableItemData data, PlantRuntimeState existingState = null)
    {
        plantData = data;
        
        if (existingState != null)
        {
            state = existingState;
        }
        else
        {
            state = new PlantRuntimeState();
            state.CurrentStageIndex = 0;
            state.TimeInCurrentStage = 0f;
            state.HarvestCount = 0;
            state.IsInFruitStage = false; 
        }

        UpdateVisual();
    }

    private void Update()
    {
        if (plantData == null || plantData.GrowthConfig == null) return;
        
        var config = plantData.GrowthConfig;
        
        // Остановка, если исчерпан лимит урожая (для кустов типа помидоров)
        if (config.IsRegrowable && state.HarvestCount >= config.MaxHarvestsPerPlant) return; 

        var stages = state.IsInFruitStage ? config.FruitStages : config.GrowthStages;
        if (stages == null || stages.Length == 0) return;

        // Проверка, дошли ли мы до последней стадии текущего этапа роста
        if (state.CurrentStageIndex >= stages.Length - 1)
        {
            // Если выросли и это многоразовый куст с плодами - переходим на рост плодов
            if (!state.IsInFruitStage && config.IsRegrowable && config.FruitStages != null && config.FruitStages.Length > 0)
            {
                state.IsInFruitStage = true;
                state.CurrentStageIndex = 0;
                state.TimeInCurrentStage = 0f;
                UpdateVisual();
            }
            return; // Достигли конца массива, просто ждем урожая
        }

        state.TimeInCurrentStage += Time.deltaTime;

        if (state.TimeInCurrentStage >= stages[state.CurrentStageIndex].DurationSeconds)
        {
            state.TimeInCurrentStage = 0f;
            state.CurrentStageIndex++;
            UpdateVisual();
        }
    }

    private void UpdateVisual()
    {
        var config = plantData.GrowthConfig;
        var stages = state.IsInFruitStage ? config.FruitStages : config.GrowthStages;
        
        if (stages == null || stages.Length == 0) return;

        int indexToCheck = Mathf.Min(state.CurrentStageIndex, stages.Length - 1);
        GameObject prefabToSpawn = stages[indexToCheck].VisualPrefab;

        if (prefabToSpawn != null)
        {
            if (currentVisual != null)
            {
                Destroy(currentVisual);
            }
            currentVisual = Instantiate(prefabToSpawn, transform);
            currentVisual.transform.localPosition = Vector3.zero;
            currentVisual.transform.localRotation = Quaternion.identity;
        }
    }

    public bool CanHarvest()
    {
        if (plantData == null || plantData.GrowthConfig == null) return false;
        var config = plantData.GrowthConfig;

        var stages = state.IsInFruitStage ? config.FruitStages : config.GrowthStages;
        if (stages == null || stages.Length == 0) return false;

        if (state.IsInFruitStage)
        {
            return state.CurrentStageIndex >= config.FruitStages.Length - 1;
        }
        else
        {
            // Нельзя собрать из обычных стадий, если есть стадии плодов
            if (config.IsRegrowable && config.FruitStages != null && config.FruitStages.Length > 0) 
                return false; 
            
            return state.CurrentStageIndex >= config.GrowthStages.Length - 1;
        }
    }

    [ContextMenu("Собрать урожай (Harvest)")]
    public void Harvest()
    {
        if (!CanHarvest())
        {
            Debug.Log("Растение еще не созрело!");
            return;
        }

        var config = plantData.GrowthConfig;
        int dropAmount = UnityEngine.Random.Range(config.HarvestDrop.MinAmount, config.HarvestDrop.MaxAmount + 1);
        
        string itemName = config.HarvestDrop.DropItem != null ? config.HarvestDrop.DropItem.name : "Неизвестный предмет";
        Debug.Log($"<color=green>ПОЛУЧЕН УРОЖАЙ: {dropAmount}x {itemName}!</color>");
        
        if (InventoryManager.Instance != null && config.HarvestDrop.DropItem != null)
        {
            for (int i = 0; i < dropAmount; i++)
            {
                InventoryManager.Instance.AddItem(config.HarvestDrop.DropItem);
            }
        }

        state.HarvestCount++;

        if (config.IsRegrowable)
        {
            if (state.HarvestCount < config.MaxHarvestsPerPlant)
            {
                // Откат к нулевой стадии плодов
                state.IsInFruitStage = true;
                state.CurrentStageIndex = 0;
                state.TimeInCurrentStage = 0f;
                UpdateVisual();
            }
            else
            {
                DigUp(); // Куст исчерпал ресурс
            }
        }
        else
        {
            DigUp(); // Одноразовое растение (картошка, морковка)
        }
    }

    [ContextMenu("Выкопать (Dig Up)")]
    public void DigUp()
    {
        // TODO: Освободить сетку от этого объекта в GridOccupancyData
        Debug.Log("Растение выкопано и удалено!");
        Destroy(gameObject);
    }
}
