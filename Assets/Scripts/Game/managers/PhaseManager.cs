using FishNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{

    private void Awake()
    {
        barbarians = GetComponent<BarbariansController>();
        banditC = GetComponent<BanditsController>();

        TurnManager.OnMyTurnStarted += OnMyBuildingPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyRollPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyBarbariansPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyCasualPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyBanditsPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyBanditsMovePhaseTurn;
        TurnManager.OnMyTurnStarted += OnMySpecialCardsPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyRemovingSpecialCardsPhaseTurn;
        TurnManager.OnMyTurnStarted += OnMyManagingMetropolyPhaseTurn;


        TurnManager.OnClientTimeReached += OnBuildingTimeLimitReached;
        TurnManager.OnClientTimeReached += OnRollTimeLimitReached;
        TurnManager.OnClientTimeReached += OnBarbariansTimeLimitReached;
        TurnManager.OnClientTimeReached += OnCasualTimeLimitReached;
        TurnManager.OnClientTimeReached += OnBanditsTimeLimitReached;
        TurnManager.OnClientTimeReached += OnBanditsMoveTimeLimitReached;
        TurnManager.OnClientTimeReached += OnSpecialCardsTimeLimitReached;
        TurnManager.OnClientTimeReached += OnRemovingSpecialCardsTimeLimitReached;
        TurnManager.OnClientTimeReached += OnManagingMetropolyTimeLimitReached;
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
        TurnManager.OnMyTurnStarted -= OnMyRemovingSpecialCardsPhaseTurn;
        TurnManager.OnMyTurnStarted -= OnMyManagingMetropolyPhaseTurn;


        TurnManager.OnClientTimeReached -= OnBuildingTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnRollTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnBarbariansTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnCasualTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnBanditsTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnBanditsMoveTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnSpecialCardsTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnRemovingSpecialCardsTimeLimitReached;
        TurnManager.OnClientTimeReached -= OnManagingMetropolyTimeLimitReached;
    }


    [SerializeField] private BuildingRecipe settlementBR, cityBR, roadBR;
    public void OnMyBuildingPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.FreeBuild)
            return;
        switch (TurnManager.currentPhaseArgs)
        {
            case 0:
                BuildingManager.instance.BeginBuilding(settlementBR);
                break;
            case 1:
                BuildingManager.instance.BeginBuilding(roadBR);
                break;
            case 2:
                BuildingManager.instance.BeginBuilding(cityBR);
                break;
        }
    }
    public void OnMyRollPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.BeforeRoll)
            return;
        DiceController.instance.allowToRoll();
    }

    private BarbariansController barbarians;
    public void OnMyBarbariansPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.Barbarians)
            return;
        barbarians.beginDestroying();
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
    private BanditsController banditC;
    public void OnMyBanditsMovePhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.BanditsMove)
            return;
        banditC.beginMoving();
    }
    public void OnMySpecialCardsPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.GettingSpecialCards)
            return;
        int diceNum = TurnManager.currentPhaseArgs & ((1 << 4) - 1);
        growthType type = (growthType)(TurnManager.currentPhaseArgs >> 3);
        if (CommodityUpgradeManager.instance.getUpgradeLevel(InstanceFinder.ClientManager.Connection.ClientId, type) + 1 >= diceNum)
            PlayerInventoriesManager.instance.RollMeSpecialCard(type);
        TurnManager.instance.endTurn();
    }
    public void OnMyRemovingSpecialCardsPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.RemovingSpecialCards)
            return;
        int diceNum = TurnManager.currentPhaseArgs & ((1 << 4) - 1);
        growthType type = (growthType)(TurnManager.currentPhaseArgs >> 3);
        if (CommodityUpgradeManager.instance.getUpgradeLevel(InstanceFinder.ClientManager.Connection.ClientId, type) + 1 >= diceNum)
        {
            if (PlayerInventoriesManager.instance.playerNumberOfSpecialCards(InstanceFinder.ClientManager.Connection.ClientId) == 3)
                CardChoiceManager.instance.CreateChoice(
            "Choose card to give off"
            , PlayerInventoriesManager.instance.playerSpecialCards(InstanceFinder.ClientManager.Connection.ClientId)
            , 1
            , (e) => { PlayerInventoriesManager.instance.destroyMySpecialCard(e[0].ID); TurnManager.instance.endTurn(); }
            , null
            , null
            ,false);
            else
                TurnManager.instance.endTurn();
        }
        else
            TurnManager.instance.endTurn();
    }

    public void OnMyManagingMetropolyPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.ManagingMetropoly)
            return;
        FindAnyObjectByType<MetropolyController>().beginBuilding();
    }
    public void OnMyGettingAdditionalCardPhaseTurn()
    {
        if (TurnManager.currentPhase != Phase.GettingAdditionalCard)
            return;
        CardChoiceManager.instance.CreateChoice(
            "Pick additional card to get:"
            , ObjectDefiner.instance.basicCards.Select(e=>e as CardSO).ToList()
            , 1
            , (e) => { PlayerInventoriesManager.instance.ChangeMyCardsQuantity(e[0].ID, 1); }
            , null
            , () => { PlayerInventoriesManager.instance.ChangeMyCardsQuantity(ObjectDefiner.instance.basicCards[Random.Range(0,ObjectDefiner.instance.basicCards.Count)].ID, 1); }
            , false);
    }












    public void OnBuildingTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.FreeBuild)
            return;

        for (int i = 0; i < 9999; i++)
        {

            CrossingController cc = BoardManager.instance.crossings.Values.ElementAt(Random.Range(0, BoardManager.instance.crossings.Count));
            if (cc.currentPiece != null)
                continue;

            bool did = false;
            switch (TurnManager.currentPhaseArgs)
            {
                case 0:
                    BuildingManager.instance.SetPieceOnServer(cc.pos, clientID, ObjectDefiner.instance.availableBuildingRecipes.IndexOf(settlementBR));
                    break;
                case 1:
                    var potentialCrossings = BoardManager.instance
                        .crossings.Values
                        .Where(e => ((e.currentPiece?.pieceType ?? PieceType.Unset) == PieceType.Settlement
                        || (e.currentPiece?.pieceType ?? PieceType.Unset) == PieceType.City)
                        && (e.currentPiece?.pieceOwnerID ?? -1) == clientID)
                        .ToList();
                    cc = potentialCrossings.ElementAt(Random.Range(0, potentialCrossings.Count));
                    List<RoadController> roadsControllers = cc.GetRoadsControllers();
                    foreach (var road in roadsControllers)
                    {
                        if (road.currentPiece == null)
                        {
                            BuildingManager.instance.SetPieceOnServer(road.pos, clientID, ObjectDefiner.instance.availableBuildingRecipes.IndexOf(roadBR));
                            did = true;
                            break;
                        }
                    }
                    break;
                case 2:
                    BuildingManager.instance.SetPieceOnServer(cc.pos, clientID, ObjectDefiner.instance.availableBuildingRecipes.IndexOf(cityBR));
                    break;
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
        
        int diceNum = TurnManager.currentPhaseArgs & ((1 << 4) - 1);
        growthType type = (growthType)(TurnManager.currentPhaseArgs >> 3);

        if (CommodityUpgradeManager.instance.getUpgradeLevel(clientID, type) + 1 >= diceNum)
            if (PlayerInventoriesManager.instance.playerNumberOfSpecialCards(InstanceFinder.ClientManager.Connection.ClientId) == 3)
                PlayerInventoriesManager.instance.giveRandomSpecial(clientID, type);

        TurnManager.instance.ForceEndTurn();
        
    }
    public void OnRemovingSpecialCardsTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.RemovingSpecialCards)
            return;
        int diceNum = TurnManager.currentPhaseArgs & ((1 << 4) - 1);
        growthType type = (growthType)(TurnManager.currentPhaseArgs >> 3);

        if (CommodityUpgradeManager.instance.getUpgradeLevel(clientID, type) + 1 >= diceNum)
            if (PlayerInventoriesManager.instance.playerNumberOfSpecialCards(InstanceFinder.ClientManager.Connection.ClientId) == 3)
                PlayerInventoriesManager.instance.removeRandomSpecial(clientID, type);
        TurnManager.instance.ForceEndTurn();
    }


    public void OnManagingMetropolyTimeLimitReached(int clientID, Phase endedPhase)
    {
        if (endedPhase != Phase.ManagingMetropoly)
            return;
        List<CityController> cities = new();

        foreach (var cc in BoardManager.instance.crossings.Values)
        {
            if (cc.currentPiece == null)
                continue;
            if (cc.currentPiece.pieceOwnerID != clientID)
                continue;
            if (cc.currentPiece.pieceType != PieceType.City)
                continue;
            cities.Add(cc.currentPiece.GetComponent<CityController>());
        }

        if (TurnManager.currentPhaseArgs == 0)
            Debug.LogError("Wrong type of parameter!!!");

        bool building = TurnManager.currentPhaseArgs < 0;

        cities = cities.Where(e => e.isMetropoly != building).ToList();
        if (cities.Count != 0)
        {
            Vector2Int pos = cities[Random.Range(0, cities.Count)].codedPos;
            BoardManager.instance.SetCityMetropoly(pos, building);
        }
        else
            Debug.LogError("Didn't found any cities (???)");

        TurnManager.instance.ForceEndTurn();
    }

}
