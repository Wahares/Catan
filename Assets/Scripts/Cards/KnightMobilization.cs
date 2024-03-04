using System.Collections.Generic;

public class KnightMobilization : BuildingRecipe
{
    public override bool CanUse(List<CardSO> cards, int clientID) => BaseCanUse(cards, clientID);
    public override void OnUsed()
    {
        if (TurnManager.isMyTurn)
            KnightManager.instance.BeginMobilization(this);
    }
}
