using UnityEngine;
using System.Collections;

public class KillZone : MonoBehaviour {

	// kill any object that falls into this
	void OnTriggerEnter(Collider c) {
		// use player's damage function so scene reloads
		if (c.gameObject.tag == "Player") {
			c.gameObject.SendMessage ("TakeDamage", 20);
		} else {
			Destroy (c.gameObject);
		}
	}
}
