using UnityEngine;
using System.Collections;

public class CloseGateBoss : MonoBehaviour {
    public enum MoveType { Up2Down, Down2Up}
    public AudioClip sound;
    public MoveType moveType;

	bool active = false;
	public Transform TheGate;
    public float topLocalPos = 3;
    public float bottomLocalPos = -3;
    Vector3 doorOriPos;
    bool openManual = false;    //mean the gate will be open by other script, call ActiveTheGate()
    // Use this for initialization
    void Start()
    {
        //doorOriPos = TheGate.position;
        //if (moveType == MoveType.Up2Down)
        //{
        //    TheGate.position = transform.position;
        //}
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.DrawSphere(new Vector2(TheGate.position.x, bottomLocalPos + transform.position.y), 0.2f);
            Gizmos.DrawSphere(new Vector2(TheGate.position.x, topLocalPos + transform.position.y), 0.2f);
            if (moveType == MoveType.Down2Up)
                TheGate.position = new Vector2(TheGate.position.x, bottomLocalPos + transform.position.y);
            else
                TheGate.position = new Vector2(TheGate.position.x, topLocalPos + transform.position.y);
        }
    }

    // Update is called once per frame
    void Update () {
        if (active)
        {
            TheGate.position = Vector2.MoveTowards(TheGate.transform.position, moveType == MoveType.Up2Down? (new Vector2(TheGate.position.x, bottomLocalPos + transform.position.y)) : (new Vector2(TheGate.position.x, topLocalPos + transform.position.y)), 0.1f);
        }
	}

    public void SetManual()
    {
        openManual = true;
    }

    public void ActiveTheGate()
    {
        if (active)
            return;

        active = true;
        SoundManager.PlaySfx(sound);
    }


    void OnTriggerEnter2D(Collider2D other){
        if (openManual)
            return;

        if (other.GetComponent<Player>() != null)
            ActiveTheGate();

    }
}
