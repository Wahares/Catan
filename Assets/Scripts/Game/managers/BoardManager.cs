using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using FishNet.Object;
using FishNet;

public class BoardManager : NetworkBehaviour
{
    private const float HEX_FACTOR = 1.73f;
    private const float TILE_DELAY = 0.05f;
    private const float TILE_FLY_SPEED = 1f;
    public static int MapSize = 3;

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

    public Vector2Int currentBanditPos;


    private void Awake()
    {
        instance = this;
        DiceController.instance.OnDiceRolled += UseDiceData;
        if (!InstanceFinder.NetworkManager.IsServer)
            return;
        GameManager.OnGameStarted += createBoard;
        currentBanditPos = -Vector2Int.right;
    }
    private void OnDestroy()
    {
        OnBoardInitialized = null;
    }
    public void createBoard()
    {
        int[] diceNums, tileTypes;
        if (TryGenerateDataProcedurally(MapSize, out diceNums, out tileTypes))
            CreateBoardFromData(MapSize, diceNums, tileTypes);
    }
    public bool TryGenerateDataProcedurally(int mapSize, out int[] diceNums, out int[] tileTypes)
    {
        int numberOfTiles = TilesOnBoard(mapSize) - 1;
        tileTypes = new int[numberOfTiles];
        diceNums = new int[numberOfTiles];


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

        return true;
    }
    public bool IsTileBlockedByBandits(Vector2Int pos) => pos == currentBanditPos;

    [ObserversRpc]
    public void CreateBoardFromData(int mapSize, int[] diceNums, int[] tileTypes)
    {
        OnBoardInitialized += () => { Debug.Log("Board Initialized"); };

        Tiles = new();
        rollActions = new();


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
                    if (NetworkManager?.IsServer ?? true)
                    {
                        rollActions.Add(7, null);
                        rollActions[7] += go.GetComponent<TileController>().OnNumberRolled;
                    }
                }
                else
                {
                    int diceNum = diceNums[tileID];
                    TileType type = (TileType)tileTypes[tileID];
                    go.GetComponent<TileController>().Initialize(type, diceNum, mapPos);
                    if (NetworkManager?.IsServer ?? true)
                    {
                        if (!rollActions.ContainsKey(diceNum))
                            rollActions.Add(diceNum, null);
                        rollActions[diceNum] += go.GetComponent<TileController>().OnNumberRolled;
                    }
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



        Invoke(nameof(BeginGame), nextTileDelay + TILE_FLY_SPEED);
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
        if (spc.pieceType == PieceType.Road)
            roads[pos].SetPiece(spc as SingleRoadController);
        else
            crossings[pos].SetPiece(spc);
    }
    public SinglePieceController getPiece(Vector2Int pos, PieceType type)
    {
        if (type == PieceType.Road)
            return roads[pos].currentPiece;
        else
            return crossings[pos].currentPiece;
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
