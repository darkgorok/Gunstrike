public sealed class InformationSignButtonTogglePresenter : IInformationSignButtonHint
{
    private readonly InformationSign.TutorialButton buttonToEnable;
    private readonly IControllerInputService controllerInputService;

    public InformationSignButtonTogglePresenter(InformationSign.TutorialButton buttonToEnable, IControllerInputService controllerInputService)
    {
        this.buttonToEnable = buttonToEnable;
        this.controllerInputService = controllerInputService;
    }

    public void SetEnabled(bool enabled)
    {
        switch (buttonToEnable)
        {
            case InformationSign.TutorialButton.Jump:
                controllerInputService.SetJumpButtonVisible(enabled);
                break;
            case InformationSign.TutorialButton.Weapon:
                controllerInputService.SetRangeButtonVisible(enabled);
                controllerInputService.SetMeleeButtonVisible(enabled);
                break;
            case InformationSign.TutorialButton.Dash:
                controllerInputService.SetDashButtonVisible(enabled);
                break;
        }
    }
}
