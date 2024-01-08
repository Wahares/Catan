using UnityEngine;
using TMPro;
public class TileController : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro coinText;
    [SerializeField]
    private Material[] materials;


    public int num;
    public TileType type;
    public Vector2Int mapPos;
    public void Initialize(TileType type,int diceNumber,Vector2Int mapPos)
    {
        this.type = type;
        num = diceNumber;
        coinText.text = diceNumber.ToString();
        GetComponent<MeshRenderer>().material = materials[(int)type];
        transform.name = $"{mapPos.x} : {mapPos.y} - [{diceNumber}] : {type}";
        this.mapPos = mapPos;
        if(type == TileType.Desert)
            Destroy(coinText.transform.parent.gameObject);
    }
    public virtual void OnNumberRolled() { }
}
public enum TileType
{
    Desert,
    Farmland,
    Forest,
    Pasture,
    ClayPit,
    Mine,
}