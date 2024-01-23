using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesContainer : ScriptableObject
{
    [field: SerializeField]
    public List<CardSO> equipableCards { get; private set; }
    [field: SerializeField]
    public List<BuildingRecipe> buildingRecipes { get; private set; }
    [field: SerializeField]
    public List<TradingOption> tradingRecipes { get; private set; }
}
