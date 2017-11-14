using UnityEngine;


public class MiniMinoTileMap : MonoBehaviour
{
    private Tetrimino _parent;

    void Start()
    {
        _parent = GetComponentInParent<Tetrimino>();
    }
    
    void OnCollisionStay2D(Collision2D other)
    {
        _parent.OnCollisionStay2D(other);
    }
}