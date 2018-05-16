using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVersion {

    public static Platform? currentPlatform = null;
    //public static bool usedVicoVR = false;

    public static void GetData()
    {
        if (currentPlatform == null)
        {
            PlatformSetsData setsData = Resources.Load("PlatformChangerData") as PlatformSetsData;
            currentPlatform = setsData.currentPlatform;
            Debug.Log("Current Platform " + currentPlatform);
        }
    }
}
