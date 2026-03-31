using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour {
	public Transform forceGroundSprite;
	public Player player;

	void Start () {
		player = FindObjectOfType<Player> ();
	}
	
	// Update is called once per frame
	void Update () {
		var healthPercent = (float) player.Health / player.maxHealth;
		forceGroundSprite.localScale = new Vector3 (healthPercent, 1, 1);
	}
}
