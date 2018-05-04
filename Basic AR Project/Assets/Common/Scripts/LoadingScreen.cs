/*===============================================================================
Copyright (c) 2015-2018 PTC Inc. All Rights Reserved.
 
Copyright (c) 2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
 
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    #region PRIVATE_MEMBER_VARIABLES
    private bool mChangeLevel = true;
    private RawImage mUISpinner;
    #endregion // PRIVATE_MEMBER_VARIABLES


    #region MONOBEHAVIOUR_METHODS
    void Start()
    {
        mUISpinner = GetComponentInChildren<RawImage>();
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        mChangeLevel = true;
    }

    void Update()
    {
        if (mUISpinner)
            mUISpinner.rectTransform.Rotate(Vector3.forward, 90.0f * Time.deltaTime);

        if (mChangeLevel)
        {
            LoadNextSceneAsync();
            mChangeLevel = false;
        }
    }
    #endregion // MONOBEHAVIOUR_METHODS


    #region PROTECTED_METHODS
    protected virtual void LoadNextSceneAsync()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
    }
    #endregion // PROTECTED_METHODS
}
