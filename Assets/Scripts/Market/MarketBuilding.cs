using UnityEngine;

/// <summary>
/// Opens the market UI when the player clicks the market building.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MarketBuilding : MonoBehaviour
{
    [SerializeField] private MarketUIController marketUI;

    [Tooltip("Maximum interaction distance. Set to 0 to disable the distance check.")]
    [SerializeField] private float interactionDistance = 5f;

    [SerializeField] private Transform playerTransform;

    private void OnMouseDown()
    {
        if (marketUI == null)
        {
            Debug.LogWarning("MarketBuilding: MarketUIController is not assigned.");
            return;
        }

        if (interactionDistance > 0f && playerTransform != null)
        {
            float distance = Vector2.Distance(playerTransform.position, transform.position);
            if (distance > interactionDistance)
            {
                Debug.Log("Too far from the market.");
                return;
            }
        }

        marketUI.Toggle();
    }
}
