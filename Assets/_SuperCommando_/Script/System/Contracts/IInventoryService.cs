public interface IInventoryService
{
    int Darts { get; set; }
    int MaxDarts { get; }
    int PowerBullets { get; set; }
    int StoredGodItems { get; set; }

    GunTypeID CurrentGunType { get; set; }

    bool IsGunPicked(GunTypeID gunId);
    void PickGun(GunTypeID gunId);
}
