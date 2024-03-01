using UnityEngine;
using DG.Tweening;

public class CityController : SettlementController
{
    public override PieceType pieceType => PieceType.City;
    public bool isMetropoly, hasWalls;
    [SerializeField]
    private GameObject wallsObj, metropolyObj;
    private GameObject currentWalls,currentMetropoly;
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
    public void makeItMetropoly()
    {
        isMetropoly = true;
        currentMetropoly = Instantiate(metropolyObj, transform);
        currentMetropoly.transform.DOKill();
        currentMetropoly.transform.localPosition = Vector3.up;
        currentMetropoly.transform.DOLocalMoveY(0, 0.5f);
    }
    public void destroyMetropoly()
    {
        isMetropoly = false;
        GameObject tmp = currentMetropoly;
        currentMetropoly = null;
        tmp.transform.DOKill();
        tmp.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => { Destroy(tmp); });
    }
    public void makeWalls()
    {
        hasWalls = true;
        currentWalls = Instantiate(wallsObj, transform);
        currentWalls.transform.DOKill();
        currentWalls.transform.localPosition = Vector3.up;
        currentWalls.transform.DOLocalMoveY(0, 0.5f);
    }
    public void destroyWalls()
    {
        hasWalls = false;
        GameObject tmp = currentWalls;
        currentWalls = null;
        tmp.transform.DOKill();
        tmp.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => { Destroy(tmp); });
    }

}
