using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlatform : MonoBehaviour, IStandOnEvent
{
    public Sprite crackedImage;
    public float timeLive = 2;
    public GameObject destroyFX;
    public GameObject smokeFX;
    public AudioClip contactSound;

    bool isWorking = false;
    
    public float goBackIn = 3f;
    Sprite oriSprite;
    SpriteRenderer spriteRen;
    Collider2D col;

    private void Start()
    {
        spriteRen = GetComponent<SpriteRenderer>();
        oriSprite = GetComponent<SpriteRenderer>().sprite;
        col = GetComponent<Collider2D>();
    }

    private IEnumerator WorkCo()
    {
        isWorking = true;

        SoundManager.PlaySfx(contactSound);
        spriteRen.sprite = crackedImage;

        if (smokeFX)
            SpawnSystemHelper.GetNextObject(smokeFX, true).transform.position = transform.position;

        yield return new WaitForSeconds(timeLive);
        Instantiate(destroyFX, transform.position, Quaternion.identity);
        spriteRen.enabled = false;
        col.enabled = false;
        //yield return new WaitForSeconds(goBackIn);
        Invoke("GoBack", goBackIn);
    }

    void GoBack()
    {
        spriteRen.sprite = oriSprite;
        spriteRen.enabled = true;
        col.enabled = true;
        isWorking = false;
    }

    public void StandOnEvent(GameObject instigator)
    {
        if (isWorking)
            return;

        StartCoroutine(WorkCo());
    }
}
