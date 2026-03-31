using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DartButtonManager : MonoBehaviour
{
    public static DartButtonManager Instance;
    public delegate void OnActionButton();
    public static OnActionButton ActionButton;

    public DartChooseUiFx dartUiFx;
    bool isWorking = false;     //no allow throw 2 dart at same time

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public bool SpawnDartFX()
    {
        if (isWorking)
            return false;

        isWorking = true;
        Instantiate(dartUiFx, Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);

        return true;
    }

    //call from DartChooseUIFx
    public void CallButtonAction()
    {
        isWorking = false;
        ActionButton();
    }
}
