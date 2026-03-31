using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSetBoolAnim : MonoBehaviour
{
    public Animator anim;
    public string boolName = "open";
    public bool stateOnStart = true;
    public float delayOnStart = 1;
    public float rate = 2;
    public AudioClip soundOpen, soundClose;
    public float playSoundPlayerInRange = 10;
    bool state;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        state = stateOnStart;
        anim.SetBool(boolName, state);
        yield return new WaitForSeconds(delayOnStart);

        while (true)
        {
            yield return new WaitForSeconds(rate);
            state = !state;
            anim.SetBool(boolName, state);

            if(Vector2.Distance(GameManager.Instance.Player.transform.position, transform.position ) < playSoundPlayerInRange)
            {
                SoundManager.PlaySfx(state ? soundOpen: soundClose);
            }
        }
    }
}
