using System.Collections.Generic;
using UnityEngine;

public class EnemyCallMinion : MonoBehaviour
{
    public GameObject minion;
    public LayerMask layerAsGround;
    public float delayCallMin = 3f;
    public float delayCallMax = 6f;
    public float distanceMin = 1f;
    public float distanceMax = 3f;
    public int numberMinionMax = 3;

    private float lastCallTime;
    private float delaySpawn;
    private bool pendingRetry;
    private bool retryFacingRight;
    private readonly List<GameObject> listEnemy = new List<GameObject>();

    private void Start()
    {
        delaySpawn = Random.Range(delayCallMin, delayCallMax);
    }

    private void Update()
    {
        if (!pendingRetry)
            return;

        TrySpawnMinion(retryFacingRight);
    }

    public int numberEnemyLive()
    {
        int live = 0;
        foreach (var obj in listEnemy)
        {
            if (obj != null && obj.activeInHierarchy)
                live++;
        }

        return live;
    }

    public bool CanCallMinion()
    {
        if (pendingRetry || numberEnemyLive() >= numberMinionMax)
            return false;

        return Time.time >= lastCallTime + delaySpawn;
    }

    public void CallMinion(bool isFacingRight)
    {
        retryFacingRight = isFacingRight;
        TrySpawnMinion(isFacingRight);
    }

    private void TrySpawnMinion(bool isFacingRight)
    {
        Vector2 randomSpawnPoint = new Vector2(
            Random.Range(transform.position.x + distanceMin * (isFacingRight ? 1 : -1), transform.position.x + distanceMax * (isFacingRight ? 1 : -1)),
            transform.position.y + 1f);

        RaycastHit2D hit = Physics2D.Raycast(randomSpawnPoint, Vector2.down, 10f, layerAsGround);
        if (!hit)
        {
            pendingRetry = true;
            return;
        }

        pendingRetry = false;
        listEnemy.Add(Instantiate(minion, hit.point + Vector2.up * 0.1f, Quaternion.identity));
        lastCallTime = Time.time;
        delaySpawn = Random.Range(delayCallMin, delayCallMax);
    }
}
