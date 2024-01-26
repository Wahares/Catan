using DG.Tweening;
using FishNet;
using System.Linq;
using UnityEngine;

public class SettlementController : SinglePieceController
{
    public override PieceType pieceType => PieceType.Settlement;

    public override PiecePlaceType placeType => PiecePlaceType.Crossing;

    public override void OnTileInvoked(TileController tc)
    {
        NormalCard card = null;
        foreach (var item in PlayerInventoriesManager.instance.availableCards)
        {
            if ((item as NormalCard)?.sourceTile == tc.type)
            {
                card = item as NormalCard;
                break;
            }
        }
        if (card == null)
        {
            Debug.LogError("Unable to find card for: " + tc.type);
            return;
        }
        distributeCard(card, tc);

    }
    protected void distributeCard(CardSO card, TileController tc)
    {
        GameObject obj = PlayerInventoriesManager.instance.createFlyingCard(false, card.ID);

        obj.transform.localScale = Vector3.zero;
        obj.transform.position = tc.transform.position;
        obj.transform.DOScale(Vector3.one/2, 0.5f);
        obj.transform.DOJump(transform.position, 0.5f, 1, 2f);
        obj.transform.DOScale(Vector3.zero, 0.5f).SetDelay(1.5f).OnComplete(() => Destroy(obj));

        if (InstanceFinder.NetworkManager.IsServer)
            PlayerInventoriesManager.instance.ChangeCardQuantity(pieceOwnerID, card.ID, 1);
    }
    public override bool CanIPlaceHere(Vector2Int mapPos)
    {
        if (BoardManager.instance.getPiece(mapPos, placeType) != null)
            return false;
        SingleRoadController[] pieces = Physics
            .OverlapSphere(BoardManager.instance.crossings[mapPos].transform.position, 0.75f
            , LayerMask.GetMask("Road"), QueryTriggerInteraction.Collide)
            .Select(e => e.GetComponent<RoadController>()?.currentPiece)
            .Where(p => p != null).ToArray();
        foreach (var piece in pieces)
            if (piece.pieceOwnerID == GameManager.instance.LocalConnection.ClientId)
                return true;
        if (TurnManager.currentPhase == Phase.PlacingVillages)
            return true;
        return false;
    }

    public virtual int getVictoryWeight() { return 1; }

}
