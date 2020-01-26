using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

/// <summary>
/// Represents a tetrimino!
/// </summary>
public class Tetrimino : MonoBehaviour
{
    private Tilemap _tm;

    private const int Layermask = (1 << Constants.BorderWallLayer) | (1 << Constants.PlacedTetriminoLayer);

    private float _minCoord;
    private float _maxCoord;

    private Vector3[] _raycastPositionOffsets = new Vector3[3];

    /// <summary>
    /// Dictates if this Tetrimino should correct its position
    /// upon creation (i.e. to avoid being collided with another
    /// tetrimino after a rotation)
    /// </summary>
    /// <returns>true if this tetrimino should correct its position, false otherwise</returns>
    protected virtual bool ShouldCorrectPosition()
    {
        return true;
    }


    /// <summary>
    /// Sets up and randomly generates this tetrimino
    /// </summary>
    void Awake()
    {
        _tm = GetComponentInChildren<Tilemap>();

        _raycastPositionOffsets[0] = Constants.InvalidTetriminoOffset;
        _raycastPositionOffsets[1] = new Vector3(0.75f, 0.75f);
        _raycastPositionOffsets[2] = Constants.InvalidTetriminoOffset;
        _minCoord = -3f;
        _maxCoord = 1.5f;


        // Tetriminos always have at least a center tile.
        // This has a chance to generate another tile at the top/bottom/right/left sides,
        // and a slightly smaller chance to generate tiles in the corners (but only if
        // a tile was generated that connects it)
        GenerateMiniMinos(1, 2);
        GenerateMiniMinos(1, 0);
        GenerateMiniMinos(2, 1);
        GenerateMiniMinos(0, 1);
    }

