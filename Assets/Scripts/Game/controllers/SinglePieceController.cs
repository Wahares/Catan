using UnityEngine;

public abstract class SinglePieceController : MonoBehaviour
{
    public int pieceOwnerID { get; protected set; }
    public Vector2Int codedPos { get; protected set; }
    [SerializeField]
    protected Material[] materials;
    [SerializeField]
    protected MeshRenderer render;
    public abstract PieceType pieceType { get; }
    public abstract PiecePlaceType placeType { get; }
    public virtual void Initialize(int ownerID, Vector2Int codedPos)
    {
        this.codedPos = codedPos;
        pieceOwnerID = ownerID;
        render.material = materials[PlayerManager.instance.playerColors[ownerID]];
        BoardManager.instance.setPiece(codedPos, this);
    }
    public virtual void OnTileInvoked(TileController source) { }
    public abstract bool CanIPlaceHere(Vector2Int mapPos);
}
public enum PieceType
{
    Unset,
    Road,
    Settlement,
    City,
    Knight
}