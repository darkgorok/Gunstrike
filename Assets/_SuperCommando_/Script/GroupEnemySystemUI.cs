using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupEnemySystemUI : MonoBehaviour {
    public static GroupEnemySystemUI Instance;
    public GameObject warning, clean;
	// Use this for initialization
	void Start () {
        Instance = this;
        DisableCo();
    }
	
	public void ShowWarning(bool show)
    {
        warning.SetActive(show);
    }

    public void ShowClean()
    {
        clean.SetActive(true);
        Invoke("DisableCo", 2);
    }

    void DisableCo()
    {
        warning.SetActive(false);
        clean.SetActive(false);
    }
}
