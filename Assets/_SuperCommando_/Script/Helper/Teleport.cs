using UnityEngine;
using VContainer;

public class Teleport : MonoBehaviour
{
    public Transform position1;
    public Transform position2;
    public float teleportTimer = 1.2f;
    public AudioClip sound;

    private GameObject lastObj;
    private IGameSessionService gameSession;
    private IAudioService audioService;

    [Inject]
    public void Construct(IGameSessionService gameSession, IAudioService audioService)
    {
        this.gameSession = gameSession;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void TeleportPlayer(Vector3 currentPos)
    {
        if (gameSession?.Player == null)
            return;

        audioService?.PlaySfx(sound);
        gameSession.Player.Teleport(currentPos == position1.position ? position2 : position1, teleportTimer);
    }

    public void TeleportObj(Vector3 currentPos, GameObject obj)
    {
        audioService?.PlaySfx(sound);
        if (obj == lastObj)
        {
            lastObj = null;
            return;
        }

        lastObj = obj;
        obj.transform.position = currentPos == position1.position ? position2.position : position1.position;
    }
}