    /// <summary>
    /// Has a 3/4 chance of generating a "minimino" (tetrimino part)
    /// at the x, y coord given. (1,1) is center.
    /// </summary>
    /// <param name="x">The x coordinate around which to generate</param>
    /// <param name="y">The y coordinate around which to generate</param>
    /// <param name="level">How many "levels" deep we've recursed.</param>
    void GenerateMiniMinos(int x, int y, int level = 0)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        
        // If we're in range, haven't already generated a tile here, and we decide to...
        if (x >= 0 && x <= 2 && y >= 0 && y <= 2 && level <= 1 && !_tm.HasTile(pos) && Random.Range(0, 3) != 0)
        {
            //...place a tetrimino here and update offsets accordingly.
            _tm.SetTile(pos, GameState.Instance.tetriminoTile);
            UpdateOffsets(x, y);
            GenerateMiniMinos(x + 1, y, level + 1);
            GenerateMiniMinos(x - 1, y, level + 1);
            GenerateMiniMinos(x, y + 1, level + 1);
            GenerateMiniMinos(x, y - 1, level + 1);
        }
    }

    /// <summary>
    /// Causes this tetrimino to exactly copy another tetrimino.
    /// </summary>
    /// <param name="source">A bool array representing another tetrimino.</param>
    public void CopyTetrimino(bool[,] source)
    {
        _raycastPositionOffsets[0] = Constants.InvalidTetriminoOffset;
        _raycastPositionOffsets[1] = new Vector3(0.75f, 0.75f);
        _raycastPositionOffsets[2] = Constants.InvalidTetriminoOffset;
        _minCoord = -3f;
        _maxCoord = 1.5f;

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                _tm.SetTile(new Vector3Int(x, y, 0), null);
                if (source[x, y])
                {
                    _tm.SetTile(new Vector3Int(x, y, 0), GameState.Instance.tetriminoTile);
                    UpdateOffsets(x, y);
                }
            }
        }
    }
    
    /// <summary>
    /// On start, if we should, corrects the position of this tetrimino
    /// (e.g. to account for rotation).
    /// </summary>
    private void Start()
    {
        if (!ShouldCorrectPosition()) return;
        
        while (transform.position.x < _minCoord)
        {
            ForceMove(Vector3.right);
        }

        while (transform.position.x > _maxCoord)
        {
            ForceMove(Vector3.left);
        }

        while (WillCollide(Vector3.down))
        {
            ForceMove(Vector3.up);
        }
    }

    /// <summary>
    /// Updates our raycast offsets to detect collisions, matching the
    /// width and height of this randomly-generated tetrimino.
    /// Should be called after tiles are added to this tetrimino.
    /// </summary>
    /// <param name="x">The x coord of the recently added tile.</param>
    /// <param name="y">The y coord of the recently added tile.</param>
    private void UpdateOffsets(int x, int y)
    {
        if (x == 0)
        {
            _minCoord = -2.5f;
        }
        else if (x == 2)
        {
            _maxCoord = 1f;
        }

        if (_raycastPositionOffsets[x] == Constants.InvalidTetriminoOffset
            || _raycastPositionOffsets[x].y > y * 0.5f)
        {
            _raycastPositionOffsets[x] = new Vector3(x * 0.5f + 0.25f, y * 0.5f + 0.25f, 0);
        }
    }

    /// <summary>
    /// Moves this tetrimino in the direction specified,
    /// ignoring collisions.
    /// </summary>
    /// <param name="direction">The direction specified. Should have magnitude 1.</param>
    private void ForceMove(Vector3 direction)
    {
        transform.position += new Vector3(0.5f * direction.x, 0.5f * direction.y, 0);
    }

    /// <summary>
    /// For convenience -- our default move is downwards.
    /// </summary>
    /// <returns>If the move was successful (true if it didn't collide with anything)</returns>
    public bool Move()
    {
        return Move(Vector3.down);
    }
    
    /// <summary>
    /// Moves this tetrimino in the direction specified,
    /// failing if it would collide.
    /// </summary>
    /// <param name="direction">The direction to move in. Should have magnitude 1.</param>
    /// <returns>If the move was successful (true if it didn't collide with anything)</returns>
    public bool Move(Vector3 direction)
    {
        bool collided = WillCollide(direction);

        if (!collided)
        {
            ForceMove(direction);
        }

        return collided;
    }

    /// <summary>
    /// Determines if a move in the direction specified
    /// would result in a collision.
    /// </summary>
    /// <param name="direction">The direction to check. Should have magnitude 1.</param>
    /// <returns>true if the movement would collide, false otherwise.</returns>
    private bool WillCollide(Vector3 direction)
    {
        // checks for collisions a teach level of this tetrimino.
        foreach (Vector3 raycastOffset in _raycastPositionOffsets)
        {
            if (raycastOffset != Constants.InvalidTetriminoOffset)
            {
                Vector3 raycastPosition = transform.position + raycastOffset;
                RaycastHit2D rh2D = Physics2D.Raycast(raycastPosition, direction, 0.5f, Layermask);
                Debug.DrawRay(raycastPosition, direction, Color.green, 1);
                if (rh2D.collider != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a bool-array representation of this tetrimino.
    /// </summary>
    /// <returns>The bool-array representation.</returns>
    bool[,] ToArray()
    {
        bool[,] retVal = new bool[3, 3];
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                retVal[x, y] = _tm.HasTile(new Vector3Int(x, y, 0));
            }
        }

        return retVal;
    }

    /// <summary>
    /// Destroys this tetrimino and replaces it with a new
    /// tetrimino that's rotated clockwise.
    /// </summary>
    /// <returns>The newly constructed tetrimino</returns>
    public GameObject RotateClockwise()
    {
        GameObject newTetrimino =
            Instantiate(GameState.Instance.tetriminoPrefab, transform.position, transform.rotation);

        newTetrimino
            .GetComponent<Tetrimino>()
            .CopyTetrimino(
                Utility.ReverseRows(
                    Utility.Transpose(
                        ToArray()
                    )
                )
            );

        Destroy(gameObject);

        return newTetrimino;
    }

    /// <summary>
    /// Destroys this tetrimino and replaces it with a new
    /// tetrimino that's rotated counterclockwise.
    /// </summary>
    /// <returns>The newly constructed tetrimino</returns>

    public GameObject RotateCounterClockwise()
    {
        GameObject newTetrimino =
            Instantiate(GameState.Instance.tetriminoPrefab, transform.position, transform.rotation);


        newTetrimino
            .GetComponent<Tetrimino>()
            .CopyTetrimino(
                Utility.Transpose(
                    Utility.ReverseRows(
                        ToArray()
                    )
                )
            );

        Destroy(gameObject);
        return newTetrimino;
    }

    /// <summary>
    /// Ensures that this tetrimino doesn't collide or go over the
    /// edge of the play area.
    /// </summary>
    /// <param name="other"></param>
    public void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject != GameState.Instance.mainGrid) return;
        
        ForceMove(transform.position.x < 0 ? Vector3.right : Vector3.left);
    }

    /// <summary>
    /// Called when this tetrimino hits the ground.
    /// Destructs it into single-minos that we can check
    /// collisions with and use to clear rows in a more fine-grained manner.
    /// Destroyes this tetrimino in the process.
    /// </summary>
    public void ToSingleMinos()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (_tm.HasTile(new Vector3Int(x, y, 0)))
                {
                    GameObject newMiniMino = Instantiate(GameState.Instance.miniMinoPrefab);

                    newMiniMino.transform.position =
                        transform.position + new Vector3(0.25f + 0.5f * x, 0.25f + 0.5f * y);
                }
            }
        }

        Destroy(gameObject);
    }
}