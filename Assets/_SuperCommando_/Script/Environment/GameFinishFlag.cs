using UnityEngine;
using System.Collections;

public class GameFinishFlag : MonoBehaviour
{
    public AudioClip sound;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Player>() == null)
            return;

        if (GameManager.Instance.State == GameManager.GameState.Finish)
            return;

        GameManager.Instance.GameFinish();
        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().SetBool("finish", true);
        SoundManager.PlaySfx(sound, 0.5f);
        Destroy(this);
    }
}
