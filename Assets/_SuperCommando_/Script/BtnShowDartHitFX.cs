//
//Attached this to Button, when press will call Dart effect for this button
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnShowDartHitFX : MonoBehaviour
{
    public GameObject dartFX;
    public GameObject targetScript;
    public float timeCallFunction = 0.25f;
    public string functionCall = "Play";
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    void OnClick()
    {
        Instantiate(dartFX, Input.mousePosition, Quaternion.identity, transform);
        CancelInvoke();
        Invoke("CallFunction", timeCallFunction);
    }

    void CallFunction()
    {
        targetScript.SendMessage(functionCall, SendMessageOptions.DontRequireReceiver);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
