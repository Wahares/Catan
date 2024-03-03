using UnityEngine;
using DG.Tweening;

public class BarbariansView : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
        transform.DOScaleY(0.2f, 0.5f).SetDelay(2);
        GameManager.OnGameStarted += setactive;
    }
    private void OnDestroy()
    {
        GameManager.OnGameStarted -= setactive;
    }
    private void setactive()
    {
        gameObject.SetActive(true);
    }
    [SerializeField]
    private float min,max;
    [SerializeField]
    private RectTransform icon;
    public void setValue(float normalizedValue)
    {
        icon.DOComplete();
        icon.DOAnchorPosX(Mathf.Lerp(min, max, normalizedValue), 1);
        transform.DOComplete();
        transform.DOScaleY(1, 0.5f);
        transform.DOScaleY(0.2f, 0.5f).SetDelay(8);
    }
}
