using System;

public sealed class SceneLoadOptions
{
    public float MinVisibleSeconds { get; set; } = 0.35f;
    public float CompleteHoldSeconds { get; set; } = 0.1f;
    public Action OnLoadingTick { get; set; }
}
