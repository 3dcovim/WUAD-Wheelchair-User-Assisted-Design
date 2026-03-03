using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

public class Vec3
{
	public float x = 0.0f;
	public float y = 0.0f;
	public float z = 0.0f;
}

public class DatosPlataforma
{
	public float[] _specForce;   // Fuerza especifica local (en realidad es un tipo de aceleracion) (m/(s*s))
	public float[] _acc;         // Aceleracion lineal local (m/(s*s))
	public float[] _speed;       // Velocidad lineal local (m/s)
	public float[] _xyz;         // Posicion absoluta (m)
	public float[] _angAcc;      // Aceleracion angular local (grados/(s*s))
	public float[] _angSpeed;    // Velocidad angular local (grados/s)
	public float[] _hpr;         // Orientacion absoluta con angulos HPR (grados)
};

public enum RightAxis
{
	X,
	Y,
	Z,
	MINUS_X,
	MINUS_Y,
	MINUS_Z,
};

public enum ForwardAxis
{
	X,
	Y,
	Z,
	MINUS_X,
	MINUS_Y,
	MINUS_Z,
};

public enum UpAxis
{
	X,
	Y,
	Z,
	MINUS_X,
	MINUS_Y,
	MINUS_Z,
};

//PlatformScript SIN HOST
public class PlatformScriptWheelChair : MonoBehaviour
{
	public HingeJoint _jointLeftWheel;
	public HingeJoint _jointRightWheel;

	public string _platformConfigFile = "./cfg/PlatformConfig.xml";
	public string _wheelChairConfigFile = "./cfg/WheelChairConfig.xml";

	private int _pauseKeyCodeDIK = 0x1F;
	public KeyCode _pauseKeyCode = KeyCode.S;
	Dictionary<KeyCode, int> _keyCodesDictionary = new Dictionary<KeyCode, int>();

	public RightAxis _RightAxis = RightAxis.X;
	public float rightLinearScale = 1.0f;
	public float rightAngularScale = 1.0f;

	public ForwardAxis _ForwardAxis = ForwardAxis.Y;
	public float forwardLinearScale = 1.0f;
	public float forwardAngularScale = 1.0f;

	public UpAxis _UpAxis = UpAxis.Z;
	public float upLinearScale = 1.0f;
	public float upAngularScale = 1.0f;

	public bool _writeAllToLog = false;
	public const float _CommFrequency = 50.0f;

	public float fFactor1 = 100.0f;
	public float fFactor2 = 0.5f;
	public float fFactor3 = 0.0f;



	float torqueMotor1, torqueMotor2, velMotor1, velMotor2;
	JointMotor m = new JointMotor();

	DatosPlataforma _physics;

	Thread _commThread;
	Thread _platformThread;
	Thread _wheelChairThread;

	Semaphore _endBarrier;
	Vector3 _vLastWorldSpeed;
	bool _bEnd = false;
	bool _bPauseWheelChair = false;


	[DllImport("PlatformModule64")]
	private static extern void platformModule();

	[DllImport("PlatformModule64")]
	private static extern bool passConfigData([MarshalAs(UnmanagedType.LPStr)] string configFile, bool bWriteAllToLog, int pauseKeyCodeDIK);

	[DllImport("PlatformModule64")]
	private static extern void passData(bool bEnd, bool bPause);

	[DllImport("PlatformModule64")]
	private static extern void passData3f(int i, float x, float y, float z);

	[DllImport("WheelChair64")]
	private static extern void createWheelChair();

	[DllImport("WheelChair64")]
	private static extern void releaseWheelChair();

	[DllImport("WheelChair64")]
	private static extern void readParamsWheelChairByCharArray([MarshalAs(UnmanagedType.LPStr)] string sXml);

	[DllImport("WheelChair64")]
	private static extern void initWheelChair();

	[DllImport("WheelChair64")]
	private static extern void updateWheelChair(float fTimeStep);

	[DllImport("WheelChair64")]
	private static extern void stopWheelChair(bool bEnabled);

	[DllImport("WheelChair64")]
	private static extern void unstopWheelChair();

	[DllImport("WheelChair64")]
	private static extern float getCurrentTorque1();

	[DllImport("WheelChair64")]
	private static extern float getCurrentTorque2();

	[DllImport("WheelChair64")]
	private static extern float getCurrentSpeed1();

	[DllImport("WheelChair64")]
	private static extern float getCurrentSpeed2();

	[DllImport("WheelChair64")]
	private static extern void setTargetTorque12(float fTorque1, float fTorque2);

	[DllImport("WheelChair64")]
	private static extern void setMaxSpeed12(float fMaxSpeed1, float fMaxSpeed2);


	// Use this for initialization
	void Start()
	{
		Debug.Log("Platform script started...");

		FillKeyCodesDictionary();
		//passData (false, false);
		bool bRes = passConfigData(_platformConfigFile, _writeAllToLog, _keyCodesDictionary[_pauseKeyCode]);

		if (!bRes)
			Debug.LogError("Platform module config file not found");

		_physics = new DatosPlataforma();
		_physics._specForce = new float[3];
		_physics._acc = new float[3];
		_physics._speed = new float[3];
		_physics._xyz = new float[3];
		_physics._angAcc = new float[3];
		_physics._angSpeed = new float[3];
		_physics._hpr = new float[3];

		_endBarrier = new Semaphore(0, 3);

		Debug.Log("Platform thread (PlatformModule.dll) started...");
		_platformThread = new Thread(new ThreadStart(PlatformThread));
		_platformThread.Start();


		Debug.Log("Wheel chair thread (WheelChair.dll) started...");
		_wheelChairThread = new Thread(new ThreadStart(WheelChairThread));
		_wheelChairThread.Start();


		Debug.Log("Comm thread started...");
		_commThread = new Thread(new ThreadStart(CommThread));
		_commThread.Start();
	}

	private void OnDestroy()
	{
		Debug.Log("Platform script being destroyed (please wait!!)...");

		_bEnd = true;
		Thread.Sleep(100);

		_endBarrier.WaitOne();
		Thread.Sleep(100);

		_endBarrier.WaitOne();
		Thread.Sleep(100);

		//_endBarrier.WaitOne();
		Debug.Log("Platform script succesfully destroyed!!");
	}

	// FixedUpdate is called once per physics update
	void FixedUpdate()
	{
		float timeStep = Time.deltaTime;

		if (_physics == null)
			return;

		lock (_physics._xyz)
		{
			if (_RightAxis == RightAxis.X)
				_physics._xyz[0] = this.GetComponent<Rigidbody>().position.x;
			else if (_RightAxis == RightAxis.Y)
				_physics._xyz[0] = this.GetComponent<Rigidbody>().position.y;
			else if (_RightAxis == RightAxis.Z)
				_physics._xyz[0] = this.GetComponent<Rigidbody>().position.z;
			else if (_RightAxis == RightAxis.MINUS_X)
				_physics._xyz[0] = -this.GetComponent<Rigidbody>().position.x;
			else if (_RightAxis == RightAxis.MINUS_Y)
				_physics._xyz[0] = -this.GetComponent<Rigidbody>().position.y;
			else if (_RightAxis == RightAxis.MINUS_Z)
				_physics._xyz[0] = -this.GetComponent<Rigidbody>().position.z;

			if (_ForwardAxis == ForwardAxis.X)
				_physics._xyz[1] = this.GetComponent<Rigidbody>().position.x;
			else if (_ForwardAxis == ForwardAxis.Y)
				_physics._xyz[1] = this.GetComponent<Rigidbody>().position.y;
			else if (_ForwardAxis == ForwardAxis.Z)
				_physics._xyz[1] = this.GetComponent<Rigidbody>().position.z;
			else if (_ForwardAxis == ForwardAxis.MINUS_X)
				_physics._xyz[1] = -this.GetComponent<Rigidbody>().position.x;
			else if (_ForwardAxis == ForwardAxis.MINUS_Y)
				_physics._xyz[1] = -this.GetComponent<Rigidbody>().position.y;
			else if (_ForwardAxis == ForwardAxis.MINUS_Z)
				_physics._xyz[1] = -this.GetComponent<Rigidbody>().position.z;

			if (_UpAxis == UpAxis.X)
				_physics._xyz[2] = this.GetComponent<Rigidbody>().position.x;
			else if (_UpAxis == UpAxis.Y)
				_physics._xyz[2] = this.GetComponent<Rigidbody>().position.y;
			else if (_UpAxis == UpAxis.Z)
				_physics._xyz[2] = this.GetComponent<Rigidbody>().position.z;
			else if (_UpAxis == UpAxis.MINUS_X)
				_physics._xyz[2] = -this.GetComponent<Rigidbody>().position.x;
			else if (_UpAxis == UpAxis.MINUS_Y)
				_physics._xyz[2] = -this.GetComponent<Rigidbody>().position.y;
			else if (_UpAxis == UpAxis.MINUS_Z)
				_physics._xyz[2] = -this.GetComponent<Rigidbody>().position.z;
		}

		Quaternion q = this.GetComponent<Rigidbody>().rotation;

		lock (_physics._hpr)
		{
			_physics._hpr[0] = q.eulerAngles[1];

			if (q.eulerAngles[0] < 180.0f)
				_physics._hpr[1] = -q.eulerAngles[0];
			else
				_physics._hpr[1] = 360.0f - q.eulerAngles[0];

			if (q.eulerAngles[2] < 180.0f)
				_physics._hpr[2] = -q.eulerAngles[2];
			else
				_physics._hpr[2] = 360.0f - q.eulerAngles[2];
		}

		Vector3 vWorldSpeed = new Vector3(this.GetComponent<Rigidbody>().velocity.x, this.GetComponent<Rigidbody>().velocity.y, this.GetComponent<Rigidbody>().velocity.z);
		Vector3 vLocalSpeed = Quaternion.Inverse(q) * vWorldSpeed;

		lock (_physics._speed)
		{
			if (_RightAxis == RightAxis.X)
				_physics._speed[0] = vLocalSpeed.x;
			else if (_RightAxis == RightAxis.Y)
				_physics._speed[0] = vLocalSpeed.y;
			else if (_RightAxis == RightAxis.Z)
				_physics._speed[0] = vLocalSpeed.z;
			else if (_RightAxis == RightAxis.MINUS_X)
				_physics._speed[0] = -vLocalSpeed.x;
			else if (_RightAxis == RightAxis.MINUS_Y)
				_physics._speed[0] = -vLocalSpeed.y;
			else if (_RightAxis == RightAxis.MINUS_Z)
				_physics._speed[0] = -vLocalSpeed.z;

			if (_ForwardAxis == ForwardAxis.X)
				_physics._speed[1] = vLocalSpeed.x;
			else if (_ForwardAxis == ForwardAxis.Y)
				_physics._speed[1] = vLocalSpeed.y;
			else if (_ForwardAxis == ForwardAxis.Z)
				_physics._speed[1] = vLocalSpeed.z;
			else if (_ForwardAxis == ForwardAxis.MINUS_X)
				_physics._speed[1] = -vLocalSpeed.x;
			else if (_ForwardAxis == ForwardAxis.MINUS_Y)
				_physics._speed[1] = -vLocalSpeed.y;
			else if (_ForwardAxis == ForwardAxis.MINUS_Z)
				_physics._speed[1] = -vLocalSpeed.z;

			if (_UpAxis == UpAxis.X)
				_physics._speed[2] = vLocalSpeed.x;
			else if (_UpAxis == UpAxis.Y)
				_physics._speed[2] = vLocalSpeed.y;
			else if (_UpAxis == UpAxis.Z)
				_physics._speed[2] = vLocalSpeed.z;
			else if (_UpAxis == UpAxis.MINUS_X)
				_physics._speed[2] = -vLocalSpeed.x;
			else if (_UpAxis == UpAxis.MINUS_Y)
				_physics._speed[2] = -vLocalSpeed.y;
			else if (_UpAxis == UpAxis.MINUS_Z)
				_physics._speed[2] = -vLocalSpeed.z;
		}

		lock (_physics._angSpeed)
		{
			if (_RightAxis == RightAxis.X)
				_physics._angSpeed[0] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.x;
			else if (_RightAxis == RightAxis.Y)
				_physics._angSpeed[0] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.y;
			else if (_RightAxis == RightAxis.Z)
				_physics._angSpeed[0] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.z;
			else if (_RightAxis == RightAxis.MINUS_X)
				_physics._angSpeed[0] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.x;
			else if (_RightAxis == RightAxis.MINUS_Y)
				_physics._angSpeed[0] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.y;
			else if (_RightAxis == RightAxis.MINUS_Z)
				_physics._angSpeed[0] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.z;

			if (_ForwardAxis == ForwardAxis.X)
				_physics._angSpeed[1] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.x;
			else if (_ForwardAxis == ForwardAxis.Y)
				_physics._angSpeed[1] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.y;
			else if (_ForwardAxis == ForwardAxis.Z)
				_physics._angSpeed[1] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.z;
			else if (_ForwardAxis == ForwardAxis.MINUS_X)
				_physics._angSpeed[1] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.x;
			else if (_ForwardAxis == ForwardAxis.MINUS_Y)
				_physics._angSpeed[1] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.y;
			else if (_ForwardAxis == ForwardAxis.MINUS_Z)
				_physics._angSpeed[1] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.z;

			if (_UpAxis == UpAxis.X)
				_physics._angSpeed[2] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.x;
			else if (_UpAxis == UpAxis.Y)
				_physics._angSpeed[2] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.y;
			else if (_UpAxis == UpAxis.Z)
				_physics._angSpeed[2] = Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.z;
			else if (_UpAxis == UpAxis.MINUS_X)
				_physics._angSpeed[2] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.x;
			else if (_UpAxis == UpAxis.MINUS_Y)
				_physics._angSpeed[2] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.y;
			else if (_UpAxis == UpAxis.MINUS_Z)
				_physics._angSpeed[2] = -Mathf.Rad2Deg * this.GetComponent<Rigidbody>().angularVelocity.z;
		}

		Vector3 vWorldAcc = (vWorldSpeed - _vLastWorldSpeed) / timeStep;
		Vector3 vLocalAcc = Quaternion.Inverse(q) * vWorldAcc;

		lock (_physics._acc)
		{
			if (_RightAxis == RightAxis.X)
				_physics._acc[0] = vLocalAcc.x;
			else if (_RightAxis == RightAxis.Y)
				_physics._acc[0] = vLocalAcc.y;
			else if (_RightAxis == RightAxis.Z)
				_physics._acc[0] = vLocalAcc.z;
			else if (_RightAxis == RightAxis.MINUS_X)
				_physics._acc[0] = -vLocalAcc.x;
			else if (_RightAxis == RightAxis.MINUS_Y)
				_physics._acc[0] = -vLocalAcc.y;
			else if (_RightAxis == RightAxis.MINUS_Z)
				_physics._acc[0] = -vLocalAcc.z;

			if (_ForwardAxis == ForwardAxis.X)
				_physics._acc[1] = vLocalAcc.x;
			else if (_ForwardAxis == ForwardAxis.Y)
				_physics._acc[1] = vLocalAcc.y;
			else if (_ForwardAxis == ForwardAxis.Z)
				_physics._acc[1] = vLocalAcc.z;
			else if (_ForwardAxis == ForwardAxis.MINUS_X)
				_physics._acc[1] = -vLocalAcc.x;
			else if (_ForwardAxis == ForwardAxis.MINUS_Y)
				_physics._acc[1] = -vLocalAcc.y;
			else if (_ForwardAxis == ForwardAxis.MINUS_Z)
				_physics._acc[1] = -vLocalAcc.z;

			if (_UpAxis == UpAxis.X)
				_physics._acc[2] = vLocalAcc.x;
			else if (_UpAxis == UpAxis.Y)
				_physics._acc[2] = vLocalAcc.y;
			else if (_UpAxis == UpAxis.Z)
				_physics._acc[2] = vLocalAcc.z;
			else if (_UpAxis == UpAxis.MINUS_X)
				_physics._acc[2] = -vLocalAcc.x;
			else if (_UpAxis == UpAxis.MINUS_Y)
				_physics._acc[2] = -vLocalAcc.y;
			else if (_UpAxis == UpAxis.MINUS_Z)
				_physics._acc[2] = -vLocalAcc.z;
		}

		Vector3 vWorldSpecForce = new Vector3(vWorldAcc[0], vWorldAcc[1], vWorldAcc[2]);
		vWorldSpecForce += q * Physics.gravity;
		Vector3 vLocalSpecForce = Quaternion.Inverse(q) * vWorldSpecForce;

		lock (_physics._specForce)
		{
			if (_RightAxis == RightAxis.X)
				_physics._specForce[0] = vLocalSpecForce.x;
			else if (_RightAxis == RightAxis.Y)
				_physics._specForce[0] = vLocalSpecForce.y;
			else if (_RightAxis == RightAxis.Z)
				_physics._specForce[0] = vLocalSpecForce.z;
			else if (_RightAxis == RightAxis.MINUS_X)
				_physics._specForce[0] = -vLocalSpecForce.x;
			else if (_RightAxis == RightAxis.MINUS_Y)
				_physics._specForce[0] = -vLocalSpecForce.y;
			else if (_RightAxis == RightAxis.MINUS_Z)
				_physics._specForce[0] = -vLocalSpecForce.z;

			if (_ForwardAxis == ForwardAxis.X)
				_physics._specForce[1] = vLocalSpecForce.x;
			else if (_ForwardAxis == ForwardAxis.Y)
				_physics._specForce[1] = vLocalSpecForce.y;
			else if (_ForwardAxis == ForwardAxis.Z)
				_physics._specForce[1] = vLocalSpecForce.z;
			else if (_ForwardAxis == ForwardAxis.MINUS_X)
				_physics._specForce[1] = -vLocalSpecForce.x;
			else if (_ForwardAxis == ForwardAxis.MINUS_Y)
				_physics._specForce[1] = -vLocalSpecForce.y;
			else if (_ForwardAxis == ForwardAxis.MINUS_Z)
				_physics._specForce[1] = -vLocalSpecForce.z;

			if (_UpAxis == UpAxis.X)
				_physics._specForce[2] = vLocalSpecForce.x;
			else if (_UpAxis == UpAxis.Y)
				_physics._specForce[2] = vLocalSpecForce.y;
			else if (_UpAxis == UpAxis.Z)
				_physics._specForce[2] = vLocalSpecForce.z;
			else if (_UpAxis == UpAxis.MINUS_X)
				_physics._specForce[2] = -vLocalSpecForce.x;
			else if (_UpAxis == UpAxis.MINUS_Y)
				_physics._specForce[2] = -vLocalSpecForce.y;
			else if (_UpAxis == UpAxis.MINUS_Z)
				_physics._specForce[2] = -vLocalSpecForce.z;
		}

		/*
		lock (physics.angAcc)
		{
	       _physics.angAcc[0] = 
   	       _physics.angAcc[1] = 
 	       _physics.angAcc[2] = 
		}
		*/



		m.force = torqueMotor1;
		m.targetVelocity = velMotor1;

		_jointLeftWheel.motor = m;

		m.targetVelocity = velMotor2;
		_jointRightWheel.motor = m;


		_vLastWorldSpeed = vWorldSpeed;

		if (Input.GetKeyDown(_pauseKeyCode))
		{
			_bPauseWheelChair = !_bPauseWheelChair;

			lock (_wheelChairThread)
			{
				if (_bPauseWheelChair == true)
					unstopWheelChair();
				else
					stopWheelChair(true);
			}
		}
	}

