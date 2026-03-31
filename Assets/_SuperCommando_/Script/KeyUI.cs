using UnityEngine;
using VContainer;

public class KeyUI : MonoBehaviour
{
    private Animator anim;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IGameSessionService gameSession)
    {
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (gameSession != null)
            anim.SetBool("got", gameSession.HasKey);
    }

    public void Get()
    {
        anim.SetBool("get", true);
    }

    public void Used()
    {
        anim.SetTrigger("use");
        anim.SetBool("get", false);
        anim.SetBool("got", false);
    }
}
