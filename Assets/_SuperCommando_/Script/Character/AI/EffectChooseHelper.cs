using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EffectType
{
	Effect1,
	Effect2,
	Effect3

}
public class EffectChooseHelper : MonoBehaviour {
	public bool setFxAsChild = false;
	public Transform followTarget;
	public float autoDestroy = 0;

	public EffectType effectChoose;
	public GameObject Effect1;
	public GameObject Effect2;
	public GameObject Effect3;
	public bool scaleWithPlayer = false;
	void Start () {
		if (autoDestroy != 0)
			Destroy (gameObject, autoDestroy);
		
		GameObject objFX = null;
		switch (effectChoose) {
		case EffectType.Effect1:
			objFX = Instantiate (Effect1, transform.position,Effect1.transform.rotation) as GameObject;
			break;
		case EffectType.Effect2:
			objFX = Instantiate (Effect2, transform.position,Effect2.transform.rotation) as GameObject;
			break;
		case EffectType.Effect3:
			objFX = Instantiate (Effect3, transform.position,Effect3.transform.rotation) as GameObject;
			break;
		default:
			break;
		}

		if (scaleWithPlayer) {

//			if (transform.root.gameObject == GameManager.Instance.Player.gameObject)
//				objFX.transform.localScale = new Vector3 (objFX.transform.localScale.x, objFX.transform.localScale.y * Mathf.Sign (transform.root.localScale.y), objFX.transform.localScale.z);


			if (GameManager.Instance.Player.transform.localScale.y < 0) {
//				Debug.LogError (GameManager.Instance.Player.transform.localScale.y);
				objFX.transform.localScale = new Vector3 (-objFX.transform.localScale.x, objFX.transform.localScale.y, objFX.transform.localScale.z);
				objFX.transform.rotation = Quaternion.Euler (objFX.transform.rotation.eulerAngles.x + 180, objFX.transform.rotation.eulerAngles.y, objFX.transform.rotation.eulerAngles.z);
			}
		}

		if (setFxAsChild) {
			objFX.transform.SetParent (transform);
			return;
		}

		objFX.AddComponent<FollowObject> ();
		objFX.GetComponent<FollowObject> ().Init (followTarget != null ? followTarget : transform);
	}
}
