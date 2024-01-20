using FishNet;
using Steamworks;
using UnityEngine;

public class SteamController : MonoBehaviour
{

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    public static ulong CurrentLobbyID = 0;
    private const string HostAdressKey = "HostAddress";

    public static SteamController instance;

    private void Start()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam not yet initialized!");
            return;
        }
        if (LobbyCreated != null)
            return;
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public static bool InLobby => CurrentLobbyID != 0;
    public void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, PlayerManager.MaxPlayers);
    }


    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;
        Debug.Log("Steam lobby created");
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey, SteamUser.GetSteamID().ToString());
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log(callback.m_steamIDFriend + " requested to join lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        Debug.Log("Entered steam lobby");
        if (InstanceFinder.NetworkManager.IsServer)
            return;
        if (FindAnyObjectByType<MenuManager>() == null)
        {
            Debug.LogError("Couldn't find any MenuManager! Leaving steam lobby...");
            LeaveLobby();
        }
        else
        {
            FindAnyObjectByType<MenuManager>().Join(CurrentLobbyID.ToString());
        }
    }

    public void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby((CSteamID)CurrentLobbyID);
        CurrentLobbyID = 0;
    }

}
