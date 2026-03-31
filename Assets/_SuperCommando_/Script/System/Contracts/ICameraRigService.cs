using UnityEngine;

public interface ICameraRigService
{
    bool IsFollowing { get; set; }
    bool ManualControl { get; set; }
    bool PauseCamera { get; set; }
    Vector2 MinBounds { get; set; }
    Vector2 MaxBounds { get; set; }
    float CameraHalfWidth { get; }
    Vector3 Position { get; set; }
    Transform Transform { get; }
    Vector3 WorldToScreenPoint(Vector3 worldPosition);
    Vector3 ViewportToWorldPoint(Vector3 viewportPoint);
    void MoveToPlayerPosition();
    void FollowPlayer();
}
