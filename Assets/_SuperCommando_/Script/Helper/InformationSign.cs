using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationSign : MonoBehaviour
{
    public enum SetButton { None, Jump, Weapon, Dash}
    public SetButton activeButton = SetButton.None;

    public enum DetectZone { Circle, Box}
    public DetectZone detectZone;
    public float radius = 1;
    public float detectHigh = 3;

    public AudioClip sound;
    public SpriteRenderer spriteRenderer;
    public GameObject inforContainer;
    Color oriColor;
    Color colorTransparent;
    bool playerInArea = false;

    public GameObject mobileTut, windowsTut;

    [Header("Spawn Item Helper")]
    public bool spawnItem = false;
    public ItemType item;
    public float spawnDelay = 2;
    public Vector2 localOffset = new Vector2(0, 0.5f);
    public Vector2 forceSpawn = new Vector2(0, 3);
    [ReadOnly] public ItemType currentItemAvailable;
    [ReadOnly] public bool isSpawning = false;

    private void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
         if(mobileTut)
        mobileTut.SetActive(true);
         if(windowsTut)
        windowsTut.SetActive(false);
#else
        if (mobileTut)
        mobileTut.SetActive(false);
        if (windowsTut)
            windowsTut.SetActive(true);
#endif

        switch (activeButton)
        {
            case SetButton.Jump:
                ControllerInput.Instance.TurnJump(false);
                break;
            case SetButton.Weapon:
                ControllerInput.Instance.TurnRange(false);
                ControllerInput.Instance.TurnMelee(false);
                break;
            case SetButton.Dash:
                ControllerInput.Instance.TurnDash(false);
                break;
        }

        oriColor = spriteRenderer.color;
        colorTransparent = oriColor;
        colorTransparent.a = 0;
        spriteRenderer.color = colorTransparent;
        InvokeRepeating("CheckPlayerInRange", 1, 0.1f);
    }

    void CheckPlayerInRange()
    {
        if (!gameObject.activeInHierarchy)
            return;

        RaycastHit2D hit;
        if (detectZone == DetectZone.Circle)
            hit = Physics2D.CircleCast(transform.position, radius, Vector2.zero, 0, GameManager.Instance.playerLayer);
        else
            hit = Physics2D.BoxCast(transform.position, new Vector2(1, detectHigh), 0, Vector2.zero, 0, GameManager.Instance.playerLayer);
        if (hit)
        {
            if (!playerInArea)
            {
                SoundManager.PlaySfx(sound);
                StartCoroutine(MMFade.FadeSpriteRenderer(spriteRenderer, 0.2f, oriColor));
                inforContainer.SetActive(true);

                switch (activeButton)
                {
                    case SetButton.Jump:
                        ControllerInput.Instance.TurnJump(true);
                        break;
                    case SetButton.Weapon:
                        ControllerInput.Instance.TurnRange(true);
                        ControllerInput.Instance.TurnMelee(true);
                        break;
                    case SetButton.Dash:
                        ControllerInput.Instance.TurnDash(true);
                        break;
                }

                if (spawnItem && !isSpawning && item != null)
                {
                    isSpawning = true;
                    InvokeRepeating("SpawnDartHelperInvoke", 0, spawnDelay);
                }
            }

            playerInArea = true;
        }
        else
        {
            if (playerInArea)
                StartCoroutine(MMFade.FadeSpriteRenderer(spriteRenderer, 0.2f, colorTransparent));

            inforContainer.SetActive(false);
            playerInArea = false;
        }
    }

    void SpawnDartHelperInvoke()
    {
        if(currentItemAvailable == null || !currentItemAvailable.gameObject.activeInHierarchy)
        {
            currentItemAvailable = Instantiate(item, transform.position + (Vector3)localOffset, Quaternion.identity);
            currentItemAvailable.Init(true, new Vector2(0, 5));
        }
    }

    private void OnDrawGizmos()
    {
        if (detectZone == DetectZone.Circle)
            Gizmos.DrawWireSphere(transform.position, radius);
        else
            Gizmos.DrawWireCube(transform.position, new Vector2(1, detectHigh));
    }
}
