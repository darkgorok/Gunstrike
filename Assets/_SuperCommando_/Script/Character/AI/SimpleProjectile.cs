using UnityEngine;
using System.Collections;

public class SimpleProjectile : Projectile, ICanTakeDamage
{
    [Header("Bullet")]
	
	public GameObject DestroyEffect;
	public int pointToGivePlayer;
    public float timeToLive = 2;
    float timeLiveCounter = 0;
	public GameObject newBulletEffect;

	public AudioClip soundHitEnemy;
	[Range(0,1)]
	public float soundHitEnemyVolume = 0.5f;
	public AudioClip soundHitNothing;
	[Range(0,1)]
	public float soundHitNothingVolume = 0.5f;

	private SpriteRenderer rend;

    void OnDisable()
    {
        isDestroy = false;
        isDetect = false;
        target = null;
        isUseRadar = false;
        timeLiveCounter = timeToLive;
    }

    private void OnEnable()
    {
        isDestroy = false;
        isDetect = false;
        target = null;
        timeLiveCounter = timeToLive;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update ()
	{
		if ((timeLiveCounter -= Time.deltaTime) <= 0) {
			DestroyProjectile ();
			return;
		}

        if (!isDetect && isUseRadar)
        {

            RaycastHit2D hit = Physics2D.CircleCast((Vector2)transform.position + Direction * radarRadius, radarRadius, Vector2.zero,0, LayerCollision);
            if (hit)
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Platform"))
                {

                    var anotherSimpleProjectile = hit.collider.gameObject.GetComponent<SimpleProjectile>();
                    if (anotherSimpleProjectile != null)
                    {
                        if (Owner != anotherSimpleProjectile.Owner)
                        {
                            isDetect = true;
                            target = hit.collider.gameObject.transform;
                        }
                    }
                    else
                    {
                        isDetect = true;
                        target = hit.collider.gameObject.transform;
                    }
                }
            }

            transform.Translate((Direction + new Vector2(InitialVelocity.x, 0)) * Speed * Time.deltaTime, Space.World);
        }
        else if (target)
        {
            var aimPos = target.GetComponent<Collider2D>().bounds.center;
            transform.position = Vector2.MoveTowards(transform.position, aimPos, Speed * Time.deltaTime);

            //rotate the rocket look to the target
            Vector3 dir = aimPos - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            angle = Mathf.Lerp(transform.eulerAngles.z > 180 ? transform.eulerAngles.z - 360 : transform.eulerAngles.z, angle, 1f);

            var finalAngle = angle < 0 ? angle - 360 : angle;
            //			finalAngle *= GameManager.Instance.Player.isFacingRight ? 1 : -1;
            transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);

            //if (Vector2.Distance(target.position, transform.position) < 0.1f)
            //    target = null;
        }
        else
        {
            if (isDetect)
            {
                if (DestroyEffect != null)
                    Instantiate(DestroyEffect, transform.position, Quaternion.identity);

                gameObject.SetActive(false);
            }
            transform.Translate(/*(Direction + new Vector2(InitialVelocity.x, 0)) * */Speed * Time.deltaTime, 0,0, Space.Self);
            //Debug.LogError((Direction + new Vector2(InitialVelocity.x, 0)) * Speed * Time.deltaTime);
        }

        base.Update();
    }
	bool isDestroy = false;
	void DestroyProjectile(){
		if (isDestroy)
			return;
		isDestroy = true;
		if (DestroyEffect != null)
        {
            SpawnSystemHelper.GetNextObject(DestroyEffect, true).transform.position = transform.position;
        }

		if (isPower && newBulletEffect) {
			var bullet = Instantiate (newBulletEffect, transform.position, Quaternion.identity) as GameObject;
			bullet.GetComponent<Grenade> ().DoExplosion ();

		}
        
        gameObject.SetActive(false);
	}


	public void TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
		if (pointToGivePlayer != 0) {
			var projectile = instigator.GetComponent<Projectile> ();
			if (projectile != null && projectile.Owner.GetComponent<Player> () != null) {
				GameManager.Instance.AddPoint (pointToGivePlayer);
				//GameManager.Instance.ShowFloatingText ("+" + pointToGivePlayer, transform.position,Color.yellow);
			}
		}

		SoundManager.PlaySfx (soundHitNothing, soundHitNothingVolume);
		DestroyProjectile ();
	}

	protected override void OnCollideOther (RaycastHit2D other)
	{
		SoundManager.PlaySfx (soundHitNothing, soundHitNothingVolume);
		DestroyProjectile ();
	}

	protected override void OnCollideTakeDamage (RaycastHit2D other, ICanTakeDamage takedamage)
	{
        if (isDestroy)
            return;

        takedamage.TakeDamage (Damage, Vector2.zero, Owner, other.point);
        if (Owner == GameManager.Instance.Player.gameObject)
            CameraPlay.EarthQuakeShake(0.1f, 60, 1.5f);

		SoundManager.PlaySfx (soundHitEnemy, soundHitEnemyVolume);
		DestroyProjectile ();
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.right * radarRadius, radarRadius);
    }
}

