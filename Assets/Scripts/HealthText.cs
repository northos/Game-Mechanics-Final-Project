using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthText : MonoBehaviour {

	public GameObject player;
	public Text healthText;
	
	// constantly update text with player's health
	void Update () {
		healthText.text = "Health: " + player.GetComponent<Player> ().health + " / " + player.GetComponent<Player>().maxHealth;
	}
}
