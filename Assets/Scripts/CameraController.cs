using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform BoardPivot, CameraPivot;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BoardPivot.DOComplete();
            BoardPivot.DORotate(BoardPivot.eulerAngles - Vector3.up * 60, 0.1f).SetEase(Ease.InSine);
            foreach (var tile in BoardManager.instance.Tiles.Values)
                tile.transform.Rotate(0, 60, 0, Space.World);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            BoardPivot.DOComplete();
            BoardPivot.DORotate(BoardPivot.eulerAngles + Vector3.up * 60, 0.1f).SetEase(Ease.InSine);
            foreach (var tile in BoardManager.instance.Tiles.Values)
                tile.transform.Rotate(0, -60, 0, Space.World);

        }
        Vector3 desiredRot = CameraPivot.localEulerAngles;
        desiredRot.x = Input.GetKey(KeyCode.Space) ? 60 : 0;
        CameraPivot.localEulerAngles = Vector3.Lerp(CameraPivot.localEulerAngles, desiredRot, Time.deltaTime * 10);

    }
}
