using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class BoardManager : MonoBehaviour
{
    private const float HEX_FACTOR = 1.73f;
    private const float TILE_DELAY = 0.05f;
    private const float TILE_FLY_SPEED = 1f;
    private const int MAP_SIZE = 3;

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

    public bool CreateData(int mapSize, out int[] diceNums, out int[] tileTypes)
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
        if(CreateData(MAP_SIZE,out diceNums, out tileTypes))
            CreateBoardFromData(MAP_SIZE, diceNums, tileTypes);
    }
    private void BeginGame()
    {
        OnBoardInitialized?.Invoke();
    }
}
