using UnityEngine;
using VContainer;

public class Star : MonoBehaviour
{
    private const float Speed = 2f;

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

    private void Update()
    {
        if (gameSession?.Player == null)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            gameSession.Player.transform.position,
            Speed * Time.deltaTime);
    }
}
