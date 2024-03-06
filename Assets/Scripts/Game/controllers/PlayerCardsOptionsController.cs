using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCardsOptionsController : MonoBehaviour
{
    public static PlayerCardsOptionsController instance { get; private set; }
    private PlayerInventoryView piv;

    [SerializeField]
    private Transform buttonsPivot;
    [SerializeField]
    private GameObject buttonPrefab;

    private HashSet<ExchangeGiver> registeredGivers = new();

    public static bool isBeingUsed = false;

    private void Awake()
    {
        instance = this;
        possibleExchanges = new();
        visibleButtons = new();
        selectedCards = new();
        TurnManager.OnMyTurnStarted += ResetTemporaryOptions;
    }
    private void Start()
    {
        piv = PlayerInventoriesManager.instance.localInventory;
    }
    private void OnDestroy()
    {
        instance = null;
        TurnManager.OnMyTurnStarted -= ResetTemporaryOptions;
    }
    public void RegisterExchangeGiver(ExchangeGiver EG)
    {
        registeredGivers.Add(EG);
    }

    private HashSet<RecipedCard> possibleExchanges;

    private List<CardSO> selectedCards;

    private Dictionary<RecipedCard, CardExchangeButton> visibleButtons;

    public void OnSelectedChanged()
    {
        selectedCards = piv.selectedCardsViews.Select(card => card.item).ToList();

        FindAllPossibilities();

        List<RecipedCard> toDelete = new();
        foreach (var button in visibleButtons)
        {
            if (!button.Key.CanUse(selectedCards, GameManager.instance.LocalConnection.ClientId))
                toDelete.Add(button.Key);
        }
        foreach (var obj in toDelete)
        {
            Destroy(visibleButtons[obj].gameObject);
            visibleButtons.Remove(obj);
        }
        foreach (var exchange in possibleExchanges)
        {
            if (visibleButtons.ContainsKey(exchange))
                continue;
            GameObject go = Instantiate(buttonPrefab, buttonsPivot);
            go.GetComponent<CardExchangeButton>().Initialize(exchange, piv);
            visibleButtons.Add(exchange, go.GetComponent<CardExchangeButton>());
        }
        int i = 0;
        foreach (var item in visibleButtons)
        {
            item.Value.transform.DOComplete();
            item.Value.transform.DOLocalMove(Vector3.up * 0.5f * i++, 0.1f);
        }
    }

    public List<RecipedCard> temporaryTradingsForRound = new();
    public void FindAllPossibilities()
    {
        possibleExchanges.Clear();
        if (TurnManager.currentPhase != Phase.CasualRound || !TurnManager.isMyTurn)
            return;
        foreach (var option in ObjectDefiner.instance.availableBuildingRecipes)
        {
            if (option.CanUse(selectedCards, GameManager.instance.LocalConnection.ClientId))
                possibleExchanges.Add(option);
        }
        foreach (var option in ObjectDefiner.instance.availableTradings)
        {
            if (option is PortTradingOption)
                continue;
            if (option.CanUse(selectedCards, GameManager.instance.LocalConnection.ClientId))
                possibleExchanges.Add(option);
        }
        foreach (var giver in registeredGivers)
            giver.TryToGiveOption(ref possibleExchanges, GameManager.instance.LocalConnection.ClientId, selectedCards);
        foreach (var option in temporaryTradingsForRound)
            if (!possibleExchanges.Contains(option))
                possibleExchanges.Add(option);
    }
    private void ResetTemporaryOptions()
    {
        if (!TurnManager.isMyTurn)
            return;
        if (TurnManager.currentPhase != Phase.BeforeRoll)
            return;
        temporaryTradingsForRound.Clear();
    }
}
