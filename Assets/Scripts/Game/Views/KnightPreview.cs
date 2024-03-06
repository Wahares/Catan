using UnityEngine;

public class KnightPreview : MonoBehaviour
{
    [SerializeField]
    private GameObject upgradePreview, trainPreview;

    public void changeMode(bool upgrade)
    {
        upgradePreview.SetActive(upgrade);
        trainPreview.SetActive(!upgrade);
    }

}
