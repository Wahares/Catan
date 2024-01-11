using DG.Tweening;
using UnityEngine;

public class PreparedDiceRotations : MonoBehaviour
{
    [SerializeField]
    private Vector3 rotationOffset;

    public void RotateToNumber(int number)
    {
        transform.localEulerAngles = rotationOffset;
        switch (number)
        {
            case 1:
                transform.Rotate(90, 0, 0);
                break;
            case 2:
                transform.Rotate(0, 0, 90);
                break;
            case 3:
                transform.Rotate(180, 0, 0);
                break;
            case 4:
                break;
            case 5:
                transform.Rotate(0, 0, -90);
                break;
            case 6:
                transform.Rotate(-90, 0, 0);
                break;
            default:
                break;
        }
    }
    public void snapToPreview(Transform previewPivot,Vector3 offset,int number)
    {
        switch (number)
        {
            case 1:
                transform.localEulerAngles = new Vector3 (90, 0, 0);
                break;
            case 2:
                transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
            case 3:
                transform.localEulerAngles = new Vector3(180, 0, 0);
                break;
            case 4:
                transform.localEulerAngles = Vector3.zero;
                break;
            case 5:
                transform.localEulerAngles = new Vector3(0, 0, -90);
                break;
            case 6:
                transform.localEulerAngles = new Vector3(-90, 0, 0);
                break;
            default:
                break;
        }
        transform.parent.DOComplete();
        transform.parent.DOMove(previewPivot.transform.position+ offset, 1);
        transform.parent.DORotate(previewPivot.transform.eulerAngles, 1);
    }

}
