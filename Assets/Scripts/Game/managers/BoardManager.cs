using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;
using FishNet.Object;
using FishNet;

public class BoardManager : NetworkBehaviour
{
    private const float HEX_FACTOR = 1.73f;
    private const float TILE_DELAY = 0.05f;
    private const float TILE_FLY_SPEED = 1f;
    public static int MapSize = 3;
    public static bool RandomizeTradingPorts = false;

    [SerializeField]
    private GameObject tilePrefab, borderPrefab, borderFillerPrefab, crossingPrefab, roadPrefab;
    [SerializeField]
    private Transform boardRoot;

    public Dictionary<Vector2Int, TileController> Tiles { get; private set; }
    public Dictionary<int, Action> rollActions { get; private set; }
    public Dictionary<Vector2Int, CrossingController> crossings { get; private set; }
    public Dictionary<Vector2Int, RoadController> roads { get; private set; }

    public static event Action OnBoardInitialized;

    public static BoardManager instance;

    public Vector2Int currentBanditPos = new Vector2Int(-1, -1);
    [SerializeField]
    private GameObject bandits;

    public int currentBarbariansPos { get; private set; }
    public readonly int numberOfBarbariansFields = 2;//8
    [SerializeField]
    private BarbariansView barbarians;

    private void Awake()
    {
        instance = this;
        currentBarbariansPos = 0;
        currentBanditPos = new Vector2Int(-1, -1);
        bandits.transform.position = Vector3.down * 10;
        DiceController.instance.OnDiceRolled += UseDiceData;
        TurnManager.OnAnyTurnStarted += MakeSettlementsAbleToGiveTradings;
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        GameManager.OnGameStarted += createBoard;
    }
    private void OnDestroy()
    {
        TurnManager.OnAnyTurnStarted -= MakeSettlementsAbleToGiveTradings;
        OnBoardInitialized = null;
    }
    public void createBoard()
    {
        int[] diceNums, tileTypes, tradingPorts;
        if (TryGenerateDataProcedurally(MapSize, out diceNums, out tileTypes, out tradingPorts, RandomizeTradingPorts))
            CreateBoardFromData(MapSize, diceNums, tileTypes, tradingPorts);
    }
    public bool TryGenerateDataProcedurally(int mapSize, out int[] diceNums, out int[] tileTypes, out int[] tradingPorts, bool randomizePorts)
    {
        int numberOfTiles = TilesOnBoard(mapSize) - 1;
        tileTypes = new int[numberOfTiles];
        diceNums = new int[numberOfTiles];

        tradingPorts = new int[CrossingsInRing(mapSize)];
        for (int i = 0; i < tradingPorts.Length; i++)
            tradingPorts[i] = -1;

        List<TileType> availableTiles = new();

        for (int i = 0; i < numberOfTiles * 2 / 3; i++)
            availableTiles.Add(TileType.Farmland + i % 3);
        for (int i = 0; i < numberOfTiles / 3; i++)
            availableTiles.Add(TileType.ClayPit + i % 2);
        for (int i = 0; i < numberOfTiles - availableTiles.Count; i++)
            availableTiles.Add(TileType.Farmland + i % 5);

        List<int> availableDiceNums = new();
        for (int i = 0; i < numberOfTiles / 10 + 1; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (availableDiceNums.Count >= numberOfTiles)
                    break;
                availableDiceNums.Add(8 + j);
                availableDiceNums.Add(6 - j);
            }
        }
        for (int i = 0; i < numberOfTiles; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableTiles.Count);
            tileTypes[i] = (int)availableTiles[randomIndex];
            availableTiles.RemoveAt(randomIndex);
            randomIndex = UnityEngine.Random.Range(0, availableDiceNums.Count);
            diceNums[i] = availableDiceNums[randomIndex];
            availableDiceNums.RemoveAt(randomIndex);
        }
        if (diceNums.Length != numberOfTiles || tileTypes.Length != numberOfTiles)
            throw new Exception("Wrong sizes of tile data containers!");

        for (int i = 0; i < numberOfTiles; i++)
            if (diceNums[i] == 0 || tileTypes[i] == 0)
                throw new Exception("Board data containers not fully generated!");

        //generate port data ------------------------------------

        List<TileType> materialsForPorts = new List<TileType> { TileType.ClayPit, TileType.Forest, TileType.Desert, TileType.Farmland, TileType.Mine, TileType.Desert, TileType.Pasture, TileType.Desert, TileType.Desert };

        int optimalSpacing = (int)(30f / CrossingsInRing(mapSize));
        int spacingsLeft = CrossingsInRing(mapSize) - materialsForPorts.Count * (2 + optimalSpacing);
        int currentIndex = 0;
        while (currentIndex != CrossingsInRing(mapSize))
        {
            tradingPorts[currentIndex] = (int)materialsForPorts[0];
            currentIndex++;
            tradingPorts[currentIndex] = (int)materialsForPorts[0];
            currentIndex++;
            materialsForPorts.RemoveAt(0);
            for (int i = 0; i < optimalSpacing; i++)
            {
                tradingPorts[currentIndex] = -1;
                currentIndex++;
            }
            Debug.Log("terefere");
            if (spacingsLeft > 0) //tutaj pewnie coœ nie gra
            {
                tradingPorts[currentIndex] = -1;
                currentIndex++;
            }

            tradingPorts[currentIndex] = (int)materialsForPorts[0];
            currentIndex++;
            tradingPorts[currentIndex] = (int)materialsForPorts[0];
            currentIndex++;
            materialsForPorts.RemoveAt(0);
            for (int i = 0; i < optimalSpacing; i++)
            {
                tradingPorts[currentIndex] = -1;
                currentIndex++;
            }
        }


        return true;
    }
    [ObserversRpc]
    public void CreateBoardFromData(int mapSize, int[] diceNums, int[] tileTypes, int[] tradingPorts)
    {
        OnBoardInitialized += () => { Debug.Log("Board Initialized"); };

        Tiles = new();
        rollActions = new();

        //borders
        Vector3 borderPos = -Vector3.right * HEX_FACTOR * mapSize;
        float borderAngle = 30;
        for (int i = 0; i < TilesInRing(mapSize); i++)
        {
            if (i % mapSize == 0)
            {
                Instantiate(borderFillerPrefab, borderPos, Quaternion.Euler(0, i / MapSize * 60, 0)).transform.parent = boardRoot;
                borderAngle -= 60;
            }
            else
                Instantiate(borderPrefab, borderPos, Quaternion.Euler(0, i / MapSize * 60, 0)).transform.parent = boardRoot;

            borderPos.z += Mathf.Cos(borderAngle * Mathf.Deg2Rad) * HEX_FACTOR;
            borderPos.x -= Mathf.Sin(borderAngle * Mathf.Deg2Rad) * HEX_FACTOR;
        }


        //tiles
        int tileID = 0;
        float nextTileDelay = TILE_DELAY * 2;
        for (int ring = 0; ring < mapSize; ring++)
        {
            Vector3 pos = -Vector3.right * HEX_FACTOR * ring;
            float angle = 30;

            for (int index = 0; index < TilesInRing(ring); index++)
            {
                GameObject go = Instantiate(tilePrefab, pos, Quaternion.Euler(-90, 0, 180));
                go.transform.parent = boardRoot;

                go.transform.position += Vector3.up * 10;
                go.transform.DOMoveY(0, TILE_FLY_SPEED).SetDelay(nextTileDelay);
                nextTileDelay += TILE_DELAY;

                Vector2Int mapPos = new Vector2Int(ring, index);
                Tiles.Add(mapPos, go.GetComponent<TileController>());


                if (ring == 0)
                {
                    go.GetComponent<TileController>().Initialize(TileType.Desert, 7, mapPos);

                    rollActions.Add(7, null);
                    rollActions[7] += go.GetComponent<TileController>().OnNumberRolled;

                }
                else
                {
                    int diceNum = diceNums[tileID];
                    TileType type = (TileType)tileTypes[tileID];
                    go.GetComponent<TileController>().Initialize(type, diceNum, mapPos);

                    if (!rollActions.ContainsKey(diceNum))
                        rollActions.Add(diceNum, null);
                    rollActions[diceNum] += go.GetComponent<TileController>().OnNumberRolled;

                    tileID++;
                }



                if (ring == 0)
                    break;
                if (index % ring == 0)
                    angle -= 60;
                pos.z += Mathf.Cos(angle * Mathf.Deg2Rad) * HEX_FACTOR;
                pos.x -= Mathf.Sin(angle * Mathf.Deg2Rad) * HEX_FACTOR;
            }
        }

        //crossings and roads
        crossings = new();
        roads = new();
        for (int i = 0; i < MapSize; i++)
        {
            for (int j = 0; j < CrossingsInRing(i); j++)
            {
                GameObject obj = Instantiate(crossingPrefab, boardRoot);
                obj.transform.position = GetCrossingPosition(new Vector2Int(i, j));
                crossings.Add(new Vector2Int(i, j), obj.GetComponent<CrossingController>().Initialize(new Vector2Int(i, j)));
            }
        }
        for (int i = 0; i < MapSize * 2 - 1; i++)
        {
            for (int j = 0; j < RoadsInRing(i); j++)
            {
                GameObject obj = Instantiate(roadPrefab, boardRoot);
                float angle = 0;
                obj.transform.position = GetRoadPosition(new Vector2Int(i, j), out angle);
                obj.transform.eulerAngles = Vector3.up * angle;
                roads.Add(new Vector2Int(i, j), obj.GetComponent<RoadController>().Initialize(new Vector2Int(i, j)));
            }
        }

        //ports  ------------------------------------------------------------------



        Invoke(nameof(BeginGame), nextTileDelay + TILE_FLY_SPEED);
    }

    public bool IsTileBlockedByBandits(Vector2Int pos) => pos == currentBanditPos;
    public void DoBanditsEffect() { bandits.transform.DOComplete(); bandits.transform.DOShakeRotation(0.5f, 1f); }
    public bool banditsExsist() => currentBanditPos != new Vector2Int(-1, -1);

    [ServerRpc(RequireOwnership = false)]
    public void moveBanditsOnServer(Vector2Int newPos, int moverID)
    {
        moveBandits(newPos);
        if (moverID == -1)
            return;
        List<SettlementController> settlements = Tiles[newPos]
            .getNearbyCrossings()
            .Select(e => e.GetComponent<SettlementController>())
            .Where(e => e != null)
            .ToList();
        foreach (var settlement in settlements)
        {
            if (settlement.pieceOwnerID != moverID)
            {
                Dictionary<int, int> cardsInHand = PlayerInventoriesManager.instance.getPlayersCardsInHand(settlement.pieceOwnerID);
                if (cardsInHand.Count != 0)
                {
                    int id = cardsInHand.ElementAt(UnityEngine.Random.Range(0, cardsInHand.Count)).Key;
                    PlayerInventoriesManager.instance.ChangeCardQuantity(settlement.pieceOwnerID, id, -1);
                    PlayerInventoriesManager.instance.ChangeCardQuantity(moverID, id, 1);
                }
            }
        }
    }

    [ObserversRpc]
    private void moveBandits(Vector2Int newPos)
    {
        currentBanditPos = newPos;
        bandits.transform.DOComplete();
        bandits.transform.DOJump(Tiles[newPos].transform.position, 0.5f, 1, 0.25f);
    }

    public void moveBarbariansOnServer() { currentBarbariansPos = (currentBarbariansPos + 1) % numberOfBarbariansFields; moveBarbarians(currentBarbariansPos); }
    [ObserversRpc]
    public void moveBarbarians(int cPos) { currentBarbariansPos = cPos; barbarians.setValue((float)currentBarbariansPos / numberOfBarbariansFields); }
    [Server]
    public void SetCityMetropoly(Vector2Int pos, bool isMetropoly)
    {
        SetCityMetropoly(pos, isMetropoly);
    }
    [ObserversRpc]
    private void SetCityMetropolyRPC(Vector2Int pos, bool ismetropoly)
    {
        if (ismetropoly)
            (crossings[pos].currentPiece as CityController).makeItMetropoly();
        else
            (crossings[pos].currentPiece as CityController).destroyMetropoly();
    }



    private void BeginGame()
    {
        OnBoardInitialized?.Invoke();
    }

    private void UseDiceData(int basic, int red, diceActions action)
    {
        rollActions[basic + red]?.Invoke();
    }


    public int numberOfPieces(int ownerID, PieceType type)
    {
        if (type == PieceType.Unset)
            return -1;
        int tmp = 0;
        if (type == PieceType.Road)
        {
            foreach (var road in roads)
            {
                if (road.Value.currentPiece == null)
                    continue;
                if (road.Value.currentPiece.pieceOwnerID == ownerID && road.Value.currentPiece.pieceType == type)
                    tmp++;
            }
        }
        else
        {
            foreach (var crossing in crossings)
            {
                if (crossing.Value.currentPiece == null)
                    continue;
                if (crossing.Value.currentPiece.pieceOwnerID == ownerID && crossing.Value.currentPiece.pieceType == type)
                    tmp++;
            }
        }
        return tmp;
    }
    public void setPiece(Vector2Int pos, SinglePieceController spc)
    {
        if (spc.placeType == PiecePlaceType.Road)
            roads[pos].SetPiece(spc as SingleRoadController);
        else
            if (spc.placeType == PiecePlaceType.Crossing)
            crossings[pos].SetPiece(spc);
    }
    public SinglePieceController getPiece(Vector2Int pos, PiecePlaceType type)
    {
        if (type == PiecePlaceType.Road)
            return roads[pos].currentPiece;
        else
            if (type == PiecePlaceType.Crossing)
            return crossings[pos].currentPiece;
        return null;
    }

    public int mySafeNumOfCards()
    {
        return safeNumOfCardOfPlayer(LocalConnection.ClientId);
    }
    public int safeNumOfCardOfPlayer(int clientID)
    {
        int sum = 7;
        foreach (var cross in crossings)
        {
            if (cross.Value.currentPiece == null)
                continue;
            if (cross.Value.currentPiece.pieceOwnerID != clientID)
                continue;
            if (cross.Value.currentPiece.pieceType != PieceType.City)
                continue;
            if ((cross.Value.currentPiece as CityController)?.hasWalls ?? false)
                sum += 2;
        }
        return sum;
    }

    public int barbariansPower()
    {
        List<CityController> cities = crossings.Values
            .Select(e => { return e?.currentPiece?.GetComponent<CityController>() ?? null; })
            .Where(e => e != null).ToList();
        return cities.Count;
    }
    public int KnightPowerOfPlayer(int clientID)
    {
        return GetPlayerPieces<KnightController>(clientID, PiecePlaceType.Crossing).Sum(e => e.isMobilized ? 1 : 0);
    }
    public bool areWeInDanger()
    {
        int sum = 0;
        foreach (var player in PlayerManager.instance.playerColors)
            sum += KnightPowerOfPlayer(player.Key);
        return sum < barbariansPower();
    }
    public List<int> currentPlayersInDanger() //players in danger of barbarians
    {
        Dictionary<int, int> playersPower = new();

        int lowest = 999;

        foreach (var player in PlayerManager.instance.playerColors)
        {
            bool canBeInDaner = false;
            foreach (var city in GetPlayerPieces<CityController>(player.Key, PiecePlaceType.Crossing))
                if (!city.isMetropoly)
                    canBeInDaner = true;

            int power = KnightPowerOfPlayer(player.Key);

            if (power < lowest)
                lowest = power;

            if (canBeInDaner)
                playersPower.Add(player.Key, power);

        }
        return playersPower.Where(e => e.Value == lowest).Select(e => e.Key).ToList();
    }
    private void MakeSettlementsAbleToGiveTradings(int clientID, Phase phase)
    {
        foreach (var crossing in crossings)
        {
            SinglePieceController spc = crossing.Value.currentPiece;
            if (spc == null)
                continue;
            if (spc.pieceOwnerID != clientID)
                continue;
            (spc as SettlementController)?.GiveTradingPermit();
        }
    }

    public List<T> GetPlayerPieces<T>(int clientID, PiecePlaceType type)
        where T : SinglePieceController
    {
        switch (type)
        {
            case PiecePlaceType.Crossing:
                return crossings.Values
                    .Select(e => e.currentPiece?.GetComponent<T>())
                    .Where(e => e != null).Where(e => e.pieceOwnerID == clientID).ToList();
            case PiecePlaceType.Road:
                return roads.Values
                    .Select(e => e.currentPiece?.GetComponent<T>())
                    .Where(e => e != null).Where(e => e.pieceOwnerID == clientID).ToList();
            default:
                return new List<T>();
        }
    }


    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(GetCrossingPosition(getThis), Vector3.one * 0.25f);
        if (Tiles == null)
            return;
        Gizmos.color = Color.green;
        for (int i = 0; i < MapSize * 2 - 1; i++)
        {
            for (int j = 0; j < RoadsInRing(i); j++)
                Gizmos.DrawCube(GetRoadPosition(new Vector2Int(i, j), out float angle), Vector3.one * 0.25f);
        }
        Gizmos.color = Color.red;
        for (int i = 0; i < MapSize; i++)
        {
            for (int j = 0; j < CrossingsInRing(i); j++)
                Gizmos.DrawCube(GetCrossingPosition(new Vector2Int(i, j)), Vector3.one * 0.4f);
        }
    }

    public Vector2Int getThis;


    public Vector3 GetCrossingPosition(Vector2Int cords)
    {
        Vector3 pos = Vector3.zero;

        int offset = 1 + cords.x * 2;//(cords.x) / 2 + cords.x + 1;

        pos.x = Mathf.Cos(Mathf.Deg2Rad * 150) * offset;
        pos.z = Mathf.Sin(Mathf.Deg2Rad * 150);

        int numOfCrossings = CrossingsInRing(cords.x);

        cords.y = (cords.y + numOfCrossings) % numOfCrossings;


        int numToTurnRight = cords.x * 2 + 1;

        bool goRight = true;
        float angle = goRight ? 30 : 90;

        int movesToDo = cords.y;
        for (int i = 0; i < 7; i++)
        {
            int move = numToTurnRight;

            for (int j = 0; j < move; j++)
            {
                if (movesToDo == 0)
                    return pos;
                if (j == move - 1)
                    goRight = false;
                pos.z += Mathf.Sin(Mathf.Deg2Rad * angle);
                pos.x += Mathf.Cos(Mathf.Deg2Rad * angle);
                angle += !goRight ? -60 : 60;
                goRight = !goRight;
                movesToDo--;
                if (movesToDo == 0)
                    return pos;
            }
        }
        Debug.LogError("Unable to find position for " + cords.ToString());
        return Vector3.zero;
    }

    public Vector3 GetRoadPosition(Vector2Int cords, out float roadAngle)
    {
        Vector3 pos;
        if (cords.x % 2 == 0)
        {
            Vector3 pos1 = GetCrossingPosition(new Vector2Int(cords.x / 2, cords.y - cords.x / 2 - 1));
            Vector3 pos2 = GetCrossingPosition(new Vector2Int(cords.x / 2, cords.y - cords.x / 2));
            pos1.y = 0;
            pos2.y = 0;
            roadAngle = Vector3.SignedAngle(Vector3.forward, pos2 - pos1, Vector3.up);
            return Vector3.Lerp(pos1, pos2, 0.5f);
        }

        TileController tileController = Tiles[new Vector2Int((cords.x + 1) / 2, cords.y)];

        pos = tileController.transform.localPosition;
        pos.y = 0;
        int ring = (cords.x + 1) / 2;
        float angle = 60 - cords.y / ring * 60;
        pos.z += Mathf.Sin(Mathf.Deg2Rad * angle) * 1.73f / 2;
        pos.x += Mathf.Cos(Mathf.Deg2Rad * angle) * 1.73f / 2;
        Vector3 tilepos = tileController.transform.localPosition;
        tilepos.y = 0;
        roadAngle = Vector3.SignedAngle(Vector3.forward, pos - tilepos, Vector3.up) + 90;

        return pos;
    }

    public int TilesInRing(int ringIndex) => ringIndex == 0 ? 1 : ringIndex * 6;
    public int TilesOnBoard(int size) => 1 + 3 * size * (size - 1);
    public int CrossingsInRing(int ringIndex) => 6 * (ringIndex * 2 + 1);
    public int RoadsInRing(int ringIndex)
    {
        if (ringIndex % 2 == 0)
            return CrossingsInRing(ringIndex / 2);
        return TilesInRing((ringIndex + 1) / 2);
    }





}
