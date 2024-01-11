using UnityEngine;
[CreateAssetMenu(fileName = "Normal", menuName = "Cards/Normal Card")]
public class NormalCard : CardSO
{
    public override cardType CardType => cardType.Normal;
    public TileType sourceTile;
}
