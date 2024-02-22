using FishNet.Object;
using UnityEngine;

public class BuildingManager : NetworkBehaviour
{
    public static BuildingManager instance;

    public Transform preview;
    private CursorController cc;

    public bool isBuilding => currentRecipe != null;
    private BuildingRecipe currentRecipe;
    private void Awake()
    {
        instance = this;
        cc = GetComponent<CursorController>();
        CursorController.Hovering += snapToCursor;
        CursorController.OnClicked += finalize;
        CursorController.OnRightClicked += cancelBuilding;
        TurnManager.OnMyTurnEnded += resetBuilding;
    }
    private void OnDestroy()
    {
        CursorController.Hovering -= snapToCursor;
        CursorController.OnClicked -= finalize;
        CursorController.OnRightClicked -= cancelBuilding;
        TurnManager.OnMyTurnEnded -= resetBuilding;
    }
    public void BeginBuilding(BuildingRecipe br)
    {
        cc.currentFocusPieceType = br.piece.GetComponent<SinglePieceController>().placeType;
        Instantiate(br.piecePreview, preview).transform.localPosition = Vector3.zero;
        currentRecipe = br;
    }

    private void snapToCursor(Vector2Int? pos, PiecePlaceType type)
    {
        if (currentRecipe == null)
            return;
        if (pos == null)
            return;
        Vector3 globalPos = Vector3.down * 10;
        Vector3 rot = Vector3.zero;
        switch (type)
        {
            case PiecePlaceType.Crossing:
                if (!(currentRecipe.piece.GetComponent<SinglePieceController>()?.CanIPlaceHere(pos ?? Vector2Int.zero) ?? true))
                    break;
                globalPos = BoardManager.instance.crossings[pos ?? Vector2Int.zero].transform.position;
                rot = BoardManager.instance.crossings[pos ?? Vector2Int.zero].transform.eulerAngles;
                break;
            case PiecePlaceType.Road:
                if (!(currentRecipe.piece.GetComponent<SinglePieceController>()?.CanIPlaceHere(pos ?? Vector2Int.zero) ?? true))
                    break;
                globalPos = BoardManager.instance.roads[pos ?? Vector2Int.zero].transform.position;
                rot = BoardManager.instance.roads[pos ?? Vector2Int.zero].transform.eulerAngles;
                break;
            case PiecePlaceType.TileMiddle:
                if (!(currentRecipe.piece.GetComponent<SinglePieceController>()?.CanIPlaceHere(pos ?? Vector2Int.zero) ?? true))
                    break;
                globalPos = BoardManager.instance.Tiles[pos ?? Vector2Int.zero].transform.position;
                rot = BoardManager.instance.Tiles[pos ?? Vector2Int.zero].transform.eulerAngles;
                break;
        }
        preview.position = globalPos;
        preview.eulerAngles = rot;
    }
    [SerializeField]
    private BuildingRecipe roadRecipe;
    private void finalize(Vector2Int? pos, PiecePlaceType type)
    {
        if (currentRecipe == null)
            return;
        if (pos == null)
            return;

        if (!(currentRecipe.piece.GetComponent<SinglePieceController>()?.CanIPlaceHere(pos ?? Vector2Int.zero) ?? true))
            return;
        BuildPiece(pos ?? Vector2Int.zero, ObjectDefiner.instance.availableBuildingRecipes.IndexOf(currentRecipe), LocalConnection.ClientId);

        resetBuilding();
        if (TurnManager.currentPhase == Phase.FreeBuild)
            TurnManager.instance.endTurn();
    }
    private void resetBuilding()
    {
        if (preview.childCount > 0)
            Destroy(preview.GetChild(0).gameObject);
        cc.currentFocusPieceType = PiecePlaceType.None;
        currentRecipe = null;
    }
    private void cancelBuilding()
    {
        if (TurnManager.currentPhase == Phase.CasualRound)
            resetBuilding();
    }
    [ServerRpc(RequireOwnership = false)]
    private void BuildPiece(Vector2Int pos, int brID, int clientID)
    {
        if (TurnManager.turnOrder[TurnManager.currentTurnID] == clientID)
            BuildPieceOnClients(pos, brID, clientID);

        BuildingRecipe br = ObjectDefiner.instance.availableBuildingRecipes[brID];
        if (TurnManager.currentPhase != Phase.FreeBuild)
            foreach (var mat in br.materials)
                PlayerInventoriesManager.instance.ChangeCardQuantity(clientID, mat.card.ID, -mat.number);
    }
    [ObserversRpc]
    private void BuildPieceOnClients(Vector2Int pos, int brID, int clientID)
    {
        BuildingRecipe br = ObjectDefiner.instance.availableBuildingRecipes[brID];

        SinglePieceController spc = Instantiate(br.piece, Vector3.zero, Quaternion.identity)
            .GetComponent<SinglePieceController>();
        spc.Initialize(clientID, pos);

    }
    [ServerRpc(RequireOwnership = false)]
    public void SetPieceOnServer(Vector2Int pos, int clientID, int brID)
    {
        if (TurnManager.currentPhase != Phase.Barbarians && TurnManager.currentPhase != Phase.FreeBuild)
            return;
        BuildPieceOnClients(pos, brID, clientID);
    }

    public BuildingRecipe brr;
    public Vector2Int here;
    [ContextMenu("build")]
    private void buildSettlement()
    {
        BuildPiece(here, ObjectDefiner.instance.availableBuildingRecipes.IndexOf(brr), LocalConnection.ClientId);
    }





}
