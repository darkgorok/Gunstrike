using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SmartFallingStone : MonoBehaviour
{
    Rigidbody2D rig;
    public float torgeForce = 100;
    public Vector2 addForcePosition = new Vector2(0, 0.5f);
    bool isWorked = false;
    public GameObject hitGroundFX;
    public AudioClip soundHitGround;

    [Header("HIT EFFECT")]
    public bool playEarthQuakeOnHit = true;
    public float _eqTime = 0.1f;
    public float _eqSpeed = 60;
    public float _eqSize = 1;

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
       
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isWorked)
            return;
        if (hitGroundFX)
            SpawnSystemHelper.GetNextObject(hitGroundFX, true).transform.position = transform.position;

        if (playEarthQuakeOnHit)
            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);

        SoundManager.PlaySfx(soundHitGround);
        rig.AddForceAtPosition(Vector2.right * torgeForce, transform.position + (Vector3) addForcePosition);
        isWorked = true;
        Destroy(this);
    }
}
