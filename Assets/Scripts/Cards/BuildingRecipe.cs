using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingRecipe", menuName = "Cards/Building Recipe")]
public class BuildingRecipe : RecipedCard
{
    public GameObject piece, piecePreview;
    public int maxOnBoard;
    public override bool CanUse(List<CardSO> cards, int clientID)
    {
        PieceType pieceType = piece.GetComponent<SinglePieceController>().pieceType;
        return BoardManager.instance.numberOfPieces(clientID,pieceType)<maxOnBoard && base.CanUse(cards,clientID);
    }
    public override void OnUsed()
    {
        BuildingManager.instance.BeginBuilding(this);
    }
    protected bool BaseCanUse(List<CardSO> cards, int clientID) => base.CanUse(cards, clientID);
}