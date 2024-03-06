using UnityEngine;

public class Engineer : SpecialCard
{
    [SerializeField]
    private DiscountedBuildingRecipe recipe;
    public override void OnUsed()
    {
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Add(recipe);
        PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
    }
}
