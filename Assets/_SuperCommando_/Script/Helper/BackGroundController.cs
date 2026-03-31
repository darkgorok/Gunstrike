using UnityEngine;
using System.Collections;

public class BackGroundController : MonoBehaviour {
	public Renderer Background;
	public float speedBG;
	public Renderer Midground;
	public float speedMG;
	public Renderer Forceground;
	public float speedFG;

	[Tooltip("if this target == null, the target will be Main Camera")]
	public Transform target;
	float startPosX;

	// Use this for initialization
	void Start () {
		if (target == null)
			target = Camera.main.transform;
		
		startPosX = target.position.x;
	}
	
	// Update is called once per frame
	void Update () {
		var x = target.position.x - startPosX;

		if (Background != null) {
			var offset = (x * speedBG) % 1;
			Background.material.mainTextureOffset = new Vector2 (offset, Background.material.mainTextureOffset.y);
		}
		if (Midground != null) {
			var offset = (x * speedMG) % 1;
			Midground.material.mainTextureOffset = new Vector2 (offset, Midground.material.mainTextureOffset.y);
		}
		if (Forceground != null) {
			var offset = (x * speedFG) % 1;
			Forceground.material.mainTextureOffset = new Vector2 (offset, Forceground.material.mainTextureOffset.y);
		}
	}
}
