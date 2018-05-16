using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SkeletonExample : MonoBehaviour 
{
	[SerializeField]GameObject jointPrefab;

	nuitrack.SkeletonTracker skeletonTracker;

	Dictionary<int, GameObject[]> skeletonParts; //dictionary to keep and manage GameObjects for joints;

	ExceptionsLogger exceptionsLogger;

  //const string debugPath = "/home/stranger/repo/depth_scanner/data/nuitrack/nuitrack.config";
  const string debugPath = "../../data/nuitrack.config";

	void Awake()
	{
		exceptionsLogger = GameObject.FindObjectOfType<ExceptionsLogger>();

		NuitrackInitState state = NuitrackLoader.InitNuitrackLibraries();
		if (state != NuitrackInitState.INIT_OK)
		{
			exceptionsLogger.AddEntry("Nuitrack native libraries iniialization error: " + Enum.GetName(typeof(NuitrackInitState), state));
		}
	}

	void Start () 
	{
		skeletonParts = new Dictionary<int, GameObject[]>();

		//nuitrack initialization and creation of depth and segmentation generators:
		try
		{
      nuitrack.Nuitrack.Init(debugPath);
      skeletonTracker = nuitrack.SkeletonTracker.Create();
			//event handler registerering:
			skeletonTracker.OnSkeletonUpdateEvent += SkeletonUpdate;

			nuitrack.Nuitrack.Run();
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	void Update()
	{
		try
		{
			nuitrack.Nuitrack.Update();
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	void ProcessSkeletons(nuitrack.SkeletonData skeletonData)
	{
		Debug.Log("NumUsers: " + skeletonData.NumUsers.ToString());

		foreach (int userId in skeletonParts.Keys)
		{
			if (skeletonData.GetSkeletonByID(userId) == null)
			{
				foreach (GameObject go in skeletonParts[userId])
				{
					if (go.activeSelf) go.SetActive(false);
				}
			}
		}

		foreach (nuitrack.Skeleton skeleton in skeletonData.Skeletons)
		{
			if (!skeletonParts.ContainsKey(skeleton.ID))
			{
				GameObject[] newJoints = new GameObject[skeleton.Joints.Length];
				for (int i = 0; i < newJoints.Length; i++)
				{
					newJoints[i] = (GameObject)Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
				}
				skeletonParts.Add (skeleton.ID, newJoints);
			}

			//if we don't have ID in dictionary then we create required array of joint GameObjects and add it to dictionary
			GameObject[] skeletonJoints = skeletonParts[skeleton.ID];

			for (int i = 0; i < skeleton.Joints.Length; i++)
			{
				if (skeleton.Joints[i].Confidence > 0.5f)
				{
					if (!skeletonJoints[i].activeSelf) skeletonJoints[i].SetActive(true);
					skeletonJoints[i].transform.position = new Vector3(skeleton.Joints[i].Real.X / 1000f, skeleton.Joints[i].Real.Y / 1000f, skeleton.Joints[i].Real.Z / 1000f);

					//skel.Joints[i].Orient.Matrix:
					// 0, 			1,	 		2, 
					// 3, 			4, 			5,
					// 6, 			7, 			8
					// -------
					// right(X),	up(Y), 		forward(Z)

					//Vector3 jointRight = 	new Vector3(  skeleton.Joints[i].Orient.Matrix[0],  skeleton.Joints[i].Orient.Matrix[3],  skeleton.Joints[i].Orient.Matrix[6] );
					Vector3 jointUp = 		new Vector3(  skeleton.Joints[i].Orient.Matrix[1],  skeleton.Joints[i].Orient.Matrix[4],  skeleton.Joints[i].Orient.Matrix[7] );
					Vector3 jointForward = 	new Vector3(  skeleton.Joints[i].Orient.Matrix[2],  skeleton.Joints[i].Orient.Matrix[5],  skeleton.Joints[i].Orient.Matrix[8] );

					skeletonJoints[i].transform.rotation = Quaternion.LookRotation(jointForward, jointUp);
				}
				else
				{
					if (skeletonJoints[i].activeSelf) skeletonJoints[i].SetActive(false);
				}
			}
		}
	}

	void OnDestroy()
	{
    try
		{
      skeletonTracker = null;
			nuitrack.Nuitrack.Release();
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	#region Event handler methods
	void SkeletonUpdate (nuitrack.SkeletonData _skeletonData)
	{
		ProcessSkeletons(_skeletonData);
	}
	#endregion
}