using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartProjectileAttack : MonoBehaviour
{
    public enum ShootingType { FixedAngle, FixedForce}
    public ShootingType shootingType;
    public enum ShootingTarget { FixedTarget, DetectedTarget }
    public ShootingTarget shootingTarget;

    [Header("Setup")]
    public ArrowProjectile arrow;
    public GameObject startFX;
    public float shootRate = 1;
    public float gravityScale = 1f;
    public Transform fixedTarget;
    [Range(0.01f, 0.1f)]
    [ReadOnly] public float stepCheck = 0.1f;
    [ReadOnly] public bool onlyShootTargetInFront = true;
    public Transform headPos;
    public Transform cannonBody;

    [Header("FIXED ANGLE SETUP")]
    public float fixedAngle = 45;
    public float stepForceCheck = 0.5f;

    [Header("FIXED FORCE SETUP")]
    public float force = 20;
    public float stepAngleCheck = 1;
    
    [Header("Sound")]
    public float soundShootVolume = 0.5f;
    public AudioClip[] soundShoot;

    private float x1, y1;
    bool isTargetRight = false;
    float lastShoot;

    [ReadOnly] public bool isAvailable = true;
    
    Animator anim;
    bool forceManualShoot = false;
    Vector2 forceManualShootPoint;

    void Start()
    {
        anim = GetComponent<Animator>();
        //StartCoroutine(AutoCheckAndShoot());
    }

    //IEnumerator AutoCheckAndShoot()
    //{
    //    while (true)
    //    {
    //        if (shootingType == ShootingType.Auto)
    //        {
    //            Shoot(false, Vector2.zero);
    //        }
    //        else
    //        {
    //            if (!isDetectedPlayer)
    //            {
    //                if (checkTargetHelper.CheckTarget())
    //                {
    //                    isDetectedPlayer = true;
    //                    Shoot(false, Vector2.zero);
    //                }
    //            }
    //            else
    //            {
    //                if (Vector2.Distance(GameManager.Instance.Player.transform.position, transform.position) < dismissTargetDistance)
    //                    Shoot(false, Vector2.zero);
    //                else
    //                    isDetectedPlayer = false;
    //            }
    //        }
    //        yield return new WaitForSeconds(0.1f);
    //    }
    //}

    public bool Shoot(GameObject targetPosition)
    {
        if (!isAvailable/* || target == null*/)
            return false;

        forceManualShoot = shootingTarget == ShootingTarget.DetectedTarget && targetPosition != null;
        if (forceManualShoot)
            forceManualShootPoint = targetPosition.transform.position;

        isTargetRight = /*Camera.main.ScreenToWorldPoint(Input.mousePosition).x*/ (forceManualShoot ? forceManualShootPoint.x : fixedTarget.position.x) > transform.position.x;

        StartCoroutine(CheckTarget());

        isAvailable = false;

        return true;
    }

    public Transform firePoint;
    IEnumerator CheckTarget()
    {
        Vector3 mouseTempLook = /*Camera.main.ScreenToWorldPoint(Input.mousePosition)*/ (forceManualShoot ? forceManualShootPoint : (Vector2)fixedTarget.position);
        mouseTempLook -= transform.position;
        yield return null;

        Vector2 fromPosition = firePoint.position;
        Vector2 target = (forceManualShoot ? forceManualShootPoint : (Vector2)fixedTarget.position);

        float beginAngle = (shootingType == ShootingType.FixedAngle)? fixedAngle : UltiHelper.Vector2ToAngle(target - fromPosition);
        Vector2 ballPos = fromPosition;

        float cloestAngleDistance = int.MaxValue;

        bool checkingPerAngle = true;

        if ((shootingType == ShootingType.FixedAngle))
            force = 1;

        while (checkingPerAngle)
        {
            int k = 0;
            Vector2 lastPos = fromPosition;
            bool isCheckingAngle = true;
            float clostestDistance = int.MaxValue;

            while (isCheckingAngle)
            {
                Vector2 shotForce = force * UltiHelper.AngleToVector2(beginAngle);
                x1 = ballPos.x + shotForce.x * Time.fixedDeltaTime * (stepCheck * k);    //X position for each point is found
                y1 = ballPos.y + shotForce.y * Time.fixedDeltaTime * (stepCheck * k) - (-(Physics2D.gravity.y * gravityScale) / 2f * Time.fixedDeltaTime * Time.fixedDeltaTime * (stepCheck * k) * (stepCheck * k));    //Y position for each point is found

                float _distance = Vector2.Distance(target, new Vector2(x1, y1));
                if (_distance < clostestDistance)
                    clostestDistance = _distance;

                if ((y1 < lastPos.y) && (y1 < target.y))
                    isCheckingAngle = false;
                else
                    k++;

                lastPos = new Vector2(x1, y1);
            }

            if (clostestDistance >= cloestAngleDistance)
                checkingPerAngle = false;
            else
            {
                cloestAngleDistance = clostestDistance;

                if (isTargetRight)
                {
                    if ((shootingType == ShootingType.FixedAngle)) {
                        force += stepForceCheck;
                    }
                    else
                    {
                        beginAngle += stepAngleCheck;
                    }
                }
                else
                {
                    if ((shootingType == ShootingType.FixedAngle))
                    {
                        force += stepForceCheck;
                    }
                    else
                    {
                        beginAngle -= stepAngleCheck;
                    }
                }
                //if (beginAngle <= maxAngle)
                //    checkingPerAngle = false;
            }
        }
        cannonBody.right = UltiHelper.AngleToVector2(beginAngle);

        yield return null;
        anim.SetTrigger("fire");

        //Fire number arrow
        ArrowProjectile _tempArrow;

        _tempArrow = Instantiate(arrow, fromPosition, Quaternion.identity);
        _tempArrow.Init(force * UltiHelper.AngleToVector2(beginAngle), gravityScale);

        SoundManager.PlaySfx(soundShoot[Random.Range(0, soundShoot.Length)], soundShootVolume);
        if (startFX)
        {
            Instantiate(startFX, headPos.position, Quaternion.identity);
        }

        StartCoroutine(ReloadingCo());
    }

    IEnumerator ReloadingCo()
    {
        isAvailable = false;
        lastShoot = Time.time;

        yield return new WaitForSeconds(0.1f);

        while (Time.time < (lastShoot + shootRate)) { yield return null; }

        anim.SetTrigger("idle");

        yield return new WaitForSeconds(0.2f);

        isAvailable = true;
    }

}
