using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hierba : MonoBehaviour {

	public GameObject sillaruedas;
	PlatformScriptWheelChair scriptA;
	void Start() {
		sillaruedas = GameObject.FindWithTag ("silla");
		scriptA = sillaruedas.GetComponent<PlatformScriptWheelChair> ();
}
	void OnTriggerEnter (Collider other) {
		if (other.GetComponent<Usuario> () != null) {
			Debug.Log ("Hierba pisada");
			scriptA.fFactor2 = 1;
			scriptA.fFactor3 = -1;
		}
	}
}