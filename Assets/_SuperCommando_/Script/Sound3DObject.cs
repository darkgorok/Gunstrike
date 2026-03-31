using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sound3DObject : MonoBehaviour {
    public AudioClip loop3DSound;
    AudioSource ascr;
   
	// Use this for initialization
	void Start () {
        ascr = GetComponent<AudioSource>();
        ascr.clip = loop3DSound;
        ascr.Play();
    }
}
