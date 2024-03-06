using System.Collections.Generic;

public class CommodityUpgradeRecipe : TradingOption
{
    public growthType type;
    public override bool CanUse(List<CardSO> cards, int clientID) => canUse(cards, clientID, 0);
    protected bool canUse(List<CardSO> cards, int clientID, int delta)
    {
        int currentLevel = CommodityUpgradeManager.instance.getUpgradeLevel(clientID, type) + delta;
        if (currentLevel == 5)
            return false;
        bool hasNormalCity = false;
        bool hasAnyCity = false;
        foreach (CrossingController cc in BoardManager.instance.crossings.Values)
        {
            if (cc.currentPiece == null)
                continue;
            if (cc.currentPiece.pieceOwnerID != clientID)
                continue;
            if (cc.currentPiece.pieceType == PieceType.City)
            {
                hasAnyCity = true;
                if (!cc.currentPiece.GetComponent<CityController>().isMetropoly)
                    hasNormalCity = true;
            }
        }

        if (!hasAnyCity)
            return false;
        if (!hasNormalCity && currentLevel >= 3)
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
