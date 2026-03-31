using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class KeyItem : MonoBehaviour {
	public AudioClip sound;
	public GameObject effect;
    private IAudioService audioService;
    private IGameSessionService gameSession;

    [Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }
	
	void OnTriggerEnter2D(Collider2D other){
		if (other.gameObject == gameSession.Player.gameObject) {
			if (effect)
				Instantiate (effect, transform.position, Quaternion.identity);
            audioService.PlaySfx(sound);
			gameSession.HasKey = true;
            //gameObject.SetActive (false);
            Destroy(gameObject);
        }
	}
}
