using UnityEngine;

[CreateAssetMenu(menuName = "Market/Contract Buyer Profile")]
public class BuyerProfileSO : MarketBuyerProfileSO
{
    [Header("Order Generation")]
    public float OrderIntervalSeconds = 120f;
    public float OfferLifetimeSeconds = 120f;
    public int MaxOutstandingOrders = 1;

    public float MinInitialOrderMultiplier = 1.2f;
    public float MaxInitialOrderMultiplier = 1.8f;

    public float MultiplierDecaySeconds = 90f;
    public float MultiplierStepSeconds = 15f;
    public float OneXHoldSeconds = 60f;
}
