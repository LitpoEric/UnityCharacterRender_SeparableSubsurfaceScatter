using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetResolution : MonoBehaviour {
	// Use this for initialization
	void Start () {
		float X = Screen.width;
		float Y = Screen.height;
		gameObject.GetComponent<CanvasScaler> ().referenceResolution = new Vector2 (X, Y);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
