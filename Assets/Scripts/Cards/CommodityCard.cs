using UnityEngine;
[CreateAssetMenu(fileName = "Commodity", menuName = "Cards/Equipable/Commodity Card")]
public class CommodityCard : CardSO
{
    public override cardType CardType => cardType.Commodity;
    public TileType sourceTile;
}
