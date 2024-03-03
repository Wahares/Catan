using System.Collections.Generic;
using UnityEngine;

public class TraderExchangeGiver : ExchangeGiver
{
    private TraderController controller;
    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<TraderController>();
    }

    protected override bool CanGive(int clientID)
    {
        return controller.currentOwner == clientID && controller.currentPos != new Vector2Int(-1, -1);
    }
    public override void TryToGiveOption(ref HashSet<RecipedCard> set, int clientID, List<CardSO> selectedCards)
    {
        if (controller.currentOwner != clientID)
            return;
        if (controller.currentPos == new Vector2Int(-1, -1))
            return;
        foreach (PortTradingOption option in tradingOptions)
        {
            if (option.materials[0].card is not NormalCard)
                continue;
            if ((option.materials[0].card as NormalCard).sourceTile != BoardManager.instance.Tiles[controller.currentPos].type)
                continue;
            set.Add(option);
            break;
        }
    }
}
