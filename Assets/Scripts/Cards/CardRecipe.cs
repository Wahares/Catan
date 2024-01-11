using UnityEngine;
[CreateAssetMenu(fileName = "Recipe", menuName = "Cards/Card Recipe")]
public class CardRecipe : ScriptableObject
{
    public Pair[] materials;
    [System.Serializable]
    public class Pair { public CardSO card; public int number; }

    public bool CanBuild(int[] inventory)
    {
        foreach (var item in materials)
            if (inventory[item.card.ID] < item.number)
                return false;
        return true;
    }


}
