using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigamePlayer : MonoBehaviour
{
    public MinigameStateManager manager;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        manager.stars.Remove(collider.gameObject);
        Destroy(collider.gameObject);
        manager.AddScore(1);
    }
}
