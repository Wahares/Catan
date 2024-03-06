public class MaterialMonopol : SpecialCard
{
    public override void OnUsed()
    {
        CardChoiceManager.instance.CreateChoice("Choose Material:"
            , ObjectDefiner.instance.basicCards
            , 1
            , (e) =>
            {
                foreach (var player in PlayerManager.instance.playerColors)
                {
                    if (player.Key == GameManager.instance.LocalConnection.ClientId)
                        continue;
                    PlayerInventoriesManager.instance.AskToGiveCard(e[0].ID, player.Key, 2);
                }
            }
            , null
            , null
            , true);
        PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
    }
}
