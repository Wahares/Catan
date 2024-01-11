using UnityEngine;

public class FlyingCardView : CardView
{

    [SerializeField]
    private Material blankBasic,blankTrade, blankPolitics, blankScience;

    public void hideTexture(growthType type)
    {
        switch (type)
        {
            case growthType.Blank:
                render.material = blankBasic;
                break;
            case growthType.Trade:
                render.material = blankTrade;
                break;
            case growthType.Politics:
                render.material = blankPolitics;
                break;
            case growthType.Science:
                render.material = blankScience;
                break;
        }
    }
}
