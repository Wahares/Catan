using UnityEngine;
using DG.Tweening;
using TMPro;
public class GeneralExchangeIconVisual : MonoBehaviour
{
    [SerializeField]
    protected TextMeshProUGUI Text;
    private Color textColor;
    public virtual void setup(ExchangeGiver EG)
    {
        textColor = Text.color;
        textColor.a = 0.5f;
        Text.DOKill();
        Text.DOBlendableColor(textColor, 1).SetDelay(5);
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position, Vector3.up);
    }

    protected virtual void OnMouseEnter()
    {
        textColor.a = 1;
        Text.DOKill();
        Text.DOBlendableColor(textColor, 0.2f);
    }
    protected virtual void OnMouseExit()
    {
        textColor.a = 0.25f;
        Text.DOKill();
        Text.DOBlendableColor(textColor, 0.2f);
    }


}
