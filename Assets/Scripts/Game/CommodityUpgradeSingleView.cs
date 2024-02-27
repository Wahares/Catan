using UnityEngine;
using UnityEngine.UI;

public class CommodityUpgradeSingleView : MonoBehaviour
{
    public const int maxLevel = 5;
    [SerializeField]
    private Image img;
    public void setValue(int value)
    {
        img.material.SetFloat("Value", (float)value / maxLevel);
    }
}
