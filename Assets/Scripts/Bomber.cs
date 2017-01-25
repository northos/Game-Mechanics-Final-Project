using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Collider))]

public class Bomber : MonoBehaviour {

	public int health;
	int maxHealth = 3;
	public float maxSpeed;
	float speed;
	Vector3 lastMove;
	public AnimationCurve acAttack;
	public AnimationCurve acRelease;
	public float detectRangeX;
	public float detectRangeZ;
	public float fireRange;
	public float minRange;
	public float prepRange;
	public float bombDelay;
	public float bombCooldown;
	float throwStart;
	float cooldownStart;
	float moveStart;
	float moveEnd;
	GameObject player;
	public GameObject bomb;
	public float bombSpeed;
	float gravity = -30;
	bool throwing = false;
	bool moving = false;
	bool cooldown = false;
	public float deathLength;	// including initial delay
	public float riseSpeed;
	public float deathDelay;
	float deathTime;
	bool dead = false;

	// get the proper color for this based on health remaining and detection status
	// shades of red for health indicator, added green (for yellow) when waiting to charge
	Color getColor(){
		if (throwing) {
			return new Color ((float)health / maxHealth, Mathf.Clamp(Time.time - throwStart, 0, bombDelay) / bombDelay * (float)health / maxHealth, 0);
		}
		return new Color ((float)health / maxHealth, 0, 0);
	}

	// calculate bomb target and throw a bomb in that direction
	void throwBomb() {
		// check how far it is to player
		float targetDist = Vector3.Distance (transform.position, player.transform.position);
		// determine how long it would take the bomb to travel that far (bombspeed is horizontal only)
		float targetTime = targetDist / bombSpeed;
		// predict where the player will be after flight time has elapsed (assuming traveling at max speed)
		Vector3 targetPos = player.transform.position + player.GetComponent<Player>().lastMove.normalized * player.GetComponent<Player> ().maxspeed * targetTime;
		// calculate an ititial force vector that would land the bomb at that position
		Vector3 direction = targetPos - transform.position;
		float actualTime = direction.magnitude / bombSpeed;
		// choose velocity to hit the ground at target position (height -2)
		float upwardVelocity = (-2 -gravity * actualTime * actualTime / 2) / actualTime;
		Vector3 velocity = direction.normalized * bombSpeed;
		velocity.y = upwardVelocity;
		// make bomb and apply force
		GameObject newBomb = (GameObject)Instantiate (bomb, transform.position + transform.up * 2f, transform.rotation);
		newBomb.GetComponent<Rigidbody> ().velocity = velocity;
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
		// darken color as health drops
		GetComponent<Renderer> ().material.color = getColor();
		// check on cooldown
		if (cooldown && Time.time - cooldownStart >= bombCooldown) {
			cooldown = false;
		}
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
		// when input is given, activate movement and mark start time
		if (!moving && dist > fireRange) {
			moving = true;
			moveStart = Time.time;
		}
		// when input ends, deactivate movement and mark end time
		if (dist < minRange && moving) {
			moving = false;
			moveEnd = Time.time;
		}
		// move if input is active
		if (moving){
			// use attack curve to set speed for 1 second after activating
			if (Time.time - moveStart <= 0.2f) {
				speed = acAttack.Evaluate (Time.time - moveStart) * maxSpeed;
			}
			// then move based on input and speed (speed will stay at max once the attack has finished)
			lastMove = player.transform.position - transform.position;
			transform.position += lastMove.normalized * speed * Time.deltaTime;
		}
		// decay movement (or don't move) if input is inactive
		if (!moving) {
			// use release curve to set speed for 1 second after deactivating
			if (Time.time - moveEnd <= 0.2f) {
				speed = acRelease.Evaluate (Time.time - moveEnd) * maxSpeed;
			} else {
				speed = 0f;
			}
			// then move based on the decayed speed and the last movement direction
			transform.position += lastMove.normalized * speed * Time.deltaTime;
		}
		// if on cooldown, skip throwing
		if (cooldown) {
			if (Time.time - cooldownStart >= bombCooldown)
				cooldown = false;
			return;
		}
		// start throwing a bomb if within outer range
		if (dist <= prepRange && !throwing) {
			throwing = true;
			throwStart = Time.time;
		}
		// throw bomb if within inner range and throw is completed
		if (dist <= fireRange && throwing && Time.time - throwStart >= bombDelay) {
			throwBomb ();
			throwing = false;
			cooldown = true;
			cooldownStart = Time.time;
		}
	}
}
