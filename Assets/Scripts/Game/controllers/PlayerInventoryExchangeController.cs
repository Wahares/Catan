using FishNet.Object;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class PlayerInventoryExchangeController : NetworkBehaviour
{
    public static PlayerInventoryExchangeController instance { get; private set; }

    [SerializeField]
    private GameObject[] buttons;


    private void Awake()
    {
        instance = this;
        currentOption = null;
        TurnManager.OnMyTurnEnded += DisableMenu;
        CursorController.OnRightClicked += DisableMenu;
    }
    private void OnDestroy()
    {
        TurnManager.OnMyTurnEnded -= DisableMenu;
        CursorController.OnRightClicked -= DisableMenu;
    }
    private TradingOption currentOption;


    [SerializeField]
    private List<CardSO> tradingCards;

    public void BeginTransaction(TradingOption option)
    {
        currentOption = option;
        CardChoiceManager.instance.CreateChoice("Exchange for:"
            , tradingCards
            , 1
            , (list) => { finalize(list[0].ID, ObjectDefiner.instance.availableTradings.IndexOf(currentOption), LocalConnection.ClientId); }
            , null
            , null,
            true);
        PlayerCardsOptionsController.isBeingUsed = true;

        /*
        currentOption = option;

        foreach (var button in buttons)
        {
            button.transform.GetChild(0).DOComplete();
            button.transform.GetChild(0).transform.position = transform.parent.position;
            button.transform.GetChild(0).transform.DOLocalMove(Vector3.zero, 0.25f);
            button.SetActive(true);
        }
        */
    }
    public void Clicked(CardSO card)
    {
        if (card.ID == currentOption.materials[0].card.ID)
            return;
        finalize(card.ID, ObjectDefiner.instance.availableTradings.IndexOf(currentOption), LocalConnection.ClientId);
        DisableMenu();
    }
    public void DisableMenu()
    {
        currentOption = null;
        foreach (var button in buttons)
            button.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void finalize(int cardID, int tradingID, int clientID)
    {
        try
        {
            var tradingMat = ObjectDefiner.instance.availableTradings[tradingID].materials[0];
            PlayerInventoriesManager.instance.ChangeCardQuantity(clientID, cardID, 1);
            PlayerInventoriesManager.instance.ChangeCardQuantity(clientID, tradingMat.card.ID, -tradingMat.number);
        }
        catch { Debug.LogError("Error while executing trading option"); }
    }
}
