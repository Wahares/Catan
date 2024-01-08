using FishNet.Authenticating;
using System;
using FishNet.Connection;
using FishNet.Managing.Server;

public class CustomAuthenticator : Authenticator
{
    public override event Action<NetworkConnection, bool> OnAuthenticationResult;

    public override void OnRemoteConnection(NetworkConnection connection)
    {
        if(!NetworkManager.IsServer)
            return;
        base.OnRemoteConnection(connection);
        if (NetworkManager.ClientManager.Clients.Count == PlayerManager.CurrentlyMaxPlayers)
            NetworkManager.ServerManager.Kick(connection,KickReason.Unset);
        OnAuthenticationResult?.Invoke(connection, connection.IsActive);
    }




}
