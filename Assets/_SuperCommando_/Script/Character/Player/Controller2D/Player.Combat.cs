using System.Collections;
using UnityEngine;

public partial class Player
{
    [Header("WEAPONS")]
    [ReadOnly] public GunHandlerState GunState;
    [ReadOnly] public GunTypeID gunTypeID;
    public Transform firePoint;
    float lastTimeShooting = -999;
    bool allowShooting = true;
    protected HealthBarEnemyNew healthBar;
    bool isShooting = false;

    [Header("GRENADE")]
    public int maxGrenade = 6;
    [ReadOnly] public int grenadeRemaining = 0;
    public Grenade grenade;
    public Transform throwPoint;
    public float throwForce = 600;
    public int grenade_damage = 100;
    public float grenade_radius = 2;

    public void AnimSetTrigger(string name)
    {
        anim.SetTrigger(name);
    }

    public void AnimSetSpeed(float value)
    {
        if (anim)
            anim.speed = value;
    }

    public void AnimSetFloat(string name, float value)
    {
        anim.SetFloat(name, value);
    }

    public void AnimSetBool(string name, bool value)
    {
        anim.SetBool(name, value);
    }

    public void SetState(GunHandlerState state)
    {
        GunState = state;
    }

    public void Shoot(bool hold)
    {
        if (!hold)
        {
            isShooting = false;
            return;
        }
        else if (isShooting && gunTypeID.shootingMethob == ShootingMethob.SingleShoot)
        {
            return;
        }

        isShooting = true;

        if (!allowShooting || gunTypeID.bullet <= 0)
            return;

        if (Time.time < lastTimeShooting + gunTypeID.rate)
            return;

        if (GunState != GunHandlerState.AVAILABLE)
            return;

        lastTimeShooting = Time.time;
        if (!gunTypeID.unlimitedBullet)
            gunTypeID.bullet--;

        AnimSetTrigger("shot");
        StartCoroutine(FireCo());

        int rightSpread = 0;
        int leftSpread = 0;

        for (int i = 0; i < gunTypeID.maxBulletPerShoot; i++)
        {
            Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;

            if (aim_angle == 45)
                direction = new Vector2(isFacingRight ? 1 : -1, 1);
            else if (aim_angle == 90)
                direction = Vector2.up;
            else if (aim_angle == -45)
                direction = new Vector2(isFacingRight ? 1 : -1, -1);
            else if (aim_angle == -90 && !isGrounded)
                direction = Vector2.down;

            var projectile = SpawnSystemHelper.GetNextObject(gunTypeID.bulletPrefab.gameObject, false).GetComponent<Projectile>();
            projectile.transform.position = firePoint.position;
            Vector3 localRandom = new Vector3(0, Random.Range(-0.1f, 0.1f), 0);
            projectile.transform.localPosition += localRandom;
            projectile.transform.right = direction;

            if (gunTypeID.isSpreadBullet)
            {
                if (i != 0)
                {
                    if (i % 2 == 1)
                    {
                        rightSpread++;
                        projectile.transform.Rotate(Vector3.forward, 10 * rightSpread);
                    }
                    else
                    {
                        leftSpread++;
                        projectile.transform.Rotate(Vector3.forward, -10 * leftSpread);
                    }
                }

                direction = projectile.transform.right;
            }

            projectile.Initialize(gameObject, direction, Vector2.zero, false, gunTypeID.useTrack, gunTypeID.damage, gunTypeID.bulletSpeed);
            projectile.gameObject.SetActive(true);

            if (gunTypeID.muzzleFX)
            {
                GameObject muzzle = SpawnSystemHelper.GetNextObject(gunTypeID.muzzleFX, firePoint.position, true);
                muzzle.transform.right = direction;
                muzzle.transform.localPosition += localRandom;
            }
        }

        audioService.PlaySfx(gunTypeID.soundFire, gunTypeID.soundFireVolume);

        CancelInvoke(nameof(CheckBulletRemain));
        Invoke(nameof(CheckBulletRemain), gunTypeID.rate);
    }

    public IEnumerator FireCo()
    {
        yield return null;

        if (gunTypeID.reloadPerShoot)
            StartCoroutine(ReloadGunSub());
    }

    void CheckBulletRemain()
    {
        if (gunTypeID.bullet > 0)
            return;

        inventoryService.CurrentGunType = null;
        gunRuntimeService.BackToDefaultGun();
    }

    public void ReloadGun()
    {
        SetState(GunHandlerState.RELOADING);
        AnimSetTrigger("reload");
        AnimSetBool("reloading", true);
        Invoke(nameof(ReloadComplete), gunTypeID.reloadTime);

        audioService.PlaySfx(gunTypeID.reloadSound, gunTypeID.reloadSoundVolume);
    }

    IEnumerator ReloadGunSub()
    {
        SetState(GunHandlerState.RELOADING);
        AnimSetBool("isReloadPerShootNeeded", true);

        yield return new WaitForSeconds(gunTypeID.reloadTime);

        SetState(GunHandlerState.AVAILABLE);
        AnimSetBool("isReloadPerShootNeeded", false);
    }

    public void ReloadComplete()
    {
        lastTimeShooting = Time.time;
        AnimSetBool("reloading", false);
        SetState(GunHandlerState.AVAILABLE);
    }

    public void SetGun(GunTypeID gunID)
    {
        anim.runtimeAnimatorController = gunID.animatorOverride;
        gunTypeID = gunID;
        AnimSetTrigger("swap-gun");
        allowShooting = false;
        audioService.PlaySfx(audioService.SwapGunClip);
        Invoke(nameof(AllowShooting), 0.3f);
    }

    void AllowShooting()
    {
        allowShooting = true;
    }

    public void ThrowGrenade()
    {
        if (grenadeRemaining <= 0)
            return;

        grenadeRemaining--;
        anim.SetTrigger("throw");
        Vector3 throwPos = throwPoint.position;
        var grenadeInstance = (Grenade)Instantiate(grenade, throwPos, Quaternion.identity);
        grenadeInstance.Init(grenade_damage, grenade_radius);

        grenadeInstance.transform.right = new Vector2(isFacingRight ? 1 : -1, 0.75f);

        Rigidbody2D grenadeBody = grenadeInstance.GetComponent<Rigidbody2D>();
        grenadeBody.AddRelativeForce(grenadeInstance.transform.right * (throwForce + Mathf.Abs(velocity.x / moveSpeed) * throwForce * 0.3f));
        grenadeBody.AddTorque(grenadeInstance.transform.right.x * 10);
    }

    public void AddGrenade(int amount)
    {
        grenadeRemaining += amount;
        grenadeRemaining = Mathf.Min(grenadeRemaining, maxGrenade);
    }
}
