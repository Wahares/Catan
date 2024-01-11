using DG.Tweening;

public class HandCardView : CardView
{
    public override void Initialize(int ID)
    {
        base.Initialize(ID);
    }
    public override void DestroyCard()
    {
        transform.DOComplete();
        transform.DOLocalMoveY(10,0.1f).SetEase(Ease.OutSine).OnComplete(()=>Destroy(gameObject));
    }
}
