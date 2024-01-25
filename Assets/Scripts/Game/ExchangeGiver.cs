using UnityEngine;

public abstract class ExchangeGiver : MonoBehaviour
{
    [field:SerializeField]
    public TradingOption TradingOption { get; private set; }
}
