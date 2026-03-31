using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DarkRoomTrigger : MonoBehaviour {
	public enum Type{ToTransparent, ToFill

    }
	public Type type;
	public bool overPlayer = false;

	SpriteRenderer sprite;
    Tilemap tileMap;
    
	Color oriColor;
	// Use this for initialization
	void Start () {
		sprite = GetComponent<SpriteRenderer> ();
        tileMap = GetComponent<Tilemap>();

        oriColor = Color.white;

		if (type == Type.ToTransparent)
			oriColor.a = 1;
		else
			oriColor.a = 0;

        if (sprite)
            sprite.color = oriColor;
        if (tileMap)
            tileMap.color = oriColor;

        if (overPlayer) {
            //			Debug.LogWarning (sprite.sortingLayerID +"/" +SortingLayer.GetLayerValueFromName ("Front"));
            if (sprite)
            {
                sprite.sortingLayerName = "Top";
                sprite.sortingOrder = -10;
            }
        } else {
            if (sprite)
            {
                sprite.sortingOrder = -10;
            }
        }
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.gameObject.layer == LayerMask.NameToLayer ("Player") || (other.gameObject.layer == LayerMask.NameToLayer ("IgnoreAll"))) {
			if (type == Type.ToTransparent)
				oriColor.a = 0;
			else
				oriColor.a = 1;
            if (sprite)
            {
                StopAllCoroutines();
                StartCoroutine(MMFade.FadeSpriteRenderer(sprite, 0.5f, oriColor));
            }
            if (tileMap)
            {
                StopAllCoroutines();
                StartCoroutine(MMFade.FadeTileMapRenderer(tileMap, 0.5f, oriColor));
            }
        }
	}


	void OnTriggerExit2D(Collider2D other){
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") || (other.gameObject.layer == LayerMask.NameToLayer("IgnoreAll")))
        {
            if (type == Type.ToTransparent)
                oriColor.a = 1;
            else
                oriColor.a = 0;
            if (sprite)
            {
                StopAllCoroutines();
                StartCoroutine(MMFade.FadeSpriteRenderer(sprite, 0.5f, oriColor));
            }
            if (tileMap)
            {
                StopAllCoroutines();
                StartCoroutine(MMFade.FadeTileMapRenderer(tileMap, 0.5f, oriColor));
            }
        }
	}
}
