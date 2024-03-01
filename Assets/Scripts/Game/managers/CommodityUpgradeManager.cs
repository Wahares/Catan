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

    public int[] MetropolyOwnersID { get; private set; }


    private void Awake()
    {
        instance = this;
        GameManager.OnGameStarted += setupPlayersLevels;
        MetropolyOwnersID = new int[3];
        for (int i = 0; i < 3; i++)
            MetropolyOwnersID[i] = -1;
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
        int nextLevel = getUpgradeLevel(nc.ClientId, type) + 1;
        setUpgradeLevel(nc.ClientId, type, nextLevel);
        PlayerInventoriesManager.instance.ChangeCardQuantity(nc.ClientId, materialID, -nextLevel);


        if (nextLevel == 4 && getMetropolyOwnerID(type) == -1)
            GiveMetropolyToPlayer(nc.ClientId, type);
        else
            if (nextLevel == 5 && getUpgradeLevel(getMetropolyOwnerID(type), type) == 4)
        {
            RevokeMetropolyToPlayer(nc.ClientId, type);
            GiveMetropolyToPlayer(nc.ClientId, type);
        }


    }
    public int getMetropolyOwnerID(growthType type) => MetropolyOwnersID[(int)type - 1];
    
    private void setMetropolyOwnerID(growthType type, int clientID) => MetropolyOwnersID[(int)type - 1] = clientID;

    [Server]
    private void GiveMetropolyToPlayer(int clientID, growthType type)
    {
        TurnManager.instance.EnqueuePhase(Phase.ManagingMetropoly, TurnManager.instance.orderOfPlayer(clientID), (int)type, 10, true);
        TurnManager.instance.EnqueuePhase(Phase.CasualRound, TurnManager.instance.orderOfPlayer(clientID), TurnManager.instance.timer, true);
        TurnManager.instance.ForceEndTurn();
    }
    [Server]
    private void RevokeMetropolyToPlayer(int clientID, growthType type)
    {
        TurnManager.instance.EnqueuePhase(Phase.ManagingMetropoly, TurnManager.instance.orderOfPlayer(clientID), -(int)type, 10, true);
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


        if (level == 4 && getMetropolyOwnerID(type) == -1)
            setMetropolyOwnerID(type, clientID);
        else
            if (level == 5 && getUpgradeLevel(getMetropolyOwnerID(type), type) == 4)
            setMetropolyOwnerID(type, clientID);


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