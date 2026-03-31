using UnityEngine;
using System.Collections;

public class KillPlayerOnTouch : MonoBehaviour
{
    public bool killEnemies = false;
    public bool killAnything = false;
    private IGameSessionService gameSession;

    [VContainer.Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

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
                gameSession.GameOver();
        }
        else if (killEnemies && other.gameObject.GetComponent(typeof(ICanTakeDamage)))
            other.gameObject.SetActive(false);
    }
}
