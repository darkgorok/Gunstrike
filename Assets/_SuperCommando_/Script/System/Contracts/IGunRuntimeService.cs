public interface IGunRuntimeService
{
    GunTypeID GetCurrentGun();
    void ResetGunBullet();
    void SetNewGunDuringGameplay(GunTypeID gunTypeId);
    void BackToDefaultGun();
}
