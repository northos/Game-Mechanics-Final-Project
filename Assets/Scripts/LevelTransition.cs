using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelTransition : MonoBehaviour {

	public string levelName;

	// move to the next level when player enters trigger collider
	void OnTriggerEnter(Collider c){
		if (c.gameObject.tag == "Player"){
			SceneManager.LoadScene(levelName);
		}
	}
}
