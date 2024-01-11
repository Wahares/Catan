using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceController : NetworkBehaviour
{
    [SerializeField]
    private Animator basicDice, redDice, actionDice;
    [SerializeField]
    private Transform previewPivot;


    public static DiceController instance;
    public event System.Action<int,int,diceActions> OnDiceRolled;


    private void Awake()
    {
        instance = this;
        Invoke("rollDice", 1);//  rollDice();
    }

    [Server]
    private void generateRoll(out ushort basic, out ushort red, out ushort action)
    {
        basic = (ushort)(Random.Range(0,6)+1);
        red = (ushort)(Random.Range(0,6)+1);
        action = (ushort)(Random.Range(0, 6) + 1);
    }
    [ContextMenu("roll")]
    [ServerRpc(RequireOwnership = false)]
    private void rollDice()
    {
        ushort basic;
        ushort red;
        ushort actions;
        generateRoll(out basic, out red, out actions);

        ushort coded = actions;
        coded |= (ushort)( red << 4);
        coded |= (ushort)(basic << 8);

        recieveRoll(coded);
    }

    [ObserversRpc]
    private void recieveRoll(ushort codedRoll)
    {
        int basic = (codedRoll & (15 << 8))>>8;
        int red = (codedRoll & (15<<4))>>4;
        int action = codedRoll&15;

        Debug.Log($"rolled: {basic} {red} {actionFromDiceNumber(action)}");

        StartCoroutine(diceRollView(basic, red, action));

        OnDiceRolled?.Invoke(basic, red, actionFromDiceNumber(action));

    }
    public diceActions actionFromDiceNumber(int number) => (diceActions)Mathf.Clamp(number - 3, 0, 3);

    private IEnumerator diceRollView(int basic,int red,int action)
    {
        basicDice.enabled = true;
        redDice.enabled = true;
        actionDice.enabled = true;

        basicDice.SetInteger("whichRoll",0);
        redDice.SetInteger("whichRoll",3);
        actionDice.SetInteger("whichRoll",4);

        basicDice.SetTrigger("doRoll");
        redDice.SetTrigger("doRoll");
        actionDice.SetTrigger("doRoll");

        basicDice.GetComponentInChildren<PreparedDiceRotations>().RotateToNumber(basic);
        redDice.GetComponentInChildren<PreparedDiceRotations>().RotateToNumber(red);
        actionDice.GetComponentInChildren<PreparedDiceRotations>().RotateToNumber(action);

        yield return new WaitForSeconds(3);

        basicDice.enabled = false;
        redDice.enabled = false;
        actionDice.enabled = false;

        basicDice.GetComponentInChildren<PreparedDiceRotations>().snapToPreview(previewPivot,-previewPivot.right/3, basic);
        redDice.GetComponentInChildren<PreparedDiceRotations>().snapToPreview(previewPivot,Vector3.zero, red);
        actionDice.GetComponentInChildren<PreparedDiceRotations>().snapToPreview(previewPivot, previewPivot.right / 3, action);

    }








}
public enum diceActions
{
    Barbarians,
    Trade,
    Politics,
    Science
}