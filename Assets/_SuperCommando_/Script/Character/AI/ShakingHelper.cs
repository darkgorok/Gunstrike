using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakingHelper : MonoBehaviour {
	public bool playOnStart = false;
	public AudioClip sound;
	bool Shaking; 
	public float shakeDecay = 0.02f;
	public float shakeIntensity = 0.2f;    
	public float wide = 0.2f;
	float ShakeDecay = 0;
	float ShakeIntensity = 0;
	private Vector3 OriginalPos;
	private Quaternion OriginalRot;
	public GameObject Target;
	public float timeShake = 1;
	public float timeRate = 1;
	bool isLoop = false;
	void Awake(){
		if (Target == null)
			Target = gameObject;

		Shaking = false;   
	}

	public virtual void Start()
	{
		if (playOnStart) {
			DoShake ();
//			InvokeRepeating ("DoShake", timeShake, timeRate);
		}
	}



	public void DoShake(bool loop = false)
	{
		if (Shaking)
			return;

		isLoop = loop;
		SoundManager.PlaySfx (sound);
		OriginalPos = Target.transform.position;
		OriginalRot = Target.transform.rotation;

		ShakeIntensity = shakeIntensity;
		ShakeDecay = shakeDecay;
		Shaking = true;
	}   

	public void StopShake(){
		Shaking = false;
		isLoop = false;
	}


	// Update is called once per frame
	void Update () 
	{
		if(ShakeIntensity > 0)
		{
			Target.transform.position = OriginalPos + Random.insideUnitSphere * ShakeIntensity;
			Target.transform.rotation = new Quaternion(OriginalRot.x + Random.Range(-ShakeIntensity, ShakeIntensity)*wide,
				OriginalRot.y + Random.Range(-ShakeIntensity, ShakeIntensity)*wide,
				OriginalRot.z + Random.Range(-ShakeIntensity, ShakeIntensity)*wide,
				OriginalRot.w + Random.Range(-ShakeIntensity, ShakeIntensity)*wide);

			ShakeIntensity -= ShakeDecay;
		}
		else if (Shaking)
		{
			if (isLoop) {
				ShakeIntensity = shakeIntensity;
				ShakeDecay = shakeDecay;
			} else
				Shaking = false;
		}
	}

	void OnDisable(){
		StopShake ();
	}
}