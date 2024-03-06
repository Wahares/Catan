using System.Collections.Generic;
using UnityEngine;

public class DiscountedCommodityUpgrade : CommodityUpgradeRecipe
{
    private Crane craneCard;
    public void setup(Crane cr) { craneCard = cr; }
    public override bool CanUse(List<CardSO> cards, int clientID) => canUse(cards, clientID, -1);

    public override void OnUsed()
    {
        base.OnUsed();
        craneCard.cardUsed();
    }

}
