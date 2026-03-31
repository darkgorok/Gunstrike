using UnityEngine;
using VContainer;

public class DialogHandler : MonoBehaviour
{
    public TextTyper RightDialog;
    public TextTyper LeftDialog;
    public AudioClip nextSound;
    public AudioClip skipSound;
    public Transform messageContainer;

    private bool isRightTop;
    private string messages;
    private int currentMessage = 0;
    private AudioClip soundMessages;
    private IAudioService audioService;
    private IDialogFlowService dialogFlowService;

    [Inject]
    public void Construct(IAudioService audioService, IDialogFlowService dialogFlowService)
    {
        this.audioService = audioService;
        this.dialogFlowService = dialogFlowService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void Init(string message, bool isRightTop, AudioClip soundMessages)
    {
        messages = message;
        this.isRightTop = isRightTop;
        this.soundMessages = soundMessages;
    }

    private void Start()
    {
        Next();
    }

    public void Next()
    {
        if (currentMessage >= 1)
        {
            dialogFlowService.Next();
            Destroy(gameObject);
            return;
        }

        if (isRightTop)
            ShowRight();
        else
            ShowLeft();

        isRightTop = !isRightTop;
        currentMessage++;
        audioService?.PlaySfx(nextSound);
    }

    public void Skip()
    {
        audioService?.PlaySfx(skipSound);
        dialogFlowService.Skip();
        Destroy(gameObject);
    }

    public void ShowLeft()
    {
        TextTyper obj = Instantiate(LeftDialog);
        obj.transform.SetParent(messageContainer.transform, false);
        obj.Init(messages);
        audioService?.PlaySfx(soundMessages);
    }

    public void ShowRight()
    {
        TextTyper obj = Instantiate(RightDialog);
        obj.transform.SetParent(messageContainer.transform, false);
        obj.Init(messages);
        audioService?.PlaySfx(soundMessages);
    }
}
