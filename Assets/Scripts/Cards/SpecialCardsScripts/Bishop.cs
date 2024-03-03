public class Bishop : SpecialCard
{
    public override bool CanUse() => BoardManager.instance.banditsExsist() && base.CanUse();
    public override void OnUsed()
    {
        FindAnyObjectByType<BanditsController>().BishopUsed(ObjectDefiner.instance.equipableCards.IndexOf(this));
        PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
    }
}
