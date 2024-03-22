using UnityEngine;

public class CityRecipe : BuildingRecipe
{
    public override void OnBuilded(Vector2Int pos, int clientID)
    {
        CrossingController cc = BoardManager.instance.crossings[pos];
        foreach (var tile in cc.GetTilesControllers())
        {
            foreach (var card in ObjectDefiner.instance.basicCards)
            {
                if (card.sourceTile == tile.type)
                {
                    PlayerInventoriesManager.instance.ChangeMyCardsQuantity(card.ID, 1);
                    break;
                }
            }
        }
        base.OnBuilded(pos, clientID);
    }
}
