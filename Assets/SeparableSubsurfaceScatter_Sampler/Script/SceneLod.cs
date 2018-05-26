using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLod : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.P)) {
			SceneManager.LoadScene ("Separable SSS");
		} else if (Input.GetKeyDown (KeyCode.O)) {
			SceneManager.LoadScene ("SkyLightSSS");
		} else if (Input.GetKeyDown (KeyCode.I)) {
			SceneManager.LoadScene ("SunOnly");
		}
	}
}
