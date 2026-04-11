using UnityEngine;

public class Well : MonoBehaviour
{
    public int currentWater = 500;
    public int maxWater = 500;
    public float refillRate = 1f;
    public float refillInterval = 10f;

    private float lastRefillTime;

    void Awake()
    {
        // Принудительно устанавливаем правильные значения при запуске
        maxWater = 500;
        if (currentWater > maxWater) currentWater = maxWater;
        if (currentWater <= 0) currentWater = maxWater;

        lastRefillTime = Time.time;
    }

    void Update()
    {
        if (currentWater < maxWater && Time.time - lastRefillTime >= refillInterval)
        {
            currentWater = Mathf.Min(currentWater + (int)refillRate, maxWater);
            lastRefillTime = Time.time;
            Debug.Log($"Колодец восстановил воду: {currentWater}/{maxWater}");
        }
    }

    public int TakeWater(int amount)
    {
        if (currentWater <= 0)
        {
            Debug.Log("Колодец пуст!");
            return 0;
        }

        int waterToTake = Mathf.Min(amount, currentWater);
        currentWater -= waterToTake;
        Debug.Log($"Взято {waterToTake} воды из колодца. Осталось: {currentWater}/{maxWater}");
        return waterToTake;
    }

    public bool HasWater()
    {
        return currentWater > 0;
    }

    public int GetCurrentWater()
    {
        return currentWater;
    }
}