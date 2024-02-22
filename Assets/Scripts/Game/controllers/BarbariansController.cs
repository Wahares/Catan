using FishNet;
using UnityEngine;

public class BarbariansController : MonoBehaviour
{

    private CursorController cc;
    [SerializeField]
    private Transform preview;
    [SerializeField]
    private BuildingRecipe settlementBR;

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

    public void beginDestroying()
    {
        if (!TurnManager.isMyTurn)
            return;

        cc.currentFocusPieceType = PiecePlaceType.Crossing;
        CursorController.Hovering += hover;
        CursorController.OnClicked += finalizeMove;
        isListening = true;

    }

    private void hover(Vector2Int? pos, PiecePlaceType placeType)
    {
        preview.position = Vector3.down * 10;
        if (TurnManager.currentPhase != Phase.Barbarians)
            return;
        if (placeType != PiecePlaceType.Crossing)
            return;
        CrossingController cc = BoardManager.instance.crossings[pos ?? Vector2Int.zero];
        if ((cc.currentPiece?.pieceType ?? PieceType.Unset) != PieceType.City)
            return;
        if (cc.currentPiece.pieceOwnerID != InstanceFinder.ClientManager.Connection.ClientId)
            return;
        preview.position = cc.transform.position;
    }

    public bool isListening = false;

    private void finalizeMove(Vector2Int? pos, PiecePlaceType placeType)
    {
        if (placeType != PiecePlaceType.Crossing)
            return;
        if (!TurnManager.isMyTurn)
            return;

        CrossingController cc = BoardManager.instance.crossings[pos ?? Vector2Int.zero];
        if ((cc.currentPiece?.pieceType ?? PieceType.Unset) != PieceType.City)
            return;
        if (cc.currentPiece.pieceOwnerID != InstanceFinder.ClientManager.Connection.ClientId)
            return;

        BuildingManager.instance.SetPieceOnServer(pos ?? Vector2Int.zero
            , InstanceFinder.ClientManager.Connection.ClientId
            , ObjectDefiner.instance.availableBuildingRecipes.IndexOf(settlementBR));



        cancelAction();
        TurnManager.instance.endTurn();
    }
    public void cancelAction()
    {
        if (!isListening)
            return;
        isListening = false;
        CursorController.Hovering -= hover;
        CursorController.OnClicked -= finalizeMove;
        preview.position = Vector3.down * 10;
    }

}
