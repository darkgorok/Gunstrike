using UnityEngine;
using System.Collections;

public class FireEvil : MonoBehaviour, ICanTakeDamage {
	public float speed = 5;
	Vector3 old_position;
    public AudioClip showUpSound, deadSound;

    public float timeLive = 5;

	// Use this for initialization
	void Start () {
		old_position = transform.position;

        Destroy(gameObject, timeLive);
	}

    private void OnEnable()
    {
        if (GameManager.Instance && GameManager.Instance.State == GameManager.GameState.Playing)
        {
            if (SoundManager.Instance)
                SoundManager.PlaySfx(showUpSound);
        }
    }

    void Update () {
		transform.Translate (speed * Time.deltaTime * transform.right.x, 0, 0, Space.World);
	}

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        //Debug.LogError("TakeDamage");
        transform.position = old_position;
        SoundManager.PlaySfx(deadSound);

        //try spawn random item
        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }

        gameObject.SetActive(false);
    }
}
