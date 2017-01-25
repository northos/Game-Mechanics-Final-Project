using UnityEngine;
using System.Collections;

public class Gate : MonoBehaviour {

	public GameObject rightGate;
	public GameObject leftGate;
	bool open = false;

	// function to open gate when key is collected
	public void Open() {
		if (!open) {
			rightGate.transform.Translate (new Vector3 (10, 0, 0));
			leftGate.transform.Translate (new Vector3 (-10, 0, 0));
			open = true;
		}
	}
}
