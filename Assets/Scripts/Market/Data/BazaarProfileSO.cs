using UnityEngine;

[CreateAssetMenu(menuName = "Market/Bazaar Profile")]
public class BazaarProfileSO : MarketBuyerProfileSO
{
    [Header("Direct Sales Pricing")]
    public float DefaultPriceMultiplier = 1f;

    public int GetPrice(SellableItem item)
    {
        if (item == null)
            return 0;

        return Mathf.Max(1, Mathf.RoundToInt(item.Price * DefaultPriceMultiplier));
    }
}
