using System.Collections.Generic;
using UnityEngine;

public class ObjectDefiner : MonoBehaviour
{
    public static ObjectDefiner instance { get; private set; }

    [SerializeField]
    private ResourcesContainer container;


    public List<CardSO> equipableCards => container.equipableCards;

    public List<BuildingRecipe> availableBuildingRecipes => container.buildingRecipes;
    public List<TradingOption> availableTradings => container.tradingRecipes;


    private void Awake()
    {
        instance = this;
    }




}
