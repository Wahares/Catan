using UnityEngine;
public class RecipedCard : ScriptableObject
{
    public Pair[] materials;
    [System.Serializable]
    public class Pair { public CardSO card; public int number; }
    public virtual bool CanUse(int[] inventory, int clientID)
    {
        foreach (var item in materials)
            if (inventory[item.card.ID] < item.number)
                return false;
        return true;
    }


}
