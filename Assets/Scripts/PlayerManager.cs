using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static readonly Color[] _playerColors = { Color.red, Color.yellow, Color.blue, Color.white, Color.green,Color.cyan,Color.gray,Color.magenta};
    public static int MaxPlayers =>_playerColors.Length;

    public static int CurrentlyMaxPlayers = 4;

    [SyncObject]
    public readonly SyncDictionary<int,int> playerColors = new();


    



    [ServerRpc(RequireOwnership = false)]
    public void markMeAsReady(NetworkConnection nc, Color color)
    {

    }
}