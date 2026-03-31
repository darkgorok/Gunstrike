/*
 * BOSS ROOT GAMEOBJET DON'T CHILD ANY OTHER OBJECTS
 */

using UnityEngine;
using System.Collections;

public class BOSS_2 : MonoBehaviour, ICanTakeDamage
{
    public bool isBoss = false;
    Animator anim;

    [Range(10, 500)]
    public int health = 500;
    [ReadOnly] public int currentHealth;
    
    public AudioClip deadSound;
    public AudioClip throwSound;
    public AudioClip hurtSound;

    public Vector2 healthBarOffset = new Vector2(0, 1.5f);
    protected HealthBarEnemyNew healthBar;

    public GameObject deadFX;

    [Header("Attack")]
    public GameObject Stone;
    public Transform attackPoint;
    public float MinAttackTime = 2f;
    public float MaxAttackTime = 4f;


    bool isDead = false;
    Rigidbody2D rig;

    [HideInInspector]
    public GameObject saveOwner;
    Material objMat;
    // Use this for initialization
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currentHealth = health;
        var healthBarObj = (HealthBarEnemyNew)Resources.Load("HealthBar", typeof(HealthBarEnemyNew));
        healthBar = (HealthBarEnemyNew)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        healthBar.Init(transform, (Vector3)healthBarOffset);

        saveOwner = Instantiate(transform.root.gameObject, transform.root.position, Quaternion.identity) as GameObject;
        saveOwner.SetActive(false);

        objMat = characterImage.material;
    }

    private void Update()
    {
        if (isDead)
            return;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * (transform.position.x > GameManager.Instance.Player.transform.position.x ? 1 : -1), transform.localScale.y, transform.localScale.z);
    }

    //send by Detect Player trigger object 
    void Play()
    {
        StartCoroutine(Attack(Random.Range(MinAttackTime, MaxAttackTime)));
    }

    IEnumerator Attack(float delay)
    {
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(delay);

        while (GameManager.Instance.State != GameManager.GameState.Playing)
        {
            yield return null;
        }
        StartCoroutine(Attack(Random.Range(MinAttackTime, MaxAttackTime)));
    }

    //Called by animation event trigger
    public void ThrowStone()
    {
        SoundManager.PlaySfx(throwSound);
        Instantiate(Stone, attackPoint.position, Quaternion.identity);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        isDead = health <= 0 ? true : false;
        if (healthBar)
            healthBar.UpdateValue(currentHealth / (float)health);
        if (isDead)
        {
            SoundManager.PlaySfx(deadSound);
            anim.SetTrigger("Dead");
          
            var boxCo = GetComponents<BoxCollider2D>();
            foreach (var box in boxCo)
            {
                box.enabled = false;
            }
            var CirCo = GetComponents<CircleCollider2D>();
            foreach (var cir in CirCo)
            {
                cir.enabled = false;
            }
            rig.isKinematic = true;

            if (isBoss)
            {
                GameManager.Instance.MissionStarCollected = 3;
                GameManager.Instance.GameFinish();
            }
            else
            {
                Invoke("DisableBoss", 2);
                //try spawn random item
                var spawnItem = GetComponent<EnemySpawnItem>();

               

                if (spawnItem != null)
                {
                    spawnItem.SpawnItem();
                }
            }
        }
        else
        {
            StartCoroutine(BlinkEffecrCo());		//begin the blink effect
            SoundManager.PlaySfx(hurtSound);
        }
    }

    [Header("BLINKING")]

    public float blinking = 1.5f;       //blinking time allowed
    bool isBlinking = false;
    public SpriteRenderer characterImage;
    public Material whiteMaterial;
    IEnumerator BlinkEffecrCo()
    {
        isBlinking = true;
        int blink = (int)(blinking * 0.5f / 0.2f);

        for (int i = 0; i < blink; i++)
        {
            characterImage.material = whiteMaterial;
            yield return new WaitForSeconds(0.2f);
            characterImage.material = objMat;
            yield return new WaitForSeconds(0.2f);
        }
        characterImage.material = objMat;
        isBlinking = false;
    }

    void DisableBoss()
    {
        if (deadFX)
            Instantiate(deadFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
