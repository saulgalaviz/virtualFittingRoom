/*===============================================================================
Copyright (c) 2016-2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

public class SamplesLoadingScreen : LoadingScreen
{
    #region PROTECTED_METHODS

    protected override void LoadNextSceneAsync()
    {
        string sceneName = SamplesMainMenu.GetSceneToLoad();

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
    }

    #endregion // PROTECTED_METHODS

}
