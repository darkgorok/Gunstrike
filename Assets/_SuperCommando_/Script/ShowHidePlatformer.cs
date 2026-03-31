using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHidePlatformer : MonoBehaviour/*, IListener*/ {
	public AudioClip sound;
	public GameObject[] platformers;

	public float showTime = 1;
	float timer;
	int current;
	public AudioSource ASource;
	// Use this for initialization
	void Start () {
		timer = showTime;
		current = 0;

		ShowPlatformer (0);
	}
	
	// Update is called once per frame
	void Update () {
		if (isStop)
			return;
		
		if (timer > 0) {
			timer -= Time.deltaTime;
			return;
		}

		timer = showTime;
		current++;
		if (current == platformers.Length)
			current = 0;

		ShowPlatformer (current);
	}

	void ShowPlatformer(int i){
		foreach (GameObject obj in platformers) {
			obj.SetActive (false);
		}

		platformers [i].SetActive (true);
		if (ASource) {
			ASource.clip = sound;
            ASource.volume = 1;
			ASource.Play ();
		}

		if(i>0)
			platformers [i-1].SetActive (true);
		else if(i == 0)
			platformers [platformers.Length - 1].SetActive (true);
	}

	bool isStop = false;
	//#region IListener implementation

	//public void IPlay ()
	//{

	//}

	//public void ISuccess ()
	//{

	//}

	//public void IPause ()
	//{

	//}

	//public void IUnPause ()
	//{

	//}

	//public void IGameOver ()
	//{

	//}

	//public void IOnRespawn ()
	//{

	//}

	//public void IOnStopMovingOn ()
	//{
	//	isStop = true;
	//}

	//public void IOnStopMovingOff ()
	//{
	//	isStop = false;
	//}

	//#endregion
}
