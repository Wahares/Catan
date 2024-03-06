using Steamworks;
using UnityEngine;
using TMPro;

public class SingleAvatarChoiceController : SingleChoiceController
{
    [SerializeField]
    private TextMeshPro nick;
    public override void Initialize(int ID)
    {
        transform.localEulerAngles = Vector3.zero;
        this.ID = ID;
        render.material = new Material(render.material);
        render.material.mainTexture = PlayerAvatar.getSteamAvatar((CSteamID)PlayerManager.instance.playerSteamIDs[ID]);
        nick.text = SteamFriends.GetFriendPersonaName((CSteamID)PlayerManager.instance.playerSteamIDs[ID]);
    }
}
