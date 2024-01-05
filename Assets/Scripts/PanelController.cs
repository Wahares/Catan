using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PanelController : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro coinText;
    [SerializeField]
    private Material[] materials;


    public int num;
    public PanelType type;
    public Vector2Int mapPos;
    public void Initialize(PanelType type,int diceNumber,Vector2Int mapPos)
    {
        this.type = type;
        num = diceNumber;
        coinText.text = diceNumber.ToString();
        GetComponent<MeshRenderer>().material = materials[(int)type];
        transform.name = $"{mapPos.x} : {mapPos.y} - [{diceNumber}] : {type}";
        this.mapPos = mapPos;
        if(type == PanelType.Desert)
            Destroy(coinText.transform.parent.gameObject);
    }
}
public enum PanelType
{
    Desert,
    Forest,
    Farmland,
    Pasture,
    ClayPit,
    Mine,
}