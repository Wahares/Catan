using FishNet;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup hostGameCanvas, mainGroupCanvas;

    private void Awake()
    {
        mainView();
    }

    public void hostView()
    {
        setGroupActive(mainGroupCanvas, false);
        setGroupActive(hostGameCanvas, true);
    }
    public void mainView()
    {
        setGroupActive(mainGroupCanvas, true);
        setGroupActive(hostGameCanvas, false);
    }

    public static void setGroupActive(CanvasGroup group, bool active)
    {
        group.alpha = active ? 1 : 0;
        group.interactable = active;
        group.blocksRaycasts = active;
    }


    [SerializeField]
    private ValuePicker mapSizePicker, timeLimitPicker, maxPlayersPicker;

    public void hostGame(bool asSteam)
    {
        BoardManager.MapSize = mapSizePicker.value;
        TurnManager.TIME_LIMIT = timeLimitPicker.value;
        TurnManager.DO_LIMIT_TURN = timeLimitPicker.value != 0;
        PlayerManager.CurrentlyMaxPlayers = maxPlayersPicker.value;
        Debug.Log($"Beginning to host {(asSteam?"Steam":"Local")} game...");
        if (asSteam)
            InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<FishySteamworks.FishySteamworks>();
        InstanceFinder.TransportManager.Transport.StartConnection(true);
        InstanceFinder.TransportManager.Transport.StartConnection(false);
    }
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientCreated;
    }
    private void OnDisable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientCreated;
    }
    public void OnClientCreated(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started)
            return;
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        SceneLoadData sld = new SceneLoadData("Game");
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }
    public void Join(string address)
    {
        InstanceFinder.TransportManager.GetTransport<Multipass>().SetClientTransport<FishySteamworks.FishySteamworks>();
        InstanceFinder.ClientManager.StartConnection(address);
    }
}
