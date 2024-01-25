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
        TurnManager.OnMyTurnStarted += OnMySpecialCardsPhaseTurn;
    }
    private void OnDestroy()
    {
        TurnManager.OnMyTurnStarted -= OnMyBuildingPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyRollPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyBarbariansPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyCasualPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyBanditsPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMySpecialCardsPhaseTurn;
    }


    [SerializeField] private BuildingRecipe settlementBR, cityBR;
    public void OnMyBuildingPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.PlacingVillages)
            return;
        if (BoardManager.instance.numberOfPieces(InstanceFinder.ClientManager.Connection.ClientId,PieceType.Settlement) == 0)
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
        TurnManager.instance.endTurn();

    }

    public void OnMyCasualPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.CasualRound)
            return;
        TurnManager.instance.EndTurnButton.gameObject.SetActive(true);

    }
    public void OnMyBanditsPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.BanditsMoreThan7)
            return;
        if (BoardManager.instance.mySafeNumOfCards()
            < PlayerInventoriesManager.instance.getPlayersCardsInHand(InstanceFinder.ClientManager.Connection.ClientId).Count)
        {
            Debug.Log("You have too many cards!");
            TurnManager.instance.endTurn();
        }
        else
            TurnManager.instance.endTurn();
    }
    public void OnMySpecialCardsPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.GettingSpecialCards)
            return;
        TurnManager.instance.endTurn();
    }
}
