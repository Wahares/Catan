using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradingFleet : SpecialCard
{
    [SerializeField]
    private TradingOption[] options;
    public override void OnUsed()
    {
        List<CardSO> cards = options.Select(card => card.materials[0].card).ToList();

        CardChoiceManager.instance.CreateChoice("Choose trade:", cards, 1, (e) =>
        {
            foreach (var option in options)
                if (option.materials[0].card == e[0])
                {
                    PlayerCardsOptionsController.instance.temporaryTradingsForRound.Add(option);
                    PlayerInventoriesManager.instance.SpecialCardUseEffect(ID);
                    break;
                }
        }, null, null, true);
    }
}
