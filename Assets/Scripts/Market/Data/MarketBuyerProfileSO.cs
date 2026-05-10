using UnityEngine;

public abstract class MarketBuyerProfileSO : ScriptableObject
{
    [Header("Info")]
    public string BuyerName;
    public Sprite BuyerIcon;
    [TextArea] public string Description;
}
