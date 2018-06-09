/*==============================================================================
Copyright (c) 2016-2017 PTC Inc. All Rights Reserved.

Copyright (c) 2015 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using System.IO;
using UnityEditor;
using UnityEngine;
    
[InitializeOnLoad]
public static class SampleOrientationSetter
{
    private static readonly string VUFORIA_SAMPLE_ORIENTATION_SETTINGS = "VUFORIA_SAMPLE_ORIENTATION_SETTINGS";

    static SampleOrientationSetter()
    {
        EditorApplication.update += UpdateOrientationSettings;
    }

    static void UpdateOrientationSettings()
    {
        // Unregister callback (executed only once)
        EditorApplication.update -= UpdateOrientationSettings;

        BuildTargetGroup androidBuildTarget = BuildTargetGroup.Android;
        
        string androidSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(androidBuildTarget);
        androidSymbols = androidSymbols ?? "";
        if (!androidSymbols.Contains(VUFORIA_SAMPLE_ORIENTATION_SETTINGS)) 
        {
            // Set default orientation to landscape left
            Debug.Log ("Setting default orientation to Landscape left.");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            // Here we set the scripting define symbols for Android
            // so we can remember that the settings were set once.
            PlayerSettings.SetScriptingDefineSymbolsForGroup(androidBuildTarget,
                                                             androidSymbols + ";" + VUFORIA_SAMPLE_ORIENTATION_SETTINGS);
        }
    }  
}
