using UnityEngine;

public interface IDialogFlowService
{
    void StartDialog(Dialogs[] dialog, GameObject owner, bool disableWhenDone = true, bool isFinishLevel = false, bool hideIconImage = false, DialogUITrigger currentTrigger = null);
    void Next();
    void Skip();
}
