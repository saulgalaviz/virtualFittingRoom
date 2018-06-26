/*============================================================================== 
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.

 * Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using UnityEngine;
using System.Collections;
using Vuforia;

public class Autofocus : MonoBehaviour
{
    #region MONOBEHAVIOUR_METHODS
    void Start () 
    {
        var vuforia = VuforiaARController.Instance;
        vuforia.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        vuforia.RegisterOnPauseCallback(OnPaused);
    }

    void Update()
    {
        // Trigger an autofocus event on tap
        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(TriggerAutofocus());
        }
    }

    
    #endregion // MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS
    private void OnVuforiaStarted()
    {
        // Try to enable continuous autofocus mode
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    private void OnPaused(bool paused)
    {
        if (!paused) // resumed
        {
            // Set again autofocus mode when app is resumed
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }

    private IEnumerator TriggerAutofocus()
    {
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);

        // Wait 2 seconds
        yield return new WaitForSeconds(2);

        // Restore continuous autofocus mode
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }
    #endregion // PRIVATE_METHODS

}
