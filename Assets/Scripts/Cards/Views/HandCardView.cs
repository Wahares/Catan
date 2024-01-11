using UnityEngine;
using DG.Tweening;

public class HandCardView : CardView
{
    private PlayerInventoryView piv;
    private void Awake()
    {
        piv = PlayerInventoriesManager.instance.localInventory;
    }
    public override void Initialize(int ID)
    {
        base.Initialize(ID);
    }
    public override void DestroyCard()
    {
        transform.DOComplete();
        transform.DOLocalMoveY(10, 0.1f).SetEase(Ease.OutSine).OnComplete(() => Destroy(gameObject));
    }
    public override void OnClicked()
    {
        SelectDeselect();
    }
    [SerializeField]
    private HandCardEffect hce;
    public void SelectDeselect()
    {
        if (piv.selectedCards.Contains(this))
        {
            hce.selected = false;
            piv.selectedCards.Remove(this);
            transform.DOLocalMoveY(0, 0.5f);
        }
        else
        {
            hce.selected = true;
            piv.selectedCards.Add(this);
            transform.DOLocalMoveY(1, 0.5f);
        }
    }







}
