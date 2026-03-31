using UnityEngine;
using System.Collections;

public class CharacterHolder : MonoBehaviour {
	public static CharacterHolder Instance;
	[HideInInspector]
	public GameObject CharacterPicked;

	public GameObject[] Characters;

	// Use this for initialization
	void Awake () {
		if (CharacterHolder.Instance != null) {
			Destroy (gameObject);
			return;
		}
		
		Instance = this;
		DontDestroyOnLoad (gameObject);

		GetPickedCharacter ();
	}

	public void GetPickedCharacter(){
		CharacterPicked = null;
		var characterIDChoosen = PlayerPrefs.GetInt (GlobalValue.ChoosenCharacterInstanceID, 0);
		foreach (var character in Characters) {
			var ID = character.GetInstanceID ();
			if (ID == characterIDChoosen) {
				CharacterPicked = character;
				return;
			}
		}
	}
}
