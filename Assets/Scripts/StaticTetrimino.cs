/// <summary>
/// Represents a Tetrimino that does not correct its position.
/// </summary>
public class StaticTetrimino : Tetrimino
{
    protected override bool ShouldCorrectPosition()
    {
        return false;
    }
}