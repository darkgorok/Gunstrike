using UnityEngine;

public sealed class LegacyKeyPresentationService : IKeyPresentationService
{
    private KeyUI cachedKeyUi;

    private KeyUI Current
    {
        get
        {
            if (cachedKeyUi == null)
                cachedKeyUi = Object.FindFirstObjectByType<KeyUI>();

            return cachedKeyUi;
        }
    }

    public void ShowCollected()
    {
        if (Current != null)
            Current.Get();
    }

    public void ShowUsed()
    {
        if (Current != null)
            Current.Used();
    }
}
