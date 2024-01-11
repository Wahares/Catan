using UnityEngine;

public abstract class CardView : MonoBehaviour
{
    [SerializeField]
    protected MeshRenderer render;
    public int ID { get; private set; }

    protected CardSO item;

    public virtual void Initialize(int ID)
    {
        transform.localEulerAngles = Vector3.zero;
        this.ID = ID;
        item = PlayerInventoriesManager.instance.availableCards[ID];
        render.material = item.cardTexture;
    }
    public virtual void OnClicked() { }
    public void OverrideTexture(Material newMaterial) => render.material = newMaterial;
    public virtual void DestroyCard() { }
}
