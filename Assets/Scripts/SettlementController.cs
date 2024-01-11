using DG.Tweening;
using UnityEngine;

public class SettlementController : SinglePieceController
{
    public override PieceType pieceType => PieceType.Settlement;
    
    public virtual void TileRolled(TileController tc)
    {
        NormalCard card = null;
        foreach (var item in PlayerInventoriesManager.instance.availableCards)
        {
            if ((item as NormalCard)?.sourceTile == tc.type)
            {
                card = item as NormalCard;
                break;
            }
            Debug.LogError("Unable to find card for: " + tc.type);
            return;
        }

        GameObject obj = PlayerInventoriesManager.instance.createFlyingCard(false, card.ID);

        obj.transform.localScale = Vector3.zero;
        obj.transform.position = tc.transform.position;
        obj.transform.DOScale(Vector3.one, 0.1f);
        obj.transform.DOJump(transform.position, 0.5f, 1, 0.25f);
        obj.transform.DOScale(Vector3.one, 0.1f).SetDelay(0.2f).OnComplete(()=>Destroy(obj));

    }

}
