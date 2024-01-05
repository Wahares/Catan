using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoardManager : MonoBehaviour
{
    private const float HEX_FACTOR = 1.73f;

    [SerializeField]
    private GameObject panelPrefab, borderPrefab;
    [SerializeField]
    private Transform boardRoot;

    public Dictionary<Vector2Int, PanelController> panels{ get;private set; }
    public Dictionary<Vector2Int, CornerController> corners{ get;private set; }
    public Dictionary<Vector2Int, RoadController> roads{ get;private set; }

    public void createBoard()
    {
        panels = new();
        for (int i = 0; i < 6; i++)
            Instantiate(borderPrefab, Vector3.zero, Quaternion.Euler(-90, 0, 30 + i * 60));

        List<int> diceNums = new List<int>(new int[] { 2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12 });

        Dictionary<PanelType, int> panelTypes = new();

        panelTypes.Add(PanelType.Forest, 4);
        panelTypes.Add(PanelType.Farmland, 4);
        panelTypes.Add(PanelType.Pasture, 4);
        panelTypes.Add(PanelType.ClayPit, 3);
        panelTypes.Add(PanelType.Mine, 3);


        float nextTile = 0.1f;
        for (int ring = 0; ring <= 2; ring++)
        {
            Vector3 pos = -Vector3.right*HEX_FACTOR*ring;
            float angle = 30;
            for (int index = 0; index < Mathf.Abs(6*ring-0.5f)+0.5f; index++)
            {

                GameObject go = Instantiate(panelPrefab, pos, Quaternion.Euler(-90, 0, 180));

                go.transform.position += Vector3.up * 10;
                go.transform.DOMoveY(0, 1).SetDelay(nextTile);
                nextTile += 0.05f;

                Vector2Int mapPos = new Vector2Int(ring, index);



                if (ring == 0)
                    go.GetComponent<PanelController>().Initialize(PanelType.Desert, 7, mapPos);
                else
                {
                    PanelType type = PanelType.Desert;
                    int diceNum;
                    while (type == PanelType.Desert)
                    {
                        PanelType randomType = (PanelType)Random.Range(1, 6);
                        if (panelTypes.ContainsKey(randomType))
                        {
                            if (panelTypes[randomType] > 0)
                            {
                                panelTypes[randomType]--;
                                type = randomType;
                                break;
                            }
                        }
                    }
                    int randomNum = Random.Range(0, diceNums.Count);
                    diceNum = diceNums[randomNum];
                    diceNums.RemoveAt(randomNum);
                    go.GetComponent<PanelController>().Initialize(type, diceNum, mapPos);
                }
                if (ring == 0)
                    break;
                if (index%ring==0)
                    angle -= 60;
                pos.z += Mathf.Cos(angle * Mathf.Deg2Rad)*HEX_FACTOR;
                pos.x -= Mathf.Sin(angle * Mathf.Deg2Rad)*HEX_FACTOR;
            }
        }






        /*

        for (int y = 0; y < 5; y++)
        {
            int numInRow = 5 - Mathf.Abs(2 - y);
            for (int x = 0; x < numInRow; x++)
            {
                Vector2Int mapPos = new Vector2Int(x, y);

                Vector3 pos = Vector3.zero;
                pos.x = 1.73f * (1 - numInRow) / 2 - x * -1.73f;
                pos.z = (y - 2) * 1.5f;
                GameObject go = Instantiate(panelPrefab, pos, Quaternion.Euler(-90, 0, 180));
                go.transform.parent = boardRoot;
                if (x == 2 && y == 2)
                {
                    go.GetComponent<PanelController>().Initialize(PanelType.Desert, 7, mapPos);
                }
                else
                {
                    PanelType type = PanelType.Desert;
                    int diceNum;
                    while (type == PanelType.Desert)
                    {
                        PanelType randomType = (PanelType)Random.Range(1, 6);
                        if (panelTypes.ContainsKey(randomType))
                        {
                            if (panelTypes[randomType] > 0)
                            {
                                panelTypes[randomType]--;
                                type = randomType;
                                break;
                            }
                        }
                    }
                    int randomNum = Random.Range(0, diceNums.Count);
                    diceNum = diceNums[randomNum];
                    diceNums.RemoveAt(randomNum);
                    go.GetComponent<PanelController>().Initialize(type, diceNum, mapPos);
                }


            }
        }
        */
    }



}
