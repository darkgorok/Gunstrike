using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomZone : MonoBehaviour {
    [Range(1, 10f)]
	public float orthographicSize = 2.5f;
	float originalZone;
	bool isZooming = false;

	void OnTriggerStay2D(Collider2D other){
		if (isZooming)
			return;
		
		if (other.GetComponent<Player> ()) {
			FindObjectOfType<CameraFollow> ().ZoomIn (orthographicSize);
			isZooming = true;
		}
	}
    
	void OnTriggerExit2D(Collider2D other){
		if (!isZooming)
			return;
		
		if (other.GetComponent<Player> ()) {
			FindObjectOfType<CameraFollow> ().ZoomOut ();
			isZooming = false;
		}
	}

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            var bond = GetComponent<BoxCollider2D>().bounds;
            Gizmos.DrawWireCube(bond.center, bond.size);
        }
    }

}
