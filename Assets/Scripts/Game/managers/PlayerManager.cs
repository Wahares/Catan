using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static readonly Color[] _playerColors = { Color.red, Color.yellow, Color.blue, Color.white, Color.green, Color.cyan, Color.gray, Color.magenta };
    public static int MaxPlayers => _playerColors.Length;

    public static int CurrentlyMaxPlayers = 4;

    [SerializeField]
    private ReadyController readyController;

    public PlayerAvatarsController avatars { get; private set; }

    public static bool playerAvailable(int conID) => instance.playerSteamIDs.ContainsKey(conID);

    public static int MyColorIndex => instance.playerColors[instance.LocalConnection.ClientId];

    public static int numOfPlayers => instance.playerColors.Count;

    [SyncObject]
    public readonly SyncDictionary<int, int> playerColors = new();
    [SyncObject]
    public readonly SyncDictionary<int, ulong> playerSteamIDs = new();

    public static PlayerManager instance;

    public static event Action<int> OnPlayerDisconnected;

    private void Awake()
    {
        instance = this;
        avatars = GetComponent<PlayerAvatarsController>();
        GameManager.OnGameStarted += () => { readyController.gameObject.SetActive(false); };
        playerColors.OnChange += OnColorUpdate;
        playerSteamIDs.OnChange += OnSteamIDAdded;
        InstanceFinder.ClientManager.OnRemoteConnectionState += PlayerDisconnected;
        InstanceFinder.ClientManager.OnRemoteConnectionState += PlayerConnected;
        InstanceFinder.ClientManager.OnClientConnectionState += OnDisconnected;
        OnPlayerDisconnected += removeDisconnectedData;
    }


    [ServerRpc(RequireOwnership = false)]
    public void markMeAsReady(int color, NetworkConnection nc = null)
    {
        playerColors.Add(nc.ClientId, color);
        readyController.checkIfCanStart();
    }
    public void OnColorUpdate(SyncDictionaryOperation op, int key, int value, bool asServer)
    {
        if (op != SyncDictionaryOperation.Add && op != SyncDictionaryOperation.Set)
            return;
        if (GameManager.started)
            return;
        if (key == LocalConnection.ClientId)
            readyController.successReady(value);
        else
            readyController.revalidate(value);
        Debug.Log($"Player {Steamworks.SteamFriends.GetFriendPersonaName((Steamworks.CSteamID)playerSteamIDs[key])}"
            + $" has chosen color {_playerColors[playerColors[key]]}");
    }
    public void OnSteamIDAdded(SyncDictionaryOperation op, int key, ulong value, bool asServer)
    {
        if (op != SyncDictionaryOperation.Add)
            return;
        Debug.Log($"{op} : {key} - {value} -- {asServer}");
        if (!asServer)
            avatars.generateRawAvatar(key, value);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        setupMe((ulong)Steamworks.SteamUser.GetSteamID());
    }
    [ServerRpc(RequireOwnership = false)]
    private void setupMe(ulong steamID, NetworkConnection nc = null)
    {
        playerSteamIDs.Add(nc.ClientId, steamID);
        setupLobbyData(nc, CurrentlyMaxPlayers);
    }
    [TargetRpc]
    public void setupLobbyData(NetworkConnection nc, int maxPlayers)
    {
        CurrentlyMaxPlayers = maxPlayers;
        readyController.initialize();
    }

    private void removeDisconnectedData(int clientID)
    {
        avatars.disableMe(clientID);
        playerColors[clientID] = -1;
        if (playerSteamIDs.ContainsKey(clientID))
        {
            Debug.Log(Steamworks.SteamFriends.GetFriendPersonaName((Steamworks.CSteamID)playerSteamIDs[clientID]) + " disconnected");
            if (InstanceFinder.NetworkManager.IsServer)
                playerSteamIDs.Remove(clientID);
        }
        else
            Debug.Log($"Connection ID {clientID} disconnected");
    }
    public void PlayerDisconnected(RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != RemoteConnectionState.Stopped)
            return;
        OnPlayerDisconnected?.Invoke(args.ConnectionId);

    }
    public void PlayerConnected(RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != RemoteConnectionState.Started)
            return;
        //Debug.Log(Steamworks.SteamFriends.GetFriendPersonaName((Steamworks.CSteamID)playerSteamIDs[args.ConnectionId]) + " joined");
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        if (GameManager.started)
        {
            ServerManager.Kick(args.ConnectionId, FishNet.Managing.Server.KickReason.Unset);
            Debug.Log($"Connection ID {args.ConnectionId} has been kicked");
        }
    }
    private void OnDestroy()
    {
        OnPlayerDisconnected = null;
        InstanceFinder.ClientManager.OnRemoteConnectionState -= PlayerDisconnected;
        InstanceFinder.ClientManager.OnRemoteConnectionState -= PlayerConnected;
        InstanceFinder.ClientManager.OnClientConnectionState -= OnDisconnected;
    }


    private void OnDisconnected(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("Left lobby - loading main maenu...");
            if (SteamController.InLobby)
                SteamController.instance.LeaveLobby();
            if (InstanceFinder.NetworkManager.IsServer)
                InstanceFinder.ServerManager.StopConnection(true);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public Dictionary<int, int> victoryPoints = new();
    public void recalculateVictoryPoints()
    {
        victoryPoints.Clear();
        foreach (var player in ClientManager.Clients)
        {
            int sum = 0;
            foreach (var crossing in BoardManager.instance.crossings)
                if ((crossing.Value.currentPiece?.pieceOwnerID ?? -999) == player.Key)
                    sum += crossing.Value.currentPiece?.GetComponent<SettlementController>()?.getVictoryWeight() ?? 0;

            for (int i = 0; i < PlayerInventoriesManager.instance.playerInventories[player.Key].Length; i++)
                if (ObjectDefiner.instance.equipableCards[i].CardType == cardType.Special)
                    if (PlayerInventoriesManager.instance.playerInventories[player.Key][i] > 0)
                        if ((ObjectDefiner.instance.equipableCards[i] as SpecialCard).IsPersistent)
                            sum++;


        }






    }



}