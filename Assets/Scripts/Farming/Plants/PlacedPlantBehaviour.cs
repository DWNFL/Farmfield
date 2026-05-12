using UnityEngine;

public class PlacedPlantBehaviour : MonoBehaviour
{
    private PlantPlaceableItemData plantData;
    private PlantRuntimeState state;
    private GameObject currentVisual;
    private Vector3Int gridPosition;

    [Header("Watering")]
    [SerializeField] private float waterDuration = 120f; // 2 минуты
    private bool wasWateredLastFrame;

    public void Init(PlantPlaceableItemData data, PlantRuntimeState existingState = null)
    {
        plantData = data;

        if (existingState != null)
        {
            state = existingState;
        }
        else
        {
            state = new PlantRuntimeState
            {
                CurrentStageIndex = 0,
                TimeInCurrentStage = 0f,
                HarvestCount = 0,
                IsInFruitStage = false
            };
        }

        UpdateVisual();
        
        // Определяем позицию в сетке
        if (PlacementSystem.Instance != null)
        {
            gridPosition = PlacementSystem.Instance.GetGrid().WorldToCell(transform.position);
        }
        
        wasWateredLastFrame = state.WaterTimer > 0;
    }

    private void Update()
    {
        if (plantData == null || plantData.GrowthConfig == null) return;

        // Обновляем таймер воды
        if (state.WaterTimer > 0)
        {
            state.WaterTimer -= Time.deltaTime;
            
            // Если вода закончилась в этом кадре
            if (state.WaterTimer <= 0)
            {
                state.WaterTimer = 0;
                UpdateSoilVisual();
                Debug.Log($"Почва в {gridPosition} высохла.");
            }
        }

        // Если не полито — не растем
        if (state.WaterTimer <= 0) return;

        var config = plantData.GrowthConfig;

        // Stop growth if regrow plant has reached harvest limit.
        if (config.IsRegrowable && state.HarvestCount >= config.MaxHarvestsPerPlant) return;

        var stages = state.IsInFruitStage ? config.FruitStages : config.GrowthStages;
        if (stages == null || stages.Length == 0) return;

        if (state.CurrentStageIndex >= stages.Length - 1)
        {
            // Transition from base growth to fruit growth for regrowable plants.
            if (!state.IsInFruitStage && config.IsRegrowable && config.FruitStages != null && config.FruitStages.Length > 0)
            {
                state.IsInFruitStage = true;
                state.CurrentStageIndex = 0;
                state.TimeInCurrentStage = 0f;
                UpdateVisual();
            }
            return;
        }

        state.TimeInCurrentStage += Time.deltaTime;

        if (state.TimeInCurrentStage >= stages[state.CurrentStageIndex].DurationSeconds)
        {
            state.TimeInCurrentStage = 0f;
            state.CurrentStageIndex++;
            UpdateVisual();
        }
    }

    public bool Water()
    {
        bool isAlreadyWatered = state.WaterTimer > 0;
        state.WaterTimer = waterDuration;
        
        if (!isAlreadyWatered)
        {
            UpdateSoilVisual();
            Debug.Log($"Растение в {gridPosition} полито.");
        }
        
        return true;
    }

    private void UpdateSoilVisual()
    {
        if (TileVisualManager.Instance != null)
        {
            TileVisualManager.Instance.SetTileWatered(gridPosition, state.WaterTimer > 0);
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

        // If plant has dedicated fruit stages, harvest should happen there only.
        if (config.IsRegrowable && config.FruitStages != null && config.FruitStages.Length > 0)
            return false;

        return state.CurrentStageIndex >= config.GrowthStages.Length - 1;
    }

    [ContextMenu("Harvest")]
    public void Harvest()
    {
        HandlePlantInteraction(PlantInteractionType.Harvest);
    }

    [ContextMenu("Dig Up")]
    public void DigUp()
    {
        HandlePlantInteraction(PlantInteractionType.DigUp);
    }

    private enum PlantInteractionType
    {
        Harvest,
        DigUp
    }

    private void HandlePlantInteraction(PlantInteractionType interactionType)
    {
        if (interactionType == PlantInteractionType.Harvest)
        {
            HandleHarvest();
            return;
        }

        HandleDigUp();
    }

    private void HandleHarvest()
    {
        if (!CanHarvest())
        {
            Debug.Log("Plant is not ready for harvest yet.");
            return;
        }

        var config = plantData.GrowthConfig;
        AddHarvestToInventory(config, "<color=green>HARVEST: {0}x {1}</color>");

        state.HarvestCount++;

        if (config.IsRegrowable && state.HarvestCount < config.MaxHarvestsPerPlant)
        {
            // Roll back to first fruit stage to regrow fruits.
            state.IsInFruitStage = true;
            state.CurrentStageIndex = 0;
            state.TimeInCurrentStage = 0f;
            UpdateVisual();
            return;
        }

        RemovePlant();
    }

    private void HandleDigUp()
    {
        // Preserve your current behavior: digging mature plant still gives drops.
        if (CanHarvest())
        {
            var config = plantData.GrowthConfig;
            AddHarvestToInventory(config, "<color=orange>DUG UP WITH CROP: {0}x {1}</color>");
        }
        else
        {
            Debug.Log("Plant dug up without crop.");
        }

        RemovePlant();
    }

    private void AddHarvestToInventory(PlantGrowthConfig config, string messageFormat)
    {
        int dropAmount = Random.Range(config.HarvestDrop.MinAmount, config.HarvestDrop.MaxAmount + 1);
        string itemName = config.HarvestDrop.DropItem != null ? config.HarvestDrop.DropItem.name : "Unknown item";

        Debug.Log(string.Format(messageFormat, dropAmount, itemName));

        if (InventoryManager.Instance == null || config.HarvestDrop.DropItem == null)
            return;

        for (int i = 0; i < dropAmount; i++)
        {
            InventoryManager.Instance.AddItem(config.HarvestDrop.DropItem);
        }
    }

    private void RemovePlant()
    {
        if (PlacementSystem.Instance != null)
        {
            PlacementSystem.Instance.RemovePlacedObject(gameObject);
        }

        Debug.Log("Plant dug up and removed.");
        Destroy(gameObject);
    }
}