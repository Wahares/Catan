using UnityEngine;

public class Medicine : SpecialCard
{
    [SerializeField]
    private BuildingRecipe discountedCityRecipe;
    public override bool CanUse() =>
        base.CanUse()
        && BoardManager.instance.numberOfPieces(GameManager.instance.LocalConnection.ClientId, PieceType.Settlement) > 0;
    public override void OnUsed()
    {
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Add(discountedCityRecipe);

        PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
    }
}
