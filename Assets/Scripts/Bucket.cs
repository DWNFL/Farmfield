using UnityEngine;

public class Bucket : MonoBehaviour
{
    public int waterInBucket = 0;
    public int maxWater = 100;

    public void TakeWaterFromWell(Well well)
    {
        if (waterInBucket >= maxWater)
        {
            Debug.Log("Ведро полное!");
            return;
        }

        if (well == null)
        {
            Debug.LogError("Нет ссылки на колодец!");
            return;
        }

        int spaceInBucket = maxWater - waterInBucket;
        int waterToTake = Mathf.Min(5, spaceInBucket); // берем по 5 воды
        int waterReceived = well.TakeWater(waterToTake);

        waterInBucket += waterReceived;
        Debug.Log($"Набрали {waterReceived} воды. В ведре: {waterInBucket}/{maxWater}");
    }

    public void WaterFlower(Flower flower)
    {
        if (waterInBucket <= 0)
        {
            Debug.Log("В ведре нет воды!");
            return;
        }

        if (flower == null)
        {
            Debug.LogError("Нет ссылки на цветок!");
            return;
        }

        if (!flower.CanBeWatered())
        {
            return;
        }

        int waterNeeded = flower.WaterNeeded();
        int waterToUse = Mathf.Min(waterInBucket, waterNeeded);

        flower.ReceiveWater(waterToUse);
        waterInBucket -= waterToUse;

        Debug.Log($"Полили цветок. Использовано {waterToUse} воды. В ведре: {waterInBucket}");
    }
}