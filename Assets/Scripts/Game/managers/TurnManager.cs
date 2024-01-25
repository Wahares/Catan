using FishNet;
using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using System;
using FishNet.Connection;

public class TurnManager : NetworkBehaviour
{
    public static int TIME_LIMIT;
    public static bool DO_LIMIT_TURN;

    public static int currentTurn;
    public static TurnManager instance;

    public static int[] turnOrder;

    public int myOrder;

    public static Action<Phase> OnPhaseChanged;

    public static Action<int> OnAnyTurnStarted;
    public static Action OnMyTurnStarted;
    public static Action OnMyTurnEnded;


    public static bool isMyTurn => currentTurn % turnOrder.Length == instance.myOrder;

    public static Phase currentPhase { get; private set; }


    private void Awake()
    {
        instance = this;
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        BoardManager.OnBoardInitialized += randomizePlayers;
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
            int rand = UnityEngine.Random.Range(0, tmp.Count);
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
        if (InstanceFinder.NetworkManager.IsServer)
            startPlacingPhase();
    }
    [Server]
    private void startPlacingPhase()
    {
        currentTurn = -1;
        EnqueuePhase(Phase.PlacingVillages, PlayerManager.instance.playerColors.Count*2);
        calcNewTurn();
    }
    [Server]
    public void changePhaseOnServer(Phase newPhase)
    {
        changePhase(newPhase);
    }

    [ObserversRpc]
    private void changePhase(Phase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log("Phase changed to: " + newPhase);
        OnPhaseChanged?.Invoke(newPhase);
    }


    public void endTurn()
    {
        if (!isMyTurn)
            return;
        calcNewTurn();
        OnMyTurnEnded?.Invoke();
    }
    [Server]
    public void ForceEndTurn()
    {
        foreach (var player in ServerManager.Clients)
        {
            if (turnOrder[currentTurn % turnOrder.Length] == player.Key)
            {
                EndMyTurn(player.Value);
                break;
            }
        }
    }
    [TargetRpc]
    private void EndMyTurn(NetworkConnection nc)
    {
        endTurn();
    }





    [Serializable]
    public class enPh { public Phase phase; public int ttl; }
    public List<enPh> enqueuedPhases = new();
    public void EnqueuePhase(Phase ph, int ttl)
    {
        enqueuedPhases.Add(new enPh() { phase = ph, ttl = ttl });
    }
    [ServerRpc(RequireOwnership = false)]
    public void calcNewTurn()
    {
        for (int i = 0; i < turnOrder.Length; i++)
        {
            int tmp = currentTurn + 1 + i;
            if (PlayerManager.playerAvailable(turnOrder[tmp % turnOrder.Length]))
            {
                if (enqueuedPhases.Count > 0)
                {
                    int ttmp = 1 + i;
                    while (ttmp > 0 && enqueuedPhases.Count != 0)
                    {
                        if (ttmp > enqueuedPhases[0].ttl)
                        {
                            ttmp = ttmp - enqueuedPhases[0].ttl;
                            enqueuedPhases.RemoveAt(0);
                        }
                        else
                        {
                            enqueuedPhases[0].ttl -= ttmp;
                            ttmp = 0;
                        }
                    }
                }

                if (enqueuedPhases.Count == 0)
                {
                    if (currentPhase != Phase.BeforeRoll)
                        changePhase(Phase.BeforeRoll);
                }
                else
                if (currentPhase != enqueuedPhases[0].phase)
                    changePhase(enqueuedPhases[0].phase);

                currentTurn = tmp;
                startNewTurn(currentTurn);
                break;
            }
        }
    }

    [ObserversRpc]
    private void startNewTurn(int turnNumber)
    {
        currentTurn = turnNumber;
        Debug.Log($"It's {PlayerManager.instance.playerSteamIDs[turnOrder[currentTurn % turnOrder.Length]]}'s turn");
        OnAnyTurnStarted?.Invoke(turnOrder[currentTurn % turnOrder.Length]);
        if (isMyTurn)
            OnMyTurnStarted?.Invoke();
    }
    [Server]
    private void playerWithTurnDisconnected(int clientID)
    {
        if (GameManager.started)
            if (clientID == turnOrder[currentTurn % turnOrder.Length])
                calcNewTurn();
    }




}
public enum Phase { GettingReady, PlacingVillages, BeforeRoll, CasualRound, BanditsMoreThan7, Barbarians }