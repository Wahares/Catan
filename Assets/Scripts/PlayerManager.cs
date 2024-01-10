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

    private PlayerAvatarsController avatars;

    public static bool playerAvailable(int conID) => instance.playerSteamIDs.ContainsKey(conID);


    [SyncObject]
    public readonly SyncDictionary<int, int> playerColors = new();
    [SyncObject]
    public readonly SyncDictionary<int, ulong> playerSteamIDs = new();

    public static PlayerManager instance;
    private void Awake()
    {
        instance = this;
        avatars = GetComponent<PlayerAvatarsController>();
        GameManager.OnGameStarted += () => { readyController.gameObject.SetActive(false); };
        playerColors.OnChange += OnColorUpdate;
        playerSteamIDs.OnChange += OnSteamIDAdded;
        InstanceFinder.ClientManager.OnRemoteConnectionState += PlayerDisconnected;
        InstanceFinder.ClientManager.OnRemoteConnectionState += PlayerConnected;
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
        if (InstanceFinder.NetworkManager.IsServer)
            readyController.initialize();
    }
    [ServerRpc(RequireOwnership = false)]
    private void setupMe(ulong steamID, NetworkConnection nc = null)
    {
        playerSteamIDs.Add(nc.ClientId, steamID);
        if (InstanceFinder.NetworkManager.IsClientOnly)
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
        avatars.disableMe(args.ConnectionId);
        playerColors[args.ConnectionId] = -1;
        if (playerSteamIDs.ContainsKey(args.ConnectionId))
        {
            Debug.Log(Steamworks.SteamFriends.GetFriendPersonaName((Steamworks.CSteamID)playerSteamIDs[args.ConnectionId]) + " disconnected");
            if (InstanceFinder.NetworkManager.IsServer)
                playerSteamIDs.Remove(args.ConnectionId);
        }
        else
            Debug.Log($"Connection ID {args.ConnectionId} disconnected");

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

}