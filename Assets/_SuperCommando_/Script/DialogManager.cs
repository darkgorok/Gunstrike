using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class DialogManager : MonoBehaviour
{
    public GameObject Panel;
    public Transform Container;
    public Image leftIconImage, rightIconImage;
    public Color colorNoTalk = Color.white;
    public TextTyper RightDialog;
    public TextTyper LeftDialog;

    private bool hideSmallFaceIcon;
    private Dialogs[] dialogs;
    private int currentDialog;
    private bool isWorking;
    private bool disableWhenDone = true;
    private bool isFinishLevel;
    private GameObject talkingGuy;
    private DialogUITrigger currentTrigger;
    private TextTyper currentDialogue;
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
        Panel.SetActive(false);
    }

    public void StartDialog(Dialogs[] dialog, GameObject obj, bool disableWhenDone = true, bool isFinishLevel = false, bool hideIconImage = false, DialogUITrigger currentTrigger = null)
    {
        if (isWorking)
            return;

        Panel.SetActive(true);
        dialogs = dialog;
        this.disableWhenDone = disableWhenDone;
        this.isFinishLevel = isFinishLevel;
        talkingGuy = obj;
        hideSmallFaceIcon = hideIconImage;
        this.currentTrigger = currentTrigger;
        isWorking = true;
        Next();
    }

    public void Next()
    {
        if (currentDialog >= dialogs.Length)
        {
            Skip();
            return;
        }

        if (dialogs[currentDialog].isLeftFirst)
        {
            ShowLeft();
            rightIconImage.color = colorNoTalk;
            leftIconImage.color = Color.white;
        }
        else
        {
            ShowRight();
            rightIconImage.color = Color.white;
            leftIconImage.color = colorNoTalk;
        }

        rightIconImage.sprite = dialogs[currentDialog].rightIcon;
        leftIconImage.sprite = dialogs[currentDialog].leftIcon;
        currentDialog++;
    }

    public void Skip()
    {
        isWorking = false;
        currentDialog = 0;

        if (currentTrigger)
            currentTrigger.FinishDialog();

        if (gameSession != null)
            gameSession.State = GameManager.GameState.Playing;

        if (talkingGuy)
            talkingGuy.SetActive(!disableWhenDone);

        if (isFinishLevel && gameSession != null)
        {
            BlackScreenUI.instance.Show(1);
            gameSession.GameFinish();
        }

        Panel.SetActive(false);
    }

    public void ShowLeft()
    {
        ReplaceDialogue(LeftDialog);
        currentDialogue.Init(dialogs[currentDialog].messages);
        audioService?.PlaySfx(dialogs[currentDialog].soundMessages);
    }

    public void ShowRight()
    {
        ReplaceDialogue(RightDialog);
        currentDialogue.Init(dialogs[currentDialog].messages);
        audioService?.PlaySfx(dialogs[currentDialog].soundMessages);
    }

    private void ReplaceDialogue(TextTyper prefab)
    {
        if (currentDialogue != null)
            Destroy(currentDialogue.gameObject);

        currentDialogue = Instantiate(prefab);
        currentDialogue.transform.SetParent(Container.transform, false);
    }
}

[System.Serializable]
public class Dialogs
{
    public Sprite leftIcon;
    public Sprite rightIcon;
    public bool isLeftFirst = false;
    public string messages;
    public AudioClip soundMessages;
}
