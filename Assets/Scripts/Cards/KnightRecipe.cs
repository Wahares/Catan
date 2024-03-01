using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightRecipe : BuildingRecipe
{
    public override bool CanUse(List<CardSO> cards, int clientID)
    {
        return base.CanUse(cards, clientID);
    }
}
