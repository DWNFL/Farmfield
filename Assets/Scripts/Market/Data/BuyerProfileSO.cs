using UnityEngine;

[CreateAssetMenu(menuName = "Market/Buyer Profile")]
public class BuyerProfileSO : ScriptableObject
{
    [Header("Информация")]
    public string BuyerName;
    public Sprite BuyerIcon;
    [TextArea] public string Description;

    [Header("Ценообразование")]
    [Tooltip("Множитель по умолчанию для всех товаров (1 = базовая цена)")]
    public float DefaultPriceMultiplier = 1f;

    [Tooltip("Исключения — индивидуальные множители для конкретных товаров")]
    public ItemPriceOverride[] PriceOverrides;

    /// <summary>
    /// Рассчитывает цену продажи для конкретного предмета у данного покупателя.
    /// </summary>
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

    /// <summary>
    /// Проверяет, будет ли данный покупатель вообще покупать этот товар.
    /// </summary>
    public bool WillBuyItem(SellableItem item)
    {
        if (item == null)
            return false;

        if (PriceOverrides != null)
        {
            for (int i = 0; i < PriceOverrides.Length; i++)
            {
                if (PriceOverrides[i].TargetItem == item)
                {
                    return PriceOverrides[i].WillBuy;
                }
            }
        }

        // Если нет в исключениях — покупает по умолчанию
        return true;
    }
}
