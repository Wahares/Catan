using FishNet;
using System.Collections.Generic;
using System.Linq;
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
        TurnManager.OnMyTurnStarted += OnMyBanditsMovePhaseTurn;
        TurnManager.OnMyTurnStarted += OnMySpecialCardsPhaseTurn;


        TurnManager.OnClientTimeReached += OnBuildingTimeLimitReached;
        TurnManager.OnClientTimeReached += OnRollTimeLimitReached;
        TurnManager.OnClientTimeReached += OnBarbariansTimeLimitReached;
        TurnManager.OnClientTimeReached += OnCasualTimeLimitReached;
        TurnManager.OnClientTimeReached += OnBanditsTimeLimitReached;
        TurnManager.OnClientTimeReached += OnBanditsMoveTimeLimitReached;
        TurnManager.OnClientTimeReached += OnSpecialCardsTimeLimitReached;
    }
    private void OnDestroy()
    {
        TurnManager.OnMyTurnStarted -= OnMyBuildingPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyRollPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyBarbariansPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyCasualPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyBanditsPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyBanditsMovePhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMySpecialCardsPhaseTurn;


        TurnManager.OnClientTimeReached -= OnBuildingTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnRollTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnBarbariansTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnCasualTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnBanditsTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnBanditsMoveTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnSpecialCardsTimeLimitReached;
    }


    [SerializeField] private BuildingRecipe settlementBR, cityBR, roadBR;
    public void OnMyBuildingPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.PlacingVillages)
            return;
        if (BoardManager.instance.numberOfPieces(InstanceFinder.ClientManager.Connection.ClientId, PieceType.Settlement) == 0)
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

        //need to click city to destroy
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
    [SerializeField]
    private BanditsController banditC;
    public void OnMyBanditsMovePhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.BanditsMove)
            return;
        banditC.beginMoving();

        //need to move bandits;

    }
    public void OnMySpecialCardsPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.GettingSpecialCards)
            return;
        TurnManager.instance.endTurn();
    }












    public void OnBuildingTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.PlacingVillages)
            return;

        for (int i = 0; i < 9999; i++)
        {

            CrossingController cc = BoardManager.instance.crossings.Values.ElementAt(Random.Range(0, BoardManager.instance.crossings.Count));
            if (cc.currentPiece != null)
                continue;
            if (BoardManager.instance.numberOfPieces(clientID, PieceType.Road) == 0)
            {
                if (BoardManager.instance.numberOfPieces(clientID, PieceType.Settlement) == 0)
                    BuildingManager.instance.SetPieceOnServer(cc.pos, clientID, ObjectDefiner.instance.availableBuildingRecipes.IndexOf(settlementBR));
                else
                    cc = BoardManager.instance
                        .crossings.Values
                        .Where(e => (e.currentPiece?.pieceType ?? PieceType.Unset) == PieceType.Settlement && (e.currentPiece?.pieceOwnerID ?? -1) == clientID)
                        .ElementAt(0);
            }
            else
            {
                if (BoardManager.instance.numberOfPieces(clientID, PieceType.City) == 0)
                    BuildingManager.instance.SetPieceOnServer(cc.pos, clientID, ObjectDefiner.instance.availableBuildingRecipes.IndexOf(cityBR));
                else
                    cc = BoardManager.instance
                        .crossings.Values
                        .Where(e => (e.currentPiece?.pieceType ?? PieceType.Unset) == PieceType.City && (e.currentPiece?.pieceOwnerID ?? -1) == clientID)
                        .ElementAt(0);
            }
            if (BoardManager.instance.numberOfPieces(clientID, PieceType.Road) == 2)
                break;
            List<RoadController> roadsControllers = cc.GetRoadsControllers();
            bool did = false;
            foreach (var road in roadsControllers)
            {
                if (road.currentPiece == null)
                {
                    BuildingManager.instance.SetPieceOnServer(road.pos, clientID, ObjectDefiner.instance.availableBuildingRecipes.IndexOf(roadBR));
                    did = true;
                    break;
                }
            }
            if (did)
                break;

            if (i == 9998)
                Debug.LogError("Endless loop!!!");
        }

        TurnManager.instance.ForceEndTurn();
    }
    public void OnRollTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.BeforeRoll)
            return;
        DiceController.instance.rollDice();
    }
    public void OnBarbariansTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.Barbarians)
            return;
        if (BoardManager.instance.currentPlayersInDanger().Contains(clientID))
        {
            List<CityController> tmp = BoardManager.instance
                .GetPlayerPieces<CityController>(clientID, PiecePlaceType.Crossing)
                .Where(e => !e.isMetropoly).ToList();
            if (tmp.Count == 0)
            {
                Debug.Log("Some error while trying to destroy one city of the player ???");
            }
            else
            {
                int brID = ObjectDefiner.instance.availableBuildingRecipes.IndexOf(settlementBR);
                BuildingManager.instance.SetPieceOnServer(tmp.ElementAt(Random.Range(0, tmp.Count)).codedPos, clientID, brID);
            }
        }
        TurnManager.instance.ForceEndTurn();
    }

    public void OnCasualTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.CasualRound)
            return;

        TurnManager.instance.ForceEndTurn();
    }
    public void OnBanditsTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.BanditsMoreThan7)
            return;


        Dictionary<int, int> cards = PlayerInventoriesManager.instance.getPlayersCardsInHand(clientID);
        int safeNum = BoardManager.instance.safeNumOfCardOfPlayer(clientID);

        int mySum = cards.Values.Sum();

        if (safeNum < mySum)
        {
            for (int i = 0; i < (mySum + 1) / 2; i++)
            {
                int ID = cards.Keys.ElementAt(Random.Range(0, cards.Count));
                cards[ID]--;
                if (cards[ID] == 0)
                    cards.Remove(ID);
                PlayerInventoriesManager.instance.ChangeCardQuantity(clientID, ID, -1);
            }
        }

        TurnManager.instance.ForceEndTurn();
    }
    public void OnBanditsMoveTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.BanditsMove)
            return;



        TurnManager.instance.ForceEndTurn();
    }


    public void OnSpecialCardsTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.GettingSpecialCards)
            return;



        TurnManager.instance.ForceEndTurn();
    }

}
