using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ZoomZone : MonoBehaviour
{
    [Range(1, 10f)]
    public float orthographicSize = 2.5f;

    bool isZooming = false;

    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private BoxCollider2D triggerCollider;

    private void Awake()
    {
        if (cameraFollow == null)
            cameraFollow = Object.FindFirstObjectByType<CameraFollow>();
        if (triggerCollider == null)
            triggerCollider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isZooming)
            return;

        if (other.TryGetComponent(out Player _) && cameraFollow != null)
        {
            cameraFollow.ZoomIn(orthographicSize);
            isZooming = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isZooming)
            return;

        if (other.TryGetComponent(out Player _) && cameraFollow != null)
        {
            cameraFollow.ZoomOut();
            isZooming = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (triggerCollider == null)
                triggerCollider = GetComponent<BoxCollider2D>();

            var bounds = triggerCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (triggerCollider == null)
            TryGetComponent(out triggerCollider);
    }
#endif
}
