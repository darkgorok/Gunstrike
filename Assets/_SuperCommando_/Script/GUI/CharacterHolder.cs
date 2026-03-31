using System.Collections;
using UnityEngine;
using VContainer;

public class CharacterHolder : MonoBehaviour
{
    public static CharacterHolder Instance;
    [HideInInspector] public GameObject CharacterPicked;
    public GameObject[] Characters;

    private IPlayerProfileService playerProfileService;

    [Inject]
    public void Construct(IPlayerProfileService playerProfileService)
    {
        this.playerProfileService = playerProfileService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        if (CharacterHolder.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        GetPickedCharacter();
    }

    public void GetPickedCharacter()
    {
        CharacterPicked = null;
        int characterInstanceId = playerProfileService.SelectedCharacterInstanceId;
        foreach (GameObject character in Characters)
        {
            if (character.GetInstanceID() != characterInstanceId)
                continue;

            CharacterPicked = character;
            return;
        }
    }
}
