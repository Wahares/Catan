using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup hostGameCanvas,mainGroupCanvas;

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

    public static void setGroupActive(CanvasGroup group,bool active)
    {
        group.alpha = active?1:0;
        group.interactable = active;
        group.blocksRaycasts = active;
    }


    [SerializeField]
    private ValuePicker mapSizePicker,timeLimitPicker;

    public void hostGame()
    {
        BoardManager.MapSize = mapSizePicker.value;
        TurnManager.TIME_LIMIT = timeLimitPicker.value;
        TurnManager.DO_LIMIT_TURN = timeLimitPicker.value != 0;
        Debug.Log("Beginning to host a game...");
    }









}
