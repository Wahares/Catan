using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CrossingController : PieceContainerController<CrossingController, SinglePieceController>
{
    public void TileNumberRolled(TileController source)
    {
        currentPiece?.OnTileInvoked(source);
    }
    public List<RoadController> GetRoadsControllers()
    {
        return Physics
            .OverlapSphere(transform.position, 0.75f
            , LayerMask.GetMask("Road"), QueryTriggerInteraction.Collide)
            .Select(e => e.GetComponent<RoadController>())
            .Where(p => p != null).ToList();
    }
}
