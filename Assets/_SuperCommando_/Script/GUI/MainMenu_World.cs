using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenu_World : MonoBehaviour
{
    public int worldNumber = 1;
    public GameObject Locked;

    [SerializeField] private Button cachedButton;

    private IProgressService progressService;

    [Inject]
    public void Construct(IProgressService progressService)
    {
        this.progressService = progressService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        if (cachedButton == null)
            cachedButton = GetComponent<Button>();
    }

    private void Start()
    {
        int worldReached = progressService.WorldReached;
        if (worldNumber <= worldReached)
        {
            Locked.SetActive(false);
        }
        else
        {
            Locked.SetActive(true);
            cachedButton.interactable = false;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cachedButton == null)
            TryGetComponent(out cachedButton);
    }
#endif
}
