public class StaticTetrimino : Tetrimino
{
    protected override bool ShouldCorrectPosition()
    {
        return false;
    }
}