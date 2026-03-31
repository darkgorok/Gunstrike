using UnityEngine;
using VContainer;

public class TeleportPoint : MonoBehaviour
{
    public Teleport Teleport;

    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameSession?.Player == null || !gameSession.Player.isPlaying)
            return;

        if (other.GetComponent<Player>())
            Teleport.TeleportPlayer(transform.position);
    }
}
