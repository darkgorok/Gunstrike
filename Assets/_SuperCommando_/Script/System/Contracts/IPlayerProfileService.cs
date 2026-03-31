public interface IPlayerProfileService
{
    int SelectedCharacterInstanceId { get; set; }
    int SelectedCharacterId { get; set; }

    bool IsCharacterUnlocked(int characterId);
    void SetCharacterUnlocked(int characterId, bool unlocked);
}
