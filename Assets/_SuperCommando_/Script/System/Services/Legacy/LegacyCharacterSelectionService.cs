using UnityEngine;

public sealed class LegacyCharacterSelectionService : ICharacterSelectionService
{
    public GameObject CurrentCharacterPrefab => CharacterHolder.Instance != null ? CharacterHolder.Instance.CharacterPicked : null;

    public void RefreshSelectedCharacter()
    {
        if (CharacterHolder.Instance != null)
            CharacterHolder.Instance.GetPickedCharacter();
    }
}
