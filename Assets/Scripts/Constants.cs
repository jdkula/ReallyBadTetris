using UnityEngine;

public static class Constants
{
    public static readonly string SavePath = Application.persistentDataPath + "/save.rbt";
    public const float TimePerRow = 1.0f;
    public static readonly Vector3 InvalidTetriminoOffset = new Vector3(0, 0, -1000f);

    public const int PlacedTetriminoLayer = 10;
    public const int BorderWallLayer = 8;

    public const int PointsPerRow = 10;
    public const int PointsPerClear = 100;
    public const int ComboMultiplier = 4;
    public const int DropLineMultiplier = 2;
}