using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceController : NetworkBehaviour
{
    [SerializeField]
    private Animator basicDice, redDice, actionDice;

    public static DiceController instance;
    public event System.Action<int,int,diceActions> OnDiceRolled;
    private void Awake()
    {
        instance = this;
        Invoke("rollDice", 1);//  rollDice();
    }

    [Server]
    private void generateRoll(out ushort basic, out ushort red, out diceActions action)
    {
        basic = (ushort)(Random.Range(0,6)+1);
        red = (ushort)(Random.Range(0,6)+1);
        action = (diceActions)Mathf.Clamp(Random.Range(0, 7)-3,0,3);
    }

    [ServerRpc(RequireOwnership = false)]
    private void rollDice()
    {
        ushort basic;
        ushort red;
        diceActions actions;
        generateRoll(out basic, out red, out actions);

        ushort coded = (ushort)actions;
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

        Debug.Log($"rolled: {basic} {red} {(diceActions)action}");
        OnDiceRolled?.Invoke(basic, red, (diceActions)action);

    }










}
public enum diceActions
{
    Barbarians,
    Trade,
    Politics,
    Science
}