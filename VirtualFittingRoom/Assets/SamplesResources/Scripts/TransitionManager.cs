/*===============================================================================
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using System.Collections.Generic;
using Vuforia;

public class TransitionManager : MonoBehaviour
{
    #region PRIVATE_MEMBER_VARIABLES
    float mTransitionCursor;
    float mCurrentTime;
    bool mPlaying;
    bool mBackward;
    BlackMaskBehaviour mBlackMask;
    MixedRealityController.Mode mCurrentMode = MixedRealityController.Mode.HANDHELD_AR;
    #endregion // PRIVATE_MEMBER_VARIABLES


    #region PUBLIC_MEMBER_VARIABLES
    static public bool isFullScreenMode = true;
    public GameObject[] AROnlyObjects;
    public GameObject[] VROnlyObjects;

    [Range(0.1f, 5.0f)]
    public float transitionDuration = 1.5f; // seconds

    public Canvas StereoViewDivider;
    public Animator m_Astronaut, m_Drone;


    public bool InAR { get { return mTransitionCursor <= 0.66f; } }
    #endregion PUBLIC_MEMBER_VARIABLES


    #region MONOBEHAVIOUR_METHODS
    void Awake()
    {
        VuforiaARController.Instance.RegisterVuforiaInitializedCallback(SetStereoViewDivider);
    }

    void OnDestroy()
    {
        VuforiaARController.Instance.UnregisterVuforiaInitializedCallback(SetStereoViewDivider);
    }

    void Start()
    {
        // At start we assume we are in AR
        mTransitionCursor = 0;

        mBlackMask = FindObjectOfType<BlackMaskBehaviour>();
        SetBlackMaskVisible(false, 0);

        VideoBackgroundManager.Instance.SetVideoBackgroundEnabled(true);

        mCurrentMode = GetMixedRealityMode();
        MixedRealityController.Instance.SetMode(mCurrentMode);

        UpdateVisibleObjects();

        mCurrentTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        float time = Time.realtimeSinceStartup;
        float deltaTime = Mathf.Clamp01(time - mCurrentTime);
        mCurrentTime = time;

        // We need to check if the video background is curently enabled
        // because Vuforia may restart the video background when the App is resumed
        // even if the app was paused in VR mode
        bool isVideoCurrentlyEnabled = IsVideoBackgroundRenderingEnabled();

        MixedRealityController.Mode mixedRealityMode = GetMixedRealityMode();

        if ((mCurrentMode != mixedRealityMode) || (InAR != isVideoCurrentlyEnabled))
        {
            // mixed reality mode to switch to
            mCurrentMode = mixedRealityMode;

            // When we transition to VR, we deactivate the Datasets 
            // before setting the mixed reality mode.
            // so to reduce CPU usage, as tracking is not needed in this phase
            // (with AutoStopCameraIfNotRequired ON by default, camera/tracker
            //  will be turned off for performance optimization).

            if (mCurrentMode == MixedRealityController.Mode.HANDHELD_VR
                || mCurrentMode == MixedRealityController.Mode.VIEWER_VR)
            {
                Debug.Log("Switching to VR: deactivating datasets");
                ActivateDatasets(false);
            }

            // As we are moving back to AR, we re-activate the Datasets,
            // before setting the mixed reality mode.
            // this will ensure that the Tracker and Camera are restarted, 
            // in case they were previously stopped when moving to VR
            // before activating the AR mode
            if (mCurrentMode == MixedRealityController.Mode.HANDHELD_AR
                || mCurrentMode == MixedRealityController.Mode.VIEWER_AR)
            {
                Debug.Log("Switching to AR: activating datasets");
                ActivateDatasets(true);
            }

            MixedRealityController.Instance.SetMode(mCurrentMode);
            UpdateVisibleObjects();
        }

        if (mPlaying)
        {
            float fadeFactor = 0;
            if (mTransitionCursor < 0.33f)
            {
                // fade to full black in first part of transition
                fadeFactor = Mathf.SmoothStep(0, 1, mTransitionCursor / 0.33f);
            }
            else if (mTransitionCursor < 0.66f)
            {
                // between 33% and 66% we stay in full black
                fadeFactor = 1;
            }
            else // > 0.66
            {
                // between 66% and 100% we fade out
                fadeFactor = Mathf.SmoothStep(1, 0, (mTransitionCursor - 0.66f) / 0.33f);
            }
            SetBlackMaskVisible(true, fadeFactor);

            float delta = (mBackward ? -1 : 1) * deltaTime / transitionDuration;
            mTransitionCursor += delta;

            if (mTransitionCursor <= 0 || mTransitionCursor >= 1)
            {
                // Done: stop animated transition
                mTransitionCursor = Mathf.Clamp01(mTransitionCursor);
                mPlaying = false;
                SetBlackMaskVisible(false, 0);
            }
        }
    }
    #endregion // MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS
    public void Play(bool reverse)
    {
        // dont' restart playing during a transition
        if (!mPlaying)
        {
            mPlaying = true;
            mBackward = reverse;
            mTransitionCursor = mBackward ? 1 : 0;
        }
    }
    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS
    void SetStereoViewDivider()
    {
        if (!isFullScreenMode)
        {
            // Activate the alignment bar if using stereo views
            //Canvas stereoDivisionCanvas = GetComponent<Canvas>();
            if (StereoViewDivider)
                StereoViewDivider.enabled = true;
        }
    }

    void ActivateDatasets(bool enableDataset)
    {
        //Disable/Enable dataset
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        IEnumerable<DataSet> datasets = objectTracker.GetDataSets();

        foreach (DataSet dataset in datasets)
        {
            if (enableDataset)
                objectTracker.ActivateDataSet(dataset);
            else
                objectTracker.DeactivateDataSet(dataset);
        }
    }

    MixedRealityController.Mode GetMixedRealityMode()
    {
        if (InAR)
        {
            return isFullScreenMode ?
                MixedRealityController.Mode.HANDHELD_AR : MixedRealityController.Mode.VIEWER_AR;
        }

        return isFullScreenMode ?
            MixedRealityController.Mode.HANDHELD_VR : MixedRealityController.Mode.VIEWER_VR;
    }


    void UpdateVisibleObjects()
    {
        foreach (var go in VROnlyObjects)
        {
            go.SetActive(!InAR);
        }

        // Start Astronaut and Drone animations in VR mode
        if (!InAR)
        {
            if (m_Astronaut)
            {
                m_Astronaut.SetBool("IsDrilling", !InAR);
            }

            if (m_Drone != null)
            {
                m_Drone.SetBool("IsScanning", !InAR);
                m_Drone.SetBool("IsShowingLaser", !InAR);
                m_Drone.SetBool("IsFacingObject", !InAR);
            }
        }
    }

    void SetBlackMaskVisible(bool visible, float fadeFactor)
    {
        if (mBlackMask)
        {
            mBlackMask.GetComponent<Renderer>().enabled = visible;
            mBlackMask.SetFadeFactor(fadeFactor);
        }
    }

    bool IsVideoBackgroundRenderingEnabled()
    {
        var backgroundPlaneBehaviour = GetCameraRigRoot().GetComponentInChildren<BackgroundPlaneBehaviour>();
        return (backgroundPlaneBehaviour && backgroundPlaneBehaviour.GetComponent<MeshRenderer>().enabled);
    }

    Transform GetCameraRigRoot()
    {
        var eyewear = DigitalEyewearARController.Instance;
        var vuforia = VuforiaBehaviour.Instance;
        return (eyewear.CentralAnchorPoint ? eyewear.CentralAnchorPoint.transform.root : vuforia.transform);
    }
    #endregion PRIVATE_METHODS
}
