using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLayerFollowPlayer : MonoBehaviour {
    List<SpriteRenderer> listSpriteRenderer;
    List<ParticleSystemRenderer> listParticle;
	// Use this for initialization
	void Start () {
        listSpriteRenderer = new List<SpriteRenderer>();
        listParticle = new List<ParticleSystemRenderer>();

        if (GetComponent<SpriteRenderer>())
            listSpriteRenderer.Add(GetComponent<SpriteRenderer>());
        if (GetComponent<ParticleSystemRenderer>())
            listParticle.Add(GetComponent<ParticleSystemRenderer>());

        var listSR = transform.GetComponentsInChildren<SpriteRenderer>();
        if (listSR.Length > 0)
        {
            foreach (var _temp in listSR)
            {
                listSpriteRenderer.Add(_temp);
            }
        }

        var listPS = transform.GetComponentsInChildren<ParticleSystemRenderer>();
        if (listPS.Length > 0)
        {
            foreach (var _temp in listPS)
            {
                listParticle.Add(_temp);
            }
        }


    }
	
	// Update is called once per frame
	void Update () {
        if (listSpriteRenderer.Count > 0)
        {
            foreach (var _SR in listSpriteRenderer)
            {
                _SR.sortingLayerName = LayerMask.LayerToName(GameManager.Instance.Player.gameObject.layer);
            }
        }

        if (listParticle.Count > 0)
        {
            foreach (var _PS in listParticle)
            {
                _PS.sortingLayerName = LayerMask.LayerToName(GameManager.Instance.Player.gameObject.layer);
            }
        }
    }
}
