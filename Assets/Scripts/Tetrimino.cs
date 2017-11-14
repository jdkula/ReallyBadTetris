using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Tetrimino : MonoBehaviour
{
    private Tilemap _tm;

    private const int Layermask = (1 << Constants.BorderWallLayer) | (1 << Constants.PlacedTetriminoLayer);

    public float MinCoord { get; private set; }
    public float MaxCoord { get; private set; }

    private Vector3[] _raycastPositionOffsets = new Vector3[3];


    // Use this for initialization
    void Awake()
    {
        
        _tm = GetComponentInChildren<Tilemap>();

        _raycastPositionOffsets[0] = Constants.InvalidTetriminoOffset;
        _raycastPositionOffsets[1] = new Vector3(0.75f, 0.75f);
        _raycastPositionOffsets[2] = Constants.InvalidTetriminoOffset;
        MinCoord = -3f;
        MaxCoord = 1.5f;
        
        
        GenerateMiniMinos(1, 2);
        GenerateMiniMinos(1, 0);
        GenerateMiniMinos(2, 1);
        GenerateMiniMinos(0, 1);
    }

    void GenerateMiniMinos(int x, int y, int level = 0)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);
        if (x < 0 || x > 2 || y < 0 || y > 2 || level > 1)
        {
            // Don't do anything
        }
        else if (!_tm.HasTile(pos))
        {
            if (Random.Range(0, 3) == 0)
            {
                _tm.SetTile(pos, GameState.Instance.TetriminoTile);
                UpdateOffsets(x, y);
                GenerateMiniMinos(x + 1, y, level + 1);
                GenerateMiniMinos(x - 1, y, level + 1);
                GenerateMiniMinos(x, y + 1, level + 1);
                GenerateMiniMinos(x, y - 1, level + 1);
            }
        }
    }

    public void CopyTetrimino(bool[,] source)
    {
        _raycastPositionOffsets[0] = Constants.InvalidTetriminoOffset;
        _raycastPositionOffsets[1] = new Vector3(0.75f, 0.75f);
        _raycastPositionOffsets[2] = Constants.InvalidTetriminoOffset;
        MinCoord = -3f;
        MaxCoord = 1.5f;
        
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                _tm.SetTile(new Vector3Int(x, y, 0), null);
                if (source[x,y])
                {
                    _tm.SetTile(new Vector3Int(x, y, 0), GameState.Instance.TetriminoTile);
                    UpdateOffsets(x, y);
                }
            }
        }
    }

    private void UpdateOffsets(int x, int y)
    {
        if (x == 0)
        {
            MinCoord = -2.5f;
        }
        else if (x == 2)
        {
            MaxCoord = 1f;
        }

        if (_raycastPositionOffsets[x] == Constants.InvalidTetriminoOffset
            || _raycastPositionOffsets[x].y > y * 0.5f)
        {
            _raycastPositionOffsets[x] = new Vector3(x * 0.5f + 0.25f, y * 0.5f + 0.25f, 0);
        }
    }

    private void ForceMove(Vector3 direction)
    {
        transform.position += new Vector3(0.5f * direction.x, 0.5f * direction.y, 0);
    }
    
    public bool Move(Vector3 direction)
    {
        bool collided = false;
        foreach(Vector3 raycastOffset in _raycastPositionOffsets)
        {
            if (raycastOffset != Constants.InvalidTetriminoOffset)
            {
                Vector3 raycastPosition = transform.position + raycastOffset;
                RaycastHit2D rh2D = Physics2D.Raycast(raycastPosition, direction, 0.5f, Layermask);
                Debug.DrawRay(raycastPosition, direction, Color.green, 1);
                if (rh2D.collider != null)
                {
                    collided = true;
                }
            }
        }
        
        if (!collided)
        {
            ForceMove(direction);
        }
        return collided;
    }

    public bool NextRow()
    {
        return Move(Vector3.down);
    }

    bool[,] ToArray()
    {
        bool[,] retVal = new bool[3,3];
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                retVal[x, y] = _tm.HasTile(new Vector3Int(x, y, 0));
            }
        }
        return retVal;
    }

    public GameObject RotateClockwise()
    {
        GameObject newTetrimino =
            Instantiate(GameState.Instance.TetriminoPrefab, transform.position, transform.rotation);
        
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
    
    public GameObject RotateCounterClockwise()
    {
        GameObject newTetrimino =
            Instantiate(GameState.Instance.TetriminoPrefab, transform.position, transform.rotation);

        
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

    public void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject == GameState.Instance.MainGrid)
        {
            if (transform.position.x < 0)
            {
                ForceMove(Vector3.right);
            }
            else
            {
                ForceMove(Vector3.left);
            }
        }
        /*else
        {
            ForceMove(Vector3.up);
        }*/
    }

    public void ToSingleMinos()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (_tm.HasTile(new Vector3Int(x, y, 0)))
                {
                    GameObject newMiniMino = Instantiate(GameState.Instance.MiniMinoPrefab);

                    newMiniMino.transform.position =
                        transform.position + new Vector3(0.25f + 0.5f * x, 0.25f + 0.5f * y);
                }
            }
        }
        Destroy(gameObject);
    }
}