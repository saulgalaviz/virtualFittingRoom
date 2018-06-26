/*===============================================================================
Copyright (c) 2015-2018 PTC Inc. All Rights Reserved.
 
Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class MenuOptions : MonoBehaviour
{
    #region PRIVATE_MEMBERS
    CameraSettings m_CameraSettings;
    TrackableSettings m_TrackableSettings;
    Toggle m_ExtTrackingToggle, m_AutofocusToggle, m_FlashToggle, m_FrontCamToggle;
    Canvas m_OptionsMenuCanvas;
    OptionsConfig m_OptionsConfig;
    #endregion //PRIVATE_MEMBERS

    public bool IsDisplayed { get; private set; }

    #region MONOBEHAVIOUR_METHODS
    protected virtual void Start()
    {
        m_CameraSettings = FindObjectOfType<CameraSettings>();
        m_TrackableSettings = FindObjectOfType<TrackableSettings>();
        m_OptionsConfig = FindObjectOfType<OptionsConfig>();
        m_OptionsMenuCanvas = GetComponentInChildren<Canvas>(true);
        m_ExtTrackingToggle = FindUISelectableWithText<Toggle>("Extended");
        m_AutofocusToggle = FindUISelectableWithText<Toggle>("Autofocus");
        m_FlashToggle = FindUISelectableWithText<Toggle>("Flash");
        m_FrontCamToggle = FindUISelectableWithText<Toggle>("FrontCamera");

        var vuforia = VuforiaARController.Instance;
        vuforia.RegisterOnPauseCallback(OnPaused);
    }
    #endregion //MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS

    public void ToggleAutofocus(bool enabled)
    {
        if (m_CameraSettings)
            m_CameraSettings.SwitchAutofocus(enabled);
    }

    public void ToggleTorch(bool enabled)
    {
        if (m_FlashToggle && m_CameraSettings)
        {
            m_CameraSettings.SwitchFlashTorch(enabled);

            // Update UI toggle status (ON/OFF) in case the flash switch failed
            m_FlashToggle.isOn = m_CameraSettings.IsFlashTorchEnabled();
        }
    }

    public void ToggleFrontCamera(bool enabled)
    {
        if (m_CameraSettings)
        {
            m_CameraSettings.SelectCamera(m_CameraSettings.IsFrontCameraActive() ?
                                          CameraDevice.CameraDirection.CAMERA_BACK :
                                          CameraDevice.CameraDirection.CAMERA_FRONT);

            // Toggle flash if it is on while switching to front camera
            if (m_CameraSettings.IsFrontCameraActive() && m_FlashToggle && m_FlashToggle.isOn)
                ToggleTorch(false);
        }
    }

    public void ToggleExtendedTracking(bool enabled)
    {
        if (m_TrackableSettings)
            m_TrackableSettings.SwitchExtendedTracking(enabled);
    }

    public void ActivateDataset(string datasetName)
    {
        if (m_TrackableSettings)
            m_TrackableSettings.ActivateDataSet(datasetName);
    }

    public void UpdateUI()
    {
        if (m_ExtTrackingToggle && m_TrackableSettings)
            m_ExtTrackingToggle.isOn = m_TrackableSettings.IsExtendedTrackingEnabled();

        if (m_FlashToggle && m_CameraSettings)
            m_FlashToggle.isOn = m_CameraSettings.IsFlashTorchEnabled();

        if (m_AutofocusToggle && m_CameraSettings)
            m_AutofocusToggle.isOn = m_CameraSettings.IsAutofocusEnabled();

        if (m_FrontCamToggle && m_CameraSettings)
            m_FrontCamToggle.isOn = m_CameraSettings.IsFrontCameraActive();
    }

    public void RestartObjectTracker()
    {
        var objTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        if (objTracker != null && objTracker.IsActive)
        {
            objTracker.Stop();

            foreach (DataSet dataset in objTracker.GetDataSets())
            {
                objTracker.DeactivateDataSet(dataset);
                objTracker.ActivateDataSet(dataset);
            }

            objTracker.Start();
        }
    }

    public void ShowOptionsMenu(bool show)
    {
        if (m_OptionsConfig && m_OptionsConfig.AnyOptionsEnabled())
        {
            if (show)
            {
                UpdateUI();
                m_OptionsMenuCanvas.gameObject.SetActive(true);
                m_OptionsMenuCanvas.enabled = true;
                IsDisplayed = true;
            }
            else
            {
                m_OptionsMenuCanvas.gameObject.SetActive(false);
                m_OptionsMenuCanvas.enabled = false;
                IsDisplayed = false;
            }
        }
    }

    #endregion //PUBLIC_METHODS


    #region PROTECTED_METHODS
    protected T FindUISelectableWithText<T>(string text) where T : UnityEngine.UI.Selectable
    {
        T[] uiElements = GetComponentsInChildren<T>(true);
        foreach (var uielem in uiElements)
        {
            string childText = uielem.GetComponentInChildren<Text>().text;
            if (childText.Contains(text))
                return uielem;
        }
        return null;
    }
    #endregion //PROTECTED_METHODS

    #region PRIVATE_METHODS
    private void OnPaused(bool paused)
    {
        if (paused)
        {
            // Handle any tasks when app is paused here:
        }
        else
        {
            // Handle any tasks when app is resume here:

            // The flash torch is switched off by the OS automatically when app is paused.
            // On resume, update torch UI toggle to match torch status.
            UpdateUI();
        }
    }
    #endregion //PRIVATE_METHODS

}
