using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NuitrackAvatarAdvanced : MonoBehaviour
{
	[SerializeField] Transform headTransform; //if not null, skeletonAvatar will move it
	[Tooltip("If platform not android, head joint (nuitrack.JointType.None) not work")]
	[SerializeField] Transform simulatedHeadJoint; 
    Vector3 startPoint; //"Waist" model bone position on start

    TPoseCalibration tPC;
    Vector3 basePivotOffset;
    float scale = 1;

    public static Vector3 leftHandPos, rightHandPos;

    /// <summary> Model bones </summary>
    Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = new Dictionary<nuitrack.JointType, ModelJoint>();
    Quaternion q180 = Quaternion.Euler(0f, 180f, 0f);
    Quaternion q0 = Quaternion.identity;

    [Header("Rigged model")]
    [SerializeField] ModelJoint[] modelJoints;

    bool firstOffset = false;

    /// <summary>
    /// Adding distance between target and parent model bones
    /// </summary>
    void AddBoneScale(nuitrack.JointType targetJoint, nuitrack.JointType parentJoint)
    {
        Vector3 targetBonePos = jointsRigged[targetJoint].bone.position;
        Vector3 parentBonePos = jointsRigged[parentJoint].bone.position;
        jointsRigged[targetJoint].parentBone = jointsRigged[parentJoint].bone;
        jointsRigged[targetJoint].baseDistanceToParent = Vector3.Distance(parentBonePos, targetBonePos);
    }

    void OnEnable()
    {
        tPC = FindObjectOfType<TPoseCalibration>();
        tPC.onSuccess += OnSuccessCalib;
    }

    void Start()
    {
        //Adding model bones and JointType keys
        //Adding model bones rotation offsets and JointType keys

        for (int i = 0; i < modelJoints.Length; i++)
        {
            //Debug.Log(modelJoints[i].jointType);
            jointsRigged.Add(modelJoints[i].jointType, modelJoints[i]);
            modelJoints[i].baseRotOffset = modelJoints[i].bone.rotation;
            if (modelJoints[i].parentJointType != nuitrack.JointType.None)
                AddBoneScale(modelJoints[i].jointType, modelJoints[i].parentJointType);
        }

        startPoint = jointsRigged[nuitrack.JointType.Waist].bone.position;
    }

    void Update()
    {
        if (CurrentUserTracker.CurrentSkeleton != null) ProcessSkeleton(CurrentUserTracker.CurrentSkeleton);
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Q))
            StartCoroutine(CalculateOffset());
    }

    /// <summary>
    /// Getting skeleton data from sensor and update model bones transforms
    /// </summary>
    void ProcessSkeleton(nuitrack.Skeleton skeleton)
    {
        if (skeleton == null) return;

        if (!firstOffset)
        {
            firstOffset = true;
            StartCoroutine(CalculateOffset());
        }

        foreach (var riggedJoint in jointsRigged)
        {
            nuitrack.Joint j = skeleton.GetJoint(riggedJoint.Key);
            if (j.Confidence > 0.5f)
            {
                //Bone position
                Vector3 newPos = (q180) * (Vector3.up * CalibrationInfo.FloorHeight + CalibrationInfo.SensorOrientation * (0.001f * j.ToVector3())) * scale + basePivotOffset;

                ModelJoint rj = riggedJoint.Value;

                //Bone scale
                if (rj.parentBone != null)
                {
                    Transform bone = rj.parentBone;
                    bone.parent = bone.root;
                    float scaleDif = rj.baseDistanceToParent / Vector3.Distance(newPos, bone.position);
                    bone.localScale = Vector3.one / scaleDif;
                }

                rj.bone.position = newPos;

                if(j.Type != nuitrack.JointType.None)
                {
                    Quaternion jointOrient = CalibrationInfo.SensorOrientation * (j.ToQuaternionMirrored());
                    rj.bone.rotation = q0 * Quaternion.Inverse(CalibrationInfo.SensorOrientation) * jointOrient * rj.baseRotOffset;
                }
            }
        }

        leftHandPos = jointsRigged[nuitrack.JointType.LeftWrist].bone.position;
        rightHandPos = jointsRigged[nuitrack.JointType.RightWrist].bone.position;
    }

    private void LateUpdate()
    {
		#if UNITY_ANDROID
        if (headTransform != null)
            headTransform.position = jointsRigged[nuitrack.JointType.None].bone.position;
		#else
		if (headTransform != null)
			headTransform.position = simulatedHeadJoint.position;
		#endif

    }

    void OnDisable()
    {
        tPC.onSuccess -= OnSuccessCalib;
    }

    private void OnSuccessCalib(Quaternion rotation)
    {
        //Model scale compensation
        scale = scale * (1 / jointsRigged[nuitrack.JointType.LeftCollar].bone.lossyScale.x);
        StartCoroutine(CalculateOffset());
    }

    /// <summary>
    /// Using difference between startPoint position and "waist" model bone position calculating model postion offset
    /// Using leg length calculating y-axis offset. For the X-axis offset is not needed
    /// </summary>
    public IEnumerator CalculateOffset()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Vector3 basePos = jointsRigged[nuitrack.JointType.Waist].bone.position;
        Vector3 newPivotOffset;
        newPivotOffset = startPoint - basePos + basePivotOffset;
        newPivotOffset.x = 0;

        Vector3 hipPos = jointsRigged[nuitrack.JointType.LeftHip].bone.position;
        Vector3 kneePos = jointsRigged[nuitrack.JointType.LeftKnee].bone.position;
        Vector3 footPos = jointsRigged[nuitrack.JointType.LeftAnkle].bone.position;

        float oldLegLength = Vector3.Distance(hipPos, kneePos) + Vector3.Distance(kneePos, footPos);
        float legLengthDiff = oldLegLength - (jointsRigged[nuitrack.JointType.LeftAnkle].baseDistanceToParent + jointsRigged[nuitrack.JointType.LeftKnee].baseDistanceToParent);

        newPivotOffset.y = newPivotOffset.y - legLengthDiff;

        basePivotOffset = newPivotOffset;

        if (Application.isEditor)
            jointsRigged[nuitrack.JointType.Waist].bone.position = basePivotOffset;
    }
}