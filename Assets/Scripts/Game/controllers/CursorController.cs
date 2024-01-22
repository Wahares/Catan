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
        if (Physics.SphereCast(cam.transform.position
            , 0.25f
            , cam.ScreenToWorldPoint(Input.mousePosition)
            , out RaycastHit hit
            , mask
            , 999
            , QueryTriggerInteraction.Collide))
        {



        }
    }
}
public enum PiecePlaceType
{
    Crossing,
    Road,
    TileMiddle
}