using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class LaserObstacle : MonoBehaviour
{
    public enum Type { Normal, Reflect }

    public Type type;

    [Tooltip("For reflect laser")]
    public float laserLength = 30;
    public int numberReflect = 3;
    public GameObject laserContactFX;

    public Transform startPoint, endPoint;
    public LayerMask reflectLayer;
    public int damage = 100;
    public AudioClip soundKillPlayer;

    [Header("Draw line")]
    public bool drawLine = false;
    public float lineWidth = 0.2f;
    public int lineCorner = 90;
    public Material lineMat;
    public float offsetLineZ = -1;

    private LineRenderer lineRen;
    private readonly List<GameObject> contactFXList = new List<GameObject>();
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
        lineRen = GetComponent<LineRenderer>();
        if (!lineRen)
            lineRen = gameObject.AddComponent<LineRenderer>();

        lineRen.useWorldSpace = true;
        lineRen.startWidth = lineWidth;
        lineRen.numCornerVertices = lineCorner;
        lineRen.material = lineMat;
        lineRen.textureMode = LineTextureMode.Tile;

        for (int i = 0; i < numberReflect; i++)
        {
            contactFXList.Add(Instantiate(laserContactFX, transform.position, laserContactFX.transform.rotation));
        }
    }

    private void Update()
    {
        if (gameSession?.Player == null)
            return;

        if (type == Type.Normal)
        {
            RaycastHit2D hit = Physics2D.Linecast(startPoint.position, endPoint.position);
            if (hit)
            {
                UpdateLinePoint(1, hit.point);
                TryDamagePlayer(hit.collider.gameObject, hit.point);
            }
            else
            {
                UpdateLinePoint(1, endPoint.position);
            }

            return;
        }

        Vector2 currentStartPoint = startPoint.position;
        Vector2 direction = startPoint.up;
        float remainingLength = laserLength;

        for (int i = 0; i < numberReflect; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentStartPoint, direction, remainingLength, reflectLayer);
            if (hit)
            {
                TryDamagePlayer(hit.collider.gameObject, hit.point);
                UpdateLinePoint(i + 1, hit.point);

                direction = Vector3.Reflect(hit.point - currentStartPoint, hit.normal);
                currentStartPoint = hit.point + direction * 0.1f;
                remainingLength -= hit.distance;
            }
            else
            {
                UpdateLinePoint(i + 1, currentStartPoint + direction.normalized * remainingLength);
                contactFXList[i].SetActive(false);
                break;
            }
        }
    }

    private void TryDamagePlayer(GameObject hitObject, Vector2 hitPoint)
    {
        if (gameSession?.Player == null || hitObject != gameSession.Player.gameObject)
            return;

        gameSession.Player.TakeDamage(damage, Vector2.zero, gameObject, hitPoint);
        audioService?.PlaySfx(soundKillPlayer);
    }

    private void UpdateLinePoint(int pos, Vector3 newPoint)
    {
        lineRen.positionCount = pos + 1;
        lineRen.SetPosition(0, startPoint.position);
        lineRen.SetPosition(pos, newPoint);
        contactFXList[pos - 1].transform.position = newPoint;
        contactFXList[pos - 1].SetActive(true);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        lineRen = GetComponent<LineRenderer>();

        if (drawLine)
        {
            if (!lineRen)
                lineRen = gameObject.AddComponent<LineRenderer>();

            lineRen.useWorldSpace = true;
            lineRen.startWidth = lineWidth;
            lineRen.material = lineMat;
            lineRen.numCornerVertices = lineCorner;
            lineRen.textureMode = LineTextureMode.Tile;

            lineRen.SetPosition(0, startPoint.position + Vector3.forward * offsetLineZ);
            lineRen.SetPosition(1, endPoint.position + Vector3.forward * offsetLineZ);
        }
        else if (lineRen)
        {
            DestroyImmediate(lineRen);
        }
    }
}
