using UnityEngine;

public sealed class LegacyControllerInputService : IControllerInputService
{
    private ControllerInput cachedControllerInput;

    private ControllerInput Current
    {
        get
        {
            if (cachedControllerInput == null)
                cachedControllerInput = Object.FindFirstObjectByType<ControllerInput>();

            return cachedControllerInput;
        }
    }

    public Vector2 MoveInput
    {
        get
        {
            if (Current == null)
                return Vector2.zero;

            return new Vector2(Current.Horizontak, Current.Vertical);
        }
        set
        {
            if (Current == null)
                return;

            Current.Horizontak = value.x;
            Current.Vertical = value.y;
        }
    }

    public void SetJumpButtonVisible(bool visible)
    {
        if (Current != null)
            Current.TurnJump(visible);
    }

    public void SetRangeButtonVisible(bool visible)
    {
        if (Current != null)
            Current.TurnRange(visible);
    }

    public void SetMeleeButtonVisible(bool visible)
    {
        if (Current != null)
            Current.TurnMelee(visible);
    }

    public void SetDashButtonVisible(bool visible)
    {
        if (Current != null)
            Current.TurnDash(visible);
    }

    public void StopMove()
    {
        if (Current != null)
            Current.StopMove();
    }

    public void SetShotHeld(bool hold)
    {
        if (Current != null)
            Current.Shot(hold);
    }
}
