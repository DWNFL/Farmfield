/// <summary>
/// Runtime-данные активного заказа (не ScriptableObject, живёт в памяти).
/// </summary>
public class ActiveOrder
{
    public OrderDataSO Template { get; private set; }
    public float TimeRemaining { get; set; }
    public int DeliveredAmount { get; set; }

    /// <summary>
    /// Заказ выполнен (сдано достаточное количество)?
    /// </summary>
    public bool IsCompleted => DeliveredAmount >= Template.RequestedAmount;

    /// <summary>
    /// Заказ просрочен (время вышло)?
    /// </summary>
    public bool IsExpired => TimeRemaining <= 0f;

    /// <summary>
    /// Сколько ещё нужно сдать.
    /// </summary>
    public int RemainingAmount => Template.RequestedAmount - DeliveredAmount;

    /// <summary>
    /// Прогресс по времени: 0 = только принял, 1 = время вышло.
    /// </summary>
    public float TimeProgress => 1f - (TimeRemaining / Template.TimeLimitSeconds);

    /// <summary>
    /// Рассчитывает итоговую награду с учётом бонуса за досрочную сдачу.
    /// Чем раньше сдал — тем больше бонус (линейная интерполяция).
    /// </summary>
    public int CalculateReward()
    {
        float earlyFactor = 1f - TimeProgress; // 1 = мгновенно, 0 = в последний момент
        int bonus = (int)(Template.EarlyBonusCoins * earlyFactor);
        return Template.RewardCoins + bonus;
    }

    public ActiveOrder(OrderDataSO template)
    {
        Template = template;
        TimeRemaining = template.TimeLimitSeconds;
        DeliveredAmount = 0;
    }
}
