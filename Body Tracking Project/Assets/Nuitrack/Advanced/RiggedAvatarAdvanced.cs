using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RiggedAvatarAdvanced : MonoBehaviour
{
    [Header("Rigged model")]
    [SerializeField] ModelJoint[] modelJoints;
    [SerializeField] Transform head;
    [SerializeField] Transform headTransform;
    Vector3 startPoint; //"Waist" model bone position on start

    TPoseCalibration tPC;
    Vector3 basePivotOffset;

    /// <summary> Model bones </summary>
    Dictionary<nuitrack.JointType, ModelJoint> jointsRigged = new Dictionary<nuitrack.JointType, ModelJoint>();

    void Start()
    {
        for (int i = 0; i < modelJoints.Length; i++)
        {
            modelJoints[i].baseRotOffset = modelJoints[i].bone.rotation;
            jointsRigged.Add(modelJoints[i].jointType, modelJoints[i]);
        }

        startPoint = jointsRigged[nuitrack.JointType.Waist].bone.position;
    }

    void OnEnable()
    {
        tPC = FindObjectOfType<TPoseCalibration>();
        tPC.onSuccess += OnSuccessCalib;
    }

    void Update()
    {
        if (CurrentUserTracker.CurrentSkeleton != null) ProcessSkeleton(CurrentUserTracker.CurrentSkeleton);
    }

    private void LateUpdate()
    {
        head.position = headTransform.position;
    }

    void ProcessSkeleton(nuitrack.Skeleton skeleton)
    {
        //Calculate the model position: take the Torso position and invert movement along the Z axis
        Vector3 torsoPos = Quaternion.Euler(0f, 180f, 0f) * (0.001f * skeleton.GetJoint(nuitrack.JointType.Torso).ToVector3());
        transform.position = torsoPos + basePivotOffset;

        foreach (var riggedJoint in jointsRigged)
        {
            //Get joint from the Nuitrack
            nuitrack.Joint joint = skeleton.GetJoint(riggedJoint.Key);

            ModelJoint modelJoint = riggedJoint.Value;

            //Calculate the model bone rotation: take the mirrored joint orientation, add a basic rotation of the model bone, invert movement along the Z axis
            Quaternion jointOrient = Quaternion.Inverse(CalibrationInfo.SensorOrientation) * (joint.ToQuaternionMirrored()) * modelJoint.baseRotOffset;
            modelJoint.bone.rotation = jointOrient;
        }
    }

    private void OnSuccessCalib(Quaternion rotation)
    {
        StartCoroutine(CalculateOffset());
    }

    public IEnumerator CalculateOffset()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Vector3 basePos = jointsRigged[nuitrack.JointType.Waist].bone.position;
        Vector3 newPivotOffset;
        newPivotOffset = startPoint - basePos + basePivotOffset;
        newPivotOffset.x = 0;

        basePivotOffset = newPivotOffset;
        Debug.Log("Сдвиг");
        //if (Application.isEditor)
        //    jointsRigged[nuitrack.JointType.Waist].bone.position = basePivotOffset;
    }

    void OnDisable()
    {
        tPC.onSuccess -= OnSuccessCalib;
    }
}