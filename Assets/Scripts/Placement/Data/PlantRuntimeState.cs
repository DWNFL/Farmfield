using System;

[Serializable]
public class PlantRuntimeState
{
    public int CurrentStageIndex;
    public float TimeInCurrentStage;
    public int HarvestCount;
    public bool IsInFruitStage;
}
