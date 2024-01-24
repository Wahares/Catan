using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    private Camera cam;
    void Awake()
    {
        cam = Camera.main;
    }

    public PiecePlaceType currentFocusPieceType;

    void Update()
    {
        LayerMask mask = 0;
        switch (currentFocusPieceType)
        {
            case PiecePlaceType.Crossing:
                mask = LayerMask.GetMask("Crossing");
                break;
            case PiecePlaceType.Road:
                mask = LayerMask.GetMask("Road");
                break;
            case PiecePlaceType.TileMiddle:
                mask = LayerMask.GetMask("Tile");
                break;
        }
        if (Physics.SphereCast(cam.ScreenPointToRay(Input.mousePosition)
            , 0.1f
            , out RaycastHit hit
            , 999
            , mask
            , QueryTriggerInteraction.Collide))
        {

            Debug.DrawLine(hit.transform.position, hit.transform.position + Vector3.up);

        }
    }
}
public enum PiecePlaceType
{
    Crossing,
    Road,
    TileMiddle
}