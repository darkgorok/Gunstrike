using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingProjectileBullet : MonoBehaviour
{
    public FallingProjectileBulletBullet bullet;
    public float speed = 10;
    public float delay = 1;
    public float space = 1.5f;
    public int numberBullet = 4;

    public List<FallingProjectileBulletBullet> bullets;

    public void Init(int _numberBulet, float _delay, float _speed, float _space)
    {
        numberBullet = _numberBulet;
        delay = _delay;
        speed = _speed;
        space = _space;
    }

    void Start()
    {
        transform.Translate(-space * (numberBullet - 1) / 2f, 0, 0);
        bullets = new List<FallingProjectileBulletBullet>();
        for (int i = 0; i < numberBullet; i++)
        {
            bullets.Add(Instantiate(bullet, transform.position + Vector3.right * space * i, Quaternion.identity));
        }

        Invoke(nameof(Action), delay);
    }

    void Action()
    {
        foreach (var spawnedBullet in bullets)
        {
            spawnedBullet.Action(Vector2.down, speed, gameObject);
        }
    }

    void Update()
    {
    }
}
