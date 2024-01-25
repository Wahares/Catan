using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoriesManager : NetworkBehaviour
{
    public static PlayerInventoriesManager instance;


    public Dictionary<int, int[]> playerInventories = new();

    public List<CardSO> availableCards => ObjectDefiner.instance.equipableCards;

    [field: SerializeField]
    public PlayerInventoryView localInventory { get; private set; }


    private void Awake()
    {
        instance = this;
        if (InstanceFinder.NetworkManager.IsServer)
            GameManager.OnGameStarted += setupPlayerInventories;
        GameManager.OnGameStarted += setupInventoriesVisuals;
    }
    [Server]
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
                cardsInHand.Add(i,inventory[i]);
        }
        return cardsInHand;
    }



    [Server]
    public void ChangeCardQuantity(int clientID, int cardType, int delta)
    {
        ChangeCardQuantityRPC(clientID, cardType, delta);
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
        return obj;
    }


}
