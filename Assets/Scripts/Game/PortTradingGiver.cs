using UnityEngine;

public class PortTradingGiver : ExchangeGiver
{
    private Vector2Int mapPos;
    [SerializeField]
    private GameObject TwoOnePrefab, ThreeOnePrefab,visualPivot;
    public void Setup(Vector2Int mapPos)
    {
        this.mapPos = mapPos;
        if (tradingOptions.Length == 0)
            Debug.LogError("TradingOptions not specified!");

        Instantiate((tradingOptions.Length > 1) ? ThreeOnePrefab : TwoOnePrefab, visualPivot.transform).GetComponent<GeneralExchangeIconVisual>().setup(this);

    }
    protected override bool CanGive(int clientID)
    {
        SinglePieceController spc = BoardManager.instance.crossings[mapPos].currentPiece;
        if (spc == null)
            return false;
        if (spc.pieceOwnerID != clientID)
            return false;
        if (spc.pieceType != PieceType.City && spc.pieceType != PieceType.Settlement)
            return false;
        return (spc as SettlementController)?.CanGiveTradingFromPort ?? false;
    }

}
