using UnityEngine;

public class CityController : SettlementController
{
    public override PieceType pieceType => PieceType.City;
    public bool isMetropoly, hasWalls;
    public override void OnTileInvoked(TileController tc)
    {
        base.OnTileInvoked(tc);

        CommodityCard card = null;
        foreach (var item in PlayerInventoriesManager.instance.availableCards)
        {
            if ((item as CommodityCard)?.sourceTile == tc.type)
            {
                card = item as CommodityCard;
                break;
            }
        }
        if (card == null)
            base.OnTileInvoked(tc);
        else
            distributeCard(card, tc);
    }
    public override bool CanIPlaceHere(Vector2Int mapPos)
    {
        SinglePieceController spc = BoardManager.instance.getPiece(mapPos, placeType);
        if (TurnManager.currentPhase == Phase.FreeBuild)
            return spc == null;
        if (spc == null)
            return false;
        if (spc.pieceOwnerID != BoardManager.instance.LocalConnection.ClientId)
            return false;
        if (spc.pieceType == PieceType.Settlement)
            return true;
        return false;
    }

    public override int getVictoryWeight()
    {
        return isMetropoly ? 4 : 2;
    }

}
