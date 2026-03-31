using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public Transform forceGroundSprite;
    [SerializeField] private Player player;

    void Start()
    {
        if (player == null)
            player = Object.FindFirstObjectByType<Player>();
    }

    void Update()
    {
        if (player == null)
            return;

        var healthPercent = (float)player.Health / player.maxHealth;
        forceGroundSprite.localScale = new Vector3(healthPercent, 1, 1);
    }
}
