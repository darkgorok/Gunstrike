using UnityEngine;
using System.Collections;

public interface ICanTakeDamage {

	//void TakeDamage (float damage, Vector2 force, GameObject instigator);

    void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint);
}
