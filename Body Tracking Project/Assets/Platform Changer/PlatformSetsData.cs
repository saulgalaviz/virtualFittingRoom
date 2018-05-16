using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Platform
{
    Default,
    GearVR,
	IOS,
    //PicoVR,
}

[System.Serializable]
public class PlatformSets
{
    [HideInInspector]
    public string name; // for array naming
    public Platform platformName;
    public bool VRSupported = false;
    public string nameProduct = "Product Name";
    public string bundleID = "Com.CompanyName.ProductName";
    public TextAsset manifest;
    public int AntiAliasing = 0;
    public string[] vrSDK;
}

[System.Serializable]
public class PlatformSetsData : ScriptableObject
{
    public PlatformSets[] platformSets = new PlatformSets[] { new PlatformSets(), new PlatformSets() };
    public Platform currentPlatform = new Platform();
}