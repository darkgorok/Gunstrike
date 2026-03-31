using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class GroupEnemySystem : MonoBehaviour
{
    public enum SHOWENEMYTYPE { ShowAll, OneByOne }
    public enum GETKEYTYPE { KEY, NOKEY }

    public SHOWENEMYTYPE showEnemyType;
    [Header("NOTE: IF PLACE BOSS INTO THIS, NEED DISABLE 'DETECTPLAYER' object in BOSS")]
    public GroupMiniEnemy[] EnemyGroup;
    public int[] showOrderGroup;
    [Tooltip("Just in case this is boss and need avtive it")]
    public bool sendMessage = false;
    public string trySendAMessage = "Play";

    public AudioClip soundShowUp;
    public AudioClip soundWarning;
    public AudioClip soundClean;
    public AudioClip soundGetKey;
    public Transform targetCameraLook;
    public float moveCameraSpeed = 5;
    public GETKEYTYPE getKeyType;

    [SerializeField] private CameraFollow mainCamera;

    private bool isWorked = false;
    private int currentGroup = 0;
    private AudioSource soundWarningScr;
    private bool isCameraLooking = false;
    private Vector3 mainCameraStartPoint;
    private float moveCameraPercent = 0f;
    private bool moveCameraToTarget = true;
    private List<MonoBehaviour> listMono;
    private List<GameObject> listEnemyNeedKill;
    private bool isFinishUse = false;
    private GameObject lastEnemyPosition;
    private bool isShowingNextGroup = false;
    private IAudioService audioService;
    private IGameplayPresentationService presentationService;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IAudioService audioService, IGameplayPresentationService presentationService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.presentationService = presentationService;
        this.gameSession = gameSession;
    }

    private void Start()
    {
        ProjectScope.Inject(this);
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<CameraFollow>();

        StartCoroutine(InitCo());

        soundWarningScr = gameObject.AddComponent<AudioSource>();
        soundWarningScr.clip = soundWarning;
        soundWarningScr.loop = true;
        soundWarningScr.volume = 1f;
    }

    private IEnumerator InitCo()
    {
        yield return null;
        listEnemyNeedKill = new List<GameObject>();
        listMono = new List<MonoBehaviour>();
        foreach (GroupMiniEnemy target in EnemyGroup)
        {
            if (target == null)
                continue;

            foreach (GameObject miniEnemy in target.miniGroup)
            {
                if (miniEnemy == null)
                    continue;

                MonoBehaviour[] monos = miniEnemy.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour mono in monos)
                {
                    listMono.Add(mono);
                    mono.enabled = false;
                }

                if (showEnemyType == SHOWENEMYTYPE.OneByOne)
                    miniEnemy.SetActive(false);
                else
                {
                    miniEnemy.SetActive(true);
                    listEnemyNeedKill.Add(miniEnemy);
                }
            }
        }

        lastEnemyPosition = new GameObject("FollowLastEnemy-GroupEnemySystem.cs");

        GameObject[] pickGroup = EnemyGroup[showOrderGroup[0] - 1].miniGroup;
        foreach (GameObject enemy in pickGroup)
        {
            if (enemy != null)
                enemy.SetActive(true);
        }
    }

    private void Update()
    {
        if (isFinishUse)
            return;

        if (isCameraLooking)
        {
            if (moveCameraToTarget)
            {
                moveCameraPercent += moveCameraSpeed * Time.deltaTime;
                mainCamera.transform.position = Vector3.Lerp(mainCameraStartPoint, targetCameraLook.position, moveCameraPercent);
                if (Vector2.Distance(mainCamera.transform.position, targetCameraLook.position) < 0.1f)
                {
                    moveCameraToTarget = false;
                    moveCameraPercent = 0f;
                }
            }
            else
            {
                moveCameraPercent += moveCameraSpeed * Time.deltaTime;
                mainCamera.transform.position = Vector3.Lerp(targetCameraLook.position, mainCameraStartPoint, moveCameraPercent);
                if (Vector2.Distance(mainCameraStartPoint, mainCamera.transform.position) < 0.1f)
                    isCameraLooking = false;
            }
        }

        if (!isWorked)
            return;

        int alive = 0;
        foreach (GameObject target in listEnemyNeedKill)
        {
            if (target != null && target.activeInHierarchy)
            {
                alive++;
                lastEnemyPosition.transform.position = target.transform.position;
            }
        }

        if (alive == 0)
        {
            if (showEnemyType == SHOWENEMYTYPE.ShowAll)
                KillTheLastEnemyEvent();
            else
                StartCoroutine(ShowNextGroup());
        }
    }

    private void KillTheLastEnemyEvent()
    {
        audioService.PlaySfx(soundClean);
        if (getKeyType == GETKEYTYPE.KEY)
        {
            gameSession.HasKey = true;
            audioService.PlaySfx(soundGetKey);
        }

        presentationService.ShowClean();
        isFinishUse = true;
    }

    private IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        if (isWorked)
            yield break;

        if (other.gameObject != gameSession.Player.gameObject)
            yield break;

        gameSession.Player.PausePlayer(true);
        audioService.PlaySfx(soundShowUp);

        mainCamera.enabled = false;
        mainCameraStartPoint = mainCamera.transform.position;
        isCameraLooking = true;
        presentationService.ShowWarning(true);
        audioService.PauseMusic(true);
        soundWarningScr.Play();

        while (isCameraLooking)
            yield return null;

        mainCamera.enabled = true;
        presentationService.ShowWarning(false);
        audioService.PauseMusic(false);
        soundWarningScr.Stop();
        gameSession.Player.PausePlayer(false);

        if (showEnemyType == SHOWENEMYTYPE.ShowAll)
            ShowAllEnemy();
        else
            StartCoroutine(ShowNextGroup());

        isWorked = true;
    }

    private void ShowAllEnemy()
    {
        foreach (MonoBehaviour mono in listMono)
        {
            mono.enabled = true;
            if (!sendMessage)
                continue;

            mono.SendMessage(trySendAMessage, SendMessageOptions.DontRequireReceiver);
            mono.SendMessage("IPlay", SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator ShowNextGroup()
    {
        if (isShowingNextGroup)
            yield break;

        isShowingNextGroup = true;
        if (currentGroup >= EnemyGroup.Length)
        {
            KillTheLastEnemyEvent();
            yield break;
        }

        int nextGroup = showOrderGroup[currentGroup];
        if (nextGroup > EnemyGroup.Length)
        {
            Debug.LogError("WRONG SET ORDER, MUST LOWER THAN OR EQUAL ENEMY GROUP NUMBER");
            yield break;
        }

        GameObject[] pickGroup = EnemyGroup[nextGroup - 1].miniGroup;
        foreach (GameObject enemy in pickGroup)
            enemy.SetActive(true);

        foreach (GameObject enemy in pickGroup)
        {
            MonoBehaviour[] monos = enemy.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour mono in monos)
            {
                listMono.Add(mono);
                mono.enabled = true;
                yield return null;
                if (!sendMessage)
                    continue;

                mono.SendMessage(trySendAMessage, SendMessageOptions.DontRequireReceiver);
                mono.SendMessage("IPlay", SendMessageOptions.DontRequireReceiver);
            }
        }

        listEnemyNeedKill = new List<GameObject>(pickGroup);
        currentGroup++;
        isShowingNextGroup = false;
    }

    private void OnDrawGizmos()
    {
        if (targetCameraLook)
            Gizmos.DrawLine(transform.position, targetCameraLook.position);
    }

    [System.Serializable]
    public class GroupMiniEnemy
    {
        public GameObject[] miniGroup;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<CameraFollow>();
    }
#endif
}
