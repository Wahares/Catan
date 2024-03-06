public class DiscountedBuildingRecipe : BuildingRecipe
{
    public override void OnUsed()
    {
        base.OnUsed();
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Remove(this);
    }
}
