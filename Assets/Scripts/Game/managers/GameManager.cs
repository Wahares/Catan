using FishNet;
using FishNet.Object;
using FishNet.Transporting.Multipass;
using System;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public static event Action OnGameStarted;
    public static event Action OnBanditsRolled;
    public static bool started = false;
    private void Start()
    {
        instance = this;
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        if(InstanceFinder.TransportManager.GetTransport<Multipass>().ClientTransport
            .Equals(InstanceFinder.TransportManager.GetTransport<FishySteamworks.FishySteamworks>()))
        SteamController.instance.CreateLobby();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            InstanceFinder.ClientManager.StopConnection();
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

    public void HandleBandits()
    {
        OnBanditsRolled?.Invoke();
    }
    private void OnDestroy()
    {
        OnGameStarted = null;
        started = false;
    }

}
