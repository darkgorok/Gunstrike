using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogHandler : MonoBehaviour
{
    public TextTyper RightDialog;
    public TextTyper LeftDialog;

    public AudioClip nextSound;
    public AudioClip skipSound;

    public Transform messageContainer;

    bool isRightTop;
    string messages;
    int currentMessage = 0;
    TextTyper currentTyper;
    AudioClip soundMessages;

    public void Init(string _message, bool _isRightTop, AudioClip _soundMessages)
    {
        messages = _message;
        isRightTop = _isRightTop;
        soundMessages = _soundMessages;
    }

    void Start()
    {
        Next();
    }

    public void Next()
    {
        if (currentMessage >= 1)
        {
            DialogManager.Instance.Next();
            Destroy(gameObject);
            return;
        }

        if (isRightTop)
            ShowLRight();
        else
            ShowLeft();

        isRightTop = !isRightTop;
        currentMessage++;
        SoundManager.PlaySfx(nextSound);
    }

    public void Skip()
    {
        SoundManager.PlaySfx(skipSound);
        DialogManager.Instance.Skip();
        Destroy(gameObject);
    }

    public void ShowLeft()
    {
        var obj = Instantiate(LeftDialog);
        obj.transform.SetParent(messageContainer.transform, false);
        obj.Init(messages);

        SoundManager.PlaySfx(soundMessages);
    }

    public void ShowLRight()
    {
        var obj = Instantiate(RightDialog);
        obj.transform.SetParent(messageContainer.transform, false);
        obj.Init(messages);

        SoundManager.PlaySfx(soundMessages);
    }
}
