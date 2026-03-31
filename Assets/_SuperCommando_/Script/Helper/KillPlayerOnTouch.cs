using UnityEngine;
using System.Collections;

public class KillPlayerOnTouch : MonoBehaviour
{
    public bool killEnemies = false;
    public bool killAnything = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<Player>();

        if (killAnything)
            other.gameObject.SetActive(false);

        else if (player != null)
        {
            //if (player.godObstacles == Player.GodObstacles.Through && player.GodMode)
            //    return;

            if (player.isPlaying)
                GameManager.Instance.GameOver();
        }
        else if (killEnemies && other.gameObject.GetComponent(typeof(ICanTakeDamage)))
            other.gameObject.SetActive(false);
    }
}