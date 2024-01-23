using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public abstract class RecipedCard : ScriptableObject
{
    public Pair[] materials;
    public Sprite icon;
    [System.Serializable]
    public class Pair { public CardSO card; public int number; }
    public virtual bool CanUse(List<CardSO> cards, int clientID)
    {
        Dictionary<int, int> remaining = materials.ToDictionary(m => m.card.ID, m => m.number);

        foreach (var mat in cards)
        {
            if (remaining.ContainsKey(mat.ID))
                remaining[mat.ID]--;
        }
        foreach (var entry in remaining)
        {
            if (entry.Value > 0)
                return false;
        }

        return true;
    }
    public abstract void OnUsed();


}
