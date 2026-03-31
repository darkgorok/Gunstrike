using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCarrierControl : MonoBehaviour, ICanTakeDamage
{
    public float speed = 2;
    public GameObject destroyObj;
    public GameObject[] dropBullet;
    public AudioClip soundDestroy;

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (destroyObj)
            Instantiate(destroyObj, transform.position, Quaternion.identity);

        Instantiate(dropBullet[Random.Range(0, dropBullet.Length)], transform.position, Quaternion.identity);
        SoundManager.PlaySfx(soundDestroy);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }
}