	void CommThread()
	{
		while (!_bEnd)
		{
			float fSleepTime = 1.0f / _CommFrequency;
			//Debug.Log("Going to sleep for " + fSleepTime + " seconds");
			Thread.Sleep((int)fSleepTime * 1000);
			//Debug.Log("Slept for " + fSleepTime + " seconds");

			lock (_physics)
			{
				//Debug.Log("Passing data to dll");
				passData3f(0, rightLinearScale * _physics._specForce[0], forwardLinearScale * _physics._specForce[1], upLinearScale * _physics._specForce[2]);
				passData3f(1, rightLinearScale * _physics._acc[0], forwardLinearScale * _physics._acc[1], upLinearScale * _physics._acc[2]);
				passData3f(2, rightLinearScale * _physics._speed[0], forwardLinearScale * _physics._speed[1], upLinearScale * _physics._speed[2]);
				passData3f(3, rightLinearScale * _physics._xyz[0], forwardLinearScale * _physics._xyz[1], upLinearScale * _physics._xyz[2]);
				passData3f(4, rightAngularScale * _physics._angAcc[0], forwardAngularScale * _physics._angAcc[1], upAngularScale * _physics._angAcc[2]);
				passData3f(5, rightAngularScale * _physics._angSpeed[0], forwardAngularScale * _physics._angSpeed[1], upAngularScale * _physics._angSpeed[2]);
				passData3f(6, upAngularScale * _physics._hpr[0], rightAngularScale * _physics._hpr[1], forwardAngularScale * _physics._hpr[2]);
				passData(false, false);
				//Debug.Log("Data passed data to dll");
			}
		}

		passData(true, false);

		_endBarrier.Release();
		Debug.Log("Comm thread terminated");
	}

