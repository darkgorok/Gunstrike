using UnityEngine;
using System.Collections;

public class MainMenuHelper : MonoBehaviour {
	public GameObject Helper;

	// Use this for initialization
	void Start () {
		HideHelper ();
	}

	public void ShowHelper(){
		Helper.SetActive (true);
	}

	public void HideHelper(){
		Helper.SetActive (false);
	}
}
