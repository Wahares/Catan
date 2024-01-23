using UnityEngine;

public class CardExchangeMaterial : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer icon;
    [SerializeField]
    private TMPro.TextMeshPro count;

    public void Initialize(RecipedCard.Pair material)
    {
        icon.sprite = material.card.cardIcon;
        count.text = material.number.ToString();
    }
}
