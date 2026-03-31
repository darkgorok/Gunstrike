using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenuUpdateCoins : MonoBehaviour
{
    public Text coins;

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

    private void Update()
    {
        coins.text = progressService.SavedCoins.ToString();
    }
}
