using FishNet;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Tugboat;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class SteamAPIfail : MonoBehaviour
{
    [SerializeField]
    private Button directConnectButton, directHost;
    private void Start()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        directHost.gameObject.SetActive(true);
        directHost.onClick.AddListener(() =>
        {
            InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<Tugboat>();
            FindAnyObjectByType<MenuManager>().hostGame(false);
        });
        directConnectButton.gameObject.SetActive(true);
        directConnectButton.onClick.AddListener(() =>
        {
            InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<Tugboat>();
            InstanceFinder.TransportManager.Transport.StartConnection(false);
        });
#else
        if(SteamManager.Initialized)
            Destroy(gameObject);
#endif
    }

    public void tryAgain()
    {
        if (SteamAPI.Init())
            Destroy(gameObject);
    }


}
