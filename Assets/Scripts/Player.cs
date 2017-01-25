using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

[RequireComponent (typeof(Rigidbody))]

public class Player : MonoBehaviour {
	// movement parameters
	public float maxspeed;
	public AnimationCurve acAttack;
	public AnimationCurve acRelease;
	public float animationLength;
	bool moving = false;
	float startTime;
	float endTime;
	float speed;
	public Vector3 lastMove;
	public GameObject cameraRig;
	public GameObject weapon;
	public GameObject diedText;
	public float cameraMaxDistance;
	public float cameraMinDistance;
	public float lerpFactor;
	bool following = false;

	public int health;
	public int maxHealth;

	public float deathLength;	// including initial delay
	public float riseSpeed;
	public float deathDelay;
	float deathTime;
	bool dead = false;

	// jumping parameters
	public float jumpForce;
	bool jumping = false;
	bool inAir = false;

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
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		yield return null;
	}

	// handle collisions for damage and landing from jumps
	void OnCollisionEnter (Collision c){
		if (c.gameObject.tag == "Enemy bullet" || c.gameObject.tag == "Enemy") {
			TakeDamage (1);
		} else if (c.gameObject.tag == "Floor") {
			inAir = false;
		}
	}

	// public method for other objects (bombs) to apply damage
	public void TakeDamage(int damage) {
		health -= damage;
		if (health <= 0) {
			dead = true;
			deathTime = Time.time;
			GetComponent<Rigidbody> ().useGravity = false;
			GetComponent<Collider> ().enabled = false;
			StartCoroutine (DeathCoroutine());
			diedText.GetComponent<Text> ().enabled = true;
		}
	}

	// move this object in the XZ plane based on WASD control input
	// also jump
	void Update () {
		if (dead)
			return;
		/**
		 * WASD movement in XZ plane
		 */
		// when input is given, activate movement and mark start time
		if (!moving && (Mathf.Abs(CrossPlatformInputManager.GetAxis ("Horizontal")) == 1f || Mathf.Abs(CrossPlatformInputManager.GetAxis ("Vertical")) == 1f)) {
			moving = true;
			startTime = Time.time;
			// check if the release curve is still active; if so, backtrack the starting point for the attack to match
			if (startTime - endTime < animationLength) {
				startTime -= (animationLength - (startTime - endTime));
			}
		}
		// when input ends, deactivate movement and mark end time
		if ((CrossPlatformInputManager.GetAxis ("Horizontal") == 0f && CrossPlatformInputManager.GetAxis ("Vertical") == 0f) && moving) {
			moving = false;
			endTime = Time.time;
		}
		// move if input is active
		if (moving){
			// use attack curve to set speed for a period after activating
			if (Time.time - startTime <= animationLength) {
				speed = acAttack.Evaluate (Time.time - startTime) * maxspeed;
			}
			// then move based on input and speed (speed will stay at max once the attack has finished)
			lastMove = new Vector3(CrossPlatformInputManager.GetAxis("Horizontal"), 0, CrossPlatformInputManager.GetAxis("Vertical"));
			transform.position += lastMove.normalized * speed * Time.deltaTime;
		}
		// decay movement (or don't move) if input is inactive
		if (!moving) {
			// use release curve to set speed for a period after deactivating
			if (Time.time - endTime <= animationLength) {
				speed = acRelease.Evaluate (Time.time - endTime) * maxspeed;
			} else {
				speed = 0f;
				lastMove = Vector3.zero;
			}
			// then move based on the decayed speed and the last movement direction
			transform.position += lastMove.normalized * speed * Time.deltaTime;
		}
		// move camera closer if player is too far away
		if (Vector3.Distance (cameraRig.transform.position, transform.position) > cameraMaxDistance) {
			following = true;
		} else if (Vector3.Distance (cameraRig.transform.position, transform.position) < cameraMinDistance) {
			following = false;
		}
		if (following) {
			cameraRig.transform.position = Vector3.Lerp (cameraRig.transform.position, transform.position, lerpFactor);
		}

		/**
		 * Jumping
		 */
		// check for jump key input
		if (CrossPlatformInputManager.GetAxis ("Jump") == 1f && !jumping && !inAir) {
			// set up jump initial conditions
			jumping = true;
			inAir = true;
			GetComponent<Rigidbody> ().AddForce (new Vector3 (0, jumpForce, 0));
		}
		// release jump when button released
		else if (CrossPlatformInputManager.GetAxis ("Jump") != 1f) {
			jumping = false;
		}

		/**
		 * Aiming
		 */
		// rotate player towards mouse position
		Ray lookRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (lookRay, out hit)) {
			Vector3 lookTarget = hit.point;
			lookTarget.y = transform.position.y;
			transform.rotation = Quaternion.LookRotation (lookTarget - transform.position, transform.up);
		}

		/**
		 * Weapon firing
		 */
		if (CrossPlatformInputManager.GetButtonDown("Fire1")) {
			weapon.SendMessage ("Fire");
			Vector3 firePosition = weapon.transform.position + weapon.transform.up * 2f;
			GetComponent<Rigidbody> ().AddExplosionForce (300f, firePosition, 10f);
		}

		// darken color as health drops
		GetComponent<Renderer> ().material.color = new Color ((float)health / maxHealth, 0, (float)health / maxHealth);
		weapon.GetComponent<Renderer> ().material.color = new Color ((float)health / maxHealth, 0, (float)health / maxHealth);
	}
}
