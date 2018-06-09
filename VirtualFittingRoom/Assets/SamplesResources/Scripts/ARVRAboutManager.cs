/*============================================================================== 
Copyright (c) 2017-2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/

using UnityEngine;
using UnityEngine.UI;

public class ARVRAboutManager : MonoBehaviour
{
    #region PUBLIC_MEMBERS
    public Text aboutText;
    #endregion //PUBLIC_MEMBERS


    #region PRIVATE_MEMBERS
    Button[] buttons;
    #endregion //PRIVATE_MEMBERS


    #region PUBLIC_METHODS
    public void OnStartFullScreen(bool willRunFullScreen)
    {
        TransitionManager.isFullScreenMode = willRunFullScreen;
        LoadNextScene();
    }
    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS
    void LoadNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
    }
    #endregion //PRIVATE_METHODS


    #region MONOBEHAVIOUR_METHODS

    void Start()
    {
        buttons = FindObjectsOfType<Button>();

        UpdateAboutText();
    }

    void Update()
    {
#if UNITY_ANDROID
        // On Android, the Back button is mapped to the Esc key
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            // Exit app
            Application.Quit();
        }
#endif
    }
    #endregion // MONOBEHAVIOUR_METHODS


    void LateUpdate()
    {
        // XR Virtual Reality might not enable at time of Start(), so attempt it in LateUpdate()
        if (!UnityEngine.XR.XRSettings.enabled)
        {
            Debug.Log("Attempting to set XR Settings to enabled.");
            UnityEngine.XR.XRSettings.enabled = true;
            // Enable Viewer (stereo) mode button only if XR Virtual Reality is enabled.
            buttons[1].interactable = UnityEngine.XR.XRSettings.enabled;
            if (UnityEngine.XR.XRSettings.enabled)
            {
                UpdateAboutText();
            }
        }
    }

    void UpdateAboutText()
    {
        if (!aboutText) return;

        string vuforiaVersion = Vuforia.VuforiaUnity.GetVuforiaLibraryVersion();
        string unityVersion = Application.unityVersion;

        string vuforia = Vuforia.VuforiaRuntime.Instance.InitializationState != Vuforia.VuforiaRuntime.InitState.NOT_INITIALIZED
                                ? "<color=green>Yes</color>"
                                : "<color=red>No (enable Vuforia in XR Settings)</color>";

        string xr = UnityEngine.XR.XRSettings.enabled
                               ? "<color=green>" + UnityEngine.XR.XRSettings.loadedDeviceName + "</color>"
                               : "<color=red>Disabled (enable VR in XR Settings)</color>";

        string about =
            "\n<size=26>Description:</size>" +
            "\nThis sample demonstrates a mixed reality experience that starts in AR and moves to VR." +
            "\n" +
            "\n<size=26>Key Functionality:</size>" +
            "\n• Transition between AR camera tracking and VR device tracking" +
            "\n" +
            "\n<size=26>Physical Targets:</size>" +
            "\n• ImageTarget: Astronaut (Included with Sample)" +
            "\n" +
            "\n<size=26>Instructions:</size>" +
            "\n• Point camera at target to view" +
            "\n• Aim the cursor at the button (labeled “VR”) to trigger the transition to VR" +
            "\n• In VR mode, look at the button on the floor to return to AR" +
            "\n" +
            "\n<size=26>Build Version Info:</size>" +
            "\n• Vuforia " + vuforiaVersion +
            "\n• Unity " + unityVersion +
            "\n" +
            "\n<size=26>Project Settings Info:</size>" +
            "\n• Vuforia Enabled: " + vuforia +
            "\n• XR Virtual Reality: " + xr +
            "\n" +
            "\n<size=26>Statistics:</size>" +
            "\nData collected is used solely for product quality improvements" +
            "\nhttps://developer.vuforia.com/legal/statistics" +
            "\n" +
            "\n<size=26>Developer Agreement:</size>" +
            "\nhttps://developer.vuforia.com/legal/vuforia-developer-agreement" +
            "\n" +
            "\n<size=26>Privacy Policy:</size>" +
            "\nhttps://developer.vuforia.com/legal/privacy" +
            "\n" +
            "\n<size=26>Terms of Use:</size>" +
            "\nhttps://developer.vuforia.com/legal/EULA" +
            "\n" +
            "\n© 2017 PTC Inc. All Rights Reserved." +
            "\n";

        aboutText.text = about;

        Debug.Log("Vuforia " + vuforiaVersion + "\nUnity " + unityVersion);

    }

}
