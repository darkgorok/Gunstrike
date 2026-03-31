using UnityEngine;

public interface IControllerInputService
{
    Vector2 MoveInput { get; set; }

    void SetJumpButtonVisible(bool visible);
    void SetRangeButtonVisible(bool visible);
    void SetMeleeButtonVisible(bool visible);
    void SetDashButtonVisible(bool visible);
    void StopMove();
    void SetShotHeld(bool hold);
}
