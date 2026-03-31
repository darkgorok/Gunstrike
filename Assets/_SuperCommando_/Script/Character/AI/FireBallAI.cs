using UnityEngine;
using System.Collections;

public class FireBallAI : MonoBehaviour,ICanTakeDamage {
	public bool isKillByProjectile = true;

	float oldY;

	// Use this for initialization
	void Start () {
		oldY = transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
		var Y = transform.position.y;

		if (Y > oldY)
			transform.localScale = new Vector3 (1, 1, 1);
		else
			transform.localScale = new Vector3 (1, -1, 1);

		oldY = Y;
	}

	#region ICanTakeDamage implementation

	public void TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
		if (!isKillByProjectile)
			return;
		
		var projectile =(Projectile) instigator.GetComponent (typeof(Projectile));

		if (projectile != null)
            Destroy(gameObject);
    }

	#endregion
}
