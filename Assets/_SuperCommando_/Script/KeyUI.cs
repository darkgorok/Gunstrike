using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyUI : MonoBehaviour
{
    public static KeyUI Instance;
    Animator anim;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if(GameManager.Instance)
            anim.SetBool("got", GameManager.Instance.isHasKey);
    }

    public void Get()
    {
        anim.SetBool("get", true);
    }

    public void Used()
    {
        anim.SetTrigger("use");
        anim.SetBool("get", false);
        anim.SetBool("got", false);
    }
}
