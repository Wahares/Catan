using FishNet.Object;
using UnityEngine;
using DG.Tweening;

public class TraderController : NetworkBehaviour
{
    [SerializeField]
    private Transform preview, trader;
    public Vector2Int currentPos { get; private set; }
    public int currentOwner { get; private set; }
    private CursorController cc;
    private int traderCardID;
    public bool isListening = false;

    protected void Awake()
    {
        cc = FindAnyObjectByType<CursorController>();
        trader.position = Vector3.up * -4f;
        preview.position = Vector3.up * -4f;
        currentPos = new Vector2Int(-1, -1);
        currentOwner = -1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveTraderOnServer(Vector2Int newPos, int newOwner)
    {
        MoveTrader(newPos, newOwner);
    }
    [ObserversRpc]
    private void MoveTrader(Vector2Int newPos, int newOwner)
    {
        currentPos = newPos;
        currentOwner = newOwner;

        trader.DOComplete();
        trader.DOJump(BoardManager.instance.Tiles[newPos].transform.position, 0.5f, 1, 0.25f);

    }


    public void BeginMoving(int cardID)
    {
        traderCardID = cardID;
        if (!TurnManager.isMyTurn)
            return;

        cc.currentFocusPieceType = PiecePlaceType.TileMiddle;
        CursorController.Hovering += hover;
        CursorController.OnClicked += finalizeMove;
        CursorController.OnRightClicked += cancelAction;
        TurnManager.OnMyTurnEnded += cancelAction;
        isListening = true;
    }

    private void hover(Vector2Int? pos, PiecePlaceType placeType)
    {
        preview.position = Vector3.down * 10;
        if (TurnManager.currentPhase != Phase.CasualRound)
            return;
        if (placeType != PiecePlaceType.TileMiddle)
            return;
        else
            preview.position = BoardManager.instance.Tiles[pos ?? Vector2Int.zero].transform.position;
    }

    private void finalizeMove(Vector2Int? pos, PiecePlaceType placeType)
    {
        if (placeType != PiecePlaceType.TileMiddle)
            return;
        if ((pos ?? Vector2Int.zero) == BoardManager.instance.currentBanditPos)
            return;
        if (!TurnManager.isMyTurn)
            return;

        MoveTraderOnServer(pos ?? Vector2Int.zero, LocalConnection.ClientId);
        cancelAction();
        PlayerInventoriesManager.instance.SpecialCardUseEffect(traderCardID);
    }
    private void cancelAction()
    {
        if (!isListening)
            return;
        isListening = false;
        cc.currentFocusPieceType = PiecePlaceType.None;
        CursorController.Hovering -= hover;
        CursorController.OnClicked -= finalizeMove;
        CursorController.OnRightClicked -= cancelAction;
        TurnManager.OnMyTurnEnded -= cancelAction;
        preview.position = Vector3.down * 10;
    }

}
