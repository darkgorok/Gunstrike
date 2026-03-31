using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveObjInRange : MonoBehaviour, IListener{
    [Header("Should delay for all object can set up correctly first")]
    //public float delayCheckObject = 3;
    public float distanceActiveContainer = 20;
    [Range(0.1f, 1f)]
    public float checkingRate = 0.3f;
   public List<Transform> listGameObjects;
    void Start()
    {
        if(listGameObjects.Count  == 0)
        {
            listGameObjects = new List<Transform>(transform.GetComponentsInChildren<Transform>());
            listGameObjects.Remove(transform);      //remove this parent out of the list, because it's added in the list
        }
    }

    void CheckDistance()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        foreach (var child in listGameObjects)
        {
            if (child != null)
                child.gameObject.SetActive(Vector2.Distance(child.transform.position, GameManager.Instance.Player.transform.position) < distanceActiveContainer);
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var child in listGameObjects)
        {
            if (child != null)
                Gizmos.DrawWireSphere(child.position, distanceActiveContainer);
        }
    }

    public void IPlay()
    {
        InvokeRepeating("CheckDistance", 0, 0.3f);
    }

    public void ISuccess()
    {
       
    }

    public void IPause()
    {
        
    }

    public void IUnPause()
    {
       
    }

    public void IGameOver()
    {
        CancelInvoke();
    }

    public void IOnRespawn()
    {
       
    }

    public void IOnStopMovingOn()
    {
      
    }

    public void IOnStopMovingOff()
    {
     
    }
}
