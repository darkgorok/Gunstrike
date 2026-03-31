using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    public static GunManager Instance;
    public List<GunTypeID> listGun;
   [ReadOnly] public List<GunTypeID> listGunPicked;

    int currentPos = 0;

    private void Awake()
    {
        if (GunManager.Instance != null)
            Destroy(gameObject);
        else
        {
            Instance = this;
            //if (GameMode.Instance)
            //{
            //    foreach (var gun in listGun)
            //    {
            //        if (GlobalValue.isPicked(gun))
            //        {
            //            AddGun(gun);
            //        }
            //    }
            //}
            //else
            //{
                for (int i = 0; i < listGun.Count; i++)
                {
                    AddGun(listGun[i]);
                }
            //}

            DontDestroyOnLoad(gameObject);
        }
    }

    public void ResetPlayerCarryGun()
    {
        listGunPicked.Clear();
        foreach (var gun in listGun)
        {
            if (GlobalValue.isPicked(gun))
            {
                AddGun(gun);
            }
        }
        currentPos = 0;
    }

    public void AddBullet(int amount)
    {
        foreach (var gun in listGunPicked)
        {
            gun.bullet += amount;
        }
    }

    public void ResetGunBullet()
    {
        foreach (var gun in listGunPicked)
        {
            gun.ResetBullet();
        }
    }

    //public void ResetGunBullet(GunTypeID gun)
    //{
    //    foreach (var g in listGunPicked)
    //    {
    //        if (g.gameObject == gun.gameObject)
    //            g.ResetBullet();
    //    }
    //}

    public void AddGun(GunTypeID gunID, bool pickImmediately = false)
    {
        listGunPicked.Add(gunID);
    }

    public void SetNewGunDuringGameplay(GunTypeID gunID)
    {
        GunTypeID pickGun = null;
        GlobalValue.currentGunTypeID = gunID;
        foreach (var gun in listGun)
        {
            if (gun.gunID == gunID.gunID)
            {
                if (!listGunPicked.Contains(gun))
                    AddGun(gun);
                else
                {
                    foreach (var _gun in listGunPicked)
                    {
                        if (_gun.gunID == gun.gunID)
                            _gun.ResetBullet();
                    }
                }

                pickGun = gun;
            }
        }

        if (pickGun != null)
        {
            NextGun(pickGun);
            pickGun.ResetBullet();
        }
    }

    public void RemoveGun(GunTypeID gunID)
    {
        listGunPicked.Remove(gunID);
    }

    public void NextGun()
    {
        currentPos++;
        if(currentPos>= listGunPicked.Count)
        {
            currentPos = 0;
        }

        GameManager.Instance.Player.SetGun(listGunPicked[currentPos]);
        SoundManager.PlaySfx(SoundManager.Instance.swapGun);
    }

    public void BackToDefaultGun()
    {
        currentPos = 0;
        GameManager.Instance.Player.SetGun(listGunPicked[currentPos]);
        SoundManager.PlaySfx(SoundManager.Instance.swapGun);
    }

    public void NextGun(GunTypeID gunID)
    {
        if (listGunPicked[currentPos].gunID == gunID.gunID)
            return;     //don't swap gun when the player holding the same gun

        for(int i = 0; i < listGunPicked.Count; i++)
        {
            if(listGunPicked[i].gunID == gunID.gunID)
            {
                currentPos = i;
                GameManager.Instance.Player.SetGun(listGunPicked[currentPos]);
                SoundManager.PlaySfx(SoundManager.Instance.swapGun);
            }
        }
    }

    public GunTypeID getGunID()
    {
        return listGunPicked[currentPos];
    }
}
