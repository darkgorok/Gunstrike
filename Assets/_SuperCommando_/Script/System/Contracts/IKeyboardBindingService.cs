using UnityEngine;

public interface IKeyboardBindingService
{
    KeyCode Jump { get; }
    KeyCode Throw { get; }
    KeyCode Shot { get; }
    KeyCode Pause { get; }
}
