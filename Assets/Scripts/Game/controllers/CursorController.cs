using System;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    private Camera cam;
    void Awake()
    {
        cam = Camera.main;
    }

    public static Action<Vector2Int?, PiecePlaceType> OnClicked;
    public static Action OnRightClicked;
    public static Action<Vector2Int?, PiecePlaceType> Hovering;

    public PiecePlaceType currentFocusPieceType;

    private IClickable currentFocused;

    void Update()
    {
        RaycastHit hit;
        if (currentFocusPieceType == PiecePlaceType.None)
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 255, QueryTriggerInteraction.Collide))
            {
                if (hit.transform.TryGetComponent(out IClickable click))
                {
                    if (currentFocused != click)
                    {
                        currentFocused?.OnHoverEnd();
                        currentFocused = click;
                        currentFocused?.OnHoverStart();
                    }
                }
                else 
                if (currentFocused != null)
                {
                    currentFocused.OnHoverEnd();
                    currentFocused = null;
                }

                if (Input.GetMouseButtonDown(0))
                    currentFocused?.OnClick();
            }
            else
            {
                if (currentFocused != null)
                {
                    currentFocused.OnHoverEnd();
                    currentFocused = null;
                }
            }
        }
        else
        {
            if (currentFocused != null)
            {
                currentFocused.OnHoverEnd();
                currentFocused = null;
            }
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
            mask |= LayerMask.GetMask("UI");
            if (Physics.SphereCast(cam.ScreenPointToRay(Input.mousePosition)
                , 0.1f
                , out hit
                , 999
                , mask
                , QueryTriggerInteraction.Collide))
            {
                Vector2Int? mapPos = null;

                switch (currentFocusPieceType)
                {
                    case PiecePlaceType.Crossing:
                        mapPos = hit.collider.GetComponent<CrossingController>().pos;
                        break;
                    case PiecePlaceType.Road:
                        mapPos = hit.collider.GetComponent<RoadController>().pos;
                        break;
                    case PiecePlaceType.TileMiddle:
                        mapPos = hit.collider.GetComponent<TileController>().mapPos;
                        break;
                }
                Hovering?.Invoke(mapPos, currentFocusPieceType);
                if (Input.GetMouseButtonDown(0))
                    OnClicked?.Invoke(mapPos, currentFocusPieceType);
                Debug.DrawLine(hit.transform.position, hit.transform.position + Vector3.up);
            }
        }
        if (Input.GetMouseButtonDown(1))
            OnRightClicked?.Invoke();
    }
}
public enum PiecePlaceType
{
    None,
    Crossing,
    Road,
    TileMiddle
}