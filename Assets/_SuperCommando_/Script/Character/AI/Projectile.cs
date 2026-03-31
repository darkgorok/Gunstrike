using UnityEngine;
using System.Collections;

public abstract class Projectile : MonoBehaviour {
    public float Speed = 10;
    public int Damage = 50;
    public LayerMask LayerCollision;

	public GameObject Owner{ get; private set; }
	public Vector2 Direction{ get; private set; }
	public Vector2 InitialVelocity{ get; private set; }

	protected bool isPower = false;
    protected bool isUseRadar = false;
    protected bool isDetect = false;

    [Header("RADAR")]
    public float radarRadius = 3;
    //public LayerMask targetLayer;
    protected Transform target;

    float originalSpeed;
    bool newSpeed = false;
    Vector2 lastPosition;

    // Use this for initialization
    public void Initialize(GameObject owner, Vector2 direction, Vector2 initialVelocity, bool powerBullet, bool _isUseRadar = false, int damage = -1, float speed = -1)
    {
        isPower = powerBullet;
        if (damage > -1)
            Damage = damage;

        if (speed > 0)
        {
            originalSpeed = Speed;
            Speed = speed;
            newSpeed = true;
        }
        else if(newSpeed)
        {
            Speed = originalSpeed;
            newSpeed = false;
        }

        transform.right = direction;    //turn the X asix to the direction
        Owner = owner;
        isUseRadar = _isUseRadar;
        Direction = direction;
        InitialVelocity = initialVelocity;
        OnInitialized();
    }

    public virtual void Start()
    {
        lastPosition = transform.position;
    }

    public virtual void Update()
    {
        var hits = Physics2D.LinecastAll(lastPosition, transform.position, LayerCollision);
        if (hits.Length > 0)
        {
            ContactTarget(hits);
        }
    }

    public virtual void LateUpdate()
    {
        lastPosition = transform.position;
    }

	public virtual void OnInitialized(){
	}

    void ContactTarget(RaycastHit2D[] hits)
    {
        foreach (var hit in hits)
        {
            bool isOwner = false;
            var anotherSimpleProjectile = hit.collider.gameObject.GetComponent<SimpleProjectile>();
            if (anotherSimpleProjectile != null)
            {
                isOwner = Owner == anotherSimpleProjectile.Owner;
            }
            if (isOwner)
            {
                OnCollideOwner();
            }
            else
            {
                var takeDamage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
                if (takeDamage != null)
                {
                    if (hit.collider.gameObject.GetComponent(typeof(Projectile)) != null)
                        OnCollideOther(hit);
                    else
                        OnCollideTakeDamage(hit, takeDamage);
                }
                else
                {
                    OnCollideOther(hit);
                }
            }
        }
    }

	protected virtual void OnNotCollideWith(RaycastHit2D other){}

	protected virtual void OnCollideOwner (){}

	protected virtual void OnCollideTakeDamage(RaycastHit2D other, ICanTakeDamage takedamage){}

	protected virtual void OnCollideOther(RaycastHit2D other){}
}
