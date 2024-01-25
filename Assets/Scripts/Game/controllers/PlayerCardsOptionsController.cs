using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCardsOptionsController : MonoBehaviour
{
    private PlayerInventoryView piv;

    [SerializeField]
    private Transform buttonsPivot;
    [SerializeField]
    private GameObject buttonPrefab;

    private void Awake()
    {
        possibleExchanges = new();
        visibleButtons = new();
        selectedCards = new();
    }
    private void Start()
    {
        piv = PlayerInventoriesManager.instance.localInventory;
    }


    private List<RecipedCard> possibleExchanges;

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
            item.Value.transform.DOLocalMove(Vector3.up * i, 0.1f);
        }
    }

    public void FindAllPossibilities()
    {
        possibleExchanges.Clear();
        foreach (var option in ObjectDefiner.instance.availableBuildingRecipes)
        {
            if (option.CanUse(selectedCards, GameManager.instance.LocalConnection.ClientId))
                possibleExchanges.Add(option);
        }
        foreach (var option in ObjectDefiner.instance.availableTradings)
        {
            if (option.CanUse(selectedCards, GameManager.instance.LocalConnection.ClientId))
                possibleExchanges.Add(option);
        }
    }
}
