using FishNet.Object;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        //GetComponent<BoardManager>().createBoard();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, PlayerManager.MaxPlayers);
    }
}
