/// <summary>
/// Provides utility methods used throughout
/// our code to accomplish tasks like rotation.
/// </summary>
public static class Utility
{
    public static bool[,] Transpose(bool[,] source)
    {
        bool[,] retVal = new bool[source.GetLength(1),source.GetLength(0)];

        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int j = 0; j < source.GetLength(1); j++)
            {
                retVal[j, i] = source[i, j];
            }
        }

        return retVal;
    }

    public static bool[,] ReverseRows(bool[,] source)
    {
        bool[,] retVal = new bool[source.GetLength(0), source.GetLength(1)];
        for (int i = 0; i < source.GetLength(0); i++)
        {
            for (int j = 0; j < source.GetLength(1); j++)
            {
                retVal[i, j] = source[i, source.GetLength(1) - j - 1];
            }
        }

        return retVal;
    }
}