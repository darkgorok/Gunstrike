using UnityEngine;

public interface IBossHealthbarService
{
    void Init(Sprite icon, int maxHealth);
    void UpdateHealth(int currentHealth);
}
