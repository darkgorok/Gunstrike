using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class Miner : MonoBehaviour, ICanTakeDamage
{
    public bool blowUpOnStart = false;
    public SpriteRenderer sprite;
    public bool blinking = true;
    public Sprite imageOn;
    public Sprite imageOff;
    public float delayOn = 0.8f;
    public float delayOff = 0.2f;
    public GameObject[] ExplosionFX;
    public AudioClip soundDestroy;

    public bool autoCheckPlayer = false;
    public float checkRadius = 2;

    [Header("Make Damage")]
    public bool canMakeDamage = true;
    public bool canFreezeEnemy = false;
    public int damageFromCenter = 100;
    public float radius = 5;
    public LayerMask targetLayer;

    [Header("HIT EFFECT")]
    public bool playEarthQuake = true;
    public float _eqTime = 0.1f;
    public float _eqSpeed = 60;
    public float _eqSize = 1;

    bool isImageOn;
    float autoCheckTimer;
    float blinkTimer;
    private IAudioService audioService;

    [Inject]
    private void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    public void Init(bool blowUpImmediately, int damageMax, float damageRadius)
    {
        blowUpOnStart = blowUpImmediately;
        damageFromCenter = damageMax;
        radius = damageRadius;
        if (blowUpOnStart)
            TakeDamage(int.MaxValue, Vector2.zero, null, transform.position);
    }

    private void OnEnable()
    {
        ProjectScope.Inject(this);

        if (blowUpOnStart)
        {
            TakeDamage(int.MaxValue, Vector2.zero, gameObject, transform.position);
            return;
        }

        autoCheckTimer = 0f;
    }

    void Start()
    {
        ResetBlinkTimer();
    }

    private void Update()
    {
        if (!blowUpOnStart && autoCheckPlayer)
        {
            autoCheckTimer -= Time.deltaTime;
            if (autoCheckTimer <= 0f)
            {
                autoCheckTimer = 0.1f;
                CheckForTarget();
            }
        }

        if (!blinking)
            return;

        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0f)
            UpdateImage();
    }

    void CheckForTarget()
    {
        var hit = Physics2D.CircleCast(transform.position, checkRadius, Vector2.zero, 0, targetLayer);
        if (hit)
            TakeDamage(int.MaxValue, Vector2.zero, gameObject, transform.position);
    }

    void UpdateImage()
    {
        isImageOn = !isImageOn;
        sprite.sprite = isImageOn ? imageOn : imageOff;
        ResetBlinkTimer();
    }

    void ResetBlinkTimer()
    {
        blinkTimer = isImageOn ? delayOff : delayOn;
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (instigator == gameObject)
            return;

        autoCheckPlayer = false;
        blinking = false;

        if (ExplosionFX.Length > 0)
        {
            foreach (var fx in ExplosionFX)
            {
                if (fx != null)
                    SpawnSystemHelper.GetNextObject(fx, true).transform.position = transform.position;
            }
        }

        if (playEarthQuake)
            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);

        if (canMakeDamage)
            CheckDamage();

        audioService?.PlaySfx(soundDestroy);
        Destroy(gameObject);
    }

    private void CheckDamage()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero, 0, targetLayer);
        if (hits.Length <= 0)
            return;

        foreach (var hit in hits)
        {
            if (canFreezeEnemy)
            {
                var canFreeze = (ICanFreeze)hit.collider.gameObject.GetComponent(typeof(ICanFreeze));
                if (canFreeze != null)
                    canFreeze.Freeze(gameObject);
            }

            var canHit = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
            if (canHit != null)
            {
                float hitDamageAffect = (radius - Vector2.Distance(transform.position, hit.point)) / radius;
                hitDamageAffect = Mathf.Max(hitDamageAffect, 0.2f);
                canHit.TakeDamage(damageFromCenter, Vector2.zero, gameObject, hit.point);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!canMakeDamage)
            return;

        if (autoCheckPlayer)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
