using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class BoardManager : MonoBehaviour
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
    public Dictionary<Vector2Int, CornerController> corners { get; private set; }
    public Dictionary<Vector2Int, RoadController> roads { get; private set; }

    public Action OnBoardInitialized;

    public static BoardManager instance;
    private void Awake()
    {
        instance = this;
    }

    public bool GenerateData(int mapSize, out int[] diceNums, out int[] tileTypes)
    {
        int NumOfTiles = 3 * mapSize * (mapSize - 1); //minus desert
        diceNums = new int[NumOfTiles];
        tileTypes = new int[NumOfTiles];

        List<int> diceNumsList = new List<int>(new int[] { 2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12 });

        Dictionary<TileType, int> TileTypes = new();

        TileTypes.Add(TileType.Forest, 4);
        TileTypes.Add(TileType.Farmland, 4);
        TileTypes.Add(TileType.Pasture, 4);
        TileTypes.Add(TileType.ClayPit, 3);
        TileTypes.Add(TileType.Mine, 3);

        int numOfTypes = 0;
        foreach (var value in TileTypes.Values)
            numOfTypes += value;

        if (numOfTypes != NumOfTiles || diceNumsList.Count != NumOfTiles)
        {
            Debug.LogError($"board numbers don't match! -> {NumOfTiles} {numOfTypes} {diceNumsList.Count}");
            return false;
        }
        for (int i = 0; i < NumOfTiles; i++)
        {
            TileType type = TileType.Desert;
            while (type == TileType.Desert)
            {
                TileType randomType = (TileType)UnityEngine.Random.Range(1, 6);
                if (TileTypes.ContainsKey(randomType))
                {
                    if (TileTypes[randomType] > 0)
                    {
                        TileTypes[randomType]--;
                        tileTypes[i] = (int)randomType;
                        break;
                    }
                }
            }
            int randomNum = UnityEngine.Random.Range(0, diceNumsList.Count);
            diceNums[i] = diceNumsList[randomNum];
            diceNumsList.RemoveAt(randomNum);
        }
        return true;
    }

    public void CreateBoardFromData(int mapSize, int[] diceNums, int[] tileTypes)
    {
        OnBoardInitialized += () => { Debug.Log("Board Initialized"); };

        Tiles = new();

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

    public void createBoard()
    {
        int[] diceNums, tileTypes;
        if (GenerateData(MapSize, out diceNums, out tileTypes))
            CreateBoardFromData(MapSize, diceNums, tileTypes);
    }
    private void BeginGame()
    {
        OnBoardInitialized?.Invoke();
    }
    private void OnDrawGizmosSelected()
    {
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

        int offset = (cords.x) / 2 + cords.x + 1;

        pos.x = Mathf.Cos(Mathf.Deg2Rad * 150) * offset;
        pos.z = Mathf.Sin(Mathf.Deg2Rad * 150) * offset;

        int numOfCrossings = CrossingsInRing(cords.x);

        cords.y = (cords.y + numOfCrossings) % numOfCrossings;

        bool inwardRing = cords.x % 2 == 1;

        int numToTurnRight = cords.x * 2 + 1;

        bool goRight = !inwardRing;
        float angle = inwardRing ? 90 : 30;

        int movesToDo = cords.y;
        for (int i = 0; i < 7; i++)
        {
            int move = numToTurnRight;
            if (i == 0 && cords.x != 0)
                move = (cords.x + 1);
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
