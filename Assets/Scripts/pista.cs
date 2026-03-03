using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pista : MonoBehaviour {

	public GameObject sillaruedas;
	PlatformScriptWheelChair scriptA;
	void Start() {
		sillaruedas = GameObject.FindWithTag ("silla");
		scriptA = sillaruedas.GetComponent<PlatformScriptWheelChair> ();
	}
	void OnTriggerEnter (Collider other) {
		if (other.GetComponent<Usuario> () != null) {
			Debug.Log ("Pista pisada");
			scriptA.fFactor2 = 100;
			scriptA.fFactor3 = 5;
		}
	}
}
