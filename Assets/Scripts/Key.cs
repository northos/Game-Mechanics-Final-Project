using UnityEngine;
using System.Collections;

public class Key : MonoBehaviour {

	public float rotateSpeed;
	public GameObject gate;

	// rotate about the X axis at a fixed speed each frame
	void Update () {
		transform.Rotate (new Vector3 (rotateSpeed * Time.deltaTime, 0, 0));
	}

	// when player collides, open gate
	void OnCollisionEnter (Collision c){
		if (c.gameObject.tag == "Player") {
			gate.SendMessage ("Open");
			Destroy (gameObject);
		}
	}
}
