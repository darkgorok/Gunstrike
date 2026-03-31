using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackScreenSprite : MonoBehaviour
{
    public static BlackScreenSprite instance;
    public SpriteRenderer spriteRenderer;
    Color oriColor;
    void Start()
    {
        instance = this;
        oriColor = spriteRenderer.color;
        oriColor.a = 0;
        spriteRenderer.color = oriColor;
    }

    // Update is called once per frame
    public void Show(float timer, Color _color)
    {
        _color.a = 0;
        spriteRenderer.color = _color;
        oriColor.a = 1;
        StartCoroutine(MMFade.FadeSpriteRenderer(spriteRenderer, timer, oriColor));
    }

    public void Show(float timer)
    {
        oriColor.a = 0;
        spriteRenderer.color = Color.black;
        oriColor.a = 1;
        StartCoroutine(MMFade.FadeSpriteRenderer(spriteRenderer, timer, oriColor));
    }

    public void Hide(float timer)
    {
        oriColor.a = 0;
        StartCoroutine(MMFade.FadeSpriteRenderer(spriteRenderer, timer, oriColor));
    }
}
