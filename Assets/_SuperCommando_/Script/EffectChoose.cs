using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectChoose : MonoBehaviour {
	public enum Type
	{
        Effect1,
		Effect2,
        Effect3

    }
	public Type effectType;
	public GameObject FX1, FX2, FX3;

    // Use this for initialization
    void Start()
    {
        if (FX1)
            FX1.SetActive(effectType == Type.Effect1);
        if (FX2)
            FX2.SetActive(effectType == Type.Effect2);
        if (FX3)
            FX3.SetActive(effectType == Type.Effect3);
    }

}
