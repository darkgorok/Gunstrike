using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartDamageObject : MonoBehaviour, ICanTakeDamage
{
    //public enum DamageType { KILL, DAMAGE}
    //public DamageType damageType;
    //public float damage = 30;

    [Header("Like Enemy")]
    public bool canBeHit = true;
    public GameObject HurtEffect;
    public GameObject DestroyEffect;        //spawn object when dead
    [Range(0, 100)]
    public float health = 50;
    float currentHealth;
    public AudioClip hurtSound;
    public AudioClip deadSound;


    // Use this for initialization
    void Start () {
        currentHealth = health;
    }

    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.gameObject == GameManager.Instance.Player.gameObject)
    //    {
    //        if (other.gameObject.GetComponent(typeof(ICanTakeDamage)))
    //        {
    //            other.gameObject.GetComponent<ICanTakeDamage>().TakeDamage(damageType == DamageType.KILL ? int.MaxValue : damage, Vector2.zero, gameObject);
    //        }
    //    }
    //}

    bool isDead = false;
    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!enabled)
            return;

        if (isDead)
            return;

        if (!canBeHit)
            return;

        currentHealth -= damage;
        //Debug.LogError(damage);
        if (currentHealth <= 0)
        {
            isDead = true;
            Destroy();
        }
        else
            HitEvent();
    }

    protected void HitEvent()
    {
        SoundManager.PlaySfx(hurtSound);
        if (HurtEffect != null)
            Instantiate(HurtEffect, transform.position, transform.rotation);
    }

    protected void Destroy()
    {
        SoundManager.PlaySfx(deadSound);

        if (DestroyEffect != null)
            Instantiate(DestroyEffect, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
