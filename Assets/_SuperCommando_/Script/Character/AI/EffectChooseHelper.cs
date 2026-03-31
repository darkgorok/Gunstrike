using UnityEngine;
using VContainer;

public enum EffectType
{
    Effect1,
    Effect2,
    Effect3
}

public class EffectChooseHelper : MonoBehaviour
{
    public bool setFxAsChild = false;
    public Transform followTarget;
    public float autoDestroy = 0;
    public EffectType effectChoose;
    public GameObject Effect1;
    public GameObject Effect2;
    public GameObject Effect3;
    public bool scaleWithPlayer = false;

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

    private void Start()
    {
        if (autoDestroy != 0)
            Destroy(gameObject, autoDestroy);

        GameObject objFX = effectChoose switch
        {
            EffectType.Effect1 => Instantiate(Effect1, transform.position, Effect1.transform.rotation),
            EffectType.Effect2 => Instantiate(Effect2, transform.position, Effect2.transform.rotation),
            EffectType.Effect3 => Instantiate(Effect3, transform.position, Effect3.transform.rotation),
            _ => null
        };

        if (objFX == null)
            return;

        if (scaleWithPlayer && gameSession?.Player != null && gameSession.Player.transform.localScale.y < 0)
        {
            objFX.transform.localScale = new Vector3(-objFX.transform.localScale.x, objFX.transform.localScale.y, objFX.transform.localScale.z);
            objFX.transform.rotation = Quaternion.Euler(objFX.transform.rotation.eulerAngles.x + 180, objFX.transform.rotation.eulerAngles.y, objFX.transform.rotation.eulerAngles.z);
        }

        if (setFxAsChild)
        {
            objFX.transform.SetParent(transform);
            return;
        }

        FollowObject followObject = objFX.AddComponent<FollowObject>();
        followObject.Init(followTarget != null ? followTarget : transform);
    }
}
