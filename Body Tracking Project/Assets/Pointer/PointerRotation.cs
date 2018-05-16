using UnityEngine;

public class PointerRotation : MonoBehaviour {

	public int hand;
    public Transform target;

    public float speed = 0.1F;
    
    void Start () {

    }

    // Update is called once per frame
    void LateUpdate () {
        if (CurrentUserTracker.CurrentSkeleton == null)
            return;

        hand = PointerPassing.hand;

        //transform.rotation = Quaternion.identity;

        if (hand % 2 == 0)
        {
            Quaternion targetRot = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.RightHand).ToQuaternion();
            transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(targetRot.x * -1, targetRot.y * 1, targetRot.z * -1, targetRot.w * 1), speed * Time.deltaTime);
        }
        else
        {
            Quaternion targetRot = CurrentUserTracker.CurrentSkeleton.GetJoint(nuitrack.JointType.LeftHand).ToQuaternion();
            transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(targetRot.x * -1, targetRot.y * 1, targetRot.z * -1, targetRot.w * 1), speed * Time.deltaTime);
        }
    }
}
