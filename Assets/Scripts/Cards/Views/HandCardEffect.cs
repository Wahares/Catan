using DG.Tweening;
using UnityEngine;

public class HandCardEffect : MonoBehaviour
{
    [SerializeField]
    protected Transform visual;
    public bool selected = false;
    private void OnMouseEnter()
    {
        if (selected)
            return;
        visual.DOComplete();
        visual.DOLocalMoveY(0.5f, 0.1f);
    }
    private void OnMouseExit()
    {
        if (selected)
            return;
        visual.DOComplete();
        visual.DOLocalMoveY(0.4f, 0.1f);
    }
    [SerializeField]
    private CardView cardView;
    private void OnMouseOver()
    {
        if (enabled)
            if (Input.GetMouseButtonDown(0))
                cardView.OnClicked();
    }
}
