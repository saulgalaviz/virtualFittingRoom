using UnityEngine;
using System.Collections.Generic;

public class TPoseCalibration : MonoBehaviour 
{
    public enum CalibrationType
    {
        TPose, 
        RightHand90DegreesUTurn
    }

    [SerializeField]CalibrationType calibrationType = CalibrationType.TPose;

    #region delegates and events
    public delegate void OnStartHandler();
    public delegate void OnProgressHandler(float progress);
    public delegate void OnFailHandler();
    public delegate void OnSuccessHandler(Quaternion rotation);

    public event OnStartHandler onStart;
    public event OnProgressHandler onProgress;
    public event OnFailHandler onFail;
    public event OnSuccessHandler onSuccess;
    #endregion

    [SerializeField]float calibrationTime;
    public float CalibrationTime {get {return this.calibrationTime;}}

    float timer;
    float cooldown;

    Vector3[] initPositions;
    Vector3[] currentPositions;

    nuitrack.JointType[] checkedJoints = new nuitrack.JointType[]
    {
    nuitrack.JointType.Head, nuitrack.JointType.Torso, 
    nuitrack.JointType.LeftShoulder, nuitrack.JointType.LeftElbow, nuitrack.JointType.LeftWrist,
    nuitrack.JointType.RightShoulder, nuitrack.JointType.RightElbow, nuitrack.JointType.RightWrist
    };

    [SerializeField]float maxAngle = 30f;
    [SerializeField]float maxSqrDifference = 10000f;

    static Quaternion sensorOrientation = Quaternion.identity;
    static public Quaternion SensorOrientation {get {return sensorOrientation;}}

	
    bool calibrationStarted;

    void Start () 
    {
        switch (calibrationType)
        {
            case CalibrationType.TPose:
            checkedJoints = new nuitrack.JointType[]
            {
                nuitrack.JointType.Head, nuitrack.JointType.Torso, 
                nuitrack.JointType.LeftShoulder, nuitrack.JointType.LeftElbow, nuitrack.JointType.LeftWrist,
                nuitrack.JointType.RightShoulder, nuitrack.JointType.RightElbow, nuitrack.JointType.RightWrist
            };
            break;
            case CalibrationType.RightHand90DegreesUTurn:
            checkedJoints = new nuitrack.JointType[]
            {
                nuitrack.JointType.Torso, nuitrack.JointType.Neck,
                nuitrack.JointType.RightShoulder, nuitrack.JointType.RightElbow, nuitrack.JointType.RightWrist
            };
            break;
        }

        DontDestroyOnLoad(this);
	    if (GameObject.FindObjectsOfType<TPoseCalibration>().Length > 1) //just in case
	    {
		    Destroy(gameObject);
	    }
	    timer = 0f;
	    cooldown = 0f;
	    calibrationStarted = false;
	    initPositions = new Vector3[checkedJoints.Length];
	    currentPositions = new Vector3[checkedJoints.Length];
    }
	
    void Update () 
    {
	    if (cooldown > 0f)
	    {
        cooldown -= Time.unscaledDeltaTime;
	    }
	    else
	    {
            if (CurrentUserTracker.CurrentUser != 0)
		    {
			    if (!calibrationStarted)
			    {
				    StartCalibration();
			    }
			    else
			    {
				    if (timer > calibrationTime)
				    {
					    calibrationStarted = false;
					    timer = 0f;
					    cooldown = calibrationTime;

					    if (onSuccess != null) onSuccess(GetHeadAngles());
				    }
				    else
				    {
					    ProcessCalibration();
					    if (!calibrationStarted)
					    {
						    timer = 0f;
						    if (onFail != null) onFail();
					    }
					    else
					    {
						    if (onProgress != null) onProgress(timer / calibrationTime);
                                timer += Time.unscaledDeltaTime;
					    }
				    }
			    }
		    }
	    }
    }

