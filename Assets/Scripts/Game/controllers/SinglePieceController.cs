using UnityEngine;

public abstract class SinglePieceController : MonoBehaviour
{
    public int pieceOwnerID { get; protected set; }
    public Vector2Int codedPos { get; protected set; }
    [SerializeField]
    private Material[] materials;
    [SerializeField]
    protected MeshRenderer render;
    public abstract PieceType pieceType { get; }
    public virtual void Initialize(int ownerID,Vector2Int codedPos)
    {
        this.codedPos = codedPos;
        pieceOwnerID = ownerID;
        render.material = new Material(materials[PlayerManager.instance.playerColors[ownerID]]);
    }
    public virtual void OnTileInvoked(TileController source) { }
}
public enum PieceType
{
    Unset,
    Road,
    Settlement,
    City,
    Knight
}