using UnityEngine;
using System.Collections;

public class Bomb : MonoBehaviour {

	public GameObject explosionLight;
	public float impactMagnitude;
	public float impactRadius;
	GameObject player;
	float lifetime = 10f;
	float startTime;

	// find player object to damage
	void Awake() {
		player = GameObject.FindGameObjectWithTag ("Player");
		startTime = Time.time;
	}

	// just in case, destroy this if it lives longer than 10 seconds
	void update() {
		if (Time.time - startTime > lifetime)
			Destroy (gameObject);
	}
		
	// explode upon colliding with anything
	// applies damage and force to the player if in range, and creates a flash of life
	void OnCollisionEnter(Collision c) {
		// skip if colliding with an enemy (usually the thrower)
		if (c.gameObject.tag == "Enemy")
			return;
		// spawn light object at explosion site
		Instantiate (explosionLight, transform.position, transform.rotation);
		// exit if the player isn't in range
		float dist = Vector3.Distance(player.transform.position, transform.position);
		if (dist <= impactRadius) {
			player.GetComponent<Rigidbody> ().AddExplosionForce (impactMagnitude, transform.position, impactRadius);
			// apply damage based on how close to the explosion the player was
			// 2 damage if closer than half radius, 1 damage if between half and full radius
			player.SendMessage ("TakeDamage", Mathf.Ceil (2 * (1 - dist / impactRadius)));
		}
		Destroy (gameObject);
	}
}
