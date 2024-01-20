using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoriesManager : NetworkBehaviour
{
    public static PlayerInventoriesManager instance;


    public Dictionary<int, int[]> playerInventories = new();

    [field: SerializeField]
    public List<CardSO> availableCards { get; private set; }

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
    [Server]
    private void ChangeCardQuantity(int clientID, int cardType, int delta)
    {
        ChangeCardQuantityRPC(clientID, cardType, delta);
    }

    [ObserversRpc]
    private void ChangeCardQuantityRPC(int clientID, int cardID, int delta)
    {
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