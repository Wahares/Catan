using UnityEngine;

public class CommodityUpgradeView : MonoBehaviour
{
    [SerializeField]
    private CommodityUpgradeSingleView politics, trade, science;
    private void Awake()
    {
        setValue(growthType.Politics, 0);
        setValue(growthType.Trade, 0);
        setValue(growthType.Science, 0);
    }
    public void setValue(growthType type,int value)
    {
        switch (type)
        {
            case growthType.Blank:
                Debug.LogError("Wrong growth type!");
                break;
            case growthType.Trade:
                trade.setValue(value);
                break;
            case growthType.Politics:
                politics.setValue(value);
                break;
            case growthType.Science:
                science.setValue(value);
                break;
        }
    }
}