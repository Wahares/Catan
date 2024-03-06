using System.Collections.Generic;
using UnityEngine;

public class KnightRecipe : BuildingRecipe
{
    public override bool CanUse(List<CardSO> cards, int clientID) => BaseCanUse(cards, clientID);
    /*public override void OnUsed()
    {
        if (TurnManager.isMyTurn)
            KnightManager.instance.BeginUpgrading(this);
    }*/
    public override void OnBuilded(Vector2Int pos, int clientID)
    {        
        SinglePieceController spc = BoardManager.instance.crossings[pos]?.currentPiece;

        if (spc != null)
            KnightManager.instance.changeLevel(pos, (spc as KnightController).currentLevel + 1);
        else
            BuildingManager.instance
                .BuildPiece(pos
                , ObjectDefiner.instance.availableBuildingRecipes.IndexOf(this)
                , GameManager.instance.LocalConnection.ClientId);

        foreach (var item in materials)
            PlayerInventoriesManager.instance.ChangeMyCardsQuantity(item.card.ID, -item.number);
    }
}
