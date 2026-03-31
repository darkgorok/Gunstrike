using UnityEngine;
using System.Collections;

public class CameraFadeInEffect : MonoBehaviour {
	public Texture whiteTexture;
	[Tooltip("Color to fade from")]
	public Color color;
	[Tooltip("Fade in time in seconds.")]
	[Range(0,10)]
	public float time;

	private float currentTime;
	private Color colorLerp;

	// Use this for initialization
	void Start () {
		currentTime = 0f;
		colorLerp = color;
	}
	
	// Update is called once per frame
	void Update () {
		currentTime += Time.deltaTime;
		colorLerp = Color.Lerp(color, Color.clear, currentTime/time);

		if (currentTime > time)
		{
			Destroy (gameObject);
		}
	}

	void OnGUI()
	{
		var guiColor = GUI.color;
		GUI.color = colorLerp;
		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), whiteTexture);
		GUI.color = guiColor;
	}
}
