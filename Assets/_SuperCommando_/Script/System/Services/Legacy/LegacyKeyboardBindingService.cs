using UnityEngine;

public sealed class LegacyKeyboardBindingService : IKeyboardBindingService
{
    private DefaultValueKeyboard cachedKeyboard;

    private DefaultValueKeyboard Current
    {
        get
        {
            if (cachedKeyboard == null)
                cachedKeyboard = Object.FindFirstObjectByType<DefaultValueKeyboard>();

            return cachedKeyboard;
        }
    }

    public KeyCode Jump => Current != null ? Current.keyJump : KeyCode.Space;
    public KeyCode Throw => Current != null ? Current.keyThrow : KeyCode.G;
    public KeyCode Shot => Current != null ? Current.keyShot : KeyCode.J;
    public KeyCode Pause => Current != null ? Current.keyPause : KeyCode.Escape;
}
