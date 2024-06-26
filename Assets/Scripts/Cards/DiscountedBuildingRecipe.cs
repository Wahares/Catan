using UnityEngine;

public class DiscountedBuildingRecipe : BuildingRecipe
{
    public override void OnBuilded(Vector2Int pos, int clientID)
    {
        base.OnBuilded(pos, clientID);
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Remove(this);
    }
}
