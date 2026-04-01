using UnityEngine;

public sealed class LegacyBossHealthbarService : IBossHealthbarService
{
    private BossHealthbar cachedBossHealthbar;
    private Sprite pendingIcon;
    private int pendingMaxHealth;
    private bool hasPendingInit;

    private BossHealthbar Current
    {
        get
        {
            if (cachedBossHealthbar == null)
                cachedBossHealthbar = Object.FindFirstObjectByType<BossHealthbar>();

            if (cachedBossHealthbar != null && hasPendingInit)
            {
                cachedBossHealthbar.Init(pendingIcon, pendingMaxHealth);
                hasPendingInit = false;
            }

            return cachedBossHealthbar;
        }
    }

    public void Init(Sprite icon, int maxHealth)
    {
        pendingIcon = icon;
        pendingMaxHealth = maxHealth;
        hasPendingInit = true;

        var current = Current;
        if (current != null)
        {
            current.Init(icon, maxHealth);
            hasPendingInit = false;
        }
    }

    public void UpdateHealth(int currentHealth)
    {
        Current?.UpdateHealth(currentHealth);
    }
}
