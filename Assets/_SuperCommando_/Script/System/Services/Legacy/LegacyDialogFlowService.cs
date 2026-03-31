using UnityEngine;

public sealed class LegacyDialogFlowService : IDialogFlowService
{
    private DialogManager cachedDialogManager;

    private DialogManager Current
    {
        get
        {
            if (cachedDialogManager == null)
                cachedDialogManager = Object.FindFirstObjectByType<DialogManager>();

            return cachedDialogManager;
        }
    }

    public void StartDialog(Dialogs[] dialog, GameObject owner, bool disableWhenDone = true, bool isFinishLevel = false, bool hideIconImage = false, DialogUITrigger currentTrigger = null)
    {
        Current?.StartDialog(dialog, owner, disableWhenDone, isFinishLevel, hideIconImage, currentTrigger);
    }

    public void Next()
    {
        Current?.Next();
    }

    public void Skip()
    {
        Current?.Skip();
    }
}
