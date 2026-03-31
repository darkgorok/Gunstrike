using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenu_World : MonoBehaviour
{
    public int worldNumber = 1;
    public GameObject Locked;

    private IProgressService progressService;

    [Inject]
    public void Construct(IProgressService progressService)
    {
        this.progressService = progressService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
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
            GetComponent<Button>().interactable = false;
        }
    }
}
