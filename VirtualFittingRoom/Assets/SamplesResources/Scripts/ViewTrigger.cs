/*===============================================================================
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using System.Collections;

public class ViewTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        VR_TRIGGER,
        AR_TRIGGER
    }

    #region PUBLIC_MEMBER_VARIABLES
    public TriggerType triggerType = TriggerType.VR_TRIGGER;
    public float activationTime = 1.5f;
    public Material focusedMaterial;
    public Material nonFocusedMaterial;
    public bool Focused { get; set; }
    #endregion // PUBLIC_MEMBER_VARIABLES


    #region PRIVATE_MEMBER_VARIABLES
    float mFocusedTime;
    bool mTriggered;
    TransitionManager mTransitionManager;
    Transform cameraTransform;
    #endregion // PRIVATE_MEMBER_VARIABLES


    #region MONOBEHAVIOUR_METHODS
    void Start()
    {
        cameraTransform = Camera.main.transform;
        mTransitionManager = FindObjectOfType<TransitionManager>();
        GetComponent<Renderer>().material = nonFocusedMaterial;
    }

    void Update()
    {
        RaycastHit hit;
        Ray cameraGaze = new Ray(cameraTransform.position, cameraTransform.forward);
        Physics.Raycast(cameraGaze, out hit, Mathf.Infinity);
        Focused = hit.collider && (hit.collider.gameObject == gameObject);


        if (mTriggered)
            return;

        UpdateMaterials(Focused);

        bool startAction = false || Input.GetMouseButtonUp(0);

        if (Focused)
        {
            // Update the "focused state" time
            mFocusedTime += Time.deltaTime;
            if ((mFocusedTime > activationTime) || startAction)
            {
                mTriggered = true;
                mFocusedTime = 0;

                // Activate transition from AR to VR or vice versa
                bool goingBackToAR = (triggerType == TriggerType.AR_TRIGGER);
                mTransitionManager.Play(goingBackToAR);
                StartCoroutine(ResetAfter(0.3f * mTransitionManager.transitionDuration));
            }
        }
        else
        {
            // Reset the "focused state" time
            mFocusedTime = 0;
        }
    }
    #endregion // MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS
    private void UpdateMaterials(bool focused)
    {
        Renderer meshRenderer = GetComponent<Renderer>();

        meshRenderer.material = focused ? focusedMaterial : nonFocusedMaterial;

        float t = focused ? Mathf.Clamp01(mFocusedTime / activationTime) : 0;

        foreach (var rnd in GetComponentsInChildren<Renderer>())
        {
            if (rnd.material.shader.name.Equals("Custom/SurfaceScan"))
            {
                rnd.material.SetFloat("_ScanRatio", t);
            }
        }
    }

    private IEnumerator ResetAfter(float seconds)
    {
        Debug.Log("Resetting View trigger after: " + seconds);

        yield return new WaitForSeconds(seconds);

        Debug.Log("Resetting View trigger: " + name);

        // Reset variables
        mTriggered = false;
        mFocusedTime = 0;
        Focused = false;
        UpdateMaterials(false);
    }
    #endregion // PRIVATE_METHODS
}