	void PlatformThread()
	{
		platformModule();

		_endBarrier.Release();
		Debug.Log("Platform thread terminated");
	}

	void WheelChairThread()
	{
		createWheelChair();
		readParamsWheelChairByCharArray(_wheelChairConfigFile);

		initWheelChair();
		Thread.Sleep(1000);

		stopWheelChair(true);
		unstopWheelChair();
		setMaxSpeed12(0.0f, 0.0f);

		bool bPaused = false;
		float fPitch, fTorque;


		while (!_bEnd)
		{
			// Comunicacion Visual --> Motores:

			lock (_physics)
			{
				fPitch = _physics._hpr[1];
			}

			if (Math.Abs(_physics._hpr[1]) > 5.0f)
			{
				fTorque = Mathf.Sin(Mathf.Deg2Rad * fPitch) * fFactor1;
				setTargetTorque12(fTorque, fTorque);
			}
			else
			{
				setTargetTorque12(fFactor3, fFactor3);
			}
			//setTargetTorque12(0.0f, 0.0f);
			updateWheelChair(0.0f);


			// Comunicacion Motores --> Visual:

			float fSpeed1 = getCurrentSpeed1();
			float fSpeed2 = getCurrentSpeed2();
			Debug.Log("Wheel chair motors speed = " + fSpeed1 + ", " + fSpeed2);

			float fTorque1 = getCurrentTorque1();
			float fTorque2 = getCurrentTorque2();
			Debug.Log("Wheel chair motors torque = " + fTorque1 + ", " + fTorque2);



			if ((_jointLeftWheel != null) && (_jointRightWheel != null))
			{
				/*JointMotor m = new JointMotor ();
				m.force = 1000000000.0f;
				m.targetVelocity = fSpeed1*fFactor2;
				_jointLeftWheel.motor = m;

				m.targetVelocity = fSpeed2*fFactor2;
				_jointRightWheel.motor = m;*/




				torqueMotor1 = 10.0f;
				velMotor1 = fSpeed1 * fFactor2;

				torqueMotor2 = 10.0f;
				velMotor2 = fSpeed2 * fFactor2;
			}
		}
		stopWheelChair(false);
		releaseWheelChair();
		_endBarrier.Release();
		Debug.Log("Wheel chair thread terminated");
	}

