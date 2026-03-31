using UnityEngine;

public sealed class LegacyBossHealthbarService : IBossHealthbarService
{
    private BossHealthbar cachedBossHealthbar;

    private BossHealthbar Current
    {
        get
        {
            if (cachedBossHealthbar == null)
                cachedBossHealthbar = Object.FindFirstObjectByType<BossHealthbar>();

            return cachedBossHealthbar;
        }
    }

    public void Init(Sprite icon, int maxHealth)
    {
        Current?.Init(icon, maxHealth);
    }

    public void UpdateHealth(int currentHealth)
    {
        Current?.UpdateHealth(currentHealth);
    }
}
