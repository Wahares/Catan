using DG.Tweening;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventoriesManager : NetworkBehaviour
{
    public static PlayerInventoriesManager instance;


    public Dictionary<int, int[]> playerInventories = new();

    public List<CardSO> availableCards => ObjectDefiner.instance.equipableCards;

    [field: SerializeField]
    public PlayerInventoryView localInventory { get; private set; }


    private Dictionary<SpecialCard, int> numberOfSpecialCardsLeft = new();
    private void Awake()
    {
        instance = this;
        GameManager.OnGameStarted += setupPlayerInventories;
        GameManager.OnGameStarted += setupInventoriesVisuals;
        if (InstanceFinder.NetworkManager.IsServer)
            foreach (var card in ObjectDefiner.instance.equipableCards)
            {
                if (card.CardType != cardType.Special)
                    continue;
                if (card is not SpecialCard)
                    continue;
                numberOfSpecialCardsLeft.Add(card as SpecialCard, (card as SpecialCard).numberInDeck);
            }
    }
    private void setupPlayerInventories()
    {
        foreach (var client in InstanceFinder.ClientManager.Clients)
            playerInventories.Add(client.Value.ClientId, new int[availableCards.Count]);
    }

    public Dictionary<int, int> getPlayersCardsInHand(int clientID)
    {
        int[] inventory = instance.playerInventories[clientID];
        Dictionary<int, int> cardsInHand = new();
        for (int i = 0; i < inventory.Length; i++)
        {
            if (ObjectDefiner.instance.equipableCards[i] is NormalCard || ObjectDefiner.instance.equipableCards[i] is CommodityCard)
                cardsInHand.Add(i, inventory[i]);
        }
        return cardsInHand;
    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangeMyCardsQuantity(int cardID, int delta, NetworkConnection nc = null)
    {
        ChangeCardQuantity(nc.ClientId, cardID, delta);
    }


    [Server]
    public void ChangeCardQuantity(int clientID, int cardID, int delta)
    {
        ChangeCardQuantityRPC(clientID, cardID, delta);
    }

    [ObserversRpc]
    private void ChangeCardQuantityRPC(int clientID, int cardID, int delta)
    {
        playerInventories[clientID][cardID] += delta;
        if (LocalConnection.ClientId == clientID)
            ChangeMyCards(cardID, delta);
        else
            ChangeSomeonesCards(clientID, cardID, delta);
    }


    private void ChangeMyCards(int cardID, int delta)
    {
        for (int i = 0; i < Mathf.Abs(delta); i++)
        {
            if (delta < 0)
                localInventory.RemoveCard(cardID);
            else
                localInventory.AddCard(cardID);
        }
    }
    private void ChangeSomeonesCards(int clientID, int cardID, int delta)
    {
        PlayerInventoryView view = GetComponent<PlayerAvatarsController>().playerAvatars[clientID].inventoryView;
        for (int i = 0; i < Mathf.Abs(delta); i++)
        {
            if (delta < 0)
                view.RemoveCard(cardID);
            else
                view.AddCard(cardID);
        }
    }

    private void setupInventoriesVisuals()
    {
        foreach (var avatar in PlayerManager.instance.avatars.playerAvatars)
        {
            if (avatar.Key == LocalConnection.ClientId)
                localInventory.GetComponent<PlayerInventoryView>().initialize(true);
            else
                avatar.Value.inventoryView.initialize(false);
        }
    }




    public void RollMeSpecialCard(growthType type)
    {
        RollSpecialCardToPlayer(type);
    }
    [ServerRpc(RequireOwnership = false)]
    private void RollSpecialCardToPlayer(growthType type, NetworkConnection nc = null)
    {
        List<SpecialCard> cardsLeft = numberOfSpecialCardsLeft
            .Where(e => e.Key.sourceType == type)
            .SelectMany(e => Enumerable.Repeat(e.Key, e.Value)).ToList();
        SpecialCard card = null;
        if (cardsLeft.Count > 0)
        {
            card = cardsLeft[Random.Range(0, cardsLeft.Count)];
            numberOfSpecialCardsLeft[card]--;
            ChangeCardQuantity(nc.ClientId, card.ID, 1);
        }
        TurnManager.instance.ForceEndTurn();
    }

    public int playerNumberOfSpecialCards(int clientID)
    {
        int[] inventory = playerInventories[clientID];
        int number = 0;
        for (int i = 0; i < inventory.Length; i++)
            if (ObjectDefiner.instance.equipableCards[i].CardViewType == cardViewType.Special)
                number += inventory[i];
        return number;
    }
    public List<CardSO> playerSpecialCards(int clientID)
    {
        int[] inventory = playerInventories[clientID];
        List<CardSO> cards = new();
        for (int i = 0; i < inventory.Length; i++)
            if (ObjectDefiner.instance.equipableCards[i].CardViewType == cardViewType.Special)
                for (int j = 0; j < inventory[i]; j++)
                    cards.Add(ObjectDefiner.instance.equipableCards[i]);
        return cards;
    }
    public void SpecialCardUsed(SpecialCard card)
    {
        numberOfSpecialCardsLeft[card]++;
    }
    [ServerRpc(RequireOwnership = false)]
    public void destroyMySpecialCard(int ID, NetworkConnection nc = null)
    {
        ChangeCardQuantity(nc.ClientId, ID, -1);
    }
    public void removeRandomSpecial(int clientID, growthType type)
    {

    }
    public void giveRandomSpecial(int clientID, growthType type)
    {

    }


    public int clientID, cardID, delta;

    [ContextMenu("change cards")]
    private void chaaaaa()
    {
        if (!GameManager.started)
            return;
        ChangeCardQuantity(clientID, cardID, delta);
    }


    [SerializeField]
    private GameObject flyingCardPrefab;
    public GameObject createFlyingCard(bool hidden, int cardID)
    {
        GameObject obj = Instantiate(flyingCardPrefab);
        obj.GetComponent<FlyingCardView>().Initialize(cardID);
        if (hidden)
        {
            growthType gt = (ObjectDefiner.instance.equipableCards[cardID] as SpecialCard)?.sourceType ?? growthType.Blank;
            obj.GetComponent<FlyingCardView>().hideTexture(gt);
        }
        return obj;
    }

    [SerializeField]
    private Transform specialCardEffectPivot;
    [ServerRpc(RequireOwnership = false)]
    public void SpecialCardUseEffect(int cardID)
    {
        if (cardID == -1)
            Debug.LogError("Tried to do effect with null card");
        else
            SpecialCardUseEffectRPC(cardID);
    }
    [ObserversRpc]
    public void SpecialCardUseEffectRPC(int cardID)
    {
        GameObject obj = createFlyingCard(false, cardID);
        obj.transform.SetParent(specialCardEffectPivot, false);
        obj.transform.forward = specialCardEffectPivot.forward;

        specialCardEffectPivot.DOComplete();
        specialCardEffectPivot.transform.localPosition = new Vector3(4, 0, 1);
        specialCardEffectPivot.transform.DOLocalMove(Vector3.zero, 2).SetEase(Ease.OutSine);
        specialCardEffectPivot.transform.DOLocalMove(new Vector3(4, 0, 1), 1).SetEase(Ease.InSine).SetDelay(4).OnComplete(() => { Destroy(obj); });
    }


}
