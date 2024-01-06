using UnityEngine;

public class ValuePicker : MonoBehaviour
{
    public int value,max,min;
    public TMPro.TextMeshProUGUI textDisplay;

    private void Awake()
    {
        textDisplay.text = ToString();
    }

    public void addNum(int num)
    {
        if(value == max && num>0)
            return;
        if(value == min && num<0)
            return;
        value += num;
        textDisplay.text = ToString();
    }
    public override string ToString()
    {
        return value + " tiles wide";
    }
}
