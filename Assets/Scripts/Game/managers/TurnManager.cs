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

    public static int[] turnOrder;

    public int myOrder;

    private void Awake()
    {
        instance = this;
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        BoardManager.OnBoardInitialized += randomizePlayers;
        BoardManager.OnBoardInitialized += startPlacingPhase;
        PlayerManager.OnPlayerDisconnected += playerWithTurnDisconnected;
    }
    [Server]
    private void randomizePlayers()
    {
        List<int> tmp = new();
        List<int> connectionsOrder = new();
        foreach (var conn in InstanceFinder.ServerManager.Clients)
            tmp.Add(conn.Value.ClientId);

        for (int i = 0; i < InstanceFinder.ServerManager.Clients.Count; i++)
        {
            int rand = Random.Range(0, tmp.Count);
            connectionsOrder.Add(tmp[rand]);
            tmp.RemoveAt(rand);
        }
        setOrder(connectionsOrder.ToArray());
    }
    [ObserversRpc]
    private void setOrder(int[] orders)
    {
        turnOrder = orders;
        for (int i = 0; i < orders.Length; i++)
        {
            if (orders[i] == LocalConnection.ClientId)
            {
                myOrder = i;
                break;
            }
            if (i == orders.Length - 1)
                Debug.LogError("Couldn't find my order!");
        }
    }
    [Server]
    private void startPlacingPhase()
    {

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
            if (PlayerManager.playerAvailable(turnOrder[tmp % turnOrder.Length]))
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
        Debug.Log($"It's {PlayerManager.instance.playerSteamIDs[turnOrder[currentTurn % turnOrder.Length]]}'s turn");
    }
    [Server]
    private void playerWithTurnDisconnected(int clientID)
    {
        if (GameManager.started)
            if (clientID == turnOrder[currentTurn % turnOrder.Length])
                calcNewTurn();
    }




}