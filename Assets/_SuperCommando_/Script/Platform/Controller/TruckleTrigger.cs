using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckleTrigger : MonoBehaviour {
    public LayerMask detectLayer;
	public enum TRUCKLEPOS{LEFT, RIGHT}
	public TRUCKLEPOS pos;
	public TruckleController truckleController;

    public Vector2 checkSize = new Vector2(1, 0.5f);
    public Vector2 offset;

    bool detected = false;

    private void Update()
    {
        var hit = Physics2D.BoxCast(transform.position + (Vector3) offset,  checkSize, 0, Vector2.zero, 0, detectLayer);
        if (hit && (GameManager.Instance.Player.isGrounded && (GameManager.Instance.Player.transform.position.y > transform.position.y) || hit.collider.gameObject.GetComponent<SimpleGravityObject>()!=null))
        {
            if (!detected)
            {
                truckleController.PlayerStandOn(pos == TRUCKLEPOS.RIGHT, hit.collider.gameObject.GetComponent<SimpleGravityObject>()!=null);
                detected = true;
            }
        }
        else
        {
            if (detected)
            {
                truckleController.PlayerLeave();
                detected = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + (Vector3)offset, checkSize);
    }
}
