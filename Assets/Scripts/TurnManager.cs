using FishNet;
using UnityEngine;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;

public class TurnManager : NetworkBehaviour
{
    public static int TIME_LIMIT;
    public static bool DO_LIMIT_TURN;

    public static int currentTurn;
    public static TurnManager instance;

    public static NetworkConnection[] turnOrder;

    public int myOrder;

    private void Awake()
    {
        instance = this;
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        BoardManager.OnBoardInitialized += randomizePlayers;
    }
    [Server]
    private void randomizePlayers()
    {
        List<NetworkConnection> tmp = new();
        List<NetworkConnection> connectionsOrder = new();
        foreach (var conn in InstanceFinder.ServerManager.Clients)
            tmp.Add(conn.Value);

        for (int i = 0; i < InstanceFinder.ServerManager.Clients.Count; i++)
        {
            int rand = Random.Range(0, tmp.Count);
            connectionsOrder.Add(tmp[rand]);
            tmp.RemoveAt(rand);
        }
        setOrder(connectionsOrder.ToArray());
    }
    [ObserversRpc]
    private void setOrder(NetworkConnection[] orders)
    {
        turnOrder = orders;
        for (int i = 0; i < orders.Length; i++)
        {
            if (orders[i].Equals(LocalConnection))
            {
                myOrder = i;
                break;
            }
            if (i == orders.Length - 1)
                Debug.LogError("Couldn't find my order!");
        }
    }

    public void endTurn()
    {
        if (currentTurn % turnOrder.Length != myOrder)
            return;
        calcNewTurn();
    }
    [ServerRpc(RequireOwnership = false)]
    public void calcNewTurn()
    {
        for (int i = 0; i < turnOrder.Length; i++)
        {
            int tmp = currentTurn + i + 1;
            if (PlayerManager.playerAvailable(turnOrder[tmp % turnOrder.Length].ClientId))
            {
                currentTurn = currentTurn + tmp;
                startNewTurn(currentTurn);
            }
        }
    }

    [ObserversRpc]
    private void startNewTurn(int turnNumber)
    {
        currentTurn = turnNumber;
        Debug.Log($"It's {PlayerManager.instance.playerSteamIDs[turnOrder[currentTurn % turnOrder.Length].ClientId]}'s turn");
    }





}