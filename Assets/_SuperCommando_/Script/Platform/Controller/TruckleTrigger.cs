using UnityEngine;
using VContainer;

public class TruckleTrigger : MonoBehaviour
{
    public LayerMask detectLayer;

    public enum TRUCKLEPOS { LEFT, RIGHT }
    public TRUCKLEPOS pos;
    public TruckleController truckleController;
    public Vector2 checkSize = new Vector2(1, 0.5f);
    public Vector2 offset;

    private bool detected;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Update()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position + (Vector3)offset, checkSize, 0, Vector2.zero, 0, detectLayer);
        bool playerStandingAbove = gameSession?.Player != null &&
                                   gameSession.Player.isGrounded &&
                                   gameSession.Player.transform.position.y > transform.position.y;
        bool simpleObjectStanding = hit && hit.collider.gameObject.GetComponent<SimpleGravityObject>() != null;

        if (hit && (playerStandingAbove || simpleObjectStanding))
        {
            if (!detected)
            {
                truckleController.PlayerStandOn(pos == TRUCKLEPOS.RIGHT, simpleObjectStanding);
                detected = true;
            }
        }
        else if (detected)
        {
            truckleController.PlayerLeave();
            detected = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + (Vector3)offset, checkSize);
    }
}
