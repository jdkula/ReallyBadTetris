using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Represents the game's save state.
/// Currently, just the high score.
/// </summary>
[System.Serializable]
public class SaveGame
{
    private int _highScore;

    public int HighScore
    {
        get => _highScore;
        set { if(_highScore < value) _highScore = value; }
    }

    public SaveGame()
    {
        HighScore = 0;
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Constants.SavePath);
        bf.Serialize(file, this);
        file.Close();
    }

    public void Load()
    {
        if (File.Exists(Constants.SavePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Constants.SavePath, FileMode.Open);
            _highScore = ((SaveGame) bf.Deserialize(file)).HighScore;
            file.Close();
        }
    }
}
