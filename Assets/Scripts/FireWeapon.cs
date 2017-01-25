using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class FireWeapon : MonoBehaviour {
	public GameObject bullet;
	public GameObject muzzleFlash;

	public void Fire() {
		Vector3 firePosition = transform.position + transform.up * 2f;
		GameObject.Instantiate(bullet, firePosition, Quaternion.LookRotation(transform.up, transform.right));
		GameObject.Instantiate (muzzleFlash, firePosition, transform.rotation);
	}
}
