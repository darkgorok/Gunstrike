using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//public enum PlayerBigImageStatus{Normal, Smile, Angry, Cry, Sad}

public class DialogManager : MonoBehaviour {
	public static DialogManager Instance;
    public GameObject Panel;
	//public DialogHandler dialogHandler;
	public Transform Container;
	public Image leftIconImage, rightIconImage; 

	public Color colorNoTalk = Color.white;
	 bool hideSmallFaceIcon;
	Dialogs[] dialogs;


	int currentDialog = 0;

	int totalDialogs;

	bool isWorking = false;

	// Use this for initialization
	void Start () {
		Instance = this;
        Panel.SetActive(false);
    }

	GameObject talkingGuy;
	bool disableWhenDone = true;
	bool isFinishLevel = false;
	DialogUITrigger currentTrigger;
    

	public void StartDialog(Dialogs[] _dialog, GameObject obj, bool _disableWhenDone = true, bool _isFinishLevel = false, bool _hideIconImage = false, DialogUITrigger _currentTrigger = null){
		Debug.Log ("StartDialog");
		if (isWorking)
			return;

        Panel.SetActive(true);
		dialogs=_dialog;

		disableWhenDone = _disableWhenDone;
		isFinishLevel = _isFinishLevel;
		talkingGuy = obj;
		totalDialogs = dialogs.Length;
		hideSmallFaceIcon = _hideIconImage;
		currentTrigger = _currentTrigger;
		Next ();
		isWorking = true;
    }

	public void Next(){
		
		if (currentDialog >= dialogs.Length) {
			Skip ();
			return;
		}
        
        if (dialogs [currentDialog].isLeftFirst) {
            ShowLeft();
            rightIconImage.color = colorNoTalk;
            leftIconImage.color = Color.white;
            rightIconImage.sprite = dialogs [currentDialog].rightIcon;
            leftIconImage.sprite = dialogs[currentDialog].leftIcon;
		} else {
            ShowLRight();
            rightIconImage.color = Color.white;
            leftIconImage.color = colorNoTalk;

            rightIconImage.sprite = dialogs [currentDialog].rightIcon;

            leftIconImage.sprite = dialogs[currentDialog].leftIcon;
        }

		currentDialog++;
	}

	public void Skip(){
		Debug.Log ("Finish Dialog");
		isWorking = false;
		currentDialog = 0;

		if (currentTrigger)
			currentTrigger.FinishDialog ();

		GameManager.Instance.State = GameManager.GameState.Playing;
		if (talkingGuy) {
			talkingGuy.SetActive (!disableWhenDone);
		}

        if (isFinishLevel) {
			BlackScreenUI.instance.Show (1);
			GameManager.Instance.GameFinish ();
		}

        Panel.SetActive(false);
    }

    public TextTyper RightDialog;
    public TextTyper LeftDialog;
    TextTyper currentDialogue;

    public void ShowLeft()
    {
        if (currentDialogue != null)
            Destroy(currentDialogue.gameObject);

        currentDialogue = Instantiate(LeftDialog);
        currentDialogue.transform.SetParent(Container.transform, false);
        currentDialogue.Init(dialogs[currentDialog].messages);

        SoundManager.PlaySfx(dialogs[currentDialog].soundMessages);
    }

    public void ShowLRight()
    {
        if (currentDialogue != null)
            Destroy(currentDialogue.gameObject);

        currentDialogue = Instantiate(RightDialog);
        currentDialogue.transform.SetParent(Container.transform, false);
        currentDialogue.Init(dialogs[currentDialog].messages);

        SoundManager.PlaySfx(dialogs[currentDialog].soundMessages);
    }
}

[System.Serializable]
public class Dialogs{
    public Sprite leftIcon;
    public Sprite rightIcon;
	public bool isLeftFirst = false;
	public string messages;
	public AudioClip soundMessages;
}
