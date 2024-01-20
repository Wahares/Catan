using DG.Tweening;
using UnityEngine;

public class PreparedDiceRotations : MonoBehaviour
{
    [SerializeField]
    private Vector3 rotationOffset;
    private Transform normalParent;

    private void Awake()
    {
        normalParent = transform.parent.parent;
    }

    public void SetToThrow(int number)
    {
        transform.parent.DOKill();
        transform.parent.parent = normalParent;
        transform.parent.localPosition = Vector3.zero;
        transform.localEulerAngles = rotationOffset;
        transform.Rotate(GetRotation(number));
    }
    private Vector3 GetRotation(int number)
    {
        switch (number)
        {
            case 1:
                return new Vector3(90, 0, 0);
            case 2:
                return new Vector3(0, 0, 90);
            case 3:
                return new Vector3(180, 0, 0);
            case 5:
                return new Vector3(0, 0, -90);
            case 6:
                return new Vector3(-90, 0, 0);
        }
        return Vector3.zero;
    }
    public void snapToPreview(Transform previewPivot,Vector3 offset,int number)
    {
        transform.parent.parent = previewPivot;
        transform.localEulerAngles = GetRotation(number);
        transform.parent.DOComplete();
        transform.parent.DOLocalMove(offset, 1);
        transform.parent.DOLocalRotate(Vector3.zero, 1);
    }

}
