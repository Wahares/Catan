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
        if (TurnManager.currentPhase == Phase.BanditsMoreThan7)
            TurnManager.instance.EnqueuePhase(Phase.BanditsMoreThan7, PlayerManager.numOfPlayers);
        else
            TurnManager.instance.changePhaseOnServer(Phase.BanditsMoreThan7);
    }
}
