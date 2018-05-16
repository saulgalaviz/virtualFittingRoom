using UnityEngine.VR;
using UnityEngine;
using System.Collections;

public class SensorRotation : MonoBehaviour 
{
	Vector3 magneticHeading = Vector3.zero;
	Vector3 gyroGravity = Vector3.down;
	Vector3 gyroRateUnbiased = Vector3.zero;

	Vector3 crossProd = Vector3.zero;

	Vector3 
	smoothedMagneticHeading = Vector3.zero, 
	smoothedGravity = Vector3.zero;

	[SerializeField]float dampCoeffVectors = 0.1f;
	[SerializeField]float dampCoeffMag = 1f;

	Quaternion baseRotation = Quaternion.identity;
	Quaternion rotation = Quaternion.identity;
	Quaternion finalRotation = Quaternion.identity;
	public Quaternion Rotation {get {return finalRotation;}}

	bool correctionOn = false;
	[SerializeField]float angleCorrectionOn = 15f;
	[SerializeField]float angleCorrectionOff = 3f;
	TPoseCalibration tPoseCalibration;

	void Start () 
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Input.compass.enabled = true;
		Input.gyro.enabled = true;
		tPoseCalibration = GameObject.FindObjectOfType<TPoseCalibration>();
		if (tPoseCalibration != null) tPoseCalibration.onSuccess += SetBaseRotation;
	}

	public void SetBaseRotation(Quaternion additionalRotation)
	{
		baseRotation = additionalRotation * Quaternion.Inverse(rotation);
	}

	void FixedUpdate () 
	{
		RotateMethod2();
	}

	void RotateMethod2()
	{
		magneticHeading = Input.compass.rawVector; 
		magneticHeading = new Vector3(-magneticHeading.y, magneticHeading.x, -magneticHeading.z); // for landscape left

		gyroGravity = Input.gyro.gravity;
		gyroGravity = new Vector3(gyroGravity.x, gyroGravity.y, -gyroGravity.z);
		gyroRateUnbiased = Vector3.Scale(Input.gyro.rotationRateUnbiased, new Vector3(-1f, -1f, 1f));

		smoothedMagneticHeading = Vector3.Slerp(smoothedMagneticHeading, magneticHeading, dampCoeffVectors);
		smoothedGravity = Vector3.Slerp(smoothedGravity, gyroGravity, dampCoeffVectors);

		crossProd = Vector3.Cross (smoothedMagneticHeading, smoothedGravity).normalized;

		if (crossProd.sqrMagnitude == 0f) // unity's warning was too annoying
		{
			crossProd = Vector3.forward;
		}

		rotation = rotation * Quaternion.Euler(gyroRateUnbiased * Time.deltaTime * Mathf.Rad2Deg);

		//gravity correction :
		Quaternion gravityDiff = Quaternion.FromToRotation(rotation * gyroGravity, Vector3.down);
		Vector3 gravityDiffXZ = new Vector3(gravityDiff.x, 0f, gravityDiff.z);
		Quaternion correction =  Quaternion.Euler(gravityDiffXZ);
		rotation = correction * rotation;

		//angle between current rotation and magnetic:
		float deltaAngle = Quaternion.Angle(rotation, Quaternion.Inverse(Quaternion.LookRotation(crossProd, -gyroGravity)));
		if (deltaAngle > angleCorrectionOn)
		{
			correctionOn = false;
		}
		if (deltaAngle < angleCorrectionOff)
		{
			correctionOn = false;
		}
		if (correctionOn)
		{
			rotation = Quaternion.RotateTowards(rotation, Quaternion.Inverse(Quaternion.LookRotation(crossProd, -gyroGravity)), Time.deltaTime * dampCoeffMag * deltaAngle);
		}
		finalRotation = baseRotation * rotation;
	}

	void OnDestroy()
	{
		if (tPoseCalibration != null) tPoseCalibration.onSuccess -= SetBaseRotation;
	}
}
