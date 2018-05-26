using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotate : MonoBehaviour {
	//public Light Sun = null;
	// Use this for initialization
	public Material SkyBoxMAT = null;
	public float Rotate = 16.0f;
	public bool Sun_Sky_RotateHwo = true;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.U)) {
			if (Sun_Sky_RotateHwo == true) {
				gameObject.transform.Rotate (Vector3.up * 0.5f, Space.Self);
				Rotate -= 0.35f;
				SkyBoxMAT.SetFloat ("_Rotation", Rotate);
			} else {
				//SkyBoxMAT.SetFloat ("_Rotation", Rotate + 1);
				Rotate += 0.35f;
				//print("_Rotation+");
			}
		} else if (Input.GetKey (KeyCode.Y)) {
			if (Sun_Sky_RotateHwo == true) {
				gameObject.transform.Rotate (-Vector3.up * 0.5f, Space.Self);
				Rotate += 0.35f;
				SkyBoxMAT.SetFloat ("_Rotation", Rotate);
			}else {
				//SkyBoxMAT.SetFloat ("_Rotation", Rotate - 1);
				Rotate -= 0.35f;
				//print("_Rotation-");
			}
		}
		if (Sun_Sky_RotateHwo == false) {
			SkyBoxMAT.SetFloat ("_Rotation", Rotate);
		}
	}
}
