using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SpecialCard", menuName = "Cards/Equipable/Special Card")]
public class SpecialCard : CardSO
{
    [field: SerializeField]
    public string cardName { get; private set; }
    [field:TextArea]
    [field: SerializeField]
    public string description { get; private set; }
    [field: SerializeField]
    public int numberInDeck { get; private set; }
    [field: SerializeField]
    public growthType sourceType { get; private set; }
    [field: SerializeField]
    public bool IsPersistent { get; private set; }

    public override cardType CardType => cardType.Special;

    [SerializeField]
    private UnityEvent<int> OnUse;
    public void use(int clientID)
    {
        OnUse?.Invoke(clientID);
    }

}
public enum growthType
{
    Blank,
    Trade,
    Politics,
    Science
}