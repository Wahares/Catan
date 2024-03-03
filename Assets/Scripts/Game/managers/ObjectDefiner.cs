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

    public Dictionary<TileType, PortTradingOption[]> PortTradingOptions;

    private void Awake()
    {
        instance = this;
        PortTradingOptions = new();
        foreach (var set in portTradings)
            PortTradingOptions.Add(set.type, set.tradings);
    }
    [SerializeField]
    private pair[] portTradings;

    [System.Serializable]
    class pair { public TileType type; public PortTradingOption[] tradings; }

    [field: SerializeField]
    public List<CardSO> basicCards { get; private set; }

}
