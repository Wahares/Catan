using System.Collections.Generic;
using UnityEngine;

public abstract class ExchangeGiver : MonoBehaviour
{
    public PortTradingOption[] tradingOptions;

    protected virtual void Awake()
    {
        FindObjectOfType<PlayerCardsOptionsController>().RegisterExchangeGiver(this);
    }

    protected abstract bool CanGive(int clientID);

    public void TryToGiveOption(ref HashSet<RecipedCard> set,int clientID)
    {
        if (CanGive(clientID))
            foreach (var option in tradingOptions)
                set.Add(option);
    }
}
