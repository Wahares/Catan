using UnityEngine;

[CreateAssetMenu(fileName = "BuildingRecipe", menuName = "Cards/Building Recipe")]
public class BuildingRecipe : RecipedCard
{
    public GameObject piece, piecePreview;
    public int maxOnBoard;
    public override bool CanUse(int[] inventory, int clientID)
    {
        PieceType pieceType = piece.GetComponent<SinglePieceController>().pieceType;
        return BoardManager.instance.numberOfPieces(clientID,pieceType)<maxOnBoard && base.CanUse(inventory,clientID);
    }
    public virtual void Use()
    {

    }
}