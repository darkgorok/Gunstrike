using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthbar : MonoBehaviour
{
    public static BossHealthbar Instance;
    public Image bossIcon;
    public Transform forceGroundSprite;
    float maxHealth = 0;
    CanvasGroup canvasG;
    void Awake()
    {
        Instance = this;
        canvasG = GetComponent<CanvasGroup>();
        canvasG.alpha = 0;
    }

    public void Init(Sprite icon, int _maxHealth)
    {
        bossIcon.sprite = icon;
        maxHealth = (float)_maxHealth;
        UpdateHealth(_maxHealth);
        canvasG.alpha = 1;
    }

    public void UpdateHealth(int current)
    {
        if (Instance)
        {
            var healthPercent = (float)current / maxHealth;
            forceGroundSprite.localScale = new Vector3(healthPercent, 1, 1);

            canvasG.alpha = healthPercent > 0 ? 1 : 0;
        }
    }
}
