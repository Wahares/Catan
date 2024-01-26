using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform BoardPivot, CameraPivot;
    private void Update()
    {
        if (GameManager.started)
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
        }

        Vector3 desiredRot = Vector3.right * 30;
        if (!Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.D))
                desiredRot = Vector3.right * 30 + Vector3.up * 35;
            else if (Input.GetKey(KeyCode.A))
                desiredRot = Vector3.right * 30 - Vector3.up * 35;
            else if (Input.GetKey(KeyCode.S))
                desiredRot = Vector3.right * 60;
        }

        Vector3 rot = Camera.main.transform.localEulerAngles;
        Camera.main.transform.localEulerAngles = new Vector3(
            Mathf.LerpAngle(rot.x, desiredRot.x, Time.deltaTime * 10)
            , Mathf.LerpAngle(rot.y, desiredRot.y, Time.deltaTime * 10)
            , 0);

        desiredRot = CameraPivot.localEulerAngles;
        desiredRot.x = Input.GetKey(KeyCode.W) ? 60 : 0;
        CameraPivot.localEulerAngles = Vector3.Lerp(CameraPivot.localEulerAngles, desiredRot, Time.deltaTime * 10);

    }
}
