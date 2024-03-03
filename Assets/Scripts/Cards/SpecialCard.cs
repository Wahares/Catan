using UnityEngine;

[CreateAssetMenu(fileName = "SpecialCard", menuName = "Cards/Equipable/Special Card")]
public abstract class SpecialCard : CardSO
{
    [field: SerializeField]
    public string cardName { get; private set; }
    [field: TextArea]
    [field: SerializeField]
    public string description { get; private set; }
    [field: SerializeField]
    public int numberInDeck { get; private set; }
    [field: SerializeField]
    public growthType sourceType { get; private set; }

    public bool IsPersistent => CardViewType == cardViewType.Persistent;

    public override cardType CardType => cardType.Special;
    public abstract void OnUsed();
    public virtual bool CanUse() => TurnManager.currentPhase == Phase.CasualRound && TurnManager.isMyTurn;
}
public enum growthType
{
    Blank,
    Trade,
    Politics,
    Science
}