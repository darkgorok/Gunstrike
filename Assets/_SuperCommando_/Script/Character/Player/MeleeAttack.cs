using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour {
	[Tooltip("What layers should be hit")]
	public LayerMask CollisionMask;
	[Tooltip("Hit more than one enemy at the same time")]
	public bool multiDamage = false;
	[Tooltip("Give damage to the enemy or object")]
	public int damageToGive;
    [ReadOnly] public int extraDamage = 0;
	[Tooltip("Apply the force to enemy if they are hit, only for Rigidbody object")]
	public Vector2 pushObject;
	public Transform MeleePoint;
	public float areaSize;

	public float attackRate = 0.2f;
	[Tooltip("Check target in range after a delay time, useful to sync the right attack time of the animation")]
	public float attackAfterTime = 0.15f;

	float nextAttack = 0;

    public AudioClip soundCombo1;
    public AudioClip soundCombo2;

    public GameObject hitEffect;

   [HideInInspector] public int combo = 2;
    public float comboTime = 0.5f;
    float timer = 0;

    [Header("HIT EFFECT")]
    public bool playEarthQuakeOnHit = true;
    public float _eqTime = 0.1f;
    public float _eqSpeed = 60;
    public float _eqSize = 1.5f;

    private void Start()
    {
        currentCombo = 0;
        extraDamage = (int) GlobalValue.UpgradeItemPower(UPGRADE_ITEM_TYPE.sword.ToString());
    }

    private void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;
    }

    public void Attack(){
        if ((currentCombo + 1) > combo)
            return;

        currentCombo++;
        currentCombo = Mathf.Clamp(currentCombo, 1, combo);
        
        timer += comboTime;
    }

	//IEnumerator CheckTargetCo(float delay){
	//	yield return new WaitForSeconds (delay);

 //       Check4Hit();
 //   }

    public void Check4Hit()
    {
       

        var hits = Physics2D.CircleCastAll(MeleePoint.position, areaSize, Vector2.zero, 0, CollisionMask);

        if (hits == null)
            return;

        foreach (var hit in hits)
        {
            var damage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
            if (damage == null)
                continue;

            var isProjectile = (Projectile) hit.collider.gameObject.GetComponent(typeof(Projectile));
           
            if (isProjectile)
            {
                isProjectile.Speed *= -1;
                isProjectile.LayerCollision = isProjectile.LayerCollision | GameManager.Instance.enemyLayer;
            }
            else
                damage.TakeDamage(damageToGive + extraDamage, pushObject, gameObject, Vector2.zero);

            if (playEarthQuakeOnHit)
            {
                CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
            }

            if (hitEffect)
                SpawnSystemHelper.GetNextObject(hitEffect, true).transform.position = hit.point;


            if (!multiDamage)
                break;
        }
    }

    #region COMBO
    [HideInInspector]
    public int currentCombo = 0;

    public void PlaySoundAttack(int combo)
    {
        switch (combo)
        {
            case 1:
                SoundManager.PlaySfx(soundCombo1);
                break;
            case 2:
                SoundManager.PlaySfx(soundCombo2);
                break;
            default:
                return;
        }
    }

    public void ComboEnd()
    {
        //Debug.LogError("ComboEnd");
        currentCombo = 0;
        GameManager.Instance.Player.anim.SetInteger("combo_", 0);
        
    }
    #endregion

    void OnDrawGizmos(){
		if (MeleePoint == null)
			return;
		
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (MeleePoint.position, areaSize);
	}
}
