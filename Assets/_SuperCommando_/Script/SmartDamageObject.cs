using UnityEngine;
using VContainer;

public class SmartDamageObject : MonoBehaviour, ICanTakeDamage
{
    [Header("Like Enemy")]
    public bool canBeHit = true;
    public GameObject HurtEffect;
    public GameObject DestroyEffect;
    [Range(0, 100)]
    public float health = 50;
    public AudioClip hurtSound;
    public AudioClip deadSound;

    private float currentHealth;
    private bool isDead;
    private IAudioService audioService;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    private void Start()
    {
        currentHealth = health;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!enabled || isDead || !canBeHit)
            return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            isDead = true;
            DestroyObject();
        }
        else
        {
            HitEvent();
        }
    }

    protected void HitEvent()
    {
        audioService?.PlaySfx(hurtSound);
        if (HurtEffect != null)
            Instantiate(HurtEffect, transform.position, transform.rotation);
    }

    protected void DestroyObject()
    {
        audioService?.PlaySfx(deadSound);
        if (DestroyEffect != null)
            Instantiate(DestroyEffect, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
