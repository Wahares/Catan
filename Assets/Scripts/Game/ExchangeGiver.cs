using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ExchangeGiver : MonoBehaviour
{
    public PortTradingOption[] tradingOptions;

    protected virtual void Awake()
    {
        FindAnyObjectByType<PlayerCardsOptionsController>().RegisterExchangeGiver(this);
    }

    protected abstract bool CanGive(int clientID);

    public virtual void TryToGiveOption(ref HashSet<RecipedCard> set, int clientID, List<CardSO> selectedCards)
    {
        if (CanGive(clientID))
            foreach (var option in tradingOptions)
            {
                Dictionary<int, int> remaining = option.materials.ToDictionary(m => m.card.ID, m => m.number);
                bool success = true;
                foreach (var mat in selectedCards)
                {
                    if (remaining.ContainsKey(mat.ID))
                        remaining[mat.ID]--;
                }
                foreach (var entry in remaining)
                {
                    if (entry.Value > 0)
                        success = false;
                }
                Debug.Log("hejo");
                if (success)
                    set.Add(option);
            }
    }
}
