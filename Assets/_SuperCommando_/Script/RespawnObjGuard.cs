using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnObjGuard : MonoBehaviour, IListener
{
    GameObject _cloneOwner;

    public void IGameOver()
    {
        
    }

    public void IOnRespawn()
    {
        _cloneOwner.SetActive(true);
        Destroy(gameObject);
    }

    public void IOnStopMovingOff()
    {
       
    }

    public void IOnStopMovingOn()
    {
       
    }

    public void IPause()
    {
      
    }

    public void IPlay()
    {
       
    }

    public void ISuccess()
    {
       
    }

    public void IUnPause()
    {
    
    }

    // Start is called before the first frame update
    void Start()
    {
        _cloneOwner = Instantiate(gameObject, transform.position, Quaternion.identity) as GameObject;
        _cloneOwner.SetActive(false);
    }
}
