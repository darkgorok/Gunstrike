using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class SpearTrap : MonoBehaviour
{
    public GameObject spearObj;
    public int numberOfSpear = 5;
    public float widthOfSpear = 0.3f;
    public bool arrangeToRight = false;
    public Vector2 offset;
    public Vector3[] localWaypoints;

    [Header("Spear Setup")]
    public float speed = 10;
    public float delayWarning = 0.5f;
    public float delayPerSpear = 0.1f;
    public AudioClip sound;

    [Header("CheckPlayer")]
    public Vector2 checkSize = new Vector2(0.5f, 2);
    public Vector3 checkOffset;

    private readonly List<GameObject> spearList = new List<GameObject>();
    private bool isTriggered;
    private int nextSpearIndex;
    private float nextActivationTimer = -1f;
    private IGameSessionService gameSession;
    private IAudioService audioService;

    [Inject]
    public void Construct(IGameSessionService gameSession, IAudioService audioService)
    {
        this.gameSession = gameSession;
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        SetupSpear();
    }

    private void Update()
    {
        if (!isTriggered)
        {
            if (Physics2D.BoxCast(transform.position + checkOffset, checkSize, 0, Vector2.zero, 0, gameSession.PlayerLayer))
            {
                isTriggered = true;
                nextSpearIndex = 0;
                nextActivationTimer = 0f;
            }

            return;
        }

        if (nextActivationTimer < 0f)
            return;

        nextActivationTimer -= Time.deltaTime;
        if (nextActivationTimer > 0f)
            return;

        ActivateNextSpear();
    }

    private void SetupSpear()
    {
        spearList.Clear();
        for (int i = 0; i < numberOfSpear; i++)
        {
            GameObject spear = Instantiate(spearObj, transform);
            spear.transform.position = (Vector2)transform.position +
                                       (arrangeToRight ? Vector2.right : Vector2.left) * widthOfSpear * i +
                                       offset +
                                       (i == 0 ? Vector2.up * 0.5f : Vector2.zero);
            spearList.Add(spear);
        }
    }

    private void ActivateNextSpear()
    {
        if (nextSpearIndex >= numberOfSpear)
        {
            nextActivationTimer = -1f;
            return;
        }

        audioService?.PlaySfx(sound);
        spearList[nextSpearIndex].AddComponent<SimplePathedMoving>().Init(0, speed, localWaypoints, false);
        nextActivationTimer = nextSpearIndex == 0 ? delayWarning : delayPerSpear;
        nextSpearIndex++;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying || numberOfSpear == 0)
            return;

        for (int i = 0; i < numberOfSpear; i++)
        {
            Gizmos.DrawSphere((Vector2)transform.position + (arrangeToRight ? Vector2.right : Vector2.left) * widthOfSpear * i + offset + (i == 0 ? Vector2.up * 0.5f : Vector2.zero), 0.1f);
        }

        if (localWaypoints.Length > 1)
        {
            Gizmos.DrawWireSphere(localWaypoints[1] + transform.position, 0.1f);
            Gizmos.DrawLine(localWaypoints[1] + transform.position, localWaypoints[0] + transform.position);
        }

        Gizmos.DrawWireCube(transform.position + checkOffset, checkSize);
    }
}
