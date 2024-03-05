using FishNet.Object;
using UnityEngine;

public class KnightManager : NetworkBehaviour
{
    public static KnightManager instance { get; private set; }

    [SerializeField]
    private GameObject lvl1Preview, UpgradePreview, MobilizationPreview;

    private CursorController cc;

    private void Awake()
    {
        instance = this;
        cc = GetComponent<CursorController>();
        TurnManager.OnMyTurnEnded += CancelUpgrade;
        TurnManager.OnMyTurnEnded += CancelMobilization;
    }
    private void OnDestroy()
    {
        instance = null;
        TurnManager.OnMyTurnEnded -= CancelUpgrade;
        TurnManager.OnMyTurnEnded -= CancelMobilization;
    }
    private KnightRecipe KR;
    private KnightMobilization KM;
    public void BeginUpgrading(KnightRecipe KR)
    {
        if (this.KR == null)
            this.KR = KR;

        cc.currentFocusPieceType = PiecePlaceType.Crossing;
        CursorController.Hovering += OnHoverUpgrade;
        CursorController.OnClicked += FinalizeUpgrade;
        CursorController.OnRightClicked += CancelUpgrade;
        PlayerCardsOptionsController.isBeingUsed = true;
    }
    public void BeginMobilization(KnightMobilization KM)
    {
        if (this.KM == null)
            this.KM = KM;
        cc.currentFocusPieceType = PiecePlaceType.Crossing;
        CursorController.Hovering += OnHoverMobilization;
        CursorController.OnClicked += FinalizeMobilization;
        CursorController.OnRightClicked += CancelMobilization;
        PlayerCardsOptionsController.isBeingUsed = true;
    }

    private void OnHoverUpgrade(Vector2Int? pos, PiecePlaceType type)
    {
        UpgradePreview.SetActive(false);
        lvl1Preview.SetActive(false);
        if (pos == null)
            return;
        Vector2Int poss = pos ?? Vector2Int.zero;

        if (!KR.piece.GetComponent<KnightController>().CanIPlaceHere(poss))
            return;

        SinglePieceController spc = BoardManager.instance.crossings[poss]?.currentPiece;

        if (spc != null)
        {
            UpgradePreview.transform.position = spc.transform.position;
            UpgradePreview.SetActive(true);
        }
        else
        {
            lvl1Preview.transform.position = BoardManager.instance.crossings[poss].transform.position;
            lvl1Preview.SetActive(true);
        }
    }
    private void OnHoverMobilization(Vector2Int? pos, PiecePlaceType type)
    {
        MobilizationPreview.SetActive(false);
        if (pos == null)
            return;
        Vector2Int poss = pos ?? Vector2Int.zero;


        SinglePieceController spc = BoardManager.instance.crossings[poss]?.currentPiece;


        if (spc == null)
            return;
        if (spc.pieceOwnerID != GameManager.instance.LocalConnection.ClientId)
            return;
        if (spc as KnightController == null)
            return;
        if ((spc as KnightController).isMobilized)
            return;
        if (!(spc as KnightController).canBeMobilized)
            return;

        MobilizationPreview.transform.position = spc.transform.position;
        MobilizationPreview.SetActive(true);
    }

    private void FinalizeUpgrade(Vector2Int? pos, PiecePlaceType placeType)
    {
        if (pos == null)
            return;
        Vector2Int poss = pos ?? Vector2Int.zero;

        if (!KR.piece.GetComponent<KnightController>().CanIPlaceHere(poss))
            return;

        SinglePieceController spc = BoardManager.instance.crossings[poss]?.currentPiece;


        if (spc != null)
            changeLevel(poss, (spc as KnightController).currentLevel + 1);
        else
            BuildingManager.instance
                .BuildPiece(poss
                , ObjectDefiner.instance.availableBuildingRecipes.IndexOf(KR)
                , GameManager.instance.LocalConnection.ClientId);

        CancelUpgrade();
    }
    private void FinalizeMobilization(Vector2Int? pos, PiecePlaceType placeType)
    {
        if (pos == null)
            return;
        Vector2Int poss = pos ?? Vector2Int.zero;


        SinglePieceController spc = BoardManager.instance.crossings[poss]?.currentPiece;


        if (spc == null)
            return;
        if (spc.pieceOwnerID != GameManager.instance.LocalConnection.ClientId)
            return;
        if (spc as KnightController == null)
            return;
        if ((spc as KnightController).isMobilized)
            return;
        if (!(spc as KnightController).canBeMobilized)
            return;
        changeMobilization(poss, true);
        PlayerInventoriesManager.instance.ChangeMyCardsQuantity(KM.materials[0].card.ID, -KM.materials[0].number);

        CancelMobilization();
    }

    [ServerRpc(RequireOwnership = false)]
    private void changeLevel(Vector2Int pos, int level)
    {
        changeLevelRPC(pos, level);
    }
    [ObserversRpc]
    private void changeLevelRPC(Vector2Int pos, int level)
    {
        (BoardManager.instance.crossings[pos].currentPiece as KnightController).ChangeLevel(level);
    }

    [ServerRpc(RequireOwnership = false)]
    public void changeMobilization(Vector2Int pos, bool mobilizated)
    {
        changeMobilizationRPC(pos, mobilizated);
    }
    [ObserversRpc]
    private void changeMobilizationRPC(Vector2Int pos, bool mobilizated)
    {
        (BoardManager.instance.crossings[pos].currentPiece as KnightController).ChangeMobilization(mobilizated);
    }



    private void CancelUpgrade()
    {
        UpgradePreview.SetActive(false);
        lvl1Preview.SetActive(false);

        CursorController.Hovering -= OnHoverUpgrade;
        CursorController.OnClicked -= FinalizeUpgrade;
        CursorController.OnRightClicked -= CancelUpgrade;

        PlayerCardsOptionsController.isBeingUsed = false;
    }
    private void CancelMobilization()
    {
        MobilizationPreview.SetActive(false);

        CursorController.Hovering -= OnHoverMobilization;
        CursorController.OnClicked -= FinalizeMobilization;
        CursorController.OnRightClicked -= CancelMobilization;

        PlayerCardsOptionsController.isBeingUsed = false;
    }

    public bool BelowMaxKnights(int clientID, int level)
    {
        int num = 0;
        foreach (var cross in BoardManager.instance.crossings)
        {
            if ((cross.Value.currentPiece?.pieceOwnerID ?? -1) != clientID)
                continue;
            if (cross.Value.currentPiece.pieceType != PieceType.Knight)
                continue;
            if ((cross.Value.currentPiece as KnightController).currentLevel == level)
                num++;
        }
        return num < KR.maxOnBoard;
    }






}