    void StartCalibration()
    {
	    Dictionary<nuitrack.JointType, nuitrack.Joint> joints = new Dictionary<nuitrack.JointType, nuitrack.Joint>();

	    {
		    int i = 0;
		    foreach (nuitrack.JointType joint in checkedJoints)
		    {
                joints.Add(joint, CurrentUserTracker.CurrentSkeleton.GetJoint(joint));
			    if (joints[joint].Confidence < 0.5f) return;
                    initPositions[i] = joints[joint].ToVector3();
			    i++;
		    }
	    }

        switch (calibrationType)
        {
            case CalibrationType.TPose:
            {
      	        Vector3[] handDeltas = new Vector3[6];

                handDeltas[0] = joints[nuitrack.JointType.LeftWrist].ToVector3()      - joints[nuitrack.JointType.RightWrist].ToVector3();
                handDeltas[1] = joints[nuitrack.JointType.LeftWrist].ToVector3()      - joints[nuitrack.JointType.LeftElbow].ToVector3();
                handDeltas[2] = joints[nuitrack.JointType.LeftElbow].ToVector3()      - joints[nuitrack.JointType.LeftShoulder].ToVector3();
                handDeltas[3] = joints[nuitrack.JointType.LeftShoulder].ToVector3()   - joints[nuitrack.JointType.RightShoulder].ToVector3();
                handDeltas[4] = joints[nuitrack.JointType.RightShoulder].ToVector3()  - joints[nuitrack.JointType.RightElbow].ToVector3();
                handDeltas[5] = joints[nuitrack.JointType.RightElbow].ToVector3()     - joints[nuitrack.JointType.RightWrist].ToVector3();

      	        for (int i = 1; i < 6; i++)
      	        {
      		        if ( Vector3.Angle (handDeltas[0], handDeltas[i]) > maxAngle) 
      		        {
      			        return;
      		        }
      	        }

      	        calibrationStarted = true;
      	        if (onStart != null) onStart();
                break;
            }
            case CalibrationType.RightHand90DegreesUTurn:
            {
                Vector3 torsoNeck     = joints[nuitrack.JointType.Neck].ToVector3() - joints[nuitrack.JointType.Torso].ToVector3();
                Vector3 shoulderElbow = joints[nuitrack.JointType.RightElbow].ToVector3() - joints[nuitrack.JointType.RightShoulder].ToVector3();
                Vector3 elbowWrist    = joints[nuitrack.JointType.RightWrist].ToVector3() - joints[nuitrack.JointType.RightElbow].ToVector3();

                if ( 
                (Mathf.Abs(Vector3.Angle(torsoNeck, shoulderElbow) - 90f) > maxAngle) ||    // !torso perpendicular to shoulder
                (Mathf.Abs(Vector3.Angle(shoulderElbow, elbowWrist) - 90f) > maxAngle) ||   // !shoulder perpendicular to elbow
                ((Mathf.Abs(shoulderElbow.z) / shoulderElbow.magnitude) > Mathf.Sin(maxAngle * Mathf.Deg2Rad)) // !shoulder and elbow are approximately on same z-depth
                )
                {
                return;
                }
                calibrationStarted = true;
                if (onStart != null) onStart();
                break;
            }
        }
    }
	
    void ProcessCalibration()
    {
	    Dictionary<nuitrack.JointType, nuitrack.Joint> joints = new Dictionary<nuitrack.JointType, nuitrack.Joint>();

	    {
		    int i = 0;
		    foreach (nuitrack.JointType joint in checkedJoints)
		    {
                joints.Add(joint, CurrentUserTracker.CurrentSkeleton.GetJoint(joint));
			    if (joints[joint].Confidence < 0.5f) 
			    {
				    calibrationStarted = false;
				    return;
			    }

                currentPositions[i] = joints[joint].ToVector3();
			    i++;
		    }
	    }
	    for (int i = 0; i < initPositions.Length; i++)
	    {
		    if ((initPositions[i] - currentPositions[i]).sqrMagnitude > maxSqrDifference)
		    {
			    calibrationStarted = false;
			    return;
		    }
	    }
    }

    Quaternion GetHeadAngles()
    {
        switch (calibrationType)
        {
            case CalibrationType.TPose: // t-pose
            {
                float angleY = -Mathf.Rad2Deg * Mathf.Atan2 ((currentPositions[4] - currentPositions[7]).z, (currentPositions[4] - currentPositions[7]).x);
                float angleX = -Mathf.Rad2Deg * Mathf.Atan2(Input.gyro.gravity.z, -Input.gyro.gravity.y);

                Vector3 torso = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.Torso).ToVector3();
                Vector3 neck = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.Neck).ToVector3();
                Vector3 diff = neck - torso;
          
                sensorOrientation = Quaternion.Euler(Mathf.Atan2(diff.z, diff.y) * Mathf.Rad2Deg, 0f, 0f);
          
                Debug.Log ("Gravity vector: " + Input.gyro.gravity.ToString("0.000") + "; AngleX: " + angleX.ToString("0") + "; AngleY: " + angleY.ToString("0"));
          
                return Quaternion.Euler(angleX, angleY, 0f);
            }
            default: // right hand's shoulder horizontal, elbow - vertical
            {
                float angleY = -Mathf.Rad2Deg * Mathf.Atan2( (currentPositions[2] - currentPositions[3]).z, (currentPositions[2] - currentPositions[3]).x);
                float angleX = -Mathf.Rad2Deg * Mathf.Atan2(Input.gyro.gravity.z, -Input.gyro.gravity.y);

                Vector3 torso = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.Torso).ToVector3();
                Vector3 neck = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.Neck).ToVector3();
                Vector3 diff = neck - torso;

                sensorOrientation = Quaternion.Euler(Mathf.Atan2(diff.z, diff.y) * Mathf.Rad2Deg, 0f, 0f);

                Debug.Log ("Gravity vector: " + Input.gyro.gravity.ToString("0.000") + "; AngleX: " + angleX.ToString("0") + "; AngleY: " + angleY.ToString("0"));

                return Quaternion.Euler(angleX, angleY, 0f);
            }
        }
    }
}
