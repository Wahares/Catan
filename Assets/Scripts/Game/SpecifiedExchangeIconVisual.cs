using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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
        icon.DOBlendableColor(new Color(1, 1, 1, 0.5f), 1).SetDelay(5);

    }


    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        icon.DOKill();
        icon.DOBlendableColor(new Color(1, 1, 1, 1), 1);
    }
    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        icon.DOKill();
        icon.DOBlendableColor(new Color(1, 1, 1, 0.5f), 1);
    }

}
