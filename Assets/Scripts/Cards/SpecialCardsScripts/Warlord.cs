using System.Linq;

public class Warlord : SpecialCard
{
    public override void OnUsed()
    {

        BoardManager.instance.crossings
            .Where(e => e.Value.currentPiece != null)
            .Where(e => e.Value.currentPiece.pieceOwnerID == GameManager.instance.LocalConnection.ClientId)
            .Where(e => e.Value.currentPiece.pieceType == PieceType.Knight)
            .ToList().ForEach(e => KnightManager.instance.changeMobilization(e.Key, true));


        PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
    }
}
