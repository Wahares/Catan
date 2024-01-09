using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static readonly Color[] _playerColors = { Color.red, Color.yellow, Color.blue, Color.white, Color.green, Color.cyan, Color.gray, Color.magenta };
    public static int MaxPlayers => _playerColors.Length;

    public static int CurrentlyMaxPlayers = 4;

    [SerializeField]
    private ReadyController readyController;


    [SyncObject]
    public readonly SyncDictionary<int, int> playerColors = new();
    [SyncObject]
    public readonly SyncDictionary<int, ulong> playerSteamIDs = new();

    public static PlayerManager instance;
    private void Awake()
    {
        instance = this;
        GameManager.OnGameStarted += () => { readyController.gameObject.SetActive(false); };
        playerColors.OnChange += OnColorUpdate;
        InstanceFinder.ClientManager.OnRemoteConnectionState += PlayerDisconnected;
    }


    [ServerRpc(RequireOwnership = false)]
    public void markMeAsReady(int color, NetworkConnection nc = null)
    {
        playerColors.Add(nc.ClientId, color);
        readyController.checkIfCanStart();
    }
    public void OnColorUpdate(SyncDictionaryOperation op, int key, int value,bool asServer)
    {
        if (op != SyncDictionaryOperation.Add && op != SyncDictionaryOperation.Set)
            return;
        if (key == LocalConnection.ClientId)
            readyController.successReady(value);
        else
            readyController.revalidate(value);
        Debug.Log($"Player {Steamworks.SteamFriends.GetFriendPersonaName((Steamworks.CSteamID)playerSteamIDs[key])}" 
            + $" has chosen color {_playerColors[playerColors[key]]}");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        setupMe((ulong)Steamworks.SteamUser.GetSteamID());
    }
    [ServerRpc(RequireOwnership = false)]
    private void setupMe(ulong steamID, NetworkConnection nc = null)
    {
        playerSteamIDs[nc.ClientId] = steamID;
        setupLobbyData(nc, CurrentlyMaxPlayers);
    }
    [TargetRpc]
    public void setupLobbyData(NetworkConnection nc, int maxPlayers)
    {
        CurrentlyMaxPlayers = maxPlayers;
        readyController.initialize();
    }


    public void PlayerDisconnected(RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != RemoteConnectionState.Stopped)
            return;
        Debug.Log(Steamworks.SteamFriends.GetFriendPersonaName((Steamworks.CSteamID)playerSteamIDs[args.ConnectionId])+" disconnected");
    }

}