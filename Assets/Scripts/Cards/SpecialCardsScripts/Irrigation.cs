
using UnityEngine;
using System.Linq;

public class Irrigation : SpecialCard
{
    [SerializeField]
    private NormalCard card;
    public override void OnUsed()
    {
        int num = 0;
        foreach (var tile in BoardManager.instance.Tiles)
        {
            if (tile.Value.type != card.sourceTile)
                continue;
            if (tile.Value.getNearbyCrossings()
                .Where(e => e.currentPiece != null)
                .Where(e => e.currentPiece.pieceOwnerID == GameManager.instance.LocalConnection.ClientId)
                .Where(e => (e.currentPiece.pieceType == PieceType.City || e.currentPiece.pieceType == PieceType.Settlement)).Count() > 0)
                num++;
        }

        PlayerInventoriesManager.instance.ChangeMyCardsQuantity(card.ID, num);

        PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
    }
}
