using UnityEngine;
using UnityEngine.UI;

public class CardExchangeMaterial : MonoBehaviour
{
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TMPro.TextMeshProUGUI count;

    public void Initialize(RecipedCard.Pair material)
    {
        icon.sprite = material.card.cardIcon;
        count.text = material.number.ToString();
    }
}