	void FillKeyCodesDictionary()
	{
		_keyCodesDictionary.Add(KeyCode.None, 0x00);
		_keyCodesDictionary.Add(KeyCode.Backspace, 0x0E);
		_keyCodesDictionary.Add(KeyCode.Delete, 0xD3);
		_keyCodesDictionary.Add(KeyCode.Tab, 0x0F);
		//_keyCodesDictionary.Add (KeyCode.Clear, );
		_keyCodesDictionary.Add(KeyCode.Return, 0x1C);
		//_keyCodesDictionary.Add (KeyCode.Pause, );
		_keyCodesDictionary.Add(KeyCode.Escape, 0x01);
		_keyCodesDictionary.Add(KeyCode.Space, 0x39);
		_keyCodesDictionary.Add(KeyCode.Keypad0, 0x52);
		_keyCodesDictionary.Add(KeyCode.Keypad1, 0x4F);
		_keyCodesDictionary.Add(KeyCode.Keypad2, 0x50);
		_keyCodesDictionary.Add(KeyCode.Keypad3, 0x51);
		_keyCodesDictionary.Add(KeyCode.Keypad4, 0x4B);
		_keyCodesDictionary.Add(KeyCode.Keypad5, 0x4C);
		_keyCodesDictionary.Add(KeyCode.Keypad6, 0x07);
		_keyCodesDictionary.Add(KeyCode.Keypad7, 0x08);
		_keyCodesDictionary.Add(KeyCode.Keypad8, 0x09);
		_keyCodesDictionary.Add(KeyCode.Keypad9, 0x0A);
		//_keyCodesDictionary.Add (KeyCode.KeypadPeriod, );
		_keyCodesDictionary.Add(KeyCode.KeypadDivide, 0x53);
		_keyCodesDictionary.Add(KeyCode.KeypadMultiply, 0x37);
		_keyCodesDictionary.Add(KeyCode.KeypadMinus, 0x4A);
		_keyCodesDictionary.Add(KeyCode.KeypadPlus, 0x4E);
		_keyCodesDictionary.Add(KeyCode.KeypadEnter, 0x1C);
		_keyCodesDictionary.Add(KeyCode.KeypadEquals, 0x0D);
		_keyCodesDictionary.Add(KeyCode.UpArrow, 0xC8);
		_keyCodesDictionary.Add(KeyCode.DownArrow, 0xD0);
		_keyCodesDictionary.Add(KeyCode.RightArrow, 0xCD);
		_keyCodesDictionary.Add(KeyCode.LeftArrow, 0xCB);
		_keyCodesDictionary.Add(KeyCode.Insert, 0xD2);
		_keyCodesDictionary.Add(KeyCode.Home, 0xC7);
		_keyCodesDictionary.Add(KeyCode.End, 0xCF);
		_keyCodesDictionary.Add(KeyCode.PageUp, 0xC9);
		_keyCodesDictionary.Add(KeyCode.PageDown, 0xD1);
		_keyCodesDictionary.Add(KeyCode.F1, 0x3B);
		_keyCodesDictionary.Add(KeyCode.F2, 0x3C);
		_keyCodesDictionary.Add(KeyCode.F3, 0x3D);
		_keyCodesDictionary.Add(KeyCode.F4, 0x3E);
		_keyCodesDictionary.Add(KeyCode.F5, 0x3F);
		_keyCodesDictionary.Add(KeyCode.F6, 0x40);
		_keyCodesDictionary.Add(KeyCode.F7, 0x41);
		_keyCodesDictionary.Add(KeyCode.F8, 0x42);
		_keyCodesDictionary.Add(KeyCode.F9, 0x43);
		_keyCodesDictionary.Add(KeyCode.F10, 0x44);
		_keyCodesDictionary.Add(KeyCode.F11, 0x57);
		_keyCodesDictionary.Add(KeyCode.F12, 0x58);
		_keyCodesDictionary.Add(KeyCode.F13, 0x64);
		_keyCodesDictionary.Add(KeyCode.F14, 0x65);
		_keyCodesDictionary.Add(KeyCode.F15, 0x66);
		_keyCodesDictionary.Add(KeyCode.Alpha1, 0x02);
		_keyCodesDictionary.Add(KeyCode.Alpha2, 0x03);
		_keyCodesDictionary.Add(KeyCode.Alpha3, 0x04);
		_keyCodesDictionary.Add(KeyCode.Alpha4, 0x05);
		_keyCodesDictionary.Add(KeyCode.Alpha5, 0x06);
		_keyCodesDictionary.Add(KeyCode.Alpha6, 0x07);
		_keyCodesDictionary.Add(KeyCode.Alpha7, 0x08);
		_keyCodesDictionary.Add(KeyCode.Alpha8, 0x09);
		_keyCodesDictionary.Add(KeyCode.Alpha9, 0x0A);
		_keyCodesDictionary.Add(KeyCode.Alpha0, 0x0B);
		//_keyCodesDictionary.Add (KeyCode.Exclaim, );
		//_keyCodesDictionary.Add (KeyCode.DoubleQuote, );
		//_keyCodesDictionary.Add (KeyCode.Hash, );
		//_keyCodesDictionary.Add (KeyCode.Dollar, );
		//_keyCodesDictionary.Add (KeyCode.Ampersand, );
		//_keyCodesDictionary.Add (KeyCode.Quote, );
		//_keyCodesDictionary.Add (KeyCode.LeftParen, );
		//_keyCodesDictionary.Add (KeyCode.RightParen, );
		//_keyCodesDictionary.Add (KeyCode.Asterisk, );
		_keyCodesDictionary.Add(KeyCode.Plus, 0x4E);
		_keyCodesDictionary.Add(KeyCode.Comma, 0x33);
		_keyCodesDictionary.Add(KeyCode.Minus, 0x0C);
		_keyCodesDictionary.Add(KeyCode.Period, 0x34);
		_keyCodesDictionary.Add(KeyCode.Slash, 0x35);
		_keyCodesDictionary.Add(KeyCode.Colon, 0x92);
		_keyCodesDictionary.Add(KeyCode.Semicolon, 0x27);
		//_keyCodesDictionary.Add (KeyCode.Less, );
		_keyCodesDictionary.Add(KeyCode.Equals, 0x0D);
		//_keyCodesDictionary.Add (KeyCode.Greater, );
		//_keyCodesDictionary.Add (KeyCode.Question, );
		//_keyCodesDictionary.Add (KeyCode.At, );
		_keyCodesDictionary.Add(KeyCode.LeftBracket, 0x1A);
		_keyCodesDictionary.Add(KeyCode.Backslash, 0x2B);
		//_keyCodesDictionary.Add (KeyCode.Caret, );
		//_keyCodesDictionary.Add (KeyCode.Underscore, );
		//_keyCodesDictionary.Add (KeyCode.BackQuote, );
		_keyCodesDictionary.Add(KeyCode.A, 0x1E);
		_keyCodesDictionary.Add(KeyCode.B, 0x30);
		_keyCodesDictionary.Add(KeyCode.C, 0x2E);
		_keyCodesDictionary.Add(KeyCode.D, 0x20);
		_keyCodesDictionary.Add(KeyCode.E, 0x12);
		_keyCodesDictionary.Add(KeyCode.F, 0x21);
		_keyCodesDictionary.Add(KeyCode.G, 0x22);
		_keyCodesDictionary.Add(KeyCode.H, 0x23);
		_keyCodesDictionary.Add(KeyCode.I, 0x17);
		_keyCodesDictionary.Add(KeyCode.J, 0x24);
		_keyCodesDictionary.Add(KeyCode.K, 0x25);
		_keyCodesDictionary.Add(KeyCode.L, 0x26);
		_keyCodesDictionary.Add(KeyCode.M, 0x32);
		_keyCodesDictionary.Add(KeyCode.N, 0x31);
		_keyCodesDictionary.Add(KeyCode.O, 0x18);
		_keyCodesDictionary.Add(KeyCode.P, 0x19);
		_keyCodesDictionary.Add(KeyCode.Q, 0x10);
		_keyCodesDictionary.Add(KeyCode.R, 0x13);
		_keyCodesDictionary.Add(KeyCode.S, 0x1F);
		_keyCodesDictionary.Add(KeyCode.T, 0x14);
		_keyCodesDictionary.Add(KeyCode.U, 0x16);
		_keyCodesDictionary.Add(KeyCode.V, 0x2F);
		_keyCodesDictionary.Add(KeyCode.W, 0x11);
		_keyCodesDictionary.Add(KeyCode.X, 0x2D);
		_keyCodesDictionary.Add(KeyCode.Y, 0x15);
		_keyCodesDictionary.Add(KeyCode.Z, 0x2C);
		_keyCodesDictionary.Add(KeyCode.Numlock, 0x45);
		_keyCodesDictionary.Add(KeyCode.CapsLock, 0x3A);
		_keyCodesDictionary.Add(KeyCode.ScrollLock, 0x46);
		_keyCodesDictionary.Add(KeyCode.RightShift, 0x36);
		_keyCodesDictionary.Add(KeyCode.LeftShift, 0x2A);
		_keyCodesDictionary.Add(KeyCode.RightControl, 0x9D);
		_keyCodesDictionary.Add(KeyCode.LeftControl, 0x1D);
		_keyCodesDictionary.Add(KeyCode.RightAlt, 0xB8);
		_keyCodesDictionary.Add(KeyCode.LeftAlt, 0x38);
		//_keyCodesDictionary.Add (KeyCode.LeftCommand, );
		//_keyCodesDictionary.Add (KeyCode.RightCommand, );
		_keyCodesDictionary.Add(KeyCode.LeftWindows, 0xDB);
		//_keyCodesDictionary.Add (KeyCode.LeftApple, );
		//_keyCodesDictionary.Add (KeyCode.RightApple, );
		_keyCodesDictionary.Add(KeyCode.RightWindows, 0xDC);
		//_keyCodesDictionary.Add (KeyCode.AltGr, );
		//_keyCodesDictionary.Add (KeyCode.Print, );
		_keyCodesDictionary.Add(KeyCode.SysReq, 0xB7);
		//_keyCodesDictionary.Add (KeyCode.Break, );
		_keyCodesDictionary.Add(KeyCode.Menu, 0xDD);
	}
}
