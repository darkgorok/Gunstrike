using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveDamageToTarget : MonoBehaviour
{
    public enum DEALDAMAGETOPLAYER { Contact, FallTo}
    public DEALDAMAGETOPLAYER deadDamageToPlayer;

    [Header("GiveDamageToTarget")]
    public GameObject Owner;
    public int Damage = 20;
    public LayerMask targetLayer;
    public Vector2 pushTargetForce = new Vector2(0, 0);

    [Header("Repeat Damage")]
    public bool repeatDamage = true;
    public float damageRate = 0.1f;

    bool isWorked = false;

    private void OnEnable()
    {
        isWorked = false;
    }

    protected IEnumerator OnTriggerStay2D(Collider2D other)
    {
        if (isWorked)
            yield break;

        if (deadDamageToPlayer == DEALDAMAGETOPLAYER.FallTo && GameManager.Instance.Player.velocity.y > 0)
            yield break;

        if (targetLayer != (targetLayer | (1 << other.gameObject.layer)) || Damage == 0)
            yield break;

        var takeDamage = (ICanTakeDamage)other.gameObject.GetComponent(typeof(ICanTakeDamage));
        if (takeDamage != null)
        {
            isWorked = true;
            takeDamage.TakeDamage(Damage, Vector2.zero, Owner == null ? gameObject : Owner, pushTargetForce);

         
            if (other.gameObject == GameManager.Instance.Player.gameObject)
            {
                var facingDirectionX = Mathf.Sign(GameManager.Instance.Player.transform.position.x - transform.position.x);
                var facingDirectionY = Mathf.Sign(GameManager.Instance.Player.velocity.y);

                GameManager.Instance.Player.SetForce(new Vector2(pushTargetForce.x * facingDirectionX,
                    pushTargetForce.y * facingDirectionY * -1));

             
            }
        }

        if (isWorked && repeatDamage)
        {
            yield return new WaitForSeconds(damageRate);
            isWorked = false;
        }
        else
            yield return null;
    }
}
