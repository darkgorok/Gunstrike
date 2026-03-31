using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckleController : MonoBehaviour {
	public Transform firstPlatform, secondPlatform;
	public Transform limitLeftPoint, limitRightPoint;
	public float speedHasPlayer = 2;
	public float speedNoPlayer = 1;

	[ReadOnlyAttribute]
	public Vector3 originalLeftPoint, originalRightPoint;

	[ReadOnlyAttribute]
	public bool isPlayerStand = false;
	[ReadOnlyAttribute]
	public bool isPlayerStandOnRight = false;
	[ReadOnlyAttribute]
	public bool isLimited = false;

	// Use this for initialization
	void Start () {
		originalLeftPoint = firstPlatform.position;
		originalRightPoint = secondPlatform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (isLimited)
			return;
		
		if (isPlayerStand) {
            if (GameManager.Instance.Player.isGrounded || isObjectStandingOn)
            {
                if (isPlayerStandOnRight)
                {
                    float speed = speedHasPlayer * Time.deltaTime;

                    firstPlatform.Translate(0, speed, 0);
                    secondPlatform.Translate(0, -speed, 0);

                    if (firstPlatform.transform.position.y >= limitLeftPoint.position.y)
                        isLimited = true;

                }
                else
                {
                    float speed = speedHasPlayer * Time.deltaTime;

                    firstPlatform.Translate(0, -speed, 0);
                    secondPlatform.Translate(0, speed, 0);

                    if (secondPlatform.transform.position.y >= limitRightPoint.position.y)
                        isLimited = true;
                }
            }
		} else {
			if (firstPlatform.position.y > originalLeftPoint.y) {

				
				float speed = speedNoPlayer * Time.deltaTime;
				firstPlatform.Translate (0, -speed, 0);
				secondPlatform.Translate (0, speed, 0);

				if (firstPlatform.transform.position.y <= originalLeftPoint.y)
					isLimited = true;
			} else if(firstPlatform.position.y < originalLeftPoint.y){


				float speed = speedNoPlayer * Time.deltaTime;
				firstPlatform.Translate (0, speed, 0);
				secondPlatform.Translate (0, -speed, 0);

				if (firstPlatform.transform.position.y >= originalLeftPoint.y)
					isLimited = true;
			}
		}
	}

    bool isObjectStandingOn = false;
	public void PlayerStandOn(bool isRight, bool isObjectStand){
        if (isPlayerStand)
            return;

        isObjectStandingOn = isObjectStand;
        isPlayerStand = true;
		isPlayerStandOnRight = isRight;
		isLimited = false;
	}

	public void PlayerLeave(){
		isPlayerStand = false;
		isLimited = false;
        isObjectStandingOn = false;

    }
}
