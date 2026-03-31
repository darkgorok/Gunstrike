using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLineHelper : MonoBehaviour {

	[Tooltip("If has 2 or more, use them instead localWaypoints")]
	public GameObject Paths;
	public List<Transform> objectLocalWaypoints;
	public List<Vector3> localWaypoints;
	Vector3[] globalWaypoints;

	[Header("Draw line")]
	public bool drawLine = false;
	public float lineWidth = 0.2f;
	public Material lineMat;
	LineRenderer lineRen;

	public bool closeLine = false;
	public bool updateOnPlay = false;
	public void Start () {

		if (Paths) {
			int childs = Paths.transform.childCount;
			objectLocalWaypoints.Clear ();
			if (childs > 0) {
				for (int i = 0; i < childs; i++) {
					if (Paths.transform.GetChild (i).gameObject.activeInHierarchy)
						objectLocalWaypoints.Add (Paths.transform.GetChild (i));
				}
			}
		}

		if (objectLocalWaypoints.Count >= 2) {
			localWaypoints.Clear ();
			for (int i = 0; i < objectLocalWaypoints.Count; i++) {
				localWaypoints.Add (objectLocalWaypoints [i].localPosition);
			}
		}

		globalWaypoints = new Vector3[localWaypoints.Count];
		for (int i =0; i < localWaypoints.Count; i++) {
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}

		lineRen = GetComponent<LineRenderer> ();

		if (!updateOnPlay)
			Destroy (this);
	}

	void Update(){
		if (objectLocalWaypoints.Count >= 2) {
			localWaypoints.Clear ();
			for (int i = 0; i < objectLocalWaypoints.Count; i++) {
				if (objectLocalWaypoints [i] == null || !objectLocalWaypoints [i].gameObject.activeInHierarchy)
					;
//					objectLocalWaypoints.RemoveAt (i);
				else
					localWaypoints.Add (objectLocalWaypoints [i].position);
			}
		}

		if (localWaypoints.Count > 1) {
			lineRen.positionCount = localWaypoints.Count;
			for (int i = 0; i < localWaypoints.Count; i++)
				lineRen.SetPosition (i, localWaypoints [i] - Vector3.forward * 0.1f);
		} else
			lineRen.positionCount = 0;
	}


	void OnDrawGizmos() {
		if (Paths) {
			int childs = Paths.transform.childCount;
			objectLocalWaypoints.Clear ();
			if (childs > 0) {
				for (int i = 0; i < childs; i++) {
					if (Paths.transform.GetChild (i).gameObject.activeInHierarchy)
						objectLocalWaypoints.Add (Paths.transform.GetChild (i));
				}
			}
		}

		if (objectLocalWaypoints.Count >= 2) {
			localWaypoints.Clear ();
			for (int i = 0; i < objectLocalWaypoints.Count; i++) {
				if (objectLocalWaypoints [i] == null || !objectLocalWaypoints [i].gameObject.activeInHierarchy)
					;
//					objectLocalWaypoints.RemoveAt (i);
				else
					localWaypoints.Add (objectLocalWaypoints [i].position);
			}
		}

		if (!Application.isPlaying || updateOnPlay) {
			lineRen = GetComponent<LineRenderer> ();

			if (drawLine) {
				if (!lineRen)
					lineRen = gameObject.AddComponent<LineRenderer> ();

				lineRen.positionCount = localWaypoints.Count;
				lineRen.useWorldSpace = true;
				lineRen.startWidth = lineWidth;
				lineRen.material = lineMat;
				lineRen.textureMode = LineTextureMode.Tile;

				for (int i = 0; i < localWaypoints.Count; i++)
					lineRen.SetPosition (i, localWaypoints [i] - Vector3.forward * 0.1f);

			} else if (lineRen)
				DestroyImmediate (lineRen);
		}

		//if (localWaypoints != null && this.enabled) {
		//	for (int i =0; i < localWaypoints.Count; i ++) {
		//		Gizmos.color = Color.red;
		//		float size = .3f;
		//		Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i] : localWaypoints[i] + transform.position;
		//		//				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
		//		//				Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
		//		if ((i + 1 >= localWaypoints.Count) && closeLine)
		//			Gizmos.DrawWireCube (globalWaypointPos,Vector3.one * 0.5f);
		//		else
		//			Gizmos.DrawWireSphere (globalWaypointPos, size);

		//		if (i + 1 >= localWaypoints.Count) {
		//			if (closeLine) {
		//				Gizmos.color = Color.yellow;
		//				if (Application.isPlaying)
		//					Gizmos.DrawLine (globalWaypoints [i], globalWaypoints [0]);
		//				else
		//					Gizmos.DrawLine (localWaypoints [i] + transform.position, localWaypoints [0] + transform.position);
		//			}
		//			return;
		//		}

		//		Gizmos.color = Color.green;
		//		if (Application.isPlaying)
		//			Gizmos.DrawLine (globalWaypoints [i], globalWaypoints [i + 1]);
		//		else
		//			Gizmos.DrawLine (localWaypoints [i] + transform.position, localWaypoints [i + 1] + transform.position);
		//	}
		//}
	}
}
