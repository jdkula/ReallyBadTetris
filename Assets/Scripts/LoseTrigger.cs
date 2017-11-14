using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseTrigger : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        GameState.Instance.Stop();
    }

}
