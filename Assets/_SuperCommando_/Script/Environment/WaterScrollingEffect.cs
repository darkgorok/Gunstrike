using UnityEngine;
using System.Collections;

public class WaterScrollingEffect : MonoBehaviour {
	public float speed;
	Renderer water;
	// Use this for initialization
	void Start () {
		water = GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		var offset = (speed * Time.time) % 1;
		water.material.mainTextureOffset = new Vector2 (offset, water.material.mainTextureOffset.y);
	}
}
