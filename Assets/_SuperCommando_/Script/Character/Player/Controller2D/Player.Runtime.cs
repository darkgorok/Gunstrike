using System.Collections;
using UnityEngine;

public partial class Player
{
    [Space]
    [Header("HIT EFFECT")]
    public bool playEarthQuakeOnHitDogge = true;
    public float _eqTime = 0.1f;
    public float _eqSpeed = 60;
    public float _eqSize = 1;
    [Space]

    int aim_angle = 0;
    bool isLieDown = false;
    float lastPosX;
    IEnumerator ForceStandingCoDo;

    public bool isFrozen { get; set; }

    void CheckBelow()
    {
        if (controller.collisions.ClosestHit.collider != null)
        {
            var standObj = (IStandOnEvent)controller.collisions.ClosestHit.collider.gameObject.GetComponent(typeof(IStandOnEvent));
            if (standObj != null)
                standObj.StandOnEvent(gameObject);
        }
    }

    void CheckBlock()
    {
        Block isBlock;
        BrokenTreasure isTreasureBlock;
        Bounds bound = controller.boxcollider.bounds;

        RaycastHit2D hit = Physics2D.Raycast(new Vector2((bound.min.x + bound.max.x) / 2f, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));
        HandleBlockHit(hit);

        hit = Physics2D.Raycast(new Vector2(bound.min.x, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));
        HandleBlockHit(hit);

        hit = Physics2D.Raycast(new Vector2(bound.max.x, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));
        HandleBlockHit(hit);

        void HandleBlockHit(RaycastHit2D raycastHit)
        {
            if (!raycastHit)
                return;

            isBlock = raycastHit.collider.gameObject.GetComponent<Block>();
            if (isBlock)
                isBlock.BoxHit();

            isTreasureBlock = raycastHit.collider.gameObject.GetComponent<BrokenTreasure>();
            if (isTreasureBlock)
                isTreasureBlock.BoxHit();
        }
    }

    void GetInput()
    {
        Vector2 controllerInput = controllerInputService != null ? controllerInputService.MoveInput : Vector2.zero;
        input = new Vector2(controllerInput.x + Input.GetAxis("Horizontal"), controllerInput.y + Input.GetAxis("Vertical"));

        if ((input.x < -0.2f && isFacingRight) || (input.x > 0.2f && !isFacingRight))
            Flip();
    }

    void LateUpdate()
    {
        if (isFrozen)
            return;

        if ((controller.raycastOrigins.bottomLeft.x < cameraRigService.MinBounds.x && velocity.x < 0) ||
            (controller.raycastOrigins.bottomRight.x > cameraRigService.MaxBounds.x && velocity.x > 0))
        {
            velocity.x = 0;
        }

        if (forceStannding || gameSession.State != GameManager.GameState.Playing)
            velocity.x = 0;

        if (controller.raycastOrigins.bottomLeft.y < cameraRigService.MinBounds.y)
            gameSession.GameOver();

        controller.Move(velocity * Time.deltaTime, input);
        if (!isDead)
            cameraRigService.FollowPlayer();

        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0;

        if (controller.collisions.below)
        {
            numberOfJumpLeft = 0;
            CheckBelow();
        }

        lastPosX = transform.position.x;
    }

    public void PausePlayer(bool pause)
    {
        StopMove();
        isPlaying = !pause;
    }

    public void Frozen(bool is_enable)
    {
        input = Vector2.zero;
        velocity = Vector2.zero;
        isFrozen = is_enable;
        anim.enabled = !is_enable;
    }

    private void Flip()
    {
        if (forceStannding)
            return;

        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        isFacingRight = transform.localScale.x > 0;
    }

    public void MoveLeft()
    {
        if (!isPlaying)
            return;

        input = new Vector2(-1, 0);
        if (isFacingRight)
            Flip();
    }

    public void MoveRight()
    {
        if (!isPlaying)
            return;

        input = new Vector2(1, 0);
        if (!isFacingRight)
            Flip();
    }

    public void StopMove()
    {
        input = Vector2.zero;
    }

    public void FallDown()
    {
        input = new Vector2(0, -1);
    }

    public void Jump()
    {
        if (!isPlaying || forceStannding)
            return;

        if (controller.collisions.below)
        {
            if (input.y == -1)
            {
                controller.JumpDown();
                return;
            }

            velocity.y = maxJumpVelocity;

            if (JumpEffect)
                SpawnSystemHelper.GetNextObject(JumpEffect, true).transform.position = transform.position;

            audioService.PlaySfx(jumpSound, jumpSoundVolume);
            numberOfJumpLeft = numberOfJumpMax;
        }
        else
        {
            numberOfJumpLeft--;
            if (numberOfJumpLeft > 0)
            {
                anim.SetTrigger("doubleJump");
                velocity.y = minJumpVelocity;

                if (JumpEffect)
                    SpawnSystemHelper.GetNextObject(JumpEffect, true).transform.position = transform.position;
                audioService.PlaySfx(jumpSound, jumpSoundVolume);
            }
        }
    }

    public void JumpOff()
    {
        if (velocity.y > minJumpVelocity)
            velocity.y = minJumpVelocity;
    }

    public void ForceStanding(float delay)
    {
        if (ForceStandingCoDo != null)
            StopCoroutine(ForceStandingCoDo);

        ForceStandingCoDo = ForceStandingCo(delay);
        StartCoroutine(ForceStandingCoDo);
    }

    IEnumerator ForceStandingCo(float delay)
    {
        forceStannding = true;
        input.x = 0;
        velocity.x = 0;
        yield return new WaitForSeconds(delay);
        forceStannding = false;
    }

