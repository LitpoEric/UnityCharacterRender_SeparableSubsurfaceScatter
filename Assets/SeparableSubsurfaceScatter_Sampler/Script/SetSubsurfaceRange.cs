using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetSubsurfaceRange : MonoBehaviour {
	public Slider RangeObject;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetSubsurfaceRangeFunction () {
		gameObject.GetComponent<SeparableSubsurfaceScatter> ().SubsurfaceScaler = RangeObject.value;
	}
}
