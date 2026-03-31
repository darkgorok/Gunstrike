using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class SimpleGravityObject : MonoBehaviour, ICanTakeDamage
{
    [Header("Setup")]
    public AudioClip soundHit;
    public float gravity = 35f;
    protected Vector3 velocity;
    protected float velocityXSmoothing = 0;
    Controller2D controller;

    [Header("Can Take Damage")]
    public bool canBeHit = true;
    public Vector2 forceBeHit = new Vector2(2, 0);
    
    void Start()
    {
        controller = GetComponent<Controller2D>();
    }
    
    void Update()
    {
        float targetVelocityX = 0;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? 0.1f : 0.2f);
        velocity.y += -gravity * Time.deltaTime;
    }

    private void LateUpdate()
    {
        controller.Move(velocity * Time.deltaTime, false);

        if (controller.collisions.above || controller.collisions.below)
            velocity.y = 0;

        if (controller.collisions.below)
        {
            CheckBelow();       //check the object below if it have Stand on event
        }
    }

    void CheckBelow()
    {
        if (controller.collisions.ClosestHit.collider != null)
        {
            var standObj = (IStandOnEvent)controller.collisions.ClosestHit.collider.gameObject.GetComponent(typeof(IStandOnEvent));
            if (standObj != null)
                standObj.StandOnEvent(gameObject);
        }
    }

    public void AddForce(Vector2 force)
    {
        velocity = force;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!canBeHit)
            return;

        int dir = (instigator.transform.position.x > transform.position.x) ? -1 : 1;
        AddForce(forceBeHit * dir);
        SoundManager.PlaySfx(soundHit);
    }
}
