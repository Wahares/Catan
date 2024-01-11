using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrossingController : PieceContainerController<CrossingController,SinglePieceController>
{

    public void TileNumberRolled(TileController source)
    {
        (currentPiece as SettlementController)?.TileRolled(source);
    }
}
