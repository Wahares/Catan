public class TradeUpgradeExchangeGiver : ExchangeGiver
{
    protected override bool CanGive(int clientID)
    {
        return CommodityUpgradeManager.instance.getUpgradeLevel(clientID, growthType.Trade) >= 3;
    }

}
