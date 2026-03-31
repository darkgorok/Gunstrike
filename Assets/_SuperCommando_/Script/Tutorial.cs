using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
	public static Tutorial Instance;

	public Image ImageTut;
	public GameObject Panel;
	public AudioClip sound;

	// Use this for initialization
	void Start () {
		Instance = this;

		Panel.SetActive (false);
	}
	
	public void Open(Sprite image){
		GameManager.Instance.Player.velocity.x = 0;
		SoundManager.PlaySfx (sound);
		ImageTut.sprite = image;
		Panel.SetActive (true);
		Time.timeScale = 0;
	}

	public void Close(){
		Panel.SetActive (false);
		Time.timeScale = 1;
	}
}
