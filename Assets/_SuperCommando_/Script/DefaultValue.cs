using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultValue : MonoBehaviour
{
    public static DefaultValue Instance;
    [Header("DEFAULT VALUE")]
    public int defaultLives = 3;
    [HideInInspector] public int defaultCoin = 100;

    [HideInInspector] public bool defaultBulletMax = false;
    [HideInInspector] public int defaultBullet = 0;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (defaultBulletMax)
            defaultBullet = int.MaxValue;
    }
}