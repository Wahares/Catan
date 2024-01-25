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

    public static TurnManager instance;

    public static int[] turnOrder;

    public int myOrder;


    public static Action<int, Phase> OnAnyTurnStarted;
    public static Action OnMyTurnStarted;
    public static Action OnMyTurnEnded;



    public static bool isMyTurn => currentTurnID == instance.myOrder;

    public static int currentTurnID;
    public static Phase currentPhase { get; private set; }


    [field: SerializeField]
    public UnityEngine.UI.Button EndTurnButton { get; private set; }


    private void Awake()
    {
        instance = this;
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        BoardManager.OnBoardInitialized += randomizePlayers;
        PlayerManager.OnPlayerDisconnected += playerWithTurnDisconnected;
        EndTurnButton.gameObject.SetActive(false);
        EndTurnButton.onClick.AddListener(() => { endTurn(); EndTurnButton.gameObject.SetActive(false); });
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
        for (int i = 0; i < turnOrder.Length; i++)
            EnqueuePhase(Phase.PlacingVillages, turnOrder.Length - 1 - i,true);
        for (int i = 0; i < turnOrder.Length; i++)
            EnqueuePhase(Phase.PlacingVillages, turnOrder.Length - 1 - i,true);
        EnqueuePhase(Phase.BeforeRoll, 0,true);

        calcNewTurn();
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
            if (turnOrder[currentTurnID] == player.Key)
            {
                EndPlayersTurn(player.Value);
                break;
            }
        }
    }
    [TargetRpc]
    private void EndPlayersTurn(NetworkConnection nc)
    {
        endTurn();
    }





    [Serializable]
    public class TurnConfiguration
    {
        public Phase phase; public int turnID;
        public TurnConfiguration(Phase phase, int turnID) { this.phase = phase; this.turnID = turnID; }
    }
    public List<TurnConfiguration> enqueuedPhases = new();
    public void EnqueuePhase(Phase ph, int turnID, bool toBack)
    {
        if (toBack)
            enqueuedPhases.Add(new TurnConfiguration(ph, turnID));
        else
            enqueuedPhases.Insert(0,new TurnConfiguration(ph, turnID));
    }
    [ServerRpc(RequireOwnership = false)]
    public void calcNewTurn()
    {

        for (int i = 0; i < 999; i++)
        {
            int nextTurnID = (currentTurnID + 1) % turnOrder.Length;
            if (enqueuedPhases.Count == 0)
                enqueuedPhases.Add(new TurnConfiguration(Phase.BeforeRoll, nextTurnID));

            TurnConfiguration tc = enqueuedPhases[0];
            enqueuedPhases.RemoveAt(0);

            if (PlayerManager.playerAvailable(turnOrder[tc.turnID]))
            {
                currentTurnID = tc.turnID;
                currentPhase = tc.phase;
                startNewTurn(tc.turnID, tc.phase);
                break;
            }

            if (i == 998)
                Debug.LogError("Endless loop!!!!");
        }

    }

    [ObserversRpc]
    private void startNewTurn(int turnID, Phase phase)
    {
        currentTurnID = turnID;
        currentPhase = phase;
        try
        {
            Debug.Log($"It's {Steamworks.SteamFriends.GetFriendPersonaName((Steamworks.CSteamID)PlayerManager.instance.playerSteamIDs[turnOrder[currentTurnID]])}'s turn at {phase} phase");
        }
        catch { Debug.Log($"It's {PlayerManager.instance.playerSteamIDs[turnOrder[currentTurnID]]}'s turn at {phase} phase"); }
        OnAnyTurnStarted?.Invoke(turnOrder[currentTurnID], phase);
        if (isMyTurn)
            OnMyTurnStarted?.Invoke();
    }
    [Server]
    private void playerWithTurnDisconnected(int clientID)
    {
        if (GameManager.started)
            if (clientID == turnOrder[currentTurnID])
                calcNewTurn();
    }



}
public enum Phase { GettingReady, PlacingVillages, BeforeRoll, CasualRound, BanditsMoreThan7, Barbarians,GettingSpecialCards }