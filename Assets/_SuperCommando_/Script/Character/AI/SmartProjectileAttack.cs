using UnityEngine;
using VContainer;

public class SmartProjectileAttack : MonoBehaviour
{
    public enum ShootingType { FixedAngle, FixedForce }
    public ShootingType shootingType;

    public enum ShootingTarget { FixedTarget, DetectedTarget }
    public ShootingTarget shootingTarget;

    [Header("Setup")]
    public ArrowProjectile arrow;
    public GameObject startFX;
    public float shootRate = 1f;
    public float gravityScale = 1f;
    public Transform fixedTarget;
    [Range(0.01f, 0.1f)] [ReadOnly] public float stepCheck = 0.1f;
    [ReadOnly] public bool onlyShootTargetInFront = true;
    public Transform headPos;
    public Transform cannonBody;
    public Transform firePoint;

    [Header("FIXED ANGLE SETUP")]
    public float fixedAngle = 45f;
    public float stepForceCheck = 0.5f;

    [Header("FIXED FORCE SETUP")]
    public float force = 20f;
    public float stepAngleCheck = 1f;

    [Header("Sound")]
    public float soundShootVolume = 0.5f;
    public AudioClip[] soundShoot;

    [ReadOnly] public bool isAvailable = true;

    private Animator anim;
    private bool forceManualShoot;
    private Vector2 forceManualShootPoint;
    private float reloadTimer = -1f;
    private float idleTriggerTimer = -1f;
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
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (reloadTimer >= 0f)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                reloadTimer = -1f;
                anim.SetTrigger("idle");
                idleTriggerTimer = 0.2f;
            }
        }

        if (idleTriggerTimer >= 0f)
        {
            idleTriggerTimer -= Time.deltaTime;
            if (idleTriggerTimer <= 0f)
            {
                idleTriggerTimer = -1f;
                isAvailable = true;
            }
        }
    }

    public bool Shoot(GameObject targetPosition)
    {
        if (!isAvailable)
            return false;

        forceManualShoot = shootingTarget == ShootingTarget.DetectedTarget && targetPosition != null;
        if (forceManualShoot)
            forceManualShootPoint = targetPosition.transform.position;

        FireProjectile();
        isAvailable = false;
        reloadTimer = shootRate;
        idleTriggerTimer = -1f;
        return true;
    }

    private void FireProjectile()
    {
        Vector2 fromPosition = firePoint.position;
        Vector2 target = forceManualShoot ? forceManualShootPoint : (Vector2)fixedTarget.position;
        bool isTargetRight = target.x > transform.position.x;
        float solvedForce = shootingType == ShootingType.FixedAngle ? 1f : force;
        float beginAngle = shootingType == ShootingType.FixedAngle ? fixedAngle : UltiHelper.Vector2ToAngle(target - fromPosition);
        float closestAngleDistance = float.MaxValue;
        bool checkingPerAngle = true;
        Vector2 ballPos = fromPosition;

        while (checkingPerAngle)
        {
            int k = 0;
            Vector2 lastPos = fromPosition;
            bool isCheckingAngle = true;
            float closestDistance = float.MaxValue;

            while (isCheckingAngle)
            {
                Vector2 shotForce = solvedForce * UltiHelper.AngleToVector2(beginAngle);
                float x = ballPos.x + shotForce.x * Time.fixedDeltaTime * (stepCheck * k);
                float y = ballPos.y + shotForce.y * Time.fixedDeltaTime * (stepCheck * k) -
                          (-(Physics2D.gravity.y * gravityScale) / 2f * Time.fixedDeltaTime * Time.fixedDeltaTime * (stepCheck * k) * (stepCheck * k));

                float distance = Vector2.Distance(target, new Vector2(x, y));
                if (distance < closestDistance)
                    closestDistance = distance;

                if (y < lastPos.y && y < target.y)
                    isCheckingAngle = false;
                else
                    k++;

                lastPos = new Vector2(x, y);
            }

            if (closestDistance >= closestAngleDistance)
            {
                checkingPerAngle = false;
            }
            else
            {
                closestAngleDistance = closestDistance;
                if (shootingType == ShootingType.FixedAngle)
                {
                    solvedForce += stepForceCheck;
                }
                else
                {
                    beginAngle += isTargetRight ? stepAngleCheck : -stepAngleCheck;
                }
            }
        }

        cannonBody.right = UltiHelper.AngleToVector2(beginAngle);
        anim.SetTrigger("fire");

        ArrowProjectile tempArrow = Instantiate(arrow, fromPosition, Quaternion.identity);
        tempArrow.Init(solvedForce * UltiHelper.AngleToVector2(beginAngle), gravityScale);

        if (soundShoot != null && soundShoot.Length > 0)
            audioService.PlaySfx(soundShoot[Random.Range(0, soundShoot.Length)], soundShootVolume);

        if (startFX != null)
            Instantiate(startFX, headPos.position, Quaternion.identity);
    }
}
