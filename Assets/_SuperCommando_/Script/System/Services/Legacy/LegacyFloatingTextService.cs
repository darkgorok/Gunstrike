using UnityEngine;

public sealed class LegacyFloatingTextService : IFloatingTextService
{
    private FloatingTextManager cachedFloatingTextManager;

    private FloatingTextManager Current
    {
        get
        {
            if (cachedFloatingTextManager == null)
                cachedFloatingTextManager = Object.FindFirstObjectByType<FloatingTextManager>();

            return cachedFloatingTextManager;
        }
    }

    public void ShowText(FloatingTextParameter parameter, Vector2 ownerPosition)
    {
        Current?.ShowText(parameter, ownerPosition);
    }
}
