using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Boss : MonoBehaviour {

	public int health;
	public int maxHealth;
	public GameObject weapon;
	// attack range data
	public float detectRangeX;
	public float detectRangeZ;
	public float attackRange;
	public float minRange;
	bool detected = false;
	float detectTime;
	// movement data
	public float maxSpeed;
	public float chargeSpeed;
	float speed;
	float moveStart;
	float moveEnd;
	bool moving = false;
	Vector3 lastMove;
	public AnimationCurve acAttack;
	public AnimationCurve acRelease;
	// charger data
	public float chargeDelay;
	public float cooldownTime;
	public float chargeForce;
	float cooldownStart;
	bool charging = false;
	bool cooldown = false;
	// shooter data
	public float fireDelay;
	float lastFireTime;
	// bomber data
	public GameObject bomb;
	public float bombDelay;
	public float bombSpeed;
	float gravity = -30;
	bool throwing = false;
	// mode data
	enum Mode {Charger, Shooter, Bomber};
	Mode mode;
	Mode[] options = new Mode[3] {Mode.Charger, Mode.Shooter, Mode.Bomber};
	float switchTime;
	public float switchInterval;
	// objects to interact with
	public GameObject victoryText;
	GameObject player;
	// death data
	public float deathLength;	// including initial delay
	public float riseSpeed;
	public float deathDelay;
	float deathTime;
	bool dead = false;

	// perform initial setup
	void Start() {
		mode = options [Random.Range (0, 2)];
		switchTime = Time.time;
		GetComponent<Renderer> ().material.color = getColor ();
		weapon.GetComponent<Renderer> ().material.color = getColor ();
		player = GameObject.FindGameObjectWithTag ("Player");
	}

	// get the proper color for this based on health remaining, mode, and status within mode
	Color getColor(){
		if (mode == Mode.Shooter) {
			return new Color (0, (float)health / maxHealth, 0);
		} else if (mode == Mode.Charger) {
			if (detected) {
				return new Color (0, (Time.time - detectTime) / chargeDelay * (float)health / maxHealth, (float)health / maxHealth);
			}
			return new Color (0, 0, (float)health / maxHealth);
		} else {	// bomber mode
			if (throwing) {
				return new Color ((float)health / maxHealth, Mathf.Clamp(Time.time - detectTime, 0, bombDelay) / bombDelay * (float)health / maxHealth, 0);
			}
			return new Color ((float)health / maxHealth, 0, 0);
		}
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
		// exit game cuz you won!
		Application.Quit();
		yield return null;
	}

	// handle bullet and player collisions
	void OnCollisionEnter (Collision c){
		if (c.gameObject.tag == "Friendly bullet") {
			--health;
		} else if (mode == Mode.Charger && c.gameObject.tag == "Player") {
			cooldown = true;
			detected = false;
			charging = false;
			cooldownStart = Time.time;
			c.gameObject.GetComponent<Rigidbody> ().AddForce (transform.forward * chargeForce);
		}
		if (health <= 0) {
			dead = true;
			deathTime = Time.time;
			GetComponent<Collider> ().enabled = false;
			GetComponent<Rigidbody> ().useGravity = false;
			StartCoroutine (DeathCoroutine());
			victoryText.GetComponent<Text> ().enabled = true;
		}
	}

	// calculate bomb target and throw a bomb in that direction
	void throwBomb() {
		Vector3 throwPosition = transform.position + transform.forward * 3f;
		// check how far it is to player
		float targetDist = Vector3.Distance (throwPosition, player.transform.position);
		// determine how long it would take the bomb to travel that far (bombspeed is horizontal only)
		float targetTime = targetDist / bombSpeed;
		// predict where the player will be after flight time has elapsed (assuming traveling at max speed)
		Vector3 targetPos = player.transform.position + player.GetComponent<Player>().lastMove.normalized * player.GetComponent<Player> ().maxspeed * targetTime;
		// calculate an ititial force vector that would land the bomb at that position
		Vector3 direction = targetPos - throwPosition;
		float actualTime = direction.magnitude / bombSpeed;
		// choose velocity to hit the ground at target position (height -2)
		float upwardVelocity = (-2 -gravity * actualTime * actualTime / 2) / actualTime;
		Vector3 velocity = direction.normalized * bombSpeed;
		velocity.y = upwardVelocity;
		// make bomb and apply force
		GameObject newBomb = (GameObject)Instantiate (bomb, throwPosition, transform.rotation);
		newBomb.GetComponent<Rigidbody> ().velocity = velocity;
	}

	// move and perform attacks
	void Update() {
		if (dead)
			return;
		
		// update color as health drops
		GetComponent<Renderer> ().material.color = getColor();
		weapon.GetComponent<Renderer> ().material.color = getColor ();

		// if cooling down from attack, do no movement
		if (cooldown) {
			// once delay has elapsed, stop being stunned
			if (Time.time - cooldownStart >= cooldownTime) {
				cooldown = false;
			}
			return;
		}

		// change modes every few seconds if not in the middle of anything
		if (Time.time - switchTime >= switchInterval && !charging && !throwing) {
			mode = options [Random.Range (0, 3)];
			switchTime = Time.time;
			// update color again for new mode
			GetComponent<Renderer> ().material.color = getColor();
			weapon.GetComponent<Renderer> ().material.color = getColor ();
		}

		// movement
		float xDist = Mathf.Abs(player.transform.position.x - transform.position.x);
		float zDist = Mathf.Abs(player.transform.position.z - transform.position.z);
		float dist = Vector3.Distance (player.transform.position, transform.position);
		// if outside detection range, ignore player
		if (xDist >= detectRangeX || zDist >= detectRangeZ) {
			detected = false;
			return;
		} else if (!detected) {
			detected = true;
			detectTime = Time.time;
			charging = (mode == Mode.Charger);
			throwing = (mode == Mode.Bomber);
		}
		// otherwise turn to face player
		else if (player.transform.position - transform.position != Vector3.zero)
			transform.rotation = Quaternion.LookRotation (player.transform.position - transform.position);

		// charger movement
		// if charging, move at charge speed and finish
		if (mode == Mode.Charger && Time.time - detectTime >= chargeDelay) {
			Vector3 moveVector = transform.forward * chargeSpeed * Time.deltaTime;
			if (moveVector.magnitude > (player.transform.position - transform.position).magnitude) {
				moveVector = player.transform.position - transform.position;
			}
			transform.position += moveVector;
		}

		// bomber and shooter movement
		// when outside of range, activate movement and mark start time
		if (!moving && dist > attackRange) {
			moving = true;
			moveStart = Time.time;
		}
		// when closest range reached, deactivate movement and mark end time
		if (dist < minRange && moving) {
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
		if (mode == Mode.Shooter && dist < attackRange && Random.Range (0.5f, 1f) <= Mathf.Pow ((Time.time - lastFireTime) / fireDelay, 2)) {
			weapon.SendMessage ("Fire");
			lastFireTime = Time.time;
		}
		// or throw bomb
		else if (mode == Mode.Bomber && dist < attackRange && Time.time - detectTime >= bombDelay) {
			throwing = false;
			detected = false;
			cooldown = true;
			cooldownStart = Time.time;
			throwBomb ();
		}
	}
}
