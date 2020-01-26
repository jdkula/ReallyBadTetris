using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Can be placed on any object with a collider.
/// Upon collision, loses the game.
/// </summary>
public class LoseTrigger : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        GameState.Instance.Lose();
    }

}
