/*===============================================================================
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using System.Collections;
using Vuforia;

public class Button3D : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES
    public float width = 0.2f; // relative to viewport
    public float height = 0.1f; // relative to viewport
    [Range(0, 2)]
    public float fadeDuration = 1.6f;
    #endregion // PUBLIC_MEMBER_VARIABLES


    #region PRIVATE_MEMBER_VARIABLES
    private float mButtonAlpha = 0;
    #endregion // PRIVATE_MEMBER_VARIABLES


    #region MONOBEHAVIOUR_METHODS
    void Update () 
    {
        Camera cam = DigitalEyewearARController.Instance.PrimaryCamera;

        // Updte position of ARButton to always be about 2 meters below the camera
        Vector3 camPos = cam.transform.position;
        Vector3 camForwardDir = cam.transform.forward;
        Vector3 camUpDir = cam.transform.up;
        Vector3 lookUpDir = (camUpDir + camForwardDir) / 2;
        Vector3 forwardDir = new Vector3(lookUpDir.x, 0, lookUpDir.z);
        forwardDir.Normalize();

        this.transform.position = camPos - 1.5f * Vector3.up + forwardDir;

        // Apply rotation and scale
        Vector3 toCameraVec = camPos - this.transform.position;
        float camDist = toCameraVec.magnitude;
        if (camDist > cam.nearClipPlane)
        { 
            // Orient the button surface to face the viewer (like a billboard)
            this.transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);

            // Adjust the scale
            float tanHalfFovX = 1.0f / cam.projectionMatrix[0, 0];
            float sx = width * tanHalfFovX * camDist;
            float sy = sx / 4; // maintain fixed aspect ratio
            this.transform.localScale = new Vector3(sx, sy, 1);
        }
    }

    void LateUpdate()
    {
        Camera cam = DigitalEyewearARController.Instance.PrimaryCamera;

        Vector2 vp = cam.WorldToViewportPoint(this.transform.position);
        if (vp.x > 0.2f && vp.x < 0.8f && vp.y > 0.2f && vp.y < 0.8f)
        {
            if (mButtonAlpha < 1)
                mButtonAlpha += Time.deltaTime / fadeDuration;
        }
        else
        {
            if (mButtonAlpha > 0)
                mButtonAlpha -= Time.deltaTime / fadeDuration;
        }

        mButtonAlpha = Mathf.Clamp01(mButtonAlpha);
        this.GetComponent<Renderer>().material.SetColor("_BaseColor", new Color(1.0f, 1.0f, 1.0f, mButtonAlpha));
    }
    #endregion // MONOBEHAVIOUR_METHODS
}
