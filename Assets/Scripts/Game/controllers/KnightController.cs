using System.Linq;
using UnityEngine;
using DG.Tweening;

public class KnightController : SinglePieceController
{
    public bool isMobilized { get; private set; }
    public bool canBeMobilized { get; private set; }
    [SerializeField]
    private MeshRenderer[] pieceParts;
    [SerializeField]
    private Transform helmet;
    [SerializeField]
    private MeshFilter flagMeshFilter;
    [SerializeField]
    private Mesh[] flagMeshes;

    public override PieceType pieceType => PieceType.Knight;

    public override PiecePlaceType placeType => PiecePlaceType.Crossing;

    public int currentLevel { get; private set; }

    public override void Initialize(int ownerID, Vector2Int codedPos)
    {

        this.codedPos = codedPos;
        pieceOwnerID = ownerID;
        foreach (var renderer in pieceParts)
            renderer.material = materials[PlayerManager.instance.playerColors[ownerID]];
        BoardManager.instance.setPiece(codedPos, this);

        isMobilized = false;
        canBeMobilized = false;

        ChangeLevel(0);
        TurnManager.OnMyTurnStarted += WaitRound;
    }

    public override bool CanIPlaceHere(Vector2Int mapPos)
    {
        SinglePieceController spc = BoardManager.instance.crossings[mapPos]?.currentPiece;
        if (spc == null)
        {
            if (!KnightManager.instance.BelowMaxKnights(GameManager.instance.LocalConnection.ClientId, 0))
                return false;

            return BoardManager.instance.crossings[mapPos].GetRoadsControllers()
            .Select(e => e.GetComponent<RoadController>()?.currentPiece)
            .Where(p => p != null).Where(p=>p.pieceOwnerID == GameManager.instance.LocalConnection.ClientId).Count()>0;
            
        }
        else
        {
            if (spc.pieceOwnerID != GameManager.instance.LocalConnection.ClientId)
                return false;
            if (spc.pieceType != PieceType.Knight)
                return false;
            if ((spc as KnightController).currentLevel >=
                (CommodityUpgradeManager.instance.getUpgradeLevel(GameManager.instance.LocalConnection.ClientId, growthType.Politics) < 3 ? 1 : 2))
                return false;
            if (!KnightManager.instance.BelowMaxKnights(GameManager.instance.LocalConnection.ClientId, (spc as KnightController).currentLevel + 1))
                return false;
        }
        return true;
    }

    private void WaitRound()
    {
        if (TurnManager.currentPhase == Phase.BeforeRoll)
            canBeMobilized = true;
        TurnManager.OnMyTurnStarted -= WaitRound;
    }
    public void ChangeMobilization(bool isMobilizated)
    {
        helmet.DOComplete();
        if (isMobilizated)
        {
            helmet.localPosition = Vector3.up * 0.002f;
            helmet.gameObject.SetActive(true);
            helmet.DOLocalMoveY(0, 0.2f);
            helmet.DOScale(1, 0.2f);
        }
        else
        {
            helmet.DOLocalMoveY(0.002f, 0.2f);
            helmet.DOScale(0, 0.2f).OnComplete(() => { helmet.gameObject.SetActive(false); });
        }
    }
    public void ChangeLevel(int level)
    {
        if (level < 0 && level > 2)
        {
            Debug.LogError("Wrong level value!");
            return;
        }
        flagMeshFilter.mesh = flagMeshes[level];
        currentLevel = level;
    }


}
