using UnityEngine;

[System.Serializable]
class ModelJoint
{
    /// <summary> Transform model bone </summary>
    public Transform bone;
    public nuitrack.JointType jointType;
    [HideInInspector] public Quaternion baseRotOffset;

    //For "Direct translation"
    public nuitrack.JointType parentJointType;
    /// <summary> Base model bones rotation offsets</summary>
    [HideInInspector] public Transform parentBone;
    // <summary> Base distance to parent bone </summary>
    [HideInInspector] public float baseDistanceToParent;
}
