using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DartButtonUI : MonoBehaviour
{
    public GameObject target;
    public string message = "Message";
    public bool setValue = false;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    void OnClick()
    {
        if (DartButtonManager.Instance.SpawnDartFX())
            DartButtonManager.ActionButton += CallFunction;
    }

    //called by DartButtonManager.cs
    public void CallFunction()
    {
        DartButtonManager.ActionButton -= CallFunction;
        target.SendMessage(message, setValue, SendMessageOptions.DontRequireReceiver);
    }
}
