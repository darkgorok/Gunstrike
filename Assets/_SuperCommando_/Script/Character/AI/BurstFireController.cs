using System;

public sealed class BurstFireController
{
    private readonly int shotsPerBurst;
    private readonly float burstCooldown;
    private readonly float shotInterval;

    private int shotsRemaining;
    private float cooldownRemaining;
    private float shotDelayRemaining;
    private bool burstActive;

    public BurstFireController(int shotsPerBurst, float burstCooldown, float shotInterval)
    {
        this.shotsPerBurst = Math.Max(1, shotsPerBurst);
        this.burstCooldown = burstCooldown;
        this.shotInterval = shotInterval;
    }

    public bool CanStartBurst => !burstActive && cooldownRemaining <= 0f;

    public void StartBurst(Action fireShot)
    {
        if (!CanStartBurst)
            return;

        burstActive = true;
        shotsRemaining = shotsPerBurst;
        shotDelayRemaining = 0f;
        TryFire(fireShot);
    }

    public void Tick(float deltaTime, Action fireShot)
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= deltaTime;

        if (!burstActive)
            return;

        shotDelayRemaining -= deltaTime;
        if (shotDelayRemaining > 0f)
            return;

        TryFire(fireShot);
    }

    public void CancelBurst()
    {
        burstActive = false;
        shotsRemaining = 0;
        shotDelayRemaining = 0f;
    }

    private void TryFire(Action fireShot)
    {
        if (shotsRemaining <= 0)
        {
            FinishBurst();
            return;
        }

        fireShot?.Invoke();
        shotsRemaining--;

        if (shotsRemaining > 0)
        {
            shotDelayRemaining = shotInterval;
            return;
        }

        FinishBurst();
    }

    private void FinishBurst()
    {
        burstActive = false;
        cooldownRemaining = burstCooldown;
        shotDelayRemaining = 0f;
        shotsRemaining = 0;
    }
}
