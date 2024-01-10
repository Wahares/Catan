using Steamworks;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public PlayerAvatar Initialize(ulong steamID)
    {
        spriteRenderer.sprite = getSteamAvatar(SteamUser.GetSteamID());
        return this;
    }
    public static Sprite getSteamAvatar(CSteamID steamID)
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
            return Sprite.Create(returnTexture, new Rect(0, 0, returnTexture.width, returnTexture.height)
                , new Vector2(0,0));
        }
        else
        {
            Debug.LogError("Couldn't get avatar.");
            return null;
        }
    }
}
