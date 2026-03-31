using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class CameraLookPoint : MonoBehaviour
{
    [Header("Follow Order")]
    [ReadOnly] public bool lookTarget = true;
    public bool setCameraLimitMin = true;
    public bool setCameraLimitMax = true;
    public bool movePlayerToPoint = true;
    public bool activeGate = true;
    public bool changeGameMusic = true;
    [Space]
    public AudioClip soundShowUp;
    public float cameraMoveToSpeed = 1;
    public float cameraMoveBackSpeed = 2;
    public Transform targetCameraLook;
    public Transform limitMinPos, limitMaxPos;
    public float movePlayerSpeed = 1.5f;
    public Transform movePlayerToPointPos;
    public float delayGate = 1f;
    public GameObject theGate;
    public AudioClip newMusic;

    private Vector3 mainCameraStartPoint;
    private CameraFollow mainCamera;
    private bool isWorked = false;
    private IAudioService audioService;
    private IGameSessionService gameSession;
    private IGameplayPresentationService presentationService;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession, IGameplayPresentationService presentationService)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
        this.presentationService = presentationService;
    }

    private void Start()
    {
        ProjectScope.Inject(this);
        mainCamera = CameraFollow.Instance;
    }

    private IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        if (isWorked)
            yield break;

        if (other.gameObject != gameSession.Player.gameObject)
            yield break;

        isWorked = true;
        if (setCameraLimitMin)
            mainCamera._min.x = limitMinPos.position.x;
        if (setCameraLimitMax)
            mainCamera._max.x = limitMaxPos.position.x;

        gameSession.Player.Frozen(true);
        presentationService.SetControllerVisible(false);
        audioService.PlaySfx(soundShowUp);

        mainCamera.isFollowing = false;
        mainCameraStartPoint = mainCamera.transform.position;

        audioService.PauseMusic(true);
        presentationService.ShowWarning(true);

        Vector3 targetPos = targetCameraLook.position;
        targetPos.z = mainCameraStartPoint.z;

        float percent = 0f;
        while (percent < 1f)
        {
            percent += Time.deltaTime * cameraMoveToSpeed;
            percent = Mathf.Clamp01(percent);
            mainCamera.transform.position = Vector3.Lerp(mainCameraStartPoint, targetPos, percent);
            yield return null;
        }

        percent = 0f;
        if (setCameraLimitMin)
        {
            Vector3 targetBack = new Vector3(mainCamera._min.x + mainCamera.CameraHalfWidth, mainCameraStartPoint.y, mainCameraStartPoint.z);
            while (percent < 1f)
            {
                percent += Time.deltaTime * cameraMoveBackSpeed;
                percent = Mathf.Clamp01(percent);
                mainCamera.transform.position = Vector3.Lerp(targetPos, targetBack, percent);
                yield return null;
            }
        }
        else
        {
            while (percent < 1f)
            {
                percent += Time.deltaTime * cameraMoveBackSpeed;
                percent = Mathf.Clamp01(percent);
                mainCamera.transform.position = Vector3.Lerp(targetPos, mainCameraStartPoint, percent);
                yield return null;
            }
        }

        percent = 0f;
        if (movePlayerToPoint)
        {
            Vector3 playerStartPos = gameSession.Player.transform.position;
            Vector3 playerEndPos = movePlayerToPointPos.position;
            playerEndPos.y = playerStartPos.y;
            while (percent < 1f)
            {
                percent += Time.deltaTime * movePlayerSpeed;
                percent = Mathf.Clamp01(percent);
                gameSession.Player.transform.position = Vector3.Lerp(playerStartPos, playerEndPos, percent);
                yield return null;
            }
        }

        if (activeGate)
        {
            theGate.SetActive(true);
            yield return new WaitForSeconds(delayGate);
        }

        if (changeGameMusic)
            audioService.PlayMusic(newMusic, 1f);

        mainCamera.isFollowing = true;
        presentationService.ShowWarning(false);
        audioService.PauseMusic(false);
        gameSession.Player.Frozen(false);
        presentationService.SetControllerVisible(true);
    }

    private void OnDrawGizmos()
    {
        if (targetCameraLook)
        {
            Gizmos.DrawLine(transform.position, targetCameraLook.position);
            Gizmos.DrawWireCube(targetCameraLook.position, new Vector2(Camera.main.orthographicSize * ((float)Screen.width / Screen.height) * 2, Camera.main.orthographicSize * 2));
        }
    }
}
