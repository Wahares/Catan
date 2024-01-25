using UnityEngine;
using System.Linq;

public class SingleRoadController : SinglePieceController
{
    public override PieceType pieceType => PieceType.Road;

    public override PiecePlaceType placeType => PiecePlaceType.Road;

    public override bool CanIPlaceHere(Vector2Int mapPos)
    {
        SinglePieceController[] pieces = Physics
            .OverlapSphere(BoardManager.instance.roads[mapPos].transform.position, 1f
            , LayerMask.GetMask("Crossing","Road"), QueryTriggerInteraction.Collide)
            .Select(e=>e.GetComponent<CrossingController>()?.currentPiece).Where(p=>p != null).ToArray();
        foreach (var piece in pieces)
        {
            if(piece.pieceOwnerID == GameManager.instance.LocalConnection.ClientId)
                if(piece.pieceType != PieceType.Knight)
                    return true;
        }
        return false;
    }
}
