using UnityEngine;

[CreateAssetMenu(menuName = "Market/Buyer Profile")]
public class BuyerProfileSO : ScriptableObject
{
    [Header("Info")]
    public string BuyerName;
    public Sprite BuyerIcon;
    [TextArea] public string Description;

    [Header("Direct Sales Pricing")]
    [Tooltip("Default price multiplier for direct bazaar sales")]
    public float DefaultPriceMultiplier = 1f;

    [Tooltip("Per-item exceptions")]
    public ItemPriceOverride[] PriceOverrides;

    [Header("Order Generation")]
    [Tooltip("How often this buyer sends a new order")]
    public float OrderIntervalSeconds = 120f;
    [Tooltip("How long unaccepted offer stays in available list")]
    public float OfferLifetimeSeconds = 120f;

    [Tooltip("Max number of uncompleted orders from this buyer (available + accepted)")]
    public int MaxOutstandingOrders = 1;

    [Tooltip("Minimum requested amount in one order")]
    public int MinOrderAmount = 10;

    [Tooltip("Maximum requested amount in one order")]
    public int MaxOrderAmount = 30;

    [Tooltip("Priority multiplier at order creation (higher = better starting reward)")]
    public float MinInitialOrderMultiplier = 1.2f;

    public float MaxInitialOrderMultiplier = 1.8f;

    [Tooltip("Seconds to decay multiplier down to 1x")]
    public float MultiplierDecaySeconds = 90f;
    [Tooltip("How often multiplier drops by one step")]
    public float MultiplierStepSeconds = 15f;

    [Tooltip("How long order stays at 1x before disappearing")]
    public float OneXHoldSeconds = 60f;

    [Header("Demand Bias")]
    [Tooltip("If enabled, this buyer prefers ProduceItem in generated orders")]
    public bool PreferFreshProducts = false;

    [Tooltip("Weight bonus for ProduceItem when selecting order item")]
    public float FreshPreferenceWeight = 2f;

    [Tooltip("If true buyer may request any item they buy; if false only template's buyer or unbound templates")]
    public bool UseAnyCompatibleTemplate = true;

    public int GetPrice(SellableItem item)
    {
        if (item == null)
            return 0;

        float multiplier = DefaultPriceMultiplier;

        if (PriceOverrides != null)
        {
            for (int i = 0; i < PriceOverrides.Length; i++)
            {
                if (PriceOverrides[i].TargetItem == item)
                {
                    if (!PriceOverrides[i].WillBuy)
                        return 0;

                    multiplier = PriceOverrides[i].PriceMultiplier;
                    break;
                }
            }
        }

        return Mathf.Max(1, Mathf.RoundToInt(item.Price * multiplier));
    }

    public bool WillBuyItem(SellableItem item)
    {
        if (item == null)
            return false;

        if (PriceOverrides != null)
        {
            for (int i = 0; i < PriceOverrides.Length; i++)
            {
                if (PriceOverrides[i].TargetItem == item)
                    return PriceOverrides[i].WillBuy;
            }
        }

        return true;
    }
}
