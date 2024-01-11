using DG.Tweening;
using UnityEngine;

public class HandCardEffect : MonoBehaviour
{
    [SerializeField]
    private Transform visual;
    private void OnMouseEnter()
    {
        visual.DOKill();
        visual.DOLocalMoveY(0.5f, 0.1f);
    }
    private void OnMouseExit()
    {
        visual.DOKill();
        visual.DOLocalMoveY(0.4f, 0.1f);
    }
}
