using UnityEngine;

public class TradingOptionButton : MonoBehaviour, IClickable
{
    [SerializeField]
    private CardSO card;

    [SerializeField]
    private MeshRenderer rend;

    private void Awake()
    {
        rend.material = new Material(card.cardTexture);
    }

    public void OnClick()
    {
        PlayerInventoryExchangeController.instance.Clicked(card);
    }

    public void OnHoverEnd()
    {
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor",Color.black);
    }

    public void OnHoverStart()
    {
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", new Color(0.25f,0.25f,0.25f));
    }
}
