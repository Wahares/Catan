using DG.Tweening;
using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceController : NetworkBehaviour
{
    [SerializeField]
    private Animator basicDice, redDice, actionDice;
    [SerializeField]
    private Transform previewPivot;
    [SerializeField]
    private Vector3 hiddenPos, rollPos;

    [SerializeField]
    private Button rollButton;

    public static DiceController instance;
    public event System.Action<int, int, diceActions> OnDiceRolled;


    private void Awake()
    {
        instance = this;
        rollButton.gameObject.SetActive(false);

        rollButton.onClick.AddListener(() => { tryToRollDice(); });

        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        OnDiceRolled += handleDiceBasedPhases;
        TurnManager.OnAnyTurnStarted += DropLock;

    }
    private void OnDestroy()
    {
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        OnDiceRolled -= handleDiceBasedPhases;
        TurnManager.OnAnyTurnStarted -= DropLock;
    }
    [Server]
    private void DropLock(int clientID, Phase ph) { if (ph == Phase.BeforeRoll) DiceLock = false; }

    private void handleDiceBasedPhases(int basic, int red, diceActions action)
    {
        if (action == diceActions.Barbarians)
        {
            BoardManager.instance.moveBarbariansOnServer();
            Debug.Log($"Barbarians moving! {BoardManager.instance.currentBarbariansPos}x{BoardManager.instance.numberOfBarbariansFields}");
            if (BoardManager.instance.currentBarbariansPos % BoardManager.instance.numberOfBarbariansFields == 0)
            {
                var playersToPunish = BoardManager.instance.currentPlayersInDanger();
                for (int i = 0; i < PlayerManager.numOfPlayers; i++)
                    if (playersToPunish.Contains(TurnManager.turnOrder[i]))
                        TurnManager.instance.EnqueuePhase(Phase.Barbarians, i, TurnManager.TIME_LIMIT / 4, true);
                if (BoardManager.instance.currentBanditPos == new Vector2Int(-1, -1))
                    BoardManager.instance.moveBanditsOnServer(new Vector2Int(0, 0), -1);
            }
        }
        if (basic + red == 7)
        {
            for (int i = 0; i < PlayerManager.numOfPlayers; i++)
                TurnManager.instance.EnqueuePhase(Phase.BanditsMoreThan7, i, TurnManager.TIME_LIMIT / 4, true);

            if (BoardManager.instance.currentBanditPos == new Vector2Int(-1, -1))
                Debug.Log("Bandits are not yet on the board - skipping BanditsMove phase...");
            else
                TurnManager.instance.EnqueuePhase(Phase.BanditsMove, TurnManager.currentTurnID, TurnManager.TIME_LIMIT / 4, true);
        }
        if (action != diceActions.Barbarians)
            for (int i = 0; i < PlayerManager.numOfPlayers; i++)
            {
                int codedSpecialCardsArgs = red;
                codedSpecialCardsArgs |= (int)action << 3;
                TurnManager.instance.EnqueuePhase(Phase.GettingSpecialCards, i, TurnManager.TIME_LIMIT / 4, codedSpecialCardsArgs, true);
            }

        TurnManager.instance.EnqueuePhase(Phase.CasualRound, TurnManager.currentTurnID, TurnManager.TIME_LIMIT, true);
        TurnManager.instance.ForceEndTurn();
    }

    public bool DiceLock = false;

    public void allowToRoll()
    {
        if (TurnManager.currentPhase != Phase.BeforeRoll)
            return;
        if (!TurnManager.isMyTurn)
            return;
        rollButton.gameObject.SetActive(true);
    }

    [Server]
    private void generateRoll(out ushort basic, out ushort red, out ushort action)
    {
        basic = (ushort)(Random.Range(0, 6) + 1);
        red = (ushort)(Random.Range(0, 6) + 1);
        action = (ushort)(Random.Range(0, 6) + 1);
    }

    public void tryToRollDice()
    {
        if (!TurnManager.isMyTurn)
            return;
        if (TurnManager.currentPhase != Phase.BeforeRoll)
            return;
        rollDice();
        rollButton.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void rollDice()
    {
        if (!GameManager.started)
        {
            Debug.LogError("Can't roll dices - game hasn't started yet!");
            return;
        }
        if (TurnManager.currentPhase != Phase.BeforeRoll)
        {
            Debug.LogError("Can't roll dices - incorrect phase!");
            return;
        }
        if (DiceLock)
        {
            Debug.LogWarning("Can't roll dices - enabled lock");
            return;
        }
        DiceLock = true;

        ushort basic;
        ushort red;
        ushort actions;
        generateRoll(out basic, out red, out actions);

        ushort coded = actions;
        coded |= (ushort)(red << 4);
        coded |= (ushort)(basic << 8);

        recieveRoll(coded);
    }
    private Coroutine currentRoll;
    private int bufferedBasic, bufferedRed, bufferedAction;
    [ObserversRpc]
    private void recieveRoll(ushort codedRoll)
    {
        rollButton.gameObject.SetActive(false);

        int basic = (codedRoll & (15 << 8)) >> 8;
        int red = (codedRoll & (15 << 4)) >> 4;
        int action = codedRoll & 15;

        Debug.Log($"rolled: {basic} {red} {actionFromDiceNumber(action)}");

        if (currentRoll != null)
        {
            StopCoroutine(currentRoll);
            try
            {
                OnDiceRolled?.Invoke(bufferedBasic, bufferedRed, actionFromDiceNumber(bufferedAction));
            }
            catch (System.Exception e) { Debug.LogException(e); }
        }
        currentRoll = StartCoroutine(diceRollView(basic, red, action));
    }
    public diceActions actionFromDiceNumber(int number) => (diceActions)Mathf.Clamp(number - 3, 0, 3);

    private IEnumerator diceRollView(int basic, int red, int action)
    {
        bufferedBasic = basic;
        bufferedRed = red;
        bufferedAction = action;

        basicDice.enabled = true;
        redDice.enabled = true;
        actionDice.enabled = true;

        basicDice.GetComponentInChildren<PreparedDiceRotations>().SetToThrow(basic);
        redDice.GetComponentInChildren<PreparedDiceRotations>().SetToThrow(red);
        actionDice.GetComponentInChildren<PreparedDiceRotations>().SetToThrow(action);

        basicDice.SetInteger("whichRoll", 0);
        redDice.SetInteger("whichRoll", 3);
        actionDice.SetInteger("whichRoll", 4);

        transform.DOComplete();
        transform.DOMove(rollPos, 0.5f);

        basicDice.SetTrigger("doRoll");
        redDice.SetTrigger("doRoll");
        actionDice.SetTrigger("doRoll");


        yield return new WaitForSeconds(3);

        basicDice.enabled = false;
        redDice.enabled = false;
        actionDice.enabled = false;

        basicDice.GetComponentInChildren<PreparedDiceRotations>().snapToPreview(previewPivot, -Vector3.right / 3, basic);
        redDice.GetComponentInChildren<PreparedDiceRotations>().snapToPreview(previewPivot, Vector3.zero, red);
        actionDice.GetComponentInChildren<PreparedDiceRotations>().snapToPreview(previewPivot, Vector3.right / 3, action);

        yield return new WaitForSeconds(1f);
        transform.DOComplete();
        transform.DOMove(hiddenPos, 0.5f);


        yield return new WaitForSeconds(2);

        basicDice.transform.DOLocalMoveZ(4, 1).SetEase(Ease.InSine);
        redDice.transform.DOLocalMoveZ(4, 1).SetEase(Ease.InSine);
        actionDice.transform.DOLocalMoveZ(4, 1).SetEase(Ease.InSine);

        yield return new WaitForSeconds(1);
        basicDice.GetComponentInChildren<PreparedDiceRotations>().SetToThrow(1);
        redDice.GetComponentInChildren<PreparedDiceRotations>().SetToThrow(1);
        actionDice.GetComponentInChildren<PreparedDiceRotations>().SetToThrow(1);

        OnDiceRolled?.Invoke(basic, red, actionFromDiceNumber(action));
        currentRoll = null;
    }








}
public enum diceActions
{
    Barbarians,
    Trade,
    Politics,
    Science
}