using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderLineUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button deliverButton;
    [SerializeField] private TMP_Text deliverButtonText;

    private Action deliverAction;

    public void Bind(ActiveOrderLine line, bool isActive, Action onDeliver)
    {
        deliverAction = onDeliver;

        if (itemIcon != null && line?.Item != null && line.Item.Icon != null)
            itemIcon.sprite = line.Item.Icon;

        if (amountText != null)
        {
            amountText.text = isActive && line != null
                ? $"{line.DeliveredAmount}/{line.RequestedAmount}"
                : $"x{(line != null ? line.RequestedAmount : 0)}";
        }

        if (priceText != null)
        {
            int value = line?.Item != null ? Mathf.Max(1, line.Item.Price) * Mathf.Max(1, line.RequestedAmount) : 0;
            priceText.text = $"{value}";
        }

        if (deliverButton != null)
        {
            deliverButton.onClick.RemoveAllListeners();
            deliverButton.gameObject.SetActive(isActive);
            deliverButton.interactable = isActive && line != null && !line.IsCompleted;
            deliverButton.onClick.AddListener(OnDeliverClicked);
        }

        if (deliverButtonText != null)
            deliverButtonText.text = "Load";
    }

    private void OnDestroy()
    {
        if (deliverButton != null)
            deliverButton.onClick.RemoveAllListeners();
    }

    private void OnDeliverClicked()
    {
        deliverAction?.Invoke();
    }
}
