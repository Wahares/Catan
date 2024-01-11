using TMPro;
using UnityEngine;

public class SpecialCardView : CardView
{
    [SerializeField]
    protected TextMeshPro nameText;
    [SerializeField]
    private TextMeshPro descText;
    public override void Initialize(int ID)
    {
        base.Initialize(ID);
        nameText.text = ((SpecialCard)item).cardName;
        descText.text = ((SpecialCard)item).description;
    }
}
