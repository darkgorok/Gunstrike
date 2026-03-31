using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu_Gamefinish : MonoBehaviour
{
    public GameObject Buttons;

    void Awake()
    {
        Buttons.SetActive(false);
    }

    // Use this for initialization
    IEnumerator Start()
    {

        Buttons.SetActive(true);
        yield return new WaitForSeconds(2f);

        Buttons.SetActive(true);
    }
}
