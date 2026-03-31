using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoBullet : MonoBehaviour
{
    public bool TA_twoDirection = true;
    public int damagePerBullet = 50;
    public Projectile projectile;
    public float bulletSpeed = 5;
    public AudioClip sound;

    public void Init(bool _TA_twoDirection, int _damagePerBullet, float _bulletSpeed)
    {
        TA_twoDirection = _TA_twoDirection;
        damagePerBullet = _damagePerBullet;
        bulletSpeed = _bulletSpeed;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        float angle = 0;
        SoundManager.PlaySfx(sound);
        for (int i = 0; i < (TA_twoDirection?2:1); i++)
        {
            angle = 180 * i;
            var _projectile = SpawnSystemHelper.GetNextObject(projectile.gameObject, false);
            _projectile.transform.position = transform.position;
            _projectile.GetComponent<Projectile>().Initialize(gameObject, UltiHelper.AngleToVector2(angle), Vector2.zero, false, false, damagePerBullet, bulletSpeed);
            _projectile.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
