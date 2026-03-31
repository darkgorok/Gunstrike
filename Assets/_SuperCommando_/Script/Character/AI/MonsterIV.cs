using UnityEngine;
using System.Collections;

public class MonsterIV : MonoBehaviour, ICanTakeDamage {
	public AudioClip soundDead;
	public GameObject deadFx;
	public int scoreRewarded = 200;
	public Transform linePoint;

	private LineRenderer line;
	Vector3 oldPosition;
	SpringJoint2D springJoint;
	Rigidbody2D rig;
    private IAudioService audioService;
    private IGameSessionService gameSession;

    [VContainer.Inject]
    public void Construct(IAudioService audioService, IGameSessionService gameSession)
    {
        this.audioService = audioService;
        this.gameSession = gameSession;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

	void Start(){
		line = GetComponent<LineRenderer> ();
		oldPosition = transform.position;
		springJoint = GetComponent<SpringJoint2D> ();
		rig = GetComponent<Rigidbody2D> ();
	}
	void Update(){
		line.SetPosition (0, linePoint.position);
		line.SetPosition (1, transform.position);
	}

	public void Dead(){
		audioService.PlaySfx(soundDead);
		gameSession.AddPoint(scoreRewarded);

		if (deadFx != null)
		Instantiate (deadFx, transform.position, Quaternion.identity);

        //try spawn random item
        var spawnItem = GetComponent<EnemySpawnItem>();
        if (spawnItem != null)
        {
            spawnItem.SpawnItem();
        }

        //turn off all colliders if the enemy have
        var boxCo = GetComponents<BoxCollider2D> ();
		foreach (var box in boxCo) {
			box.enabled = false;
		}
		var CirCo = GetComponents<CircleCollider2D> ();
		foreach (var cir in CirCo) {
			cir.enabled = false;
		}

		springJoint.enabled = false;
		line.enabled = false;
		rig.linearVelocity = Vector2.zero;
		rig.AddForce (new Vector2 (0, 300f));

        Destroy(gameObject, 1);
    }

	public void TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
		Dead ();
	}
}
