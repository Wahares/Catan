using UnityEngine;
using DG.Tweening;

public class CityController : SettlementController
{
    public override PieceType pieceType => PieceType.City;
    public bool isMetropoly, hasWalls;
    [SerializeField]
    private GameObject wallsObj, metropolyObj, wallsPlate;

    [SerializeField]
    private Material[] wallsMats, metropolisMats;
    public override void Initialize(int ownerID, Vector2Int codedPos)
    {
        base.Initialize(ownerID, codedPos);

        wallsObj.GetComponent<MeshRenderer>().material = wallsMats[PlayerManager.instance.playerColors[ownerID]];
        wallsPlate.GetComponent<MeshRenderer>().material = wallsMats[PlayerManager.instance.playerColors[ownerID]];

        metropolyObj.GetComponent<MeshRenderer>().material = metropolisMats[PlayerManager.instance.playerColors[ownerID]];
    }

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

        metropolyObj.transform.DOKill();
        metropolyObj.transform.localPosition = Vector3.up*0.3f;
        metropolyObj.SetActive(true);
        metropolyObj.transform.DOLocalMoveY(-0.03f, 0.5f);
    }
    public void destroyMetropoly()
    {
        isMetropoly = false;
        metropolyObj.transform.DOKill();
        metropolyObj.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => { metropolyObj.SetActive(false); });
    }
    public void makeWalls()
    {
        hasWalls = true;

        wallsObj.transform.parent.transform.DOKill();
        wallsObj.transform.parent.transform.localPosition = Vector3.up * 0.3f;
        wallsObj.transform.parent.gameObject.SetActive(true);
        wallsObj.transform.parent.transform.DOLocalMoveY(-0.03f, 0.5f);
    }
    public void destroyWalls()
    {
        hasWalls = false;
        wallsObj.transform.parent.transform.DOKill();
        wallsObj.transform.parent.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
        {
            wallsObj.transform.parent.gameObject.SetActive(false);
        });
    }

}
