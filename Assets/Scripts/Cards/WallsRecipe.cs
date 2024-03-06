
using System.Collections.Generic;
using UnityEngine;

public class WallsRecipe : BuildingRecipe
{
    public override bool CanUse(List<CardSO> cards, int clientID)
    {
        int numOfWalls = 0;
        int numOfCities = 0;
        foreach (var cc in BoardManager.instance.crossings)
        {
            if (cc.Value.currentPiece == null)
                continue;
            if (cc.Value.currentPiece is not CityController)
                continue;
            if ((cc.Value.currentPiece as CityController).hasWalls)
                numOfWalls++;
            numOfCities++;
        }
        if (numOfWalls >= maxOnBoard)
            return false;
        if(numOfCities == numOfWalls)
            return false;


        return BaseCanUse(cards, clientID);
    }
    public override void OnUsed()
    {
        base.OnUsed();

    }
    public override void OnBuilded(Vector2Int pos, int clientID)
    {
        base.OnBuilded(pos, clientID);

        foreach (var item in materials)
            PlayerInventoriesManager.instance.ChangeMyCardsQuantity(item.card.ID, -item.number);
    }
}
