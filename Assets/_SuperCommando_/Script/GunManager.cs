using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class GunManager : MonoBehaviour
{
    public static GunManager Instance;
    public List<GunTypeID> listGun;
    [ReadOnly] public List<GunTypeID> listGunPicked;

    private int currentPos = 0;
    private IAudioService audioService;
    private IGameSessionService gameSession;
    private IInventoryService inventoryService;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession, IInventoryService inventoryService)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
        this.inventoryService = inventoryService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);

        if (GunManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        for (int i = 0; i < listGun.Count; i++)
        {
            AddGun(listGun[i]);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void ResetPlayerCarryGun()
    {
        listGunPicked.Clear();
        foreach (GunTypeID gun in listGun)
        {
            if (inventoryService.IsGunPicked(gun))
                AddGun(gun);
        }

        currentPos = 0;
    }

    public void AddBullet(int amount)
    {
        foreach (GunTypeID gun in listGunPicked)
        {
            gun.bullet += amount;
        }
    }

    public void ResetGunBullet()
    {
        foreach (GunTypeID gun in listGunPicked)
        {
            gun.ResetBullet();
        }
    }

    public void AddGun(GunTypeID gunID, bool pickImmediately = false)
    {
        listGunPicked.Add(gunID);
    }

    public void SetNewGunDuringGameplay(GunTypeID gunID)
    {
        GunTypeID pickGun = null;
        inventoryService.CurrentGunType = gunID;

        foreach (GunTypeID gun in listGun)
        {
            if (gun.gunID != gunID.gunID)
                continue;

            if (!listGunPicked.Contains(gun))
            {
                AddGun(gun);
            }
            else
            {
                foreach (GunTypeID pickedGun in listGunPicked)
                {
                    if (pickedGun.gunID == gun.gunID)
                        pickedGun.ResetBullet();
                }
            }

            pickGun = gun;
        }

        if (pickGun == null)
            return;

        NextGun(pickGun);
        pickGun.ResetBullet();
    }

    public void RemoveGun(GunTypeID gunID)
    {
        listGunPicked.Remove(gunID);
    }

    public void NextGun()
    {
        currentPos++;
        if (currentPos >= listGunPicked.Count)
            currentPos = 0;

        gameSession.Player.SetGun(listGunPicked[currentPos]);
        audioService.PlaySfx(audioService.SwapGunClip);
    }

    public void BackToDefaultGun()
    {
        currentPos = 0;
        gameSession.Player.SetGun(listGunPicked[currentPos]);
        audioService.PlaySfx(audioService.SwapGunClip);
    }

    public void NextGun(GunTypeID gunID)
    {
        if (listGunPicked[currentPos].gunID == gunID.gunID)
            return;

        for (int i = 0; i < listGunPicked.Count; i++)
        {
            if (listGunPicked[i].gunID != gunID.gunID)
                continue;

            currentPos = i;
            gameSession.Player.SetGun(listGunPicked[currentPos]);
            audioService.PlaySfx(audioService.SwapGunClip);
        }
    }

    public GunTypeID getGunID()
    {
        return listGunPicked[currentPos];
    }
}
