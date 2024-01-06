using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class CameraController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.DOComplete();
            transform.DORotate(transform.eulerAngles + Vector3.up * 60, 0.1f).SetEase(Ease.InSine);
            foreach (var tile in BoardManager.instance.Tiles.Values)
                tile.transform.Rotate(0, 60, 0, Space.World);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            transform.DOComplete();
            transform.DORotate(transform.eulerAngles - Vector3.up * 60, 0.1f).SetEase(Ease.InSine);
            foreach (var tile in BoardManager.instance.Tiles.Values)
                tile.transform.Rotate(0, -60, 0, Space.World);

        }
        Vector3 desiredRot = transform.localEulerAngles;
        desiredRot.x = Input.GetKey(KeyCode.Space) ? 60 : 0;
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, desiredRot, Time.deltaTime * 10);

    }
}
