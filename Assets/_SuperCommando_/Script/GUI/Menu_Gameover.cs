using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using UnityEngine.Advertisements;
using VContainer;

public class Menu_Gameover : MonoBehaviour {
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void TryAgain()
    {
        gameSession.ResetLevel();
    }
}
