using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Collider))]

public class Shooter : MonoBehaviour {

	public int health;
	int maxHealth = 3;
	public float maxSpeed;
	float speed;
	float moveStart;
	float moveEnd;
	Vector3 lastMove;
	public AnimationCurve acAttack;
	public AnimationCurve acRelease;
	public float detectRangeX;
	public float detectRangeZ;
	public float maxFireRange;
	public float minFireRange;
	public float fireDelay;
	float lastFireTime;
	GameObject player;
	public GameObject weapon;
	bool moving = false;
	public float deathLength;	// including initial delay
	public float riseSpeed;
	public float deathDelay;
	float deathTime;
	bool dead = false;

	// find player object and set starting color based on helath
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player");
		GetComponent<Renderer> ().material.color = new Color (0, (float)health / maxHealth, 0);
		weapon.GetComponent<Renderer> ().material.color = new Color (0, (float)health / maxHealth, 0);
	}

	// coroutine for death behavior
	IEnumerator DeathCoroutine() {
		// wait a bit, starting to lighten color
		while (Time.time - deathTime < deathDelay) {
			GetComponent<Renderer> ().material.color = new Color ((Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength);
			weapon.GetComponent<Renderer> ().material.color = new Color ((Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength);
			yield return null;
		}
		// slowly rise, still lightening color
		while (Time.time - deathTime < deathLength) {
			transform.position += new Vector3 (0, 1, 0) * riseSpeed;
			GetComponent<Renderer> ().material.color = new Color ((Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength);
			weapon.GetComponent<Renderer> ().material.color = new Color ((Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength, (Time.time - deathTime) / deathLength);
			yield return null;
		}
		Destroy (gameObject);
		yield return null;
	}

	// handle bullet collisions
	void OnCollisionEnter (Collision c){
		if (c.gameObject.tag == "Friendly bullet") {
			--health;
		}
		if (health <= 0) {
			dead = true;
			deathTime = Time.time;
			GetComponent<Collider> ().enabled = false;
			GetComponent<Rigidbody> ().useGravity = false;
			StartCoroutine (DeathCoroutine());
		}
	}

	void Update () {
		if (dead)
			return;
		float xDist = Mathf.Abs(player.transform.position.x - transform.position.x);
		float zDist = Mathf.Abs(player.transform.position.z - transform.position.z);
		Vector3 rayPoint1 = transform.position + transform.up * 2;
		Vector3 rayPoint2 = player.transform.position + player.transform.up * 2;
		float dist = Vector3.Distance (rayPoint1, rayPoint2);
		Ray ray = new Ray (rayPoint1, rayPoint2 - rayPoint1);
		// if outside detection range or blocked, ignore player
		if (xDist >= detectRangeX || zDist >= detectRangeZ || Physics.Raycast(ray, dist - 2)) {
			return;
		}
		// otherwise turn to face player
		else if (player.transform.position - transform.position != Vector3.zero)
			transform.rotation = Quaternion.LookRotation (player.transform.position - transform.position);
		// when input is given, activate movement and mark start time
		if (!moving && dist > maxFireRange) {
			moving = true;
			moveStart = Time.time;
		}
		// when input ends, deactivate movement and mark end time
		if (dist < minFireRange && moving) {
			moving = false;
			moveEnd = Time.time;
		}
		// move if input is active
		if (moving){
			// use attack curve to set speed for a period after activating
			if (Time.time - moveStart <= 0.2f) {
				speed = acAttack.Evaluate (Time.time - moveStart) * maxSpeed;
			}
			// then move based on input and speed (speed will stay at max once the attack has finished)
			lastMove = player.transform.position - transform.position;
			transform.position += lastMove.normalized * speed * Time.deltaTime;
		}
		// decay movement (or don't move) if input is inactive
		if (!moving) {
			// use release curve to set speed for a period after deactivating
			if (Time.time - moveEnd <= 0.2f) {
				speed = acRelease.Evaluate (Time.time - moveEnd) * maxSpeed;
			} else {
				speed = 0f;
			}
			// then move based on the decayed speed and the last movement direction
			transform.position += lastMove.normalized * speed * Time.deltaTime;
		}
		// if in range, fire
		// interval between firing can be anywhere from a quarter of the chosen delay up to the full delay, getting more likely as time passes
		if (dist < maxFireRange && Random.Range(0.5f, 1f) <= Mathf.Pow((Time.time - lastFireTime) / fireDelay, 2)) {
			weapon.SendMessage ("Fire");
			lastFireTime = Time.time;
		}

		// darken color as health drops
		GetComponent<Renderer> ().material.color = new Color (0, (float)health / maxHealth, 0);
		weapon.GetComponent<Renderer> ().material.color = new Color (0, (float)health / maxHealth, 0);
	}
}
