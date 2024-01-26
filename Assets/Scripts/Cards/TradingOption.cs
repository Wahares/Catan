using UnityEngine;
[CreateAssetMenu(fileName = "Option", menuName = "Cards/Trading Option")]
public class TradingOption : RecipedCard
{
    public override void OnUsed()
    {
        PlayerInventoryExchangeController.instance.BeginTransaction(this);
    }
}
