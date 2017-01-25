using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Collider))]

public class Charger : MonoBehaviour {

	public int health;
	int maxHealth = 3;
	public float moveSpeed;
	public float detectRangeX;
	public float detectRangeZ;
	public float chargeDelay;
	public float stunDelay;
	public float impactMagnitude;
	float detectTime;
	float stunnedTime;
	GameObject player;
	bool detected = false;
	bool stunned = false;
	public float deathLength;	// including initial delay
	public float riseSpeed;
	public float deathDelay;
	float deathTime;
	bool dead = false;

	// get the proper color for this based on health remaining and detection status
	// shades of blue for health indicator, added green when waiting to charge
	Color getColor(){
		if (detected) {
			return new Color (0, (Time.time - detectTime) / chargeDelay * (float)health / maxHealth, (float)health / maxHealth);
		}
		return new Color (0, 0, (float)health / maxHealth);
	}

	// find player object and set starting color based on helath
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
		GetComponent<Renderer> ().material.color = getColor();
	}

	// coroutine for death behavior
	IEnumerator DeathCoroutine() {
		// wait a bit, starting to lighten color
		while (Time.time - deathTime < deathDelay) {
			GetComponent<Renderer> ().material.color = new Color ((Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength);
			yield return null;
		}
		// slowly rise, still lightening color
		while (Time.time - deathTime < deathLength) {
			transform.position += new Vector3 (0, 1, 0) * riseSpeed;
			GetComponent<Renderer> ().material.color = new Color ((Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength);
			yield return null;
		}
		Destroy (gameObject);
		yield return null;
	}

	// handle bullet and player collisions
	void OnCollisionEnter (Collision c){
		if (c.gameObject.tag == "Friendly bullet") {
			--health;
		} else if (c.gameObject.tag == "Player") {
			stunned = true;
			detected = false;
			stunnedTime = Time.time;
			c.gameObject.GetComponent<Rigidbody> ().AddForce (transform.forward * impactMagnitude);
		}
		if (health <= 0) {
			dead = true;
			deathTime = Time.time;
			GetComponent<Collider> ().enabled = false;
			GetComponent<Rigidbody> ().useGravity = false;
			StartCoroutine (DeathCoroutine());
		}
	}

	// update color and perform movement/attack as appropriate
	void Update () {
		if (dead)
			return;
		// darken color as health drops
		GetComponent<Renderer> ().material.color = getColor();
		// if stunned, do no movement
		if (stunned) {
			// once delay has elapsed, stop being stunned
			if (Time.time - stunnedTime >= stunDelay) {
				stunned = false;
			}
			return;
		}
		float xDist = Mathf.Abs(player.transform.position.x - transform.position.x);
		float zDist = Mathf.Abs(player.transform.position.z - transform.position.z);
		Vector3 rayPoint1 = transform.position + transform.up * 3;
		Vector3 rayPoint2 = player.transform.position + player.transform.up * 3;
		float dist = Vector3.Distance (rayPoint1, rayPoint2);
		Ray ray = new Ray (rayPoint1, rayPoint2 - rayPoint1);
		// if outside detection range or blocked, ignore player
		if (xDist >= detectRangeX || zDist >= detectRangeZ || Physics.Raycast(ray, dist - 2)) {
			detected = false;
		}
		// if in range, change to pursue mode
		else if (!detected) {
			detected = true;
			detectTime = Time.time;
		}
		// while in pursue mode, keep facing player, wait for a time, then charge.
		if (detected){
			if (player.transform.position - transform.position != Vector3.zero)
				transform.rotation = Quaternion.LookRotation (player.transform.position - transform.position);
			// once delay has passed, charge player as long as this is visible
			if (Time.time - detectTime >= chargeDelay) {
				Vector3 moveVector = transform.forward * moveSpeed * Time.deltaTime;
				if (moveVector.magnitude > (player.transform.position - transform.position).magnitude) {
					moveVector = player.transform.position - transform.position;
				}
				transform.position += moveVector;
			}
		}
	}
}
