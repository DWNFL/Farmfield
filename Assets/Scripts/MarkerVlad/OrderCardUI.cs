using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderCardUI : MonoBehaviour
{
    [Header("Тексты")]
    public TextMeshProUGUI buyerNameText; // Для объекта Header
    public TextMeshProUGUI itemNameText;  // Для объекта Summary
    public TextMeshProUGUI amountText;    // Тоже можно Summary
    public TextMeshProUGUI rewardText;    // Тоже можно Header
    public TextMeshProUGUI timerText;     // Текст ВНУТРИ TimerZone

    [Header("Слайдеры")]
    public Slider timerSlider;            // Слайдер ВНУТРИ TimerZone

    [Header("Кнопки")]
    public Button acceptButton;           // Сюда тяни Accept
    public Button rejectButton;           // Сюда тяни Отказ
}