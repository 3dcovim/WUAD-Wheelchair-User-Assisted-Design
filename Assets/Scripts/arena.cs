using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arena : MonoBehaviour {

	public GameObject sillaruedas;
	PlatformScriptWheelChair scriptA;
	void Start() {
		sillaruedas = GameObject.FindWithTag ("silla");
		scriptA = sillaruedas.GetComponent<PlatformScriptWheelChair> ();
	}
	void OnTriggerEnter (Collider other) {
		if (other.GetComponent<Usuario> () != null) {
			Debug.Log ("Arena pisada");
			scriptA.fFactor2 = 0.2f;
			scriptA.fFactor3 = -5;
		}
	}
}
