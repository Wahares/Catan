using UnityEngine;

public abstract class CardSO : ScriptableObject
{
    public Material cardTexture;
    public int ID => PlayerInventoriesManager.instance.availableCards.IndexOf(this);
    public abstract cardType CardType { get; }
    public cardViewType CardViewType;
}
public enum cardType
{
    Normal,
    Commodity,
    Special,
}
public enum cardViewType
{
    Normal,
    Special,
    Persistent
}