using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using FishNet.Object;

public class BoardManager : NetworkBehaviour
{
    private const float HEX_FACTOR = 1.73f;
    private const float TILE_DELAY = 0.05f;
    private const float TILE_FLY_SPEED = 1f;
    public static int MapSize = 3;

    [SerializeField]
    private GameObject TilePrefab, borderPrefab;
    [SerializeField]
    private Transform boardRoot;

    public Dictionary<Vector2Int, TileController> Tiles { get; private set; }
    public Dictionary<int, Action> rollActions { get; private set; }
    public Dictionary<Vector2Int, CornerController> corners { get; private set; }
    public Dictionary<Vector2Int, RoadController> roads { get; private set; }

    public Action OnBoardInitialized;

    public static BoardManager instance;
    private void Awake()
    {
        instance = this;
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

    [ObserversRpc]
    public void CreateBoardFromData(int mapSize, int[] diceNums, int[] tileTypes)
    {
        OnBoardInitialized += () => { Debug.Log("Board Initialized"); };

        Tiles = new();
        rollActions = new();


        for (int i = 0; i < 6; i++)
            Instantiate(borderPrefab, Vector3.zero, Quaternion.Euler(-90, 0, 30 + i * 60)).transform.parent = boardRoot;


        int tileID = 0;
        float nextTileDelay = TILE_DELAY * 2;
        for (int ring = 0; ring < mapSize; ring++)
        {
            Vector3 pos = -Vector3.right * HEX_FACTOR * ring;
            float angle = 30;

            for (int index = 0; index < Mathf.Abs(6 * ring - 0.5f) + 0.5f; index++)
            {
                GameObject go = Instantiate(TilePrefab, pos, Quaternion.Euler(-90, 0, 180));
                go.transform.parent = boardRoot;

                go.transform.position += Vector3.up * 10;
                go.transform.DOMoveY(0, TILE_FLY_SPEED).SetDelay(nextTileDelay);
                nextTileDelay += TILE_DELAY;

                Vector2Int mapPos = new Vector2Int(ring, index);
                Tiles.Add(mapPos, go.GetComponent<TileController>());


                if (ring == 0)
                    go.GetComponent<TileController>().Initialize(TileType.Desert, 7, mapPos);
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
        Invoke(nameof(BeginGame), nextTileDelay + TILE_FLY_SPEED);
    }

    private void BeginGame()
    {
        OnBoardInitialized?.Invoke();
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
                Gizmos.DrawCube(GetRoadPosition(new Vector2Int(i, j)), Vector3.one * 0.25f);
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

    public Vector3 GetRoadPosition(Vector2Int cords)
    {
        Vector3 pos;
        if (cords.x % 2 == 0)
            return Vector3.Lerp(GetCrossingPosition(new Vector2Int(cords.x / 2, cords.y - cords.x / 2 - 1))
                , GetCrossingPosition(new Vector2Int(cords.x / 2, cords.y - cords.x / 2)), 0.5f);

        TileController tileController = Tiles[new Vector2Int((cords.x + 1) / 2, cords.y)];

        pos = tileController.transform.position;
        int ring = (cords.x + 1) / 2;
        float angle = 60 - cords.y / ring * 60;
        pos.z += Mathf.Sin(Mathf.Deg2Rad * angle) * 1.73f / 2;
        pos.x += Mathf.Cos(Mathf.Deg2Rad * angle) * 1.73f / 2;



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
