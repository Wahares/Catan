using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpecifiedExchangeIconVisual : GeneralExchangeIconVisual
{
    [SerializeField]
    private Image icon;



    public override void setup(ExchangeGiver EG)
    {
        base.setup(EG);
        icon.sprite = EG.tradingOptions[0].icon;
        icon.DOKill();
        icon.DOBlendableColor(new Color(1, 1, 1, 0.25f), 0.2f).SetDelay(5);

    }


    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        icon.DOKill();
        icon.DOBlendableColor(new Color(1, 1, 1, 1), 0.2f);
    }
    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        icon.DOKill();
        icon.DOBlendableColor(new Color(1, 1, 1, 0.25f), 0.2f);
    }

}
