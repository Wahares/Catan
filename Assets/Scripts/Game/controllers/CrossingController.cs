public class CrossingController : PieceContainerController<CrossingController,SinglePieceController>
{
    public void TileNumberRolled(TileController source)
    {
        currentPiece?.OnTileInvoked(source);
    }
}
