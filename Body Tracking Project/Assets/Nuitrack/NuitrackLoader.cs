using System;
using System.Collections;
using UnityEngine;
using nuitrack;
using System.Threading;

public enum NuitrackInitState
{
	INIT_OK = 0,
	INIT_NUITRACK_MANAGER_NOT_INSTALLED = 1,
	INIT_NUITRACK_RESOURCES_NOT_INSTALLED = 2,
	INIT_NUITRACK_SERVICE_ERROR = 3,
	INIT_NUITRACK_NOT_SUPPORTED = 4
}

public static class NuitrackLoader
{
	public static bool initComplete;

	public static NuitrackInitState initState = NuitrackInitState.INIT_NUITRACK_NOT_SUPPORTED;

	public static NuitrackInitState InitNuitrackLibraries()
	{
		#if UNITY_ANDROID
		Debug.Log ("InitNuitrackLibraries() starts.");
		try
		{
			initComplete = false;
			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaClass javaNuitrackClass = new AndroidJavaClass("com.tdv.nuitrack.sdk.Nuitrack");
			javaNuitrackClass.CallStatic("init", jo, new NuitrackCallback());
			while (!initComplete)
			{
				Thread.Sleep(50);
			}
		}
		catch (System.Exception ex)
		{
			Debug.Log("Exception: " + ex);
		}
		#endif
		return initState;
	}
}

public class NuitrackCallback : AndroidJavaProxy
{
	public NuitrackCallback() : base("com.tdv.nuitrack.sdk.Nuitrack$NuitrackCallback") { }
	void onInitSuccess(AndroidJavaObject context)
	{
		Debug.Log ("Nuitrack callback: onInitSuccess");
		NuitrackLoader.initState = NuitrackInitState.INIT_OK;
		NuitrackLoader.initComplete = true;
	}
	void onInitFailure(int errorId)
	{
		Debug.Log ("Nuitrack callback: onInitFailure");
		NuitrackLoader.initState = (NuitrackInitState)errorId;
		NuitrackLoader.initComplete = true;
	}
}