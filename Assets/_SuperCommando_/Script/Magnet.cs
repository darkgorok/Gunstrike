using UnityEngine;
using System.Collections;

public class Magnet : MonoBehaviour {
    public static Magnet Instance;
    public GameObject icon;
    bool isWorking = false;

    private void Awake()
    {
        Instance = this;
        icon.SetActive(false);
    }

    public void ActiveMagnet(float timeUse = 5){
        StopAllCoroutines();
		StartCoroutine (ActiveMagnetCo(timeUse));
	}

	IEnumerator ActiveMagnetCo(float timeUse){
        icon.SetActive(true);
        isWorking = true;
        yield return new WaitForSeconds (timeUse);
        isWorking = false;
        icon.SetActive (false);
	}

    void OnTriggerStay2D(Collider2D other){
        if (!isWorking)
            return;

		if (other.gameObject.CompareTag("Coin")) {
			other.gameObject.AddComponent<Star> ();
		}
	}
}
