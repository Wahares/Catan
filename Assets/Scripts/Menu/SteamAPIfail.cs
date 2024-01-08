using Steamworks;
using UnityEngine;

public class SteamAPIfail : MonoBehaviour
{
    private void Start()
    {
        if(SteamManager.Initialized)
            Destroy(gameObject);
    }

    public void tryAgain()
    {
        if(SteamAPI.Init())
            Destroy(gameObject);
    }


}
