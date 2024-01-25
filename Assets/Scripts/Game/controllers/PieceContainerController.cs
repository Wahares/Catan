using DG.Tweening;
using UnityEngine;

public class PieceContainerController<T, T1> : MonoBehaviour
where T : MonoBehaviour
where T1 : SinglePieceController
{
    public T1 currentPiece;
    public Vector2Int pos;

    public T Initialize(Vector2Int pos)
    {
        this.pos = pos;
        return this as T;
    }

    public virtual void SetPiece(T1 pieceController)
    {
        if (currentPiece != null)
            currentPiece.transform.DOScale(0, 0.5f).OnComplete(() => Destroy(currentPiece.gameObject));
        currentPiece = pieceController;
        if (pieceController == null)
            return;
        currentPiece.transform.parent = transform;
        currentPiece.transform.localPosition = Vector3.up;
        currentPiece.transform.localEulerAngles = Vector3.zero;
        currentPiece.transform.localScale = Vector3.zero;
        currentPiece.transform.DOLocalMoveY(0, 0.25f);
        currentPiece.transform.DOScale(Vector3.one, 0.15f);
    }



}
