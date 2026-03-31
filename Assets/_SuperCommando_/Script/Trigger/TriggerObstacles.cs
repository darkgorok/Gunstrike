using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerObstacles : TriggerEvent/*, IListener */{
	public GameObject[] targets;
	public bool disableTargetOnStart = true;
	List<MonoBehaviour> listMono;

	bool isWorked = false;
	//Vector3 originalPos;
	// Use this for initialization
	void Awake () {
		Init ();

		//originalPos = target.transform.position;
	}

	void Init(){

		listMono = new List<MonoBehaviour> ();
        foreach (var obj in targets)
        {
            if (obj != null)
            {
                MonoBehaviour[] monos = obj.GetComponents<MonoBehaviour>();
                foreach (var mono in monos)
                {
                    listMono.Add(mono);
                    mono.enabled = false;
                }

                obj.SetActive(!disableTargetOnStart);
            }
        }

		isWorked = false;
	}

    public override void OnContactPlayer()
    {
        base.OnContactPlayer();
        if (isWorked)
            return;
        foreach (var obj in targets)
        {
            if (obj != null)
            {
                obj.SetActive(true);

                foreach (var mono in listMono)
                {
                    mono.enabled = true;
                }
            }
        }
        isWorked = true;
    }

    void OnDrawGizmos()
    {
        if (targets.Length > 0)
        {
            foreach (var obj in targets)
            {
                if (obj)
                    Gizmos.DrawLine(transform.position, obj.transform.position);
                
            }
        }

        Gizmos.DrawCube(transform.position, new Vector2(0.5f, 5f));
    }
}
