using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum NeedVicoVR
{
    NotNecessary,
    Yes,
    No,
}

public class EnviromentSwitcher : MonoBehaviour {

    [SerializeField]Platform targetPlatform;
    [SerializeField]GameObject[] showGO = null, hideGO = null;
    //[SerializeField]NeedVicoVR needVicoVR;

    private void Awake()
    {
        GameVersion.GetData();
        Switch();
    }

    void Switch()
    {
        if (GameVersion.currentPlatform == targetPlatform)
        {
            //if ((needVicoVR == NeedVicoVR.Yes) && !GameVersion.usedVicoVR)
            //    return;

            //if ((needVicoVR == NeedVicoVR.No) && GameVersion.usedVicoVR)
            //    return;

            for (int i = 0; i < showGO.Length; i++)
            {
                showGO[i].SetActive(true);
            }

            for (int i = 0; i < hideGO.Length; i++)
            {
                hideGO[i].SetActive(false);
            }
        }
    }
}
