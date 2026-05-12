using UnityEngine;

public class Flower : MonoBehaviour
{
    public int waterNeeded = 10;
    public int waterReceived = 0;  // ← сделал public
    public bool isWatered = false;

    public int timesWatered = 0;    // ← сделал public
    public int maxTimesWatered = 3;
    private float lastWaterTime = -999f;
    public float waterCooldown = 5f;

    void Start()
    {
        waterReceived = 0;
        timesWatered = 0;
        isWatered = false;
    }

    public int WaterNeeded()
    {
        return waterNeeded - waterReceived;
    }

    public bool CanBeWatered()
    {
        if (isWatered)
        {
            Debug.Log("Цветок уже полностью полит!");
            return false;
        }

        if (timesWatered >= maxTimesWatered)
        {
            Debug.Log($"Цветок нельзя больше поливать! Максимум {maxTimesWatered} раз.");
            return false;
        }

        if (Time.time - lastWaterTime < waterCooldown)
        {
            float remainingCooldown = waterCooldown - (Time.time - lastWaterTime);
            Debug.Log($"Нужно подождать {remainingCooldown:F1} секунд!");
            return false;
        }

        return true;
    }

    public void ReceiveWater(int amount)
    {
        if (!CanBeWatered())
            return;

        waterReceived += amount;
        timesWatered++;
        lastWaterTime = Time.time;

        Debug.Log($"Цветок получил {amount} воды. Всего: {waterReceived}/{waterNeeded}");

        if (waterReceived >= waterNeeded)
        {
            isWatered = true;
            Debug.Log("🌸 Цветок расцвел! 🌸");

            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green;
            }
        }
    }
}