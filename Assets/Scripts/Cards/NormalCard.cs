using UnityEngine;
[CreateAssetMenu(fileName = "Normal", menuName = "Cards/Equipable/Normal Card")]
public class NormalCard : CardSO
{
    public override cardType CardType => cardType.Normal;
    public TileType sourceTile;
}
