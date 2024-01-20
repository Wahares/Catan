using UnityEngine;

public abstract class SinglePieceController : MonoBehaviour
{
    public int pieceOwnerID;
    public Vector2Int codedPos;
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
}
public enum PieceType
{
    Unset,
    Road,
    Settlement,
    City,
    Knight
}