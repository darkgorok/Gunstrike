using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
public class SmartFallingStone : MonoBehaviour
{
    public float torgeForce = 100;
    public Vector2 addForcePosition = new Vector2(0, 0.5f);
    public GameObject hitGroundFX;
    public AudioClip soundHitGround;

    [Header("HIT EFFECT")]
    public bool playEarthQuakeOnHit = true;
    public float _eqTime = 0.1f;
    public float _eqSpeed = 60;
    public float _eqSize = 1;

    private Rigidbody2D rig;
    private bool isWorked;
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

    private void Start()
    {
        rig = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isWorked)
            return;

        if (hitGroundFX)
            SpawnSystemHelper.GetNextObject(hitGroundFX, true).transform.position = transform.position;

        if (playEarthQuakeOnHit)
            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);

        audioService?.PlaySfx(soundHitGround);
        rig.AddForceAtPosition(Vector2.right * torgeForce, transform.position + (Vector3)addForcePosition);
        isWorked = true;
        Destroy(this);
    }
}
