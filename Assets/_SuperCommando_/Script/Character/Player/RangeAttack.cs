using UnityEngine;
using System.Collections;

public class RangeAttack : MonoBehaviour {
	
	public Transform FirePoint;
	
    public bool standToFire = true;
    public float standTime = 0.1f;
    [Tooltip("fire projectile after this delay, useful to sync with the animation of firing action")]
	public float fireDelay;
	public float fireRate;
	public bool inverseDirection = false;
    public float bulletSpeed = 10;

    [ReadOnly] public int extraDamage = 0;
    [Header("NORMAL BULLET")]
    [Range(1, 6)]
    public int normalSpeadBullet = 1;
    public Projectile Projectile;
    public int normalDamage = 30;

    [Header("POWER BULLET")]
   [HideInInspector] public bool useTrack = false;
    [Range(1, 12)]
    public int speadBullet = 5;
    public Projectile ProjectilePower;
    public int powerDamage = 30;

    float nextFire = 0;

    public AudioClip soundAttack;

    private void Start()
    {
        extraDamage = (int) GlobalValue.UpgradeItemPower(UPGRADE_ITEM_TYPE.dart.ToString());
    }

    public bool Fire(bool power)
    {
        if (power)
        {
            if ( GlobalValue.powerBullet > 0 && Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                
                GlobalValue.powerBullet--;
                StartCoroutine(DelayAttack(fireDelay, true));
                return true;
            }
            else
                return false;
        }

        else if (((DefaultValue.Instance && DefaultValue.Instance.defaultBulletMax) || (GlobalValue.Bullets > 0 || GameManager.Instance.hideGUI)) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            if (DefaultValue.Instance && DefaultValue.Instance.defaultBulletMax)
                ;
            else
                GlobalValue.Bullets--;
            StartCoroutine(DelayAttack(fireDelay, false));
            return true;
        }
        else
            return false;
    }

	IEnumerator DelayAttack(float time, bool powerBullet){
		yield return new WaitForSeconds (time);

		var direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

		if (inverseDirection )
			direction *= -1;

        Vector2 firePoint = FirePoint.position;

        if (powerBullet)
        {
            bool spawnUp = false;
            for (int i = 0; i < speadBullet; i++)
            {
                direction.y += i * 0.1f * (spawnUp ? 1 : -1);
                spawnUp = !spawnUp;

                var projectile = SpawnSystemHelper.GetNextObject(ProjectilePower.gameObject, false).GetComponent<Projectile>();
                projectile.transform.position = firePoint;

                //var projectile = (Projectile)Instantiate(ProjectilePower, firePoint, Quaternion.identity);

                projectile.Initialize(gameObject, direction, Vector2.zero, powerBullet, useTrack, powerDamage + extraDamage, bulletSpeed);
                projectile.gameObject.SetActive(true);
            }
        }
        else
        {
            bool spawnUp = false;
            for (int i = 0; i < normalSpeadBullet; i++)
            {
                direction.y += i * 0.1f * (spawnUp ? 1 : -1);
                spawnUp = !spawnUp;
                
                //var projectile = (Projectile)Instantiate(Projectile, firePoint, Quaternion.identity);
                var projectile = SpawnSystemHelper.GetNextObject(Projectile.gameObject, false).GetComponent<Projectile>();
                projectile.transform.position = firePoint;
                projectile.Initialize(gameObject, direction, Vector2.zero, powerBullet, false, normalDamage + extraDamage, bulletSpeed);
                projectile.gameObject.SetActive(true);
            }
        }
        
        SoundManager.PlaySfx(soundAttack);
    }
}
