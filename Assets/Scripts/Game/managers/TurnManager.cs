using FishNet;
using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using System;
using FishNet.Connection;
using System.Linq;

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
    public static int currentPhaseArgs { get; private set; }


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
        DiceController.instance.OnDiceRolled += handleDiceBasedPhases;
    }
    private void OnDestroy()
    {
        BoardManager.OnBoardInitialized -= showTimeLimit;
        OnMyTurnEnded -= hideEndButton;
        DiceController.instance.OnDiceRolled -= handleDiceBasedPhases;
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
            if (InstanceFinder.IsServer)
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
    public int orderOfPlayer(int clientID)
    {
        for (int i = 0; i < turnOrder.Length; i++)
            if (turnOrder[i] == clientID)
                return i;
        Debug.LogError("Couldn't find order of this player");
        return -1;
    }

    [Server]
    private void startPlacingPhase()
    {
        for (int i = 0; i < turnOrder.Length; i++)
        {
            EnqueuePhase(Phase.FreeBuild, turnOrder.Length - 1 - i, TIME_LIMIT / 4, 0, true);
            EnqueuePhase(Phase.FreeBuild, turnOrder.Length - 1 - i, TIME_LIMIT / 8, 1, true);
        }
        for (int i = 0; i < turnOrder.Length; i++)
        {
            EnqueuePhase(Phase.FreeBuild, turnOrder.Length - 1 - i, TIME_LIMIT / 4, 2, true);
            EnqueuePhase(Phase.FreeBuild, turnOrder.Length - 1 - i, TIME_LIMIT / 8, 1, true);
        }
        EnqueuePhase(Phase.BeforeRoll, 0, TIME_LIMIT / 4, true);

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
        public Phase phase; public int turnID; public float time; public int args;
        public TurnConfiguration(Phase phase, int turnID, float time) : this(phase, turnID, time, 0) { }
        public TurnConfiguration(Phase phase, int turnID, float time, int args) { this.phase = phase; this.turnID = turnID; this.time = time; this.args = args; }
    }
    public List<TurnConfiguration> enqueuedPhases = new();
    public void EnqueuePhase(Phase ph, int turnID, float time, bool toBack)
    {
        EnqueuePhase(ph, turnID, time, -1, toBack);
    }
    public void EnqueuePhase(Phase ph, int turnID, float time, int args, bool toBack)
    {
        if (toBack)
            enqueuedPhases.Add(new TurnConfiguration(ph, turnID, time, args));
        else
            enqueuedPhases.Insert(0, new TurnConfiguration(ph, turnID, time, args));
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
                enqueuedPhases.Add(new TurnConfiguration(Phase.BeforeRoll, nextTurnID, TIME_LIMIT / 4));
            }

            TurnConfiguration tc = enqueuedPhases[0];
            enqueuedPhases.RemoveAt(0);

            currentTurnID = tc.turnID;
            currentPhase = tc.phase;
            currentPhaseArgs = tc.args;
            if (PlayerManager.playerAvailable(turnOrder[tc.turnID]))
            {
                startNewTurn(tc.turnID, tc.phase, tc.time, tc.args);
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
    private void startNewTurn(int turnID, Phase phase, float time, int args)
    {
        currentTurnID = turnID;
        currentPhase = phase;
        currentPhaseArgs = args;
        timer = time;
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


    private void handleDiceBasedPhases(int basic, int red, diceActions action)
    {
        if (action == diceActions.Barbarians)
        {
            BoardManager.instance.moveBarbariansOnServer();
            Debug.Log($"Barbarians moving! {BoardManager.instance.currentBarbariansPos}x{BoardManager.instance.numberOfBarbariansFields}");
            if (BoardManager.instance.currentBarbariansPos % BoardManager.instance.numberOfBarbariansFields == 0)
            {
                var playersToPunish = BoardManager.instance.currentPlayersInDanger();
                for (int i = 0; i < PlayerManager.numOfPlayers; i++)
                    if (playersToPunish.Contains(turnOrder[i]))
                        EnqueuePhase(Phase.Barbarians, i, TIME_LIMIT / 4, true);
                if (BoardManager.instance.currentBanditPos == new Vector2Int(-1, -1))
                    BoardManager.instance.moveBanditsOnServer(new Vector2Int(0, 0), -1);
            }
        }
        if (basic + red == 7)
        {
            for (int i = 0; i < PlayerManager.numOfPlayers; i++)
                EnqueuePhase(Phase.BanditsMoreThan7, i, TIME_LIMIT / 4, true);

            if (BoardManager.instance.currentBanditPos == new Vector2Int(-1, -1))
                Debug.Log("Bandits are not yet on the board - skipping BanditsMove phase...");
            else
                EnqueuePhase(Phase.BanditsMove, currentTurnID, TIME_LIMIT / 4, true);
        }
        if (action != diceActions.Barbarians)
            for (int i = 0; i < PlayerManager.numOfPlayers; i++)
            {
                int codedSpecialCardsArgs = red;
                codedSpecialCardsArgs |= (int)action << 3;
                EnqueuePhase(Phase.RemovingSpecialCards, i, TIME_LIMIT / 4, codedSpecialCardsArgs, true);
                EnqueuePhase(Phase.GettingSpecialCards, i, TIME_LIMIT / 4, codedSpecialCardsArgs, true);
            }
        for (int i = 0; i < PlayerManager.numOfPlayers; i++)
        {
            int playerID = turnOrder[i];
            if (CommodityUpgradeManager.instance.getUpgradeLevel(playerID, growthType.Science) < 3)
                continue;
            bool hasSettlementOrCity = BoardManager.instance.Tiles
                .Where(e => e.Value.num == basic + red)
                .Select(e => e.Value.getNearbyCrossings())
                .Where((e) =>
                {
                    bool hasPiece = false;
                    foreach (var crossing in e)
                    {
                        if (crossing.currentPiece == null)
                            continue;
                        if ((crossing.currentPiece as SettlementController) != null)
                            hasPiece = true;
                    }
                    return hasPiece;
                }).Count() > 0;
            if (hasSettlementOrCity)
                EnqueuePhase(Phase.GettingAdditionalCard, i, 10, true);
        }

        instance.EnqueuePhase(Phase.CasualRound, currentTurnID, TIME_LIMIT, true);
        instance.ForceEndTurn();
    }

















}
public enum Phase { GettingReady, FreeBuild, BeforeRoll, CasualRound, BanditsMoreThan7, BanditsMove, Barbarians, GettingSpecialCards, RemovingSpecialCards, ManagingMetropoly, GettingAdditionalCard }