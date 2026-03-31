using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_SuperAttackFlame : MonoBehaviour {
	public LayerMask layerGround;
	public float timeOn = 1f;
	public float timeOff = 1.5f;

	public int damage = 20;

	public ParticleSystem beginParticSys;
	public EffectType effectChoose;
	public ParticleSystem[] FX1;
	public ParticleSystem[] FX2;
	public ParticleSystem[] FX3;
    ParticleSystem[] ParticlsSys;
	bool hitPlayer = false;
	BoxCollider2D box2D;
	public AudioClip sound;

	// Use this for initialization
	void Start () {

        switch (effectChoose)
        {
            case EffectType.Effect1:
                ParticlsSys = FX1;
                break;
            case EffectType.Effect2:
                ParticlsSys = FX2;
                break;
            case EffectType.Effect3:
                ParticlsSys = FX3;
                break;
            default:
                break;
        }

		foreach(var child in FX1){
			child.gameObject.SetActive (false);
		}

		foreach(var child in FX2){
			child.gameObject.SetActive (false);
		}

		foreach(var child in FX3){
			child.gameObject.SetActive (false);
		}

        foreach (var child in ParticlsSys){
			child.gameObject.SetActive (false);
		}

		box2D = GetComponent<BoxCollider2D> ();
		box2D.enabled = false;

		RaycastHit2D hit = Physics2D.Raycast (GameManager.Instance.Player.transform.position + Vector3.up, /*GameManager.Instance.Player.inverseGravity ? Vector2.up : */Vector2.down, 10, layerGround);
		if (hit) {
			transform.position = hit.point;

			Invoke ("TurnOn", timeOn);
		}
	}

	public void TurnOn(){
		box2D.enabled = true;
		foreach(var child in ParticlsSys){
			child.gameObject.SetActive (true);
			var em = child.emission;
			em.enabled = true;
		}

		SoundManager.PlaySfx (sound);

		Invoke ("TurnOff", timeOn);
	}

	public void TurnOff(){
//		foreach(var child in ParticlsSys){
		foreach(var child in ParticlsSys){
			var em = child.emission;
			em.enabled = false;
		}

		var em2 = beginParticSys.emission;
		em2.enabled = false;
		box2D.enabled = false;
		beginParticSys.gameObject.SetActive (false);
//		Invoke ("TurnOn", timeOff);
	}
	
	void OnTriggerEnter2D(Collider2D other){
		if (hitPlayer)
			return;

		if (GameManager.Instance.Player.gameObject.layer == LayerMask.NameToLayer ("HidingZone"))
			return;
		
		if (other.GetComponent<Player> ()) {
			other.GetComponent<Player> ().TakeDamage (damage, Vector2.zero, gameObject, Vector2.zero);
			hitPlayer = true;
			box2D.enabled = false;
		}
	}
}
