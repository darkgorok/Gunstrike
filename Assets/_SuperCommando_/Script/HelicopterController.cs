using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CheckTargetHelper))]

public class HelicopterController : MonoBehaviour, ICanTakeDamage
{
    public int health = 200;
    public Grenade bomb;
    public float dropRate = 1.5f;
    public AudioClip soundDestroy;
    float lastDropTime = -999;

    [ReadOnly] public bool allowMoving = false;
    public float speed = 2;

    public GameObject explosionFX;

    CheckTargetHelper checkTargetHelper;

    private void Start()
    {
        checkTargetHelper = GetComponent<CheckTargetHelper>();
    }

    private void Update()
    {
        if (!allowMoving)
        {
            if (checkTargetHelper.CheckTarget(transform.position.x > GameManager.Instance.Player.transform.position.x ? 1 : -1))
            {
                allowMoving = true;
                GameManager.Instance.PauseCamera(true);
            }
        }

        if (allowMoving)
        {
            if (Mathf.Abs(transform.position.x - GameManager.Instance.Player.transform.position.x) > 0.1f)
                transform.Translate(speed * Time.deltaTime * (transform.position.x > GameManager.Instance.Player.transform.position.x ? -1 : 1), 0, 0);

            if(Time.time > (lastDropTime + dropRate))
            {
                lastDropTime = Time.time;

                Vector3 throwPos = transform.position;
                var obj = (Grenade)Instantiate(bomb, throwPos, Quaternion.identity);
                obj.Init(100, 0.75f, false, true);
            }
        }
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        health -= damage;
        if (health <= 0)
        {
            Instantiate(explosionFX, transform.position, Quaternion.identity);
            GameManager.Instance.PauseCamera(false);
            SoundManager.PlaySfx(soundDestroy);
            Destroy(gameObject);
        }
    }
}