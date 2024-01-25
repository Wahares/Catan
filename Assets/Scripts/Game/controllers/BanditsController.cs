using FishNet.Object;

public class BanditsController : NetworkBehaviour
{
    private void Awake()
    {
        GameManager.OnBanditsRolled += Rolled;
    }
    private void OnDestroy()
    {
        GameManager.OnBanditsRolled -= Rolled;
    }

    private void Rolled()
    {
        TurnManager.instance.EnqueuePhase(Phase.BanditsMoreThan7, PlayerManager.numOfPlayers, false);
    }
}
