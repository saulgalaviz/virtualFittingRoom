/*============================================================================== 
Copyright (c) 2015-2017 PTC Inc. All Rights Reserved.
Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   
==============================================================================*/
using UnityEngine;
using System.Collections;

public class BackToAbout : MonoBehaviour
{
    #region PRIVATE_MEMBERS
    private const float DOUBLE_TAP_MAX_DELAY = 0.5f;//seconds
    private int mTapCount = 0;
    private float mTimeSinceLastTap = 0;
    #endregion //PRIVATE_MEMBERS

    #region MONOBEHAVIOUR_METHODS
    void Start()
    {
        mTapCount = 0;
        mTimeSinceLastTap = 0;
    }

    void Update()
    {
#if UNITY_ANDROID
        // On Android, the Back button is mapped to the Esc key
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GoToAboutPage();
        }
#else
        HandleTap();
#endif
    }
    #endregion // MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS
    public void GoToAboutPage()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("1-About");
    }
    #endregion

    #region PRIVATE_METHODS
    private void HandleTap()
    {
        if (mTapCount == 1)
        {
            mTimeSinceLastTap += Time.deltaTime;
            if (mTimeSinceLastTap > DOUBLE_TAP_MAX_DELAY)
            {
                // reset touch count and timer
                mTapCount = 0;
                mTimeSinceLastTap = 0;
            }
        }
        else if (mTapCount == 2)
        {
            // we got a double tap
            OnDoubleTap();

            // reset touch count and timer
            mTimeSinceLastTap = 0;
            mTapCount = 0;
        }

        if (Input.GetMouseButtonUp(0))
        {
            mTapCount++;
        }
    }

    protected virtual void OnDoubleTap()
    {
        GoToAboutPage();
    }
    #endregion // PRIVATE_METHODS

}
