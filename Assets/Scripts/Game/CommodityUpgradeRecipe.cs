using System.Collections.Generic;

public class CommodityUpgradeRecipe : TradingOption
{
    public growthType type;
    public override bool CanUse(List<CardSO> cards, int clientID)
    {
        int currentLevel = CommodityUpgradeManager.instance.getUpgradeLevel(clientID, type);
        if (currentLevel == 5)
            return false;

        int remaining = currentLevel + 1;

        foreach (var item in cards)
            if (item.ID == materials[0].card.ID)
                remaining--;
        
        return remaining <= 0;
    }
    public override void OnUsed()
    {
        CommodityUpgradeManager.instance.Upgrade(type, materials[0].card.ID);
    }
}
