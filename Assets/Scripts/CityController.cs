using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityController : SettlementController
{
    public override PieceType pieceType => PieceType.City;
    public override void TileRolled(TileController tc)
    {
        base.TileRolled(tc);
    }
}
