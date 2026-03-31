using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(LineRenderer))]
public class LaserObstacle : MonoBehaviour {
	public enum Type {Normal, Reflect}
	public Type type;

	[Tooltip("For reflect laser")]
	public float laserLength = 30;
	public int numberReflect = 3;
    public GameObject laserContactFX;

	public Transform startPoint, endPoint;
	public LayerMask reflectLayer;
	public int damage = 100;
	public AudioClip soundKillPlayer;

    [Header("Draw line")]
    public bool drawLine = false;
    public float lineWidth = 0.2f;
    public int lineCorner = 90;
    public Material lineMat;
    public float offsetLineZ = -1;
    LineRenderer lineRen;

    List<GameObject> contactFXList;

    void Start () {
        lineRen = GetComponent<LineRenderer>();
        if (!lineRen)
            lineRen = gameObject.AddComponent<LineRenderer>();
        
        lineRen.useWorldSpace = true;
        lineRen.startWidth = lineWidth;
        lineRen.numCornerVertices = lineCorner;
        lineRen.material = lineMat;
        lineRen.textureMode = LineTextureMode.Tile;

        contactFXList = new List<GameObject>();
        for (int i = 0; i < numberReflect; i++)
        {
            contactFXList.Add(Instantiate(laserContactFX, transform.position, laserContactFX.transform.rotation));
        }
    }

	void Update () {
		if (type == Type.Normal) {
			RaycastHit2D hit = Physics2D.Linecast (startPoint.position, endPoint.position);
			if (hit) {
				UpdateLinePoint (1, hit.point);
				if (hit.collider.gameObject == GameManager.Instance.Player.gameObject) {
					GameManager.Instance.Player.TakeDamage (damage, Vector2.zero, gameObject, hit.point);
					SoundManager.PlaySfx (soundKillPlayer);
				}
			} else
				UpdateLinePoint (1, endPoint.position);
		} else if (type == Type.Reflect) {
			Vector2 _startPoint = startPoint.position;
			Vector2 _direction = startPoint.up;
			float _length = laserLength;

			for (int i = 0; i < numberReflect; i++) {

               

                RaycastHit2D hit = Physics2D.Raycast (_startPoint, _direction, _length,reflectLayer);
				if (hit) {
					if (hit.collider.gameObject == GameManager.Instance.Player.gameObject) {
						GameManager.Instance.Player.TakeDamage (damage, Vector2.zero, gameObject, hit.point);
						SoundManager.PlaySfx (soundKillPlayer);
					}

					UpdateLinePoint (i+1, hit.point);

					_direction = Vector3.Reflect (hit.point - _startPoint, hit.normal);
					_startPoint = hit.point + _direction * 0.1f;
					_length -= hit.distance;
				}else{
					UpdateLinePoint (i+1, _startPoint + _direction.normalized * _length);
                    contactFXList[i].SetActive(false);
                    break;
				}
			}
		}   
	}

	void UpdateLinePoint(int pos, Vector3 newPoint){
        lineRen.positionCount = pos + 1;
        lineRen.SetPosition (0, startPoint.position);
        lineRen.SetPosition (pos, newPoint);
        
        contactFXList[pos - 1].transform.position = newPoint;
    }

	void OnDrawGizmos() {
        if (Application.isPlaying)
            return;

        lineRen = GetComponent<LineRenderer>();

        if (drawLine)
        {
            if (!lineRen)
                lineRen = gameObject.AddComponent<LineRenderer>();
            
            lineRen.useWorldSpace = true;
            lineRen.startWidth = lineWidth;
            lineRen.material = lineMat;
            lineRen.numCornerVertices = lineCorner;
            lineRen.textureMode = LineTextureMode.Tile;

            lineRen.SetPosition(0, startPoint.position + Vector3.forward * offsetLineZ);
            lineRen.SetPosition(1, endPoint.position + Vector3.forward * offsetLineZ);

        }
        else if (lineRen)
            DestroyImmediate(lineRen);
    }
}