    public void SetForce(Vector2 force, bool springPush = false)
    {
        if (!springPush && isBlinking)
            return;

        if (springPush)
            numberOfJumpLeft = numberOfJumpMax;

        velocity = (Vector3)force;
    }

    public void AddForce(Vector2 force)
    {
        velocity += (Vector3)force;
    }

    public void RespawnAt(Vector2 pos)
    {
        transform.position = pos;
        if (respawnFX)
            Instantiate(respawnFX, pos, respawnFX.transform.rotation);

        isPlaying = true;
        isDead = false;
        Health = maxHealth;

        audioService.PlaySfx(respawnSound, 0.8f);
        ResetAnimation();
        controller.HandlePhysic = true;
        StartCoroutine(BlinkingCo(1.5f));
    }

    void HandleAnimation()
    {
        anim.SetFloat("speed", Mathf.Abs(velocity.x));
        anim.SetFloat("height_speed", velocity.y);
        anim.SetBool("isGrounded", controller.collisions.below);
        anim.SetFloat("inputY", input.y);
        anim.SetFloat("inputX", input.x);

        isLieDown = Mathf.Abs(velocity.x) < 0.1f && input.x < 0.3f && input.y < -0.7f && isGrounded;
        anim.SetBool("isLieDown", isLieDown);
        if (gameSession.State != GameManager.GameState.Playing)
            return;

        Vector2 normalizedInput = input.normalized;
        if (normalizedInput == Vector2.zero)
            aim_angle = 0;
        else if (normalizedInput.y > 0.9f)
            aim_angle = 90;
        else if (normalizedInput.y < -0.9f)
            aim_angle = -90;
        else if (normalizedInput.y < 0.5f && normalizedInput.y > -0.5f && (normalizedInput.x > 0.5f || normalizedInput.x < -0.5f))
            aim_angle = 0;
        else if (normalizedInput.y > 0.5f && (normalizedInput.x > 0.5f || normalizedInput.x < -0.5f))
            aim_angle = 45;
        else if (normalizedInput.y < -0.5f && (normalizedInput.x > 0.5f || normalizedInput.x < -0.5f))
            aim_angle = -45;

        anim.SetInteger("lookAngle", aim_angle);
    }

    void ResetAnimation()
    {
        anim.SetFloat("speed", 0);
        anim.SetFloat("height_speed", 0);
        anim.SetBool("isGrounded", true);
        anim.SetBool("isWall", false);
        anim.SetTrigger("reset");
    }

    public void GameFinish()
    {
        StopMove();
        isPlaying = false;
        anim.SetTrigger("finish");
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isPlaying || isBlinking)
            return;

        audioService.PlaySfx(hurtSound, hurtSoundVolume);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = hitPoint == Vector3.zero ? instigator.transform.position : hitPoint;

        if (Health <= 0)
        {
            gameSession.GameOver();
        }
        else
        {
            anim.SetTrigger("hurt");
            StartCoroutine(BlinkingCo(rateGetDmg));
        }

        if (instigator != null)
        {
            int dirKnockBack = instigator.transform.position.x > transform.position.x ? -1 : 1;
            SetForce(new Vector2(knockbackForce * dirKnockBack, 0));
        }
    }

    IEnumerator BlinkingCo(float time)
    {
        isBlinking = true;
        int blink = (int)(time * 0.5f / 0.1f);
        for (int i = 0; i < blink; i++)
        {
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.1f);
        }

        isBlinking = false;
    }

    public void GiveHealth(int hearthToGive, GameObject instigator)
    {
        Health = Mathf.Min(Health + hearthToGive, maxHealth);
    }

    public void Kill()
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        forceStannding = false;
        isDead = true;
        StopAllCoroutines();
        StopMove();
        audioService.PlaySfx(deadSound, deadSoundVolume);
        anim.SetTrigger("dead");
        SetForce(new Vector2(0, 7f));
        Health = 0;
    }

    public void Teleport(Transform newPos, float timer)
    {
        StartCoroutine(TeleportCo(newPos, timer));
    }

    IEnumerator TeleportCo(Transform newPos, float timer)
    {
        StopMove();
        isPlaying = false;
        Color color = Color.white;

        float transparentSpeed = 3;
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * transparentSpeed;
            color.a = Mathf.Clamp01(alpha);
            yield return null;
        }

        transform.position = newPos.position;
        yield return new WaitForSeconds(timer);

        isPlaying = true;
        yield return null;
        isPlaying = false;

        alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * transparentSpeed;
            color.a = Mathf.Clamp01(alpha);
            yield return null;
        }

        color.a = 1;
        isPlaying = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlaying)
            return;

        var isTriggerEvent = collision.GetComponent<TriggerEvent>();
        if (isTriggerEvent != null)
            isTriggerEvent.OnContactPlayer();

        if (collision.CompareTag("Checkpoint"))
        {
            RaycastHit2D hitGround = Physics2D.Raycast(collision.transform.position, Vector2.down, 100, gameSession.GroundLayer);
            gameSession.SaveCheckpoint(hitGround ? hitGround.point : (Vector2)collision.transform.position);
        }

        if (collision.CompareTag("DeadZone"))
            gameSession.GameOver();

        var collectItem = (ICanCollect)collision.GetComponent(typeof(ICanCollect));
        if (collectItem != null)
            collectItem.Collect();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isPlaying)
            return;

        var itemType = collision.GetComponent<ItemType>();
        if (itemType)
            itemType.Collect();
    }

    public void IPlay()
    {
        isPlaying = true;
    }

    public void ISuccess() { }
    public void IPause() { }
    public void IUnPause() { }

    public void IGameOver()
    {
        Kill();
    }

    public void IOnRespawn() { }
    public void IOnStopMovingOn() { }
    public void IOnStopMovingOff() { }
}
