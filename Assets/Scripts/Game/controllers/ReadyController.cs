using FishNet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyController : MonoBehaviour
{
    [SerializeField]
    private List<Button> colorButtons;
    [SerializeField]
    private Button readyButton;

    [SerializeField]
    private GameObject loadingScreen;

    private void Awake() { loadingScreen.SetActive(true); }

    public void initialize()
    {
        for (int i = 0; i < PlayerManager.MaxPlayers - PlayerManager.CurrentlyMaxPlayers; i++)
        {
            Button btn = colorButtons[colorButtons.Count - 1];
            Destroy(btn.gameObject);
            colorButtons.Remove(btn);
        }

        for (int i = 0; i < colorButtons.Count; i++)
            colorButtons[i].GetComponent<Image>().color = PlayerManager._playerColors[i];

        readyButton.onClick.AddListener(() => { readyClicked(); });
        setAvailableColors();
        Destroy(loadingScreen);
    }
    private int currentColorID;

    public void changeColor(int index)
    {
        if (PlayerManager.instance.playerColors.Collection.Values.Contains(index))
            return;
        if (PlayerManager.instance.playerColors.Collection.Keys.Contains(InstanceFinder.ClientManager.Connection.ClientId))
            return;
        currentColorID = index;
        readyButton.interactable = true;
        readyButton.GetComponent<Image>().color = PlayerManager._playerColors[currentColorID]- new Color(0, 0, 0, 0.5f);
    }
    public void revalidate(int updatedColor)
    {
        setAvailableColors();
        if (updatedColor != currentColorID)
            return;
        readyButton.interactable = false;
        currentColorID = -1;
        readyButton.GetComponent<Image>().color = Color.white;
    }
    private void setAvailableColors()
    {
        for (int i = 0; i < colorButtons.Count; i++)
            colorButtons[i].interactable = !PlayerManager.instance.playerColors.Collection.Values.Contains(i);
    }
    public void successReady(int finalColor)
    {
        currentColorID = finalColor;
        readyButton.GetComponent<Image>().color = PlayerManager._playerColors[currentColorID] - new Color(0,0,0,0.5f);
        readyButton.interactable = false;
        setAvailableColors();
    }
    public void readyClicked()
    {
        PlayerManager.instance.markMeAsReady(currentColorID);
    }
    [SerializeField]
    private Button startButton;
    private void Start()
    {
        if (InstanceFinder.NetworkManager.IsServer)
            startButton.onClick.AddListener(() => { GameManager.instance.startGame(); });
        else
            startButton.gameObject.SetActive(false);
        checkIfCanStart();
    }
    public void checkIfCanStart()
    {
        startButton.interactable = false;
        if (PlayerManager.instance.playerColors.Collection.Count < InstanceFinder.ServerManager.Clients.Count)
            return;
        startButton.interactable = true;
    }

}
