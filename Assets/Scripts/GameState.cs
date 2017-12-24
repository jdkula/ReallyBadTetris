using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public Tile TetriminoTile;
    public Tile UiBlackTile;
    public Tile UiRedTile;
    public GameObject StaticTetriminoPrefab;
    public GameObject TetriminoPrefab;
    public GameObject MiniMinoPrefab;
    public GameObject MainGrid;

    public Tetrimino Current;
    public Tetrimino Next;
    public Tetrimino NextNext;

    public Text PointsText;
    public Text LinesText;
    public Text HighScoreText;

    private float _timeLeft = Constants.TimePerRow;

    private bool _currentAtBottom;
    private bool _stopped;
    private int _combo = 0;

    private int _score;
    private int _lines;

    private SaveGame _save;


    void Awake()
    {
        _save = new SaveGame();
        _save.Load();
        _save.Save();
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        NextNext = Instantiate(StaticTetriminoPrefab).GetComponent<Tetrimino>();
        MoveUp();
        NextNext = Instantiate(StaticTetriminoPrefab).GetComponent<Tetrimino>();

        PointsText.text = "0";
        LinesText.text = "0";
        HighScoreText.text = _save.HighScore.ToString();
    }

    void MoveUp()
    {
        Vector3 pos = NextNext.transform.position;
        NextNext.transform.position = pos + new Vector3(0, 3.5f, 0);
        Next = NextNext;
    }

    void StartNext()
    {
        Current = Next;
        Current.transform.position = new Vector3(-0.5f, 4);
        MoveUp();
        NextNext = Instantiate(StaticTetriminoPrefab).GetComponent<Tetrimino>();
    }

    int CheckLines()
    {
        int linesCleared = 0;
        int layerMask = 1 << Constants.PlacedTetriminoLayer;

        for (float y = -4.25f; y < 5; y += 0.5f)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(-2.5f, y), Vector2.right, 5, layerMask);

            if (hits.Length == 10)
            {
                linesCleared++;
                _lines++;
                foreach (RaycastHit2D hit in hits)
                {
                    Destroy(hit.collider.gameObject);
                }
                foreach (GameObject minimino in GameObject.FindGameObjectsWithTag("MiniMino"))
                {
                    Vector3 pos = minimino.transform.position;
                    if (pos.y > y)
                    {
                        minimino.transform.position -= new Vector3(0, 0.5f, 0);
                    }
                }
                y -= 0.5f;
            }
        }

        LinesText.text = _lines.ToString();
        return linesCleared;
    }

    public void Stop()
    {
        Time.timeScale = 0;
        _stopped = true;
        _save.HighScore = _score;
        HighScoreText.text = _save.HighScore.ToString();
        _save.Save();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_stopped)
        {
            if (Current != null)
            {
                if (_timeLeft <= 0)
                {
                    bool currentNowAtBottom = Current.NextRow();
                    if (currentNowAtBottom && _currentAtBottom)
                    {
                        Current.ToSingleMinos();
                        int linesCleared = CheckLines();
                        if (linesCleared > 0)
                        {
                            _score += (_combo == 0 ? 1 : Constants.ComboMultiplier * _combo) * Constants.PointsPerClear * linesCleared;
                        }
                        StartNext();
                    }
                    else
                    {
                        _currentAtBottom = currentNowAtBottom;
                    }
                    _score += Constants.PointsPerRow;
                    PointsText.text = _score.ToString();
                    _timeLeft = Constants.TimePerRow;
                }
                else
                {
                    _timeLeft -= Time.deltaTime;
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (_currentAtBottom)
                    {
                        _timeLeft = Constants.TimePerRow;
                    }
                    Current.Move(Vector3.left);
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (_currentAtBottom)
                    {
                        _timeLeft = Constants.TimePerRow;
                    }
                    Current.Move(Vector3.right);
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    _timeLeft = -1;
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    int rowsDropped = 0;
                    while (!Current.NextRow())
                    {
                        rowsDropped++;
                    }
                    _score += rowsDropped * Constants.DropLineMultiplier * Constants.PointsPerRow;

                    PointsText.text = _score.ToString();
                    _currentAtBottom = true;
                    _timeLeft = -1;
                }
                if (Input.GetKeyDown(KeyCode.RightShift))
                {
                    Current = Current.RotateClockwise().GetComponent<Tetrimino>();
                }
                if (Input.GetKeyDown(KeyCode.Slash))
                {
                    Current = Current.RotateCounterClockwise().GetComponent<Tetrimino>();
                }
            }
            else
            {
                StartNext();
            }
        }
        
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    void OnDestroy()
    {
        Stop();
    }
}