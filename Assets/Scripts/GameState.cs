using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public Tile tetriminoTile;
    public GameObject staticTetriminoPrefab;
    public GameObject tetriminoPrefab;
    public GameObject miniMinoPrefab;
    public GameObject mainGrid;

    public Text pointsText;
    public Text linesText;
    public Text highScoreText;

    public Vector3 nextOffset;

    private Tetrimino _current;
    private Tetrimino _next;
    private Tetrimino _nextNext;

    // Represents the time per row movement, in seconds.
    private float _timeLeft = Constants.TimePerRow;

    private bool _currentAtBottom;
    private bool _stopped;
    private int _combo = 0;

    private int _score;
    private int _lines;

    private SaveGame _save;

    /// <summary>
    /// Before initializing the rest of the game, load
    /// save data and set up our static Instance (so that this
    /// global game state can be quickly accessed from other scripts).
    /// </summary>
    void Awake()
    {
        _save = new SaveGame();
        _save.Load();
        Instance = this;
    }

    /// <summary>
    /// Initializes the first two tetriminos and score text
    /// </summary>
    void Start()
    {
        // Generate initial tetriminos (the prefab should already be positioned).
        _nextNext = Instantiate(staticTetriminoPrefab).GetComponent<Tetrimino>();
        ShiftQueue();
        _nextNext = Instantiate(staticTetriminoPrefab).GetComponent<Tetrimino>();

        // Set up score text
        pointsText.text = "0";
        linesText.text = "0";
        highScoreText.text = _save.HighScore.ToString();
    }

    /// <summary>
    /// Moves the tetrimino in 2nd position to 1st position.
    /// </summary>
    void ShiftQueue()
    {
        Vector3 pos = _nextNext.transform.position;
        _nextNext.transform.position = pos + nextOffset;
        _next = _nextNext;
    }

    /// <summary>
    /// Dequeues a tetrimino from the queue and adds another
    /// tetrimino to the back of the queue.
    /// </summary>
    /// Moves the tetrimino in 1st to the top and starts it moving;
    /// then moves the tetrimino in 2nd to 1st; then instantiates a new
    /// tetrimino in second.
    void DequeueTetrimino()
    {
        _current = _next;
        _current.transform.position = new Vector3(-0.5f, 4);
        ShiftQueue();
        _nextNext = Instantiate(staticTetriminoPrefab).GetComponent<Tetrimino>();
    }

    /// <summary>
    /// Checks all the lines to see if they should be cleared,
    /// and clears them if so.
    /// </summary>
    /// <returns>The number of lines cleared</returns>
    int CheckLines()
    {
        int linesCleared = 0;
        int layerMask = 1 << Constants.PlacedTetriminoLayer;

        // for each row
        for (float y = -4.25f; y < 5; y += 0.5f)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(-2.5f, y), Vector2.right, 5, layerMask);

            if (hits.Length == 10) // each row is 10 miniminos long.
            {
                linesCleared++;
                _lines++;
                foreach (RaycastHit2D hit in hits)
                {
                    Destroy(hit.collider.gameObject);
                }

                // shift all miniminos above this row down by one row.
                foreach (GameObject minimino in GameObject.FindGameObjectsWithTag("MiniMino"))
                {
                    Vector3 pos = minimino.transform.position;
                    if (pos.y > y)
                    {
                        minimino.transform.position -= new Vector3(0, 0.5f, 0);
                    }
                }

                // if we clear a row, we need to check it again (to check the row above it.
                y -= 0.5f;
            }
        }

        linesText.text = _lines.ToString();
        return linesCleared;
    }

    /// <summary>
    /// Loses the game
    /// </summary>
    public void Lose()
    {
        Time.timeScale = 0;
        _stopped = true;
        _save.HighScore = _score;
        highScoreText.text = _save.HighScore.ToString();
        _save.Save();
    }

    /// <summary>
    /// Handles minimino movement and input, as well as scoring.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (_stopped) return;


        if (_current == null)
        {
            DequeueTetrimino();
            return;
        }

        if (_timeLeft > 0)
        {
            _timeLeft -= Time.deltaTime;
        }
        else
        {
            bool isCurrentAtBottom = _current.Move();

            // in the case that isCurrentAtBottom = false and _currentAtBottom = true,
            // the player moved the tetrimino along the bottom of a row.
            if (isCurrentAtBottom && _currentAtBottom)
            {
                _current.ToSingleMinos();
                int linesCleared = CheckLines();
                if (linesCleared > 0)
                {
                    _score += (_combo == 0 ? 1 : Constants.ComboMultiplier * _combo) * Constants.PointsPerClear *
                              linesCleared;
                }

                DequeueTetrimino();
            }

            // keeping this below allows the user to move this tetrimino around the bottom of a row.
            _currentAtBottom = isCurrentAtBottom;

            _score += Constants.PointsPerRow;
            pointsText.text = _score.ToString();
            _timeLeft = Constants.TimePerRow;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_currentAtBottom)
            {
                _timeLeft = Constants.TimePerRow;
            }

            _current.Move(Vector3.left);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_currentAtBottom)
            {
                _timeLeft = Constants.TimePerRow;
            }

            _current.Move(Vector3.right);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _timeLeft = -1;
        }

        // "slam" -- puts the tetrimino all the way to the bottom.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int rowsDropped = 0;
            while (!_current.Move())
            {
                rowsDropped++;
            }

            _score += rowsDropped * Constants.DropLineMultiplier * Constants.PointsPerRow;

            pointsText.text = _score.ToString();
            _currentAtBottom = true;
            _timeLeft = -1;
        }

        // rotation creates a new GameObject, so we reassign current.
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            _current = _current.RotateClockwise().GetComponent<Tetrimino>();
        }

        if (Input.GetKeyDown(KeyCode.Slash))
        {
            _current = _current.RotateCounterClockwise().GetComponent<Tetrimino>();
        }
    }
    
    /// <summary>
    /// Saves the game when the GameState is destroyed (i.e. the application is quitting).
    /// </summary>
    void OnDestroy()
    {
        Lose();
    }
}