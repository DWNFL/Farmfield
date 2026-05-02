using UnityEngine;

/// <summary>
/// Компонент здания рынка. Вешается на объект в сцене.
/// При клике открывает UI панель рынка.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MarketBuilding : MonoBehaviour
{
    [SerializeField] private MarketUIController marketUI;

    [Tooltip("Максимальная дистанция взаимодействия (0 = без ограничений)")]
    [SerializeField] private float interactionDistance = 5f;

    [SerializeField] private Transform playerTransform;

    private void OnMouseDown()
    {
        if (marketUI == null)
        {
            Debug.LogWarning("MarketBuilding: MarketUIController не назначен!");
            return;
        }

        // Проверка дистанции
        if (interactionDistance > 0f && playerTransform != null)
        {
            float dist = Vector3.Distance(playerTransform.position, transform.position);
            if (dist > interactionDistance)
            {
                Debug.Log("Слишком далеко от рынка!");
                return;
            }
        }

        marketUI.Toggle();
    }
}
