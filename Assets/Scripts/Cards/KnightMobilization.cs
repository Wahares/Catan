using System.Collections.Generic;
using UnityEngine;

public class KnightMobilization : BuildingRecipe
{
    public override bool CanUse(List<CardSO> cards, int clientID) => BaseCanUse(cards, clientID);
    /*public override void OnUsed()
    {
        if (TurnManager.isMyTurn)
            KnightManager.instance.BeginMobilization(this);
    }*/
    public override void OnBuilded(Vector2Int pos, int clientID)
    {
        KnightManager.instance.changeMobilization(pos, true);
        foreach (var item in materials)
            PlayerInventoriesManager.instance.ChangeMyCardsQuantity(item.card.ID, -item.number);
    }
}
