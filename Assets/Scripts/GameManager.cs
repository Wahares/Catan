using FishNet;
using FishNet.Object;
using Steamworks;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public static event Action OnGameStarted;
    public static bool started = false;
    private void Start()
    {
        instance = this;
        if (InstanceFinder.NetworkManager.IsServer)
            OnGameStarted += GetComponent<BoardManager>().createBoard;
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, PlayerManager.MaxPlayers);
    }

    public void startGame()
    {
        startGameRPC();
    }
    [ObserversRpc]
    private void startGameRPC()
    {
        OnGameStarted?.Invoke();
        started = true;
    }

}
