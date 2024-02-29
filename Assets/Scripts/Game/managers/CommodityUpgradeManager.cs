using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class CommodityUpgradeManager : NetworkBehaviour
{
    public static CommodityUpgradeManager instance;

    public Dictionary<int, int> PlayersLevels = new();

    [SerializeField]
    private CommodityUpgradeView localCUV;


    private void Awake()
    {
        instance = this;
        GameManager.OnGameStarted += setupPlayersLevels;
    }
    private void OnDestroy()
    {
        instance = null;
        GameManager.OnGameStarted -= setupPlayersLevels;
    }
    private void setupPlayersLevels()
    {
        foreach (var player in ClientManager.Clients.Keys)
            PlayersLevels[player] = 0;
    }
    public void Upgrade(growthType type, int materialID)
    {
        if (type == growthType.Blank)
        {
            Debug.LogError("Wrong type of growth!");
            return;
        }
        UpgradeOnServer(type, materialID);
    }
    [ServerRpc(RequireOwnership = false)]
    private void UpgradeOnServer(growthType type, int materialID, NetworkConnection nc = null)
    {
        setUpgradeLevel(nc.ClientId, type, getUpgradeLevel(nc.ClientId, type) + 1);
        PlayerInventoriesManager.instance.ChangeCardQuantity(nc.ClientId, materialID, -getUpgradeLevel(nc.ClientId, type)-1);
    }


    [ObserversRpc]
    private void setUpgradeLevel(int clientID, growthType type, int level)
    {
        if (type == growthType.Blank)
        {
            Debug.LogError("Wrong type of growth!");
            return;
        }
        PlayersLevels[clientID] = PlayersLevels[clientID] & ((~0) ^ (255 << (8 * (int)type - 8)));
        PlayersLevels[clientID] = PlayersLevels[clientID] | (level << (8 * (int)type - 8));
        if (clientID == LocalConnection.ClientId)
            localCUV.setValue(type, level);
        else
            PlayerManager.instance.avatars.playerAvatars[clientID].upgradeView.setValue(type, level);
    }
    public int getUpgradeLevel(int clientID, growthType type)
    {
        if (type == growthType.Blank)
        {
            Debug.LogError("Wrong type of growth!");
            return 0;
        }
        return (PlayersLevels[clientID] >> (8 * (int)type - 8)) & 255;
    }


}