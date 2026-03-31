using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SetActiveObjInRange : MonoBehaviour, IListener
{
    [Header("Should delay for all object can set up correctly first")]
    public float distanceActiveContainer = 20;
    [Range(0.1f, 1f)]
    public float checkingRate = 0.3f;
    public List<Transform> listGameObjects;

    private bool isActiveChecking;
    private float checkTimer;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        if (listGameObjects.Count == 0)
        {
            listGameObjects = new List<Transform>(transform.GetComponentsInChildren<Transform>());
            listGameObjects.Remove(transform);
        }
    }

    private void Update()
    {
        if (!isActiveChecking || gameSession == null || gameSession.State != GameManager.GameState.Playing)
            return;

        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f)
            return;

        checkTimer = Mathf.Max(0.1f, checkingRate);
        CheckDistance();
    }

    private void CheckDistance()
    {
        if (gameSession?.Player == null)
            return;

        foreach (Transform child in listGameObjects)
        {
            if (child != null)
                child.gameObject.SetActive(Vector2.Distance(child.transform.position, gameSession.Player.transform.position) < distanceActiveContainer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (Transform child in listGameObjects)
        {
            if (child != null)
                Gizmos.DrawWireSphere(child.position, distanceActiveContainer);
        }
    }

    public void IPlay()
    {
        isActiveChecking = true;
        checkTimer = 0f;
    }

    public void ISuccess() { }
    public void IPause() { }
    public void IUnPause() { }

    public void IGameOver()
    {
        isActiveChecking = false;
    }

    public void IOnRespawn() { }
    public void IOnStopMovingOn() { }
    public void IOnStopMovingOff() { }
}
