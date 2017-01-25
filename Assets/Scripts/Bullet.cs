using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
	public float bulletSpeed;
	public float destroyDistance;
	public float destroyTime;
	float startTime;
	Vector3 startPosition;

	// record time and place of creation
	void Start() {
		startTime = Time.time;
		startPosition = transform.position;
	}

	// destroy bullet when it collides with anything except another bullet
	void OnCollisionEnter(Collision c){
		if (c.gameObject.tag != "Friendly bullet" && c.gameObject.tag != "Enemy bullet") {
			Destroy (gameObject);
		}
	}

	// move bullet and destroy it after moving too far or after a period of time
	void Update () {
		transform.position += transform.forward * bulletSpeed * Time.deltaTime;
		if (Vector3.Distance (transform.position, startPosition) > destroyDistance || Time.time - startTime > destroyTime) {
			Destroy (gameObject);
		}
	}
}
