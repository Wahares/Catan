using System.Collections.Generic;
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

    public override void OnClicked()
    {
        if ((item as SpecialCard).CanUse())
        {
            CardChoiceManager.instance.CreateChoice("", new List<CardSO>() { item }, 0
                , (e) => { (item as SpecialCard).OnUsed(); }
                , null, null, true);
            PlayerCardsOptionsController.isBeingUsed = true;
        }

    }
    public override void DestroyCard()
    {
        Destroy(gameObject);
    }



}
