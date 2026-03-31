using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class DialogUITrigger : MonoBehaviour
{
    public bool isFinishLevel = false;
    public bool disableWhenDone = false;
    [HideInInspector] public bool canTalkAgain = false;
    [HideInInspector] public bool hideSmallFaceIcon;
    [HideInInspector] public bool givePlayerAKey = false;
    public AudioClip soundDetectPlayer;

    public Dialogs[] dialogs;
    public Dialogs[] talkAgainDialogs;

    [Header("BOSS ON START BEHAVIOR")]
    public bool hideBossOnStart = false;
    public GameObject showBossEffect;
    public AudioClip bossVisibleSound;

    [Header("Show Boss Option")]
    public bool activeBoss = false;
    public BossManager bossObject;

    [Header("CHANGE GAME MUSIC")]
    public bool changeMusic = true;
    public AudioClip music;
    public float musicVolume = 0.8f;

    [Header("SET CAMERA MIN MAX")]
    public float limitLeftPos = -3;
    public float limitRightPos = 7;
    public bool setCameraLimitMin = true;
    public bool setCameraLimitMax = true;
    [Space]
    public KeyItem keyItem;

    [HideInInspector] public bool isTalk = false;
    [HideInInspector] public bool isTalking = false;
    [HideInInspector] public bool isTakingFinish = false;

    private bool isGaveAKey = false;
    private bool isFirstTalk = true;
    private IAudioService audioService;
    private IControllerInputService controllerInputService;
    private IGameSessionService gameSession;
    private IGameplayPresentationService presentationService;
    private ICameraRigService cameraRigService;
    private IDialogFlowService dialogFlowService;

    [Inject]
    public void Construct(IAudioService audioService, IControllerInputService controllerInputService, IGameSessionService gameSession, IGameplayPresentationService presentationService, ICameraRigService cameraRigService, IDialogFlowService dialogFlowService)
    {
        this.audioService = audioService;
        this.controllerInputService = controllerInputService;
        this.gameSession = gameSession;
        this.presentationService = presentationService;
        this.cameraRigService = cameraRigService;
        this.dialogFlowService = dialogFlowService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        if (hideBossOnStart)
            bossObject.gameObject.SetActive(false);
    }

    private IEnumerator OnTriggerEnter2D(Collider2D other)
    {
        if (isTalk && !canTalkAgain)
            yield break;

        if (other.GetComponent<Player>() == null)
            yield break;

        audioService.PlaySfx(soundDetectPlayer, 0.8f);
        if (setCameraLimitMin)
            cameraRigService.MinBounds = new Vector2(transform.position.x - limitLeftPos, cameraRigService.MinBounds.y);
        if (setCameraLimitMax)
            cameraRigService.MaxBounds = new Vector2(transform.position.x + limitRightPos, cameraRigService.MaxBounds.y);

        gameSession.Player.velocity.x = 0;
        audioService.PauseMusic(true);
        controllerInputService.StopMove();
        presentationService.SetControllerVisible(false);
        presentationService.SetGameplayUiVisible(false);

        if (setCameraLimitMin)
        {
            Vector3 targetPos = cameraRigService.Position;
            targetPos.z = cameraRigService.Position.z;

            cameraRigService.IsFollowing = false;
            Vector3 mainCameraStartPoint = cameraRigService.Position;

            float percent = 0f;
            Vector3 targetBack = new Vector3(cameraRigService.MinBounds.x + cameraRigService.CameraHalfWidth, mainCameraStartPoint.y, mainCameraStartPoint.z);
            while (percent < 1f)
            {
                percent += Time.deltaTime;
                percent = Mathf.Clamp01(percent);
                cameraRigService.Position = Vector3.Lerp(targetPos, targetBack, percent);
                yield return null;
            }

            cameraRigService.IsFollowing = true;
        }

        if (hideBossOnStart)
        {
            bossObject.gameObject.SetActive(true);
            if (showBossEffect)
                Instantiate(showBossEffect, bossObject.gameObject.transform.position, Quaternion.identity);

            audioService.PlaySfx(bossVisibleSound);
            yield return new WaitForSeconds(2f);
        }

        isTalk = true;
        isTalking = true;
        dialogFlowService.StartDialog(isFirstTalk ? dialogs : talkAgainDialogs, gameObject, disableWhenDone, isFinishLevel, hideSmallFaceIcon, this);
        isFirstTalk = false;
    }

    public void FinishDialog()
    {
        StartCoroutine(FinishDialogueCo());
    }

    private IEnumerator FinishDialogueCo()
    {
        if (givePlayerAKey && !isGaveAKey)
        {
            isGaveAKey = true;
            Instantiate(keyItem, gameSession.Player.transform.position, Quaternion.identity);
        }

        if (activeBoss)
        {
            yield return new WaitForSeconds(1f);
            bossObject.Play();
        }

        audioService.PauseMusic(false);
        if (changeMusic)
            audioService.PlayMusic(music, musicVolume);

        controllerInputService.StopMove();
        presentationService.SetControllerVisible(true);
        presentationService.SetGameplayUiVisible(true);

        isTakingFinish = true;
    }

    private void OnDrawGizmos()
    {
        if (activeBoss && bossObject)
            Gizmos.DrawLine(transform.position, bossObject.transform.position);

        if (setCameraLimitMin)
        {
            Gizmos.DrawLine(transform.position + Vector3.left * limitLeftPos, transform.position);
            Gizmos.DrawSphere(transform.position + Vector3.left * limitLeftPos, 0.2f);
        }

        if (setCameraLimitMax)
        {
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * limitRightPos);
            Gizmos.DrawSphere(transform.position + Vector3.right * limitRightPos, 0.2f);
        }
    }
}
