using UnityEngine;
using System.Collections;

public class WheelChair : MonoBehaviour {

	public float rpm=0.0f;
	public float difRuedas = 0;
	public float torque = 1000000000.0f;
	public HingeJoint ruedaIzq;
	public HingeJoint ruedaDer;

	// Update is called once per frame
	void Update () {
		if (ruedaDer != null && ruedaIzq != null) 
		{
			JointMotor m = new JointMotor ();
			m.force=torque;
			m.targetVelocity = rpm +(rpm*difRuedas);
			ruedaIzq.motor=m;
			m.targetVelocity = rpm -(rpm*difRuedas);
			ruedaDer.motor=m;
		}
	}
}
