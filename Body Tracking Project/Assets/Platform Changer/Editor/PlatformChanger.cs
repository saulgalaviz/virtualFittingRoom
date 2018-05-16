using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class PlatformChanger : EditorWindow {
    public PlatformSets[] platformSets;
    public Platform targetPlatform;

    PlatformSetsData setsData;

    Dictionary<Platform, PlatformSets> platforms = new Dictionary<Platform, PlatformSets>();

    bool valueChanged = false;

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/Platform Changer")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(PlatformChanger));
    }

    void OnEnable()
    {
        LoadData();

        platformSets = setsData.platformSets;
    }

    void LoadData()
    {
        setsData = Resources.Load("PlatformChangerData") as PlatformSetsData;
        if (setsData == null)
        {
            setsData = CreateInstance<PlatformSetsData>();
            AssetDatabase.CreateAsset(setsData, "Assets/Resources/PlatformChangerData.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PlatformChangerData.asset created");
        }
    }

    void OnValidate()
    {
        valueChanged = true;
    }

    void SaveSettings()
    {
        LoadData();

        valueChanged = false;
        setsData.platformSets = platformSets;
        EditorUtility.SetDirty(setsData);
    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);

        SerializedProperty platformProperty = so.FindProperty("targetPlatform");
        EditorGUILayout.PropertyField(platformProperty); // True means show children
        so.ApplyModifiedProperties(); // Remember to apply modified properties

        SerializedProperty stringsProperty = so.FindProperty("platformSets");
        EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
        so.ApplyModifiedProperties(); // Remember to apply modified properties

        if (GUILayout.Button("Change Platform"))
        {
            ChangePlatform();
            PlayerSettings.virtualRealitySupported = GetPlatform().VRSupported;
            PlayerSettings.productName = GetPlatform().nameProduct;
			PlayerSettings.applicationIdentifier = GetPlatform().bundleID;
            PlayerPrefs.SetString("Platform", GetPlatform().platformName.ToString());
            QualitySettings.antiAliasing = 2;
            UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Android, GetPlatform().vrSDK);
        }

        if (GUILayout.Button(valueChanged ? "Save Settings*" : "Save Settings"))
        {
            SaveSettings();
        }

        if (PlayerPrefs.GetString("Platform") != "")
            GUILayout.TextField("Current Platform: " + PlayerPrefs.GetString("Platform"));
        else
            GUILayout.TextField("Press [Change Platform] button");

        GUILayout.TextField("VR support: " + PlayerSettings.virtualRealitySupported);
		GUILayout.TextField("Bundle Id: " + PlayerSettings.applicationIdentifier);
        GUILayout.TextField("Name: " + PlayerSettings.productName);
    }

    public void ChangePlatform()
    {
        string path = Application.dataPath + @"\Plugins\Android\AndroidManifest.xml";
        TextAsset manifestText = new TextAsset();

        platforms.Clear();
        foreach (PlatformSets sets in platformSets)
        {
            platforms.Add(sets.platformName, sets);
            sets.name = sets.platformName.ToString();
        }

        if (platforms.ContainsKey(targetPlatform))
        {
            manifestText = platforms[targetPlatform].manifest;
        }
        else
            Debug.LogError("Not find " + targetPlatform + " key");

        if (manifestText != null)
        {
            File.WriteAllText(path, manifestText.text);
            Debug.Log("Platform changed " + " #" + (int)targetPlatform + " " + targetPlatform);
        }
        else
            Debug.LogError("Not assigned " + targetPlatform + " manifest");

        PlatformSetsData setsData = Resources.Load("PlatformChangerData") as PlatformSetsData;

        setsData.platformSets = platformSets;
        setsData.currentPlatform = targetPlatform;
        EditorUtility.SetDirty(setsData);
    }

    public PlatformSets GetPlatform()
    {
        return platforms[targetPlatform];
    }
}
