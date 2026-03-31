using UnityEngine;
using System.Collections;

public class ChasingStone : MonoBehaviour, ICanTakeDamage {
	Player player;
	public float speedMin = 2f;
	public float speedMax = 3f;
	float speed;
    public Vector2 direction = Vector2.zero;

	public GameObject ExplosionFx;

    public AudioClip destroySound;

    [Header("RADAR")]
    
    public float radarRadius = 3;
    bool isUseRadar = false;
    bool isChasingPlayer = true;

    public void Init(float YOffset = 0.5f, float XOffset = 0, bool _isUseRadar = false)
    {
        direction = (new Vector3(GameManager.Instance.Player.transform.position.x + XOffset, GameManager.Instance.Player.transform.position.y + YOffset, GameManager.Instance.Player.transform.position.z) - transform.position).normalized;

        isUseRadar = _isUseRadar;
    }

    // Use this for initialization
    void Start()
    {
        player = FindObjectOfType<Player>();
        if (direction == Vector2.zero)
            direction = (player.transform.position - transform.position).normalized;
        speed = Random.Range(speedMin, speedMax);
    }
	
	// Update is called once per frame
	void Update () {

        if (isChasingPlayer && isUseRadar)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);

            Vector3 dir = player.transform.position - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            angle = Mathf.Lerp(transform.eulerAngles.z > 180 ? transform.eulerAngles.z - 360 : transform.eulerAngles.z, angle, 1f);

            //var finalAngle = angle < 0 ? angle - 360 : angle;
            //transform.rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);

            if (Vector2.Distance(player.transform.position, transform.position) < 0.1f)
                isChasingPlayer = false;
        }
        else
        {
            transform.Translate(speed * direction * Time.deltaTime);
        }

        //transform.Translate (speed * direction*Time.deltaTime);
	}

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (ExplosionFx)
            Instantiate(ExplosionFx, transform.position, Quaternion.identity);

        SoundManager.PlaySfx(destroySound, 0.7f);
        gameObject.SetActive(false);
    }
}
