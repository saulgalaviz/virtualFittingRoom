using UnityEngine;
using System.Collections;

public static class NuitrackUtils
{
    public static Vector3 ToVector3(this nuitrack.Vector3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static Vector3 ToVector3(this nuitrack.Joint joint)
    {
	    return new Vector3(joint.Real.X, joint.Real.Y, joint.Real.Z);
    }

    public static Quaternion ToQuaternion(this nuitrack.Joint joint)
    {
        //Vector3 jointRight =  new Vector3( joint.Orient.Matrix[0], joint.Orient.Matrix[3], joint.Orient.Matrix[6] );   //X(Right) not really needed here
        Vector3 jointUp =       new Vector3( joint.Orient.Matrix[1], joint.Orient.Matrix[4], joint.Orient.Matrix[7] );   //Y(Up)
        Vector3 jointForward =  new Vector3( joint.Orient.Matrix[2], joint.Orient.Matrix[5], joint.Orient.Matrix[8] );   //Z(Forward)
        return Quaternion.LookRotation(jointForward, jointUp);
    }

    public static Quaternion ToQuaternionMirrored(this nuitrack.Joint joint)
    {
        /* Debug:
        if (joint.Type == nuitrack.JointType.Torso)
        {
            Debug.Log("Torso matrix: " + 
            joint.Orient.Matrix[0].ToString() + ",  " + joint.Orient.Matrix[3].ToString() + ",  " + joint.Orient.Matrix[6].ToString() + "; " + 
            joint.Orient.Matrix[1].ToString() + ",  " + joint.Orient.Matrix[4].ToString() + ",  " + joint.Orient.Matrix[7].ToString() + "; " + 
            joint.Orient.Matrix[2].ToString() + ",  " + joint.Orient.Matrix[5].ToString() + ",  " + joint.Orient.Matrix[8].ToString());
        }*/ 

        //Vector3 jointRight =  new Vector3(  joint.Orient.Matrix[0], -joint.Orient.Matrix[3],  joint.Orient.Matrix[6] );   //X(Right) not really needed here
        Vector3 jointUp =       new Vector3( -joint.Orient.Matrix[1],  joint.Orient.Matrix[4], -joint.Orient.Matrix[7] );   //Y(Up)
        Vector3 jointForward =  new Vector3(  joint.Orient.Matrix[2], -joint.Orient.Matrix[5],  joint.Orient.Matrix[8] );   //Z(Forward)
	
        if (jointForward.magnitude < 0.01f) return Quaternion.identity; //should not happen
        return Quaternion.LookRotation(jointForward, jointUp);
    }
}
