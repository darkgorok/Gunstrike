using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour {
    public bool onlyBlowWhenContactCollision;

    public float delayBlowUp = 0.7f;
    [Header("Explosion Damage")]
	public AudioClip soundDestroy;
	public GameObject[] DestroyFX;

	public LayerMask collisionLayer;
	public int makeDamage = 100;
	public float radius = 3;
    // Use this for initialization
    bool isBlowingUp = false;

	Rigidbody2D rig;
    Animator anim;

    Collider2D _collider;
    [ReadOnly] public float collideWithTheGroundUnderPosY = 1000;

    public void Init(int _damage, float _radius, bool blowImmediately = false, bool blowOnContactCollision = false, float _collideWithTheGroundUnderPosY = -1)
    {
        makeDamage = _damage;
        radius = _radius;
        onlyBlowWhenContactCollision = blowOnContactCollision;

        GetComponent<Collider2D>().enabled = false;
        if (_collideWithTheGroundUnderPosY != -1)
            collideWithTheGroundUnderPosY = _collideWithTheGroundUnderPosY;
        else
            collideWithTheGroundUnderPosY = 1000;

        if (blowImmediately)
        {
            DoExplosion();
        }
    }

    void Awake(){
		rig = GetComponent<Rigidbody2D> ();
        _collider = GetComponent<Collider2D>();

    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!_collider.enabled)
        {
            if (transform.position.y < collideWithTheGroundUnderPosY)
                _collider.enabled = true;
        }
    }

    IEnumerator OnCollisionEnter2D(Collision2D other){
        if (isBlowingUp)
            yield break;

        //only blow when contact the ground
        var hits = Physics2D.CircleCastAll(transform.position, 0.2f, Vector2.zero, 0, GameManager.Instance.groundLayer);
        if (hits == null)
            yield break;

        isBlowingUp = true;
        float delayCounter = 0;

        if (!onlyBlowWhenContactCollision)
        {
            while (delayCounter < delayBlowUp)
            {
                //anim.speed = Mathf.Lerp(anim.speed, 3, 2 * Time.deltaTime);
                delayCounter += Time.deltaTime;
                yield return null;
            }
        }

        DoExplosion();
    }

	public void DoExplosion(){

		var hits = Physics2D.CircleCastAll (transform.position, radius, Vector2.zero,0, collisionLayer);
		if (hits == null)
			return;

		foreach (var hit in hits) {
			var damage = (ICanTakeDamage) hit.collider.gameObject.GetComponent (typeof(ICanTakeDamage));
			if (damage == null)
				continue;

			damage.TakeDamage (makeDamage,Vector2.zero, gameObject, Vector2.zero);
		}

        foreach(var fx in DestroyFX)
        {
            if (fx)
            {
                var hitGround = Physics2D.Raycast(transform.position, Vector2.down, 100, GameManager.Instance.groundLayer);
                SpawnSystemHelper.GetNextObject(fx, true).transform.position = (hitGround ? (Vector3)hitGround.point : transform.position);
            }
        }
        

		SoundManager.PlaySfx (soundDestroy);
        Destroy(gameObject);
    }

	void OnDrawGizmos(){
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (transform.position, radius);
	}
}
