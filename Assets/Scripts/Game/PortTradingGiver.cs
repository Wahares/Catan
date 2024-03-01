using UnityEngine;

public class PortTradingGiver : ExchangeGiver
{
    private Vector2Int mapPos;
    [SerializeField]
    private GameObject TwoOnePrefab, ThreeOnePrefab, visualPivot;
    public void Setup(Vector2Int mapPos, TileType materialType)
    {
        this.mapPos = mapPos;
        tradingOptions = ObjectDefiner.instance.PortTradingOptions[materialType];
        if (tradingOptions.Length == 0)
            Debug.LogError("TradingOptions not specified!");
        //desert means unset specification
        GameObject obj = Instantiate(materialType == TileType.Desert ? ThreeOnePrefab : TwoOnePrefab, visualPivot.transform);
        obj.transform.position = visualPivot.transform.position;
        obj.transform.forward = obj.transform.position - Camera.main.transform.position;
        obj.GetComponent<GeneralExchangeIconVisual>().setup(this);

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
