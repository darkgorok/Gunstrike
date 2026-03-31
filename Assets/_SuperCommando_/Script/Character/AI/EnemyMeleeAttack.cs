using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("ADDP/Enemy AI/[ENEMY] Melee Attack")]
public class EnemyMeleeAttack : MonoBehaviour {
	public LayerMask targetPlayer;
	public Transform checkPoint;
    public Transform meleePoint;
    public float detectDistance = 1;
	public float meleeRate = 1;
	float lastShoot = 0;
	public bool isAttacking { get; set; }
	public GameObject MeleeObj;

    public float meleeAttackZone = .7f;
    public float meleeAttackCheckPlayer = 0.1f;
    public int meleeDamage = 20;  //give damage to player
    public AudioClip[] soundAttacks;
    void Start(){
		//meleePoint.SetActive (false);
	}

	public bool AllowAction(){
		return Time.time - lastShoot > meleeRate;
	}

	// Update is called once per frame
	public bool CheckPlayer (bool isFacingRight) {
		RaycastHit2D hit = Physics2D.Raycast (checkPoint.position, isFacingRight ? Vector2.right : Vector2.left, detectDistance, targetPlayer);
		if (hit)
			return true;
		else
			return false;
	}

	public void Action(){
		
		
		lastShoot = Time.time;
	}


	void EndAttack(){
		isAttacking = false;
	}

	/// <summary>
	/// Called by Enemy
	/// </summary>
	public void Check4Hit(){
        var hit = Physics2D.CircleCast(meleePoint.position, meleeAttackZone, Vector2.zero, 0, targetPlayer);
        if (hit)
        {
            var damage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
            if (damage != null)
            {
                damage.TakeDamage(meleeDamage, Vector2.zero, gameObject, hit.point);
            }
        }

        if (soundAttacks.Length > 0)
            SoundManager.PlaySfx(soundAttacks[Random.Range(0, soundAttacks.Length)]);
        //meleePoint.SetActive (true);
    }

	public void EndCheck4Hit(){
		//meleePoint.SetActive (false);

		CancelInvoke ();
		Invoke ("EndAttack", 1);
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawLine (checkPoint.position, checkPoint.position + Vector3.right * detectDistance);
        Gizmos.DrawSphere(checkPoint.position + Vector3.right * detectDistance, 0.1f);

        if (meleePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(meleePoint.position, meleeAttackZone);
        }
    }
}
