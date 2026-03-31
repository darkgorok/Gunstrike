using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimation : MonoBehaviour
{
    public SpriteRenderer ownerSpriteRenderer;
    public float ratePerSprite = 0.1f;
    public Sprite[] spriters;
    int currentPos = 0;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ChangeSprite", Random.Range(0f, 0.1f), ratePerSprite);
    }

    void ChangeSprite()
    {
        ownerSpriteRenderer.sprite = spriters[currentPos];
        currentPos++;
        if (currentPos >= spriters.Length)
            currentPos = 0;
    }
}
