using UnityEngine;

public interface ICharacterSelectionService
{
    GameObject CurrentCharacterPrefab { get; }
    void RefreshSelectedCharacter();
}
