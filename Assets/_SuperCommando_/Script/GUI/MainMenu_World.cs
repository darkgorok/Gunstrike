using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu_World : MonoBehaviour {
	public int worldNumber = 1;
	public GameObject Locked;

	// Use this for initialization
	void Start () {
		var worldreached = PlayerPrefs.GetInt (GlobalValue.WorldReached, 1);
		if (worldNumber <= worldreached)
			Locked.SetActive (false);
		else {
			Locked.SetActive (true);
			GetComponent<Button> ().interactable = false;
		}
	}

	//public void OpenWorld(){
	//	MainMenuHomeScene.Instance.OpenWorld (worldNumber);
	//}
}
