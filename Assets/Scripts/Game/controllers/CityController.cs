using DG.Tweening;
using UnityEngine;

public class CityController : SettlementController
{
    public override PieceType pieceType => PieceType.City;
    public override void OnTileInvoked(TileController tc)
    {
        base.OnTileInvoked(tc);




        CommodityCard card = null;
        foreach (var item in PlayerInventoriesManager.instance.availableCards)
        {
            if ((item as CommodityCard)?.sourceTile == tc.type)
            {
                card = item as CommodityCard;
                break;
            }
        }
        if (card == null)
        {
            base.OnTileInvoked(tc);
            return;
        }

        GameObject obj = PlayerInventoriesManager.instance.createFlyingCard(false, card.ID);

        obj.transform.localScale = Vector3.zero;
        obj.transform.position = tc.transform.position;
        obj.transform.DOScale(Vector3.one, 0.1f);
        obj.transform.DOJump(transform.position, 0.5f, 1, 0.25f);
        obj.transform.DOScale(Vector3.one, 0.1f).SetDelay(0.2f).OnComplete(() => Destroy(obj));

    }
}
