using UnityEngine;

public sealed class LegacyTutorialOverlayService : ITutorialOverlayService
{
    private Tutorial cachedTutorial;

    private Tutorial Current
    {
        get
        {
            if (cachedTutorial == null)
                cachedTutorial = Object.FindFirstObjectByType<Tutorial>();

            return cachedTutorial;
        }
    }

    public void Open(Sprite image)
    {
        Current?.Open(image);
    }

    public void Close()
    {
        Current?.Close();
    }
}
