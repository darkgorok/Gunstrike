using UnityEngine;

public class MonsterSnail : EnemyAI
{
    [Header("Owner")]
    public Animator anim;
    public float timeBackToAlive = 3f;

    private float shellRecoverTimer = -1f;
    private bool shakeTriggered;

    public override void Start()
    {
        base.Start();
        healthType = HealthType.HitToKill;
    }

    public override void Update()
    {
        base.Update();

        if (shellRecoverTimer < 0f)
            return;

        shellRecoverTimer -= Time.deltaTime;
        if (!shakeTriggered && shellRecoverTimer <= 1f)
        {
            shakeTriggered = true;
            anim.SetTrigger("shake");
        }

        if (shellRecoverTimer > 0f)
            return;

        anim.SetBool("hit", false);
        currentHitLeft = maxHitToKill;
        isSocking = false;
        isPlaying = true;
        shellRecoverTimer = -1f;
        shakeTriggered = false;
    }

    protected override void HitEvent()
    {
        if (currentHitLeft == 1)
        {
            AudioService.PlaySfx(hurtSound, hurtSoundVolume);
            if (HurtEffect != null)
                Instantiate(HurtEffect, transform.position, transform.rotation);

            SetForce(0f, 0f);
            anim.SetBool("hit", true);
            isPlaying = false;
            isSocking = true;
            shellRecoverTimer = timeBackToAlive;
            shakeTriggered = false;
        }
        else
        {
            base.HitEvent();
            if (isDead)
                Dead();
        }
    }

    protected override void Dead()
    {
        shellRecoverTimer = -1f;
        shakeTriggered = false;
        base.Dead();

        SetForce(0, 5);
        controller.HandlePhysic = false;
    }

    protected override void OnRespawn()
    {
        anim.SetBool("hit", false);
        shellRecoverTimer = -1f;
        shakeTriggered = false;
        controller.HandlePhysic = true;
    }
}
