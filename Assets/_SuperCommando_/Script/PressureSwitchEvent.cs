using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressureSwitchEvent : MonoBehaviour, IStandOnEvent
{
    [Header("SIMPLE MOVING")]
    public bool moveTarget = false;
    public Transform target;
    public Vector2 localActive;
    public Vector2 localDisactive = Vector2.one;
    Vector3 targetA, targetD;
    public float moveSpeedA = 1;
    public float moveSpeedD = 3;
    bool movingTarget = false;
    float movePercent = 0;

    [Header("SEND MESSSAGE")]
    public GameObject targetObject;
    public string eventOnMessage = "On";
    public string eventOffMessage = "Off";
    public AudioClip soundOn, soundOff;
    Animator anim;
    
    [ReadOnly] public bool state = false;
    private void Start()
    {
        anim = GetComponent<Animator>();
        targetA = target.position + (Vector3)localActive;
        targetD = target.position + (Vector3)localDisactive;
        target.position = targetD;
    }

    private void Update()
    {
        if (state)
        {
            if (moveTarget)
            {
                movePercent += moveSpeedA * Time.deltaTime;
                movePercent = Mathf.Clamp01(movePercent);
                target.position = Vector3.Lerp(targetD, targetA, movePercent);
            }
        }
        else
        {
            if (moveTarget)
            {
                movePercent -= moveSpeedD * Time.deltaTime;
                movePercent = Mathf.Clamp01(movePercent);
                target.position = Vector3.Lerp(targetD, targetA, movePercent);
            }
        }
    }

    public void StandOnEvent(GameObject instigator)
    {
        if (state)
            return;

        StartCoroutine(StandOnCo(instigator.GetComponent<Player>() != null));
    }

    IEnumerator StandOnCo(bool isPlayer)
    {
        state = true;
        anim.SetBool("state", state);
        if (targetObject)
            targetObject.SendMessage(eventOnMessage, SendMessageOptions.DontRequireReceiver);
        else
            Debug.LogWarning("NEED SET TARGET TO THIS: " + gameObject.name);

        SoundManager.PlaySfx(soundOn);
        if (!isPlayer)
            yield break;

        while (GameManager.Instance.Player.isGrounded) { yield return null; }
        state = false;
        anim.SetBool("state", state);
        if (targetObject)
            targetObject.SendMessage(eventOffMessage, SendMessageOptions.DontRequireReceiver);
        else
            Debug.LogWarning("NEED SET TARGET TO THIS: " + gameObject.name);
        SoundManager.PlaySfx(soundOff);

    } 

    void OnDrawGizmos()
    {
        if (Application.isPlaying || !enabled)
            return;

        if (target != null && moveTarget)
        {
            float size = .3f;
            Gizmos.DrawLine(target.position + (Vector3)localActive, target.position + (Vector3)localDisactive);
            Gizmos.DrawWireSphere(target.position + (Vector3)localActive, size);
            Gizmos.DrawWireSphere(target.position + (Vector3)localDisactive, size);
        }
    }
}
