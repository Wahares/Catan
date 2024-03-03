using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public class BanditsController : NetworkBehaviour
{
    private CursorController cc;
    [SerializeField]
    private Transform preview;

    private void Awake()
    {
        preview.position = Vector3.down * 10;
        cc = GetComponent<CursorController>();
        TurnManager.OnMyTurnEnded += cancelAction;
    }
    private void OnDestroy()
    {
        TurnManager.OnMyTurnEnded -= cancelAction;
    }

    public void beginMoving()
    {
        if (!TurnManager.isMyTurn)
            return;

        cc.currentFocusPieceType = PiecePlaceType.TileMiddle;
        CursorController.Hovering += hover;
        CursorController.OnClicked += finalizeMove;
        isListening = true;

    }

    private void hover(Vector2Int? pos, PiecePlaceType placeType)
    {
        preview.position = Vector3.down * 10;
        if (TurnManager.currentPhase != Phase.BanditsMove)
            return;
        if (placeType != PiecePlaceType.TileMiddle)
            return;
        else
            preview.position = BoardManager.instance.Tiles[pos ?? Vector2Int.zero].transform.position;
    }

    public bool isListening = false;

    private void finalizeMove(Vector2Int? pos, PiecePlaceType placeType)
    {
        if (placeType != PiecePlaceType.TileMiddle)
            return;
        if ((pos ?? Vector2Int.zero) == BoardManager.instance.currentBanditPos)
            return;
        if (!TurnManager.isMyTurn)
            return;

        BoardManager.instance.moveBanditsOnServer(pos ?? Vector2Int.zero, LocalConnection.ClientId);
        cancelAction();
        TurnManager.instance.endTurn();
    }
    public void cancelAction()
    {
        if (!isListening)
            return;
        isListening = false;
        cc.currentFocusPieceType = PiecePlaceType.None;
        CursorController.Hovering -= hover;
        CursorController.OnClicked -= finalizeMove;
        preview.position = Vector3.down * 10;
    }
    public void BishopUsed(int cardID) => BishopUsedRPC(cardID);

    [ServerRpc(RequireOwnership = false)]
    private void BishopUsedRPC(int cardID,NetworkConnection nc = null)
    {
        PlayerInventoriesManager.instance.ChangeCardQuantity(nc.ClientId, cardID, -1);
    }


}
