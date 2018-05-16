using UnityEngine;
using System.Collections;

public class CameraRotation : MonoBehaviour {
	[SerializeField] SensorRotation sensRotation;
	// Use this for initialization
	void Start(){
		sensRotation = FindObjectOfType<SensorRotation> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (sensRotation != null)
		{
			gameObject.transform.localRotation = sensRotation.Rotation;
		}
	}
}
