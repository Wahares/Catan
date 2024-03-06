using UnityEngine;

public class Crane : SpecialCard
{
    [SerializeField]
    private DiscountedCommodityUpgrade discountedPolitics, discountedScience, discountedTrade;

    public override void OnUsed()
    {
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Add(discountedPolitics);
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Add(discountedScience);
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Add(discountedTrade);

        discountedPolitics.setup(this);
        discountedScience.setup(this);
        discountedTrade.setup(this);

        PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
    }
    public void cardUsed()
    {
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Remove(discountedPolitics);
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Remove(discountedScience);
        PlayerCardsOptionsController.instance.temporaryTradingsForRound.Remove(discountedTrade);
    }
}
