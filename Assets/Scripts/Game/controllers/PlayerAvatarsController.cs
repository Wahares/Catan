using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAvatarsController : NetworkBehaviour
{
    public Dictionary<int, PlayerAvatar> playerAvatars = new();
    [SerializeField]
    private GameObject avatarPrefab;

    public void generateRawAvatar(int ncID, ulong steamID)
    {
        if (playerAvatars.ContainsKey(ncID))
        {
            Debug.LogWarning("Tried to add " + ncID + " second time!");
            return;
        }
        playerAvatars.Add(ncID, Instantiate(avatarPrefab, Vector3.zero
            , Quaternion.Euler(0, 360 * (playerAvatars.Count / PlayerManager.instance.playerSteamIDs.Count), 0))
            .GetComponent<PlayerAvatar>().Initialize(steamID));
        if (ncID == LocalConnection.ClientId)
            playerAvatars[ncID].gameObject.SetActive(false);
    }
    public void moveToPlace(int queueNumber, int queueSize)
    {
        transform.localEulerAngles = Vector3.up * 360 * queueNumber / queueSize;
    }
    public void disableMe(int ncID)
    {
        playerAvatars[ncID].render.material.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }
    public void setupVisiblePlayers()
    {
        foreach (var item in PlayerManager.instance.playerSteamIDs)
            generateRawAvatar(item.Key, item.Value);
    }

}
