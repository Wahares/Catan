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

    public static Action<int, Phase> OnClientTimeReached;



    public static bool isMyTurn => currentTurnID == instance.myOrder;

    public static int currentTurnID;
    public static Phase currentPhase { get; private set; }


    [field: SerializeField]
    public UnityEngine.UI.Button EndTurnButton { get; private set; }
    [SerializeField]
    private TMPro.TextMeshProUGUI timeText;


    public float timer { get; private set; }

    private void Awake()
    {
        instance = this;
        EndTurnButton.gameObject.SetActive(false);
        EndTurnButton.onClick.AddListener(() => { endTurn(); });
        OnMyTurnEnded += hideEndButton;
        timer = Mathf.Infinity;
        timeText.gameObject.SetActive(false);
        BoardManager.OnBoardInitialized += showTimeLimit;
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        BoardManager.OnBoardInitialized += randomizePlayers;
        PlayerManager.OnPlayerDisconnected += playerWithTurnDisconnected;
    }
    private void OnDestroy()
    {
        BoardManager.OnBoardInitialized -= showTimeLimit;
        OnMyTurnEnded -= hideEndButton;
    }
    private void showTimeLimit()
    {
        if (DO_LIMIT_TURN)
            timeText.gameObject.SetActive(true);
    }
    private void Update()
    {
        if (!DO_LIMIT_TURN)
            return;
        timer -= Time.deltaTime;
        if (timer > 0)
            timeText.text = $"{(int)timer / 60}:{(int)timer % 60:00}";
        else
            timeText.text = "0:00";
        if (timer > 1000000)
            timeText.text = "";
        if (timer < 0)
        {
            Debug.LogWarning("Force ending current turn due to time limit...");
            OnClientTimeReached?.Invoke(turnOrder[currentTurnID], currentPhase);
            timer = Mathf.Infinity;
        }
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
            EnqueuePhase(Phase.PlacingVillages, turnOrder.Length - 1 - i, true);
        for (int i = 0; i < turnOrder.Length; i++)
            EnqueuePhase(Phase.PlacingVillages, turnOrder.Length - 1 - i, true);
        EnqueuePhase(Phase.BeforeRoll, 0, true);

        calcNewTurn();
    }

    private void hideEndButton()
    {
        EndTurnButton.gameObject.SetActive(false);
    }

    public void endTurn()
    {
        if (!isMyTurn)
            return;
        AskServerForNextTurn(currentPhase, LocalConnection.ClientId);
        Debug.Log("Ending my turn...");
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
            enqueuedPhases.Insert(0, new TurnConfiguration(ph, turnID));
    }
    [ServerRpc(RequireOwnership = false)]
    private void AskServerForNextTurn(Phase phase, int clientID)
    {
        if (turnOrder[currentTurnID] == clientID && phase == currentPhase)
            calcNewTurn();
        else
            Debug.LogError("Tried to skip phantom turn!");
    }
    public void calcNewTurn()
    {
        for (int i = 0; i < 999; i++)
        {
            int nextTurnID = (currentTurnID + 1) % turnOrder.Length;
            if (enqueuedPhases.Count == 0)
            {
                Debug.Log("adding next roll turn");
                enqueuedPhases.Add(new TurnConfiguration(Phase.BeforeRoll, nextTurnID));
            }

            TurnConfiguration tc = enqueuedPhases[0];
            enqueuedPhases.RemoveAt(0);

            currentTurnID = tc.turnID;
            currentPhase = tc.phase;
            if (PlayerManager.playerAvailable(turnOrder[tc.turnID]))
            {
                startNewTurn(tc.turnID, tc.phase);
                break;
            }
            else
            {
                Debug.LogWarning("Player out of reach - skipping his turn");
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
        timer = (phase == Phase.CasualRound ? 1 : 0.25f) * TIME_LIMIT;
        try
        {
            Debug.Log($"It's [{turnOrder[currentTurnID]}]:{Steamworks.SteamFriends.GetFriendPersonaName((Steamworks.CSteamID)PlayerManager.instance.playerSteamIDs[turnOrder[currentTurnID]])}'s turn at {phase} phase");
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
public enum Phase { GettingReady, PlacingVillages, BeforeRoll, CasualRound, BanditsMoreThan7, BanditsMove, Barbarians, GettingSpecialCards }