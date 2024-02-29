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
        CursorController.OnClicked += finalizeDestroy;
        isListening = true;

    }

    private void hover(Vector2Int? pos, PiecePlaceType placeType)
    {
        preview.position = Vector3.down * 10;
        if (!IsValid(pos, placeType))
            return;
        CrossingController cc = BoardManager.instance.crossings[pos ?? Vector2Int.zero];
        preview.position = cc.transform.position;
    }
    private void finalizeDestroy(Vector2Int? pos, PiecePlaceType placeType)
    {
        if (!IsValid(pos, placeType))
            return;

        BuildingManager.instance.SetPieceOnServer(pos ?? Vector2Int.zero
            , InstanceFinder.ClientManager.Connection.ClientId
            , ObjectDefiner.instance.availableBuildingRecipes.IndexOf(settlementBR));



        cancelAction();
        TurnManager.instance.endTurn();
    }

    public bool isListening = false;
    private bool IsValid(Vector2Int? pos, PiecePlaceType placeType)
    {
        if (TurnManager.currentPhase != Phase.Barbarians)
            return false;
        if (placeType != PiecePlaceType.Crossing)
            return false;
        SinglePieceController piece = BoardManager.instance.crossings[pos ?? Vector2Int.zero].currentPiece;
        if (piece == null)
            return false;
        if (piece.pieceType != PieceType.City)
            return false;
        if ((piece as CityController).isMetropoly)
            return false;
        if (piece.pieceOwnerID != InstanceFinder.ClientManager.Connection.ClientId)
            return false;
        return true;
    }
    public void cancelAction()
    {
        if (!isListening)
            return;
        isListening = false;
        CursorController.Hovering -= hover;
        CursorController.OnClicked -= finalizeDestroy;
        preview.position = Vector3.down * 10;
    }

}
