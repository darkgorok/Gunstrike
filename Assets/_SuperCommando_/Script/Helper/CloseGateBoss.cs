using UnityEngine;
using VContainer;

public class CloseGateBoss : MonoBehaviour
{
    public enum MoveType { Up2Down, Down2Up }

    public AudioClip sound;
    public MoveType moveType;
    public Transform TheGate;
    public float topLocalPos = 3;
    public float bottomLocalPos = -3;

    private bool active;
    private bool openManual;
    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        Gizmos.DrawSphere(new Vector2(TheGate.position.x, bottomLocalPos + transform.position.y), 0.2f);
        Gizmos.DrawSphere(new Vector2(TheGate.position.x, topLocalPos + transform.position.y), 0.2f);
        TheGate.position = new Vector2(TheGate.position.x, moveType == MoveType.Down2Up ? bottomLocalPos + transform.position.y : topLocalPos + transform.position.y);
    }

    private void Update()
    {
        if (!active)
            return;

        TheGate.position = Vector2.MoveTowards(
            TheGate.transform.position,
            moveType == MoveType.Up2Down
                ? new Vector2(TheGate.position.x, bottomLocalPos + transform.position.y)
                : new Vector2(TheGate.position.x, topLocalPos + transform.position.y),
            0.1f);
    }

    public void SetManual()
    {
        openManual = true;
    }

    public void ActiveTheGate()
    {
        if (active)
            return;

        active = true;
        audioService?.PlaySfx(sound);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!openManual && other.GetComponent<Player>() != null)
            ActiveTheGate();
    }
}
