using UnityEngine;

public sealed class LegacyLevelSelectionState : ILevelSelectionState
{
    private Transform currentHighestLevelTransform;

    public Transform CurrentHighestLevelTransform
    {
        get => currentHighestLevelTransform;
        set => currentHighestLevelTransform = value;
    }
}
