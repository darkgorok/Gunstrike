using UnityEngine;

public sealed class LegacyCameraRigService : ICameraRigService
{
    private CameraFollow cachedCameraFollow;
    private Camera cachedCamera;

    private CameraFollow Current
    {
        get
        {
            if (cachedCameraFollow == null)
            {
                cachedCameraFollow = Object.FindFirstObjectByType<CameraFollow>();
                cachedCamera = cachedCameraFollow != null ? cachedCameraFollow.GetComponent<Camera>() : null;
            }

            return cachedCameraFollow;
        }
    }

    private Camera CurrentCamera
    {
        get
        {
            _ = Current;
            return cachedCamera;
        }
    }

    public bool IsFollowing
    {
        get => Current != null && Current.isFollowing;
        set
        {
            if (Current != null)
                Current.isFollowing = value;
        }
    }

    public bool ManualControl
    {
        get => Current != null && Current.manualControl;
        set
        {
            if (Current != null)
                Current.manualControl = value;
        }
    }

    public bool PauseCamera
    {
        get => Current != null && Current.pauseCamera;
        set
        {
            if (Current != null)
                Current.pauseCamera = value;
        }
    }

    public Vector2 MinBounds
    {
        get => Current != null ? Current._min : Vector2.zero;
        set
        {
            if (Current != null)
                Current._min = value;
        }
    }

    public Vector2 MaxBounds
    {
        get => Current != null ? Current._max : Vector2.zero;
        set
        {
            if (Current != null)
                Current._max = value;
        }
    }

    public float CameraHalfWidth => Current != null ? Current.CameraHalfWidth : 0f;

    public Vector3 Position
    {
        get => Current != null ? Current.transform.position : Vector3.zero;
        set
        {
            if (Current != null)
                Current.transform.position = value;
        }
    }

    public Transform Transform => Current != null ? Current.transform : null;

    public Vector3 WorldToScreenPoint(Vector3 worldPosition)
    {
        if (CurrentCamera == null)
            return worldPosition;

        return CurrentCamera.WorldToScreenPoint(worldPosition);
    }

    public Vector3 ViewportToWorldPoint(Vector3 viewportPoint)
    {
        if (CurrentCamera == null)
            return viewportPoint;

        return CurrentCamera.ViewportToWorldPoint(viewportPoint);
    }

    public void MoveToPlayerPosition()
    {
        if (Current != null)
            Current.MoveCameraToPlayerPos();
    }

    public void FollowPlayer()
    {
        if (Current != null)
            Current.DoFollowPlayer();
    }
}
