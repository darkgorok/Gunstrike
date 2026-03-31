using UnityEngine;

public sealed class LegacyGunRuntimeService : IGunRuntimeService
{
    private GunManager cachedGunManager;

    private GunManager Current
    {
        get
        {
            if (cachedGunManager == null)
                cachedGunManager = Object.FindFirstObjectByType<GunManager>();

            return cachedGunManager;
        }
    }

    public GunTypeID GetCurrentGun()
    {
        return Current != null ? Current.getGunID() : null;
    }

    public void ResetGunBullet()
    {
        Current?.ResetGunBullet();
    }

    public void SetNewGunDuringGameplay(GunTypeID gunTypeId)
    {
        Current?.SetNewGunDuringGameplay(gunTypeId);
    }

    public void BackToDefaultGun()
    {
        Current?.BackToDefaultGun();
    }
}
