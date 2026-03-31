using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultValueKeyboard : MonoBehaviour {
	public static DefaultValueKeyboard Instance;

    [Header("KEYBOARD CONTROL")]
    public KeyCode keyJump;
    public KeyCode keyThrow;
    public KeyCode keyShot;
    public KeyCode keyPause;

    void Awake(){
		Instance = this;
	}
}
