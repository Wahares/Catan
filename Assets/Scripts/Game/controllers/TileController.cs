using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class TileController : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro coinText;
    [SerializeField]
    private Material[] materials;


    public int num;
    public TileType type;
    public Vector2Int mapPos;
    public void Initialize(TileType type, int diceNumber, Vector2Int mapPos)
    {
        this.type = type;
        num = diceNumber;
        coinText.text = diceNumber.ToString();
        GetComponent<MeshRenderer>().material = materials[(int)type];
        transform.name = $"{mapPos.x} : {mapPos.y} - [{diceNumber}] : {type}";
        this.mapPos = mapPos;
        if (type == TileType.Desert)
            Destroy(coinText.transform.parent.gameObject);
    }
    public void OnNumberRolled()
    {
        if (type == TileType.Desert)
        {
            GameManager.instance.HandleBandits();
            return;
        }
        transform.DOKill();
        transform.DOScaleZ(3, 0.5f).SetEase(Ease.InSine).OnComplete(() => transform.DOScaleZ(1, 0.5f).SetEase(Ease.OutSine));





    }

    public List<CrossingController> getNearbyPieces()
    {
        return Physics.OverlapSphere(transform.position, 2, LayerMask.GetMask("Piece"), QueryTriggerInteraction.Collide)
            .Select(p => p.GetComponent<CrossingController>())
            .Where(p => p != null).ToList();
    }





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