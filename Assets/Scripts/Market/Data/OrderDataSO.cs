using UnityEngine;

[CreateAssetMenu(menuName = "Market/Order Template")]
public class OrderDataSO : ScriptableObject
{
    [Header("Покупатель")]
    public BuyerProfileSO Buyer;

    [Header("Запрос")]
    [Tooltip("Какой предмет нужен")]
    public SellableItem RequestedItem;

    [Tooltip("Сколько штук нужно")]
    public int RequestedAmount = 10;

    [Header("Награда")]
    [Tooltip("Базовая награда в монетах за выполнение заказа")]
    public int RewardCoins = 50;

    [Tooltip("Бонус монет за досрочную сдачу (макс. бонус при мгновенной сдаче)")]
    public int EarlyBonusCoins = 25;

    [Header("Время")]
    [Tooltip("Время на выполнение заказа в секундах")]
    public float TimeLimitSeconds = 300f;
}
