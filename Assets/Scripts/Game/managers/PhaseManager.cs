using FishNet;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{

    private void Awake()
    {
        TurnManager.OnMyTurnStarted += OnMyBuildingPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyRollPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyBarbariansPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyCasualPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyBanditsPhaseTurn;
    }
    private void OnDestroy()
    {
        TurnManager.OnMyTurnStarted -= OnMyBuildingPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyRollPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyBarbariansPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyCasualPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyBanditsPhaseTurn;
    }


    [SerializeField] private BuildingRecipe settlementBR, cityBR;
    public void OnMyBuildingPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.PlacingVillages)
            return;
        if (TurnManager.currentTurn < TurnManager.turnOrder.Length)
            BuildingManager.instance.BeginBuilding(settlementBR);
        else
            BuildingManager.instance.BeginBuilding(cityBR);
    }
    public void OnMyRollPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.BeforeRoll)
            return;
        DiceController.instance.allowToRoll();
    }
    public void OnMyBarbariansPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.Barbarians)
            return;
        if (!BoardManager.instance.currentPlayersInDanger().Contains(InstanceFinder.ClientManager.Connection.ClientId))
            return;

        //need to destroy city

    }
    public void OnMyCasualPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.CasualRound)
            return;
    }
    public void OnMyBanditsPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.BanditsMoreThan7)
            return;
        if (BoardManager.instance.mySafeNumOfCards()
            < PlayerInventoriesManager.instance.getPlayersCardsInHand(InstanceFinder.ClientManager.Connection.ClientId).Count)
        {
            Debug.Log("You have too many cards!");
        }
        else
            TurnManager.instance.endTurn();
    }
}
