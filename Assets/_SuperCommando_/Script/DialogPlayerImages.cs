using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogPlayerImages : MonoBehaviour {
	public static DialogPlayerImages Instance;
	public PlayerImage[] listPlayerImage;

	// Use this for initialization
	void Start () {
		Instance = this;
		//DontDestroyOnLoad (gameObject);
	}

	public PlayerImage GetListImage(int ID){
		foreach (var obj in listPlayerImage) {
			if (obj.ID == ID)
				return obj;
		}
		return null;
	}

}

[System.Serializable]
public class PlayerImage{
	public int ID = 1;
	public Sprite Normal, Cry, Sad, Smile, Angry;
}

