/*============================================================================== 
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved. 

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/
using UnityEngine;
using System.Collections;
using Vuforia;

public class Reticle : MonoBehaviour
{
    #region PRIVATE_METHODS
    private const float mScale = 0.012f; // relative to viewport width
    #endregion


    #region MONOBEHAVIOUR_METHODS
    void Update()
    {
        Camera cam = DigitalEyewearARController.Instance.PrimaryCamera ?? Camera.main;
        if (cam.projectionMatrix.m00 > 0 || cam.projectionMatrix.m00 < 0)
        {
            // We adjust the reticle depth
            if (VideoBackgroundManager.Instance.VideoBackgroundEnabled)
            {
                // When the frustum skewing is applied (e.g. in AR view),
                // we shift the Reticle at the background depth,
                // so that the reticle appears in focus in stereo view
                BackgroundPlaneBehaviour bgPlane = cam.GetComponentInChildren<BackgroundPlaneBehaviour>();
                float bgDepth = bgPlane.transform.localPosition.z;
                if (bgDepth > cam.nearClipPlane)
                    this.transform.localPosition = Vector3.forward * bgDepth;
                else
                    this.transform.localPosition = Vector3.forward * (cam.nearClipPlane + 0.5f);
            }
            else
            {
                // if the frustum is not skewed, then we apply a default depth (which works nicely in VR view)
                this.transform.localPosition = Vector3.forward * (cam.nearClipPlane + 0.5f);
            }

            // We scale the reticle to be a small % of viewport width
            float localDepth = this.transform.localPosition.z;
            float tanHalfFovX = 1.0f / cam.projectionMatrix[0, 0];
            float tanHalfFovY = 1.0f / cam.projectionMatrix[1, 1];
            float maxTanFov = Mathf.Max(tanHalfFovX, tanHalfFovY);
            float viewWidth = 2 * maxTanFov * localDepth;
            this.transform.localScale = new Vector3(mScale * viewWidth, mScale * viewWidth, 1);
        }
    }
    #endregion // MONOBEHAVIOUR_METHODS
}
