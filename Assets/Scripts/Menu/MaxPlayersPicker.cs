public class MaxPlayersPicker : ValuePicker
{
    protected override void Awake()
    {
        base.Awake();
        max = PlayerManager.MaxPlayers;
    }
    public override string ToString()
    {
        return "max " + value + " players";
    }
}
