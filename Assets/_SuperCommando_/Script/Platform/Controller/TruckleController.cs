using UnityEngine;
using VContainer;

public class TruckleController : MonoBehaviour
{
    public Transform firstPlatform, secondPlatform;
    public Transform limitLeftPoint, limitRightPoint;
    public float speedHasPlayer = 2;
    public float speedNoPlayer = 1;

    [ReadOnly] public Vector3 originalLeftPoint, originalRightPoint;
    [ReadOnly] public bool isPlayerStand = false;
    [ReadOnly] public bool isPlayerStandOnRight = false;
    [ReadOnly] public bool isLimited = false;

    private bool isObjectStandingOn = false;
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

    private void Start()
    {
        originalLeftPoint = firstPlatform.position;
        originalRightPoint = secondPlatform.position;
    }

    private void Update()
    {
        if (isLimited)
            return;

        if (isPlayerStand)
        {
            if (gameSession?.Player != null && (gameSession.Player.isGrounded || isObjectStandingOn))
            {
                if (isPlayerStandOnRight)
                {
                    float speed = speedHasPlayer * Time.deltaTime;
                    firstPlatform.Translate(0, speed, 0);
                    secondPlatform.Translate(0, -speed, 0);
                    if (firstPlatform.transform.position.y >= limitLeftPoint.position.y)
                        isLimited = true;
                }
                else
                {
                    float speed = speedHasPlayer * Time.deltaTime;
                    firstPlatform.Translate(0, -speed, 0);
                    secondPlatform.Translate(0, speed, 0);
                    if (secondPlatform.transform.position.y >= limitRightPoint.position.y)
                        isLimited = true;
                }
            }

            return;
        }

        if (firstPlatform.position.y > originalLeftPoint.y)
        {
            float speed = speedNoPlayer * Time.deltaTime;
            firstPlatform.Translate(0, -speed, 0);
            secondPlatform.Translate(0, speed, 0);
            if (firstPlatform.transform.position.y <= originalLeftPoint.y)
                isLimited = true;
        }
        else if (firstPlatform.position.y < originalLeftPoint.y)
        {
            float speed = speedNoPlayer * Time.deltaTime;
            firstPlatform.Translate(0, speed, 0);
            secondPlatform.Translate(0, -speed, 0);
            if (firstPlatform.transform.position.y >= originalLeftPoint.y)
                isLimited = true;
        }
    }

    public void PlayerStandOn(bool isRight, bool isObjectStand)
    {
        if (isPlayerStand)
            return;

        isObjectStandingOn = isObjectStand;
        isPlayerStand = true;
        isPlayerStandOnRight = isRight;
        isLimited = false;
    }

    public void PlayerLeave()
    {
        isPlayerStand = false;
        isLimited = false;
        isObjectStandingOn = false;
    }
}
