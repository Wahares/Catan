using System.Linq;
using UnityEngine;

public class KnightController : SinglePieceController
{
    public bool isMobilized { get; private set; }

    public override PieceType pieceType => PieceType.Knight;

    public override PiecePlaceType placeType => PiecePlaceType.Crossing;

    public override bool CanIPlaceHere(Vector2Int mapPos)
    {
        if (BoardManager.instance.getPiece(mapPos, placeType) != null)
            return false;
        SingleRoadController[] pieces = Physics
            .OverlapSphere(BoardManager.instance.crossings[mapPos].transform.position
            , 0.75f, LayerMask.GetMask("Road"), QueryTriggerInteraction.Collide)
            .Select(e => e.GetComponent<RoadController>()?.currentPiece)
            .Where(p => p != null).ToArray();
        foreach (var piece in pieces)
            if (piece.pieceOwnerID == GameManager.instance.LocalConnection.ClientId)
                return true;
        return false;
    }
}
