public class Trader : SpecialCard
{
    public override void OnUsed()
    {
        FindAnyObjectByType<TraderController>().BeginMoving(ID);
        PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
    }
}
