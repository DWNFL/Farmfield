using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI priceText;
    public TMP_InputField quantityInput;

    [Header("Buttons")]
    public Button applyButton; // Основная кнопка (ОК)
    public Button sellOneButton; // Продать 1
    public Button sellAllButton; // Продать всё
    public TextMeshProUGUI applyButtonText;

    public void Setup(string name, string price, string myAmount, bool isSell)
    {
        itemNameText.text = name;
        priceText.text = price;
        amountText.text = "У вас: " + myAmount;

        // Если это покупка, кнопки "Продать 1" и "Продать всё" нам не нужны
        if (sellOneButton != null) sellOneButton.gameObject.SetActive(isSell);
        if (sellAllButton != null) sellAllButton.gameObject.SetActive(isSell);

        applyButtonText.text = isSell ? "ПРОДАТЬ" : "КУПИТЬ";
    }
}