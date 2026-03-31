using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleChasePlayer : MonoBehaviour
{

    public float speed = 10;

    //	public float timeToDestroy = 7;
    public AudioClip soundShowUp;

    public SpriteRenderer image;
    Vector3 originalPos;

    //	float time = 0;
    //	public bool stopCounting = false;
    public GameObject endPoint;
    Vector3 endPos;
    public AudioClip sound;
    AudioSource EngineAudio;
    void Awake()
    {
        originalPos = transform.position;
        if (endPoint)
            endPos = endPoint.transform.position;
        EngineAudio = gameObject.AddComponent<AudioSource>();
        EngineAudio.clip = sound;
        EngineAudio.Play();
        EngineAudio.loop = true;
        EngineAudio.volume = 0;
    }


    void OnEnable()
    {
        if (!GameManager.Instance)
            return;

        if (GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        //		image.color = new Color(1f,1f,1f,0f);
        StartCoroutine(MMFade.FadeSprite(image, 0.2f, new Color(1f, 1f, 1f, 1f)));
        SoundManager.PlaySfx(soundShowUp);
        //		GameManager.instance.listeners.Add (this);
    }
    bool isMovingBack = false;
    void LateUpdate()
    {

        if (GameManager.Instance.State != GameManager.GameState.Playing)
        {
            EngineAudio.volume = 0;
            return;
        }

        transform.Translate(speed * Time.deltaTime, 0, 0, Space.Self);
        EngineAudio.volume = 0.8f;
        if (!isMovingBack)
        {
            if (endPoint)
            {
                if (Mathf.Abs(transform.position.x - endPos.x) < 0.5f)
                {
                    speed = -speed;
                    EngineAudio.Stop();
                    isMovingBack = true;
                }
            }
        }
        else
        {
            if (Mathf.Abs(transform.position.x - originalPos.x) < 0.5f)
            {
                gameObject.SetActive(false);
            }
        }

        //		if (stopCounting)
        //			return;

        //		time += Time.deltaTime;
        //		if (time > timeToDestroy - 1) {
        //			StartCoroutine(MMFade.FadeSprite(image,0.2f,new Color(1f,1f,1f,0f)));	
        //			Destroy (gameObject, 0.2f);
        //			stopCounting = true;
        //		}
    }

    #region IListener implementation

    public void IPlay()
    {
    }

    public void ISuccess()
    {
    }

    public void IPause()
    {
    }

    public void IUnPause()
    {
    }

    public void IGameOver()
    {
    }

    public void IOnRespawn()
    {
        transform.position = originalPos;
        gameObject.SetActive(false);
        EngineAudio.Play();
    }

    public void IOnStopMovingOn()
    {
    }

    public void IOnStopMovingOff()
    {
    }

    #endregion

    //void OnCollisionEnter2D(Collision2D other)
    //{

        //if (other.gameObject.CompareTag("Player"))
        //{
        //    if (GameManager.Instance.Player.isBoostSpeed || FindObjectOfType<Shield>())
        //    {

        //        return;
        //    }

        //    //				isDead = true;
        //    GameManager.instance.GameOver();
        //}

    //}

    void OnDrawGizmos()
    {
        if (endPoint)
            Gizmos.DrawLine(transform.position, endPoint.transform.position);
    }

}
