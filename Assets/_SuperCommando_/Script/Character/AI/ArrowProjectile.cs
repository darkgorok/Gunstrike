using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowProjectile : Projectile, IListener
{
    public bool parentToHitObject = true;
    public bool destroyWhenContact = false;
    Vector2 oldPos;
    public GameObject DestroyEffect;
    public int pointToGivePlayer;
    public float timeToLive = 3;
    public AudioClip soundHitEnemy;
    [Range(0, 1)]
    public float soundHitEnemyVolume = 0.5f;
    public AudioClip soundHitNothing;
    [Range(0, 1)]
    public float soundHitNothingVolume = 0.5f;

    [Header("EXPLOSTION")]
    public bool doExplosionDamage = false;
    public Grenade _grenade;
    public int explosionDamage = 100;
    public float explosionRadius = 1;

    float timeToLiveCounter = 0;
    

    bool isHit = false;
    Rigidbody2D rig;

    void OnEnable()
    {
        timeToLiveCounter = timeToLive;
        isHit = false;

        if (rig == null)
            rig = GetComponent<Rigidbody2D>();
        rig.isKinematic = false;
    }

    public void Init(Vector2 velocityForce, float gravityScale)
    {
        rig = GetComponent<Rigidbody2D>();
        rig.gravityScale = gravityScale;
        rig.linearVelocity = velocityForce;
    }

    public override void Start()
    {
        base.Start();
        oldPos = transform.position;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    IEnumerator DestroyProjectile(float delay = 0)
    {
        var rig = GetComponent<Rigidbody2D>();
        rig.linearVelocity = Vector2.zero;
        rig.isKinematic = true;
        if (!destroyWhenContact)
            yield return new WaitForSeconds(delay);

        if (DestroyEffect != null)
            SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position;

        var obj = (Grenade)Instantiate(_grenade, transform.position, Quaternion.identity);
        obj.Init( explosionDamage, explosionRadius, true);

        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage, Vector2 force, Vector2 hitPoint, GameObject instigator, Vector3 point)
    {
        SoundManager.PlaySfx(soundHitNothing, soundHitNothingVolume);
        StartCoroutine(DestroyProjectile(1));
    }

    protected override void OnCollideOther(RaycastHit2D other)
    {
        SoundManager.PlaySfx(soundHitNothing, soundHitNothingVolume);
        StartCoroutine(DestroyProjectile(3));
        if (parentToHitObject)
            transform.parent = other.collider.gameObject.transform;

    }

    protected override void OnCollideTakeDamage(RaycastHit2D other, ICanTakeDamage takedamage)
    {
        //Debug.LogError(other.name);
        base.OnCollideTakeDamage(other, takedamage);
        takedamage.TakeDamage(int.MaxValue, Vector2.zero, Owner, other.point);
        SoundManager.PlaySfx(soundHitEnemy, soundHitEnemyVolume);
        StartCoroutine(DestroyProjectile(0));
    }
    
    bool isStop = false;
    #region IListener implementation

    public void IPlay()
    {
        //		throw new System.NotImplementedException ();
    }

    public void ISuccess()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IPause()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IUnPause()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IGameOver()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IOnRespawn()
    {
        //		throw new System.NotImplementedException ();
    }

    public void IOnStopMovingOn()
    {
        //		Debug.Log ("IOnStopMovingOn");
        //		anim.enabled = false;
        isStop = true;
        //		GetComponent<Rigidbody2D> ().isKinematic = true;
    }

    public void IOnStopMovingOff()
    {
        //		anim.enabled = true;
        isStop = false;
        //		GetComponent<Rigidbody2D> ().isKinematic = false;
    }

    public void TakeDamage(float damage, Vector2 force, Vector2 hitPoint, GameObject instigator, Vector2 point)
    {
        StartCoroutine(DestroyProjectile(0));
    }

    #endregion
}
