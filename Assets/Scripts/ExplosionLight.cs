using UnityEngine;
using System.Collections;

public class ExplosionLight : MonoBehaviour {

	public float range;
	public float maxIntensity;
	public float halfLife;
	float startTime;
	Light lightComp;

	// set up initial parameters
	void Awake () {
		lightComp = GetComponent<Light> ();
		lightComp.range = range;
		lightComp.intensity = 0;
		startTime = Time.time;
	}
	
	// increase and decrease the intensity over its lifetime
	void Update () {
		lightComp.intensity = maxIntensity * Mathf.Abs(halfLife - Mathf.Abs (halfLife - (Time.time - startTime)));
		if (Time.time - startTime >= 2 * halfLife)
			Destroy (gameObject);
	}
}
