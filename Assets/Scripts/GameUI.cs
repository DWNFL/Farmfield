using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text statusText;
    public Bucket bucket;
    public Well well;
    public Flower flower;

    void Start()
    {
        if (statusText == null)
        {
            CreateUIWithFont();
        }

        if (bucket == null) bucket = FindObjectOfType<Bucket>();
        if (well == null) well = FindObjectOfType<Well>();
        if (flower == null) flower = FindObjectOfType<Flower>();
    }

    void CreateUIWithFont()
    {
        // Создаем Canvas
        GameObject canvasObj = GameObject.Find("GameCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("GameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Создаем текст
        GameObject textObj = new GameObject("StatusText");
        textObj.transform.SetParent(canvasObj.transform);

        statusText = textObj.AddComponent<Text>();

        // Пробуем разные варианты шрифтов
        Font font = null;

        // Вариант 1: LegacyRuntime
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Вариант 2: если не работает, используем системный шрифт
        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 28);
        }

        statusText.font = font;
        statusText.fontSize = 28;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperLeft;
        statusText.text = "Загрузка...";

        RectTransform rect = statusText.rectTransform;
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(400, 300);

        Debug.Log("UI создан со шрифтом: " + (font != null ? font.name : "null"));
    }

    void Update()
    {
        if (statusText == null) return;

        string status = "=== СОСТОЯНИЕ ===\n\n";

        if (bucket != null)
            status += $"Ведро: {bucket.waterInBucket}/{bucket.maxWater}\n";

        if (well != null)
            status += $"Колодец: {well.currentWater}/{well.maxWater}\n";

        if (flower != null)
        {
            status += $"\nЦветок: {flower.waterReceived}/{flower.waterNeeded}\n";
            status += $"Поливов: {flower.timesWatered}/{flower.maxTimesWatered}\n";
            if (flower.isWatered) status += "РАСЦВЕЛ!\n";
        }

        statusText.text = status;
    }
}