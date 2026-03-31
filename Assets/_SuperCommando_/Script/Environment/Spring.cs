using UnityEngine;
using System.Collections;

public class Spring : MonoBehaviour, IStandOnEvent {
	public float pushUp;
	public AudioClip soundEffect;
	[Tooltip("Push player if his position Y > this position Y + this offset Y")]
	public float centerOffset = 0.2f;
	[Range(0,1)]
	public float soundEffectVolume = 0.5f;

	Animator anim;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
    
   void Push()
    {
        GameManager.Instance.Player.SetForce(new Vector2(GameManager.Instance.Player.velocity.x, pushUp), true);
        
        if (anim != null)
            anim.SetTrigger("push");

        SoundManager.PlaySfx(soundEffect, soundEffectVolume);
    }

	void OnDrawGizmosSelected(){
		Gizmos.DrawWireSphere (new Vector3 (transform.position.x, transform.position.y + centerOffset, transform.position.z),0.1f);
	}

    public void StandOnEvent(GameObject instigator)
    {
        Push();
    }
}
