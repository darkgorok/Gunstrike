using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using UnityEngine.Advertisements;

public class Menu_Gameover : MonoBehaviour {

    public void TryAgain()
    {
        GameManager.Instance.ResetLevel();
    }
}
