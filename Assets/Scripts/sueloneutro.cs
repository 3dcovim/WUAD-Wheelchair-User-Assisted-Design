using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sueloneutro : MonoBehaviour {

	public GameObject sillaruedas;
	PlatformScriptWheelChair scriptA;
	void Start() {
		sillaruedas = GameObject.FindWithTag ("silla");
		scriptA = sillaruedas.GetComponent<PlatformScriptWheelChair> ();
	}
	void OnTriggerEnter (Collider other) {
		if (other.GetComponent<Usuario> () != null) {
			Debug.Log ("Suelo pisado");
			scriptA.fFactor2 = 10;
			scriptA.fFactor3 = 0;
		}
	}
}

