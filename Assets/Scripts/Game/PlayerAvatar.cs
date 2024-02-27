using Steamworks;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour
{
    public MeshRenderer render;
    [field:SerializeField]
    public PlayerInventoryView inventoryView { get; private set; }
    [field: SerializeField]
    public CommodityUpgradeView upgradeView { get; private set; }
    public PlayerAvatar Initialize(ulong steamID)
    {
        render.material = new Material(render.material);
        render.material.mainTexture = getSteamAvatar(SteamUser.GetSteamID());
        return this;
    }
    public static Texture2D getSteamAvatar(CSteamID steamID)
    {
        int FriendAvatar = SteamFriends.GetLargeFriendAvatar(steamID);
        uint ImageWidth;
        uint ImageHeight;
        bool success = SteamUtils.GetImageSize(FriendAvatar, out ImageWidth, out ImageHeight);

        if (success && ImageWidth > 0 && ImageHeight > 0)
        {
            byte[] Image = new byte[ImageWidth * ImageHeight * 4];
            Texture2D returnTexture = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
            success = SteamUtils.GetImageRGBA(FriendAvatar, Image, (int)(ImageWidth * ImageHeight * 4));
            if (success)
            {
                returnTexture.LoadRawTextureData(Image);
                returnTexture.Apply();
            }
            return returnTexture;
        }
        else
        {
            Debug.LogError("Couldn't get avatar.");
            return null;
        }
    }
}
