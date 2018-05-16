using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SkeletonsVisualization : MonoBehaviour 
{
	[SerializeField]GameObject jointPrefab;
	Dictionary<int, GameObject[]> skeletonParts; //dictionary to keep and manage GameObjects for joints;
	NuitrackModules nuitrackModules;

  IssuesProcessor issuesProcessor;

	void Start () 
	{
    issuesProcessor = IssuesProcessor.Instance;
		skeletonParts = new Dictionary<int, GameObject[]>();
		nuitrackModules = FindObjectOfType<NuitrackModules>();
	}
	
	void Update()
	{
		ProcessSkeletons(nuitrackModules.SkeletonData);
	}
	
  void HideAllSkeletons()
  {
    foreach (int userId in skeletonParts.Keys)
    {
      foreach (GameObject go in skeletonParts[userId])
      {
        if (go.activeSelf) go.SetActive(false);
      }
    }
  }

	void ProcessSkeletons(nuitrack.SkeletonData skeletonData)
	{
		if (skeletonData == null) 
    {
      HideAllSkeletons();
      return;
    }
		//Debug.Log("NumUsers: " + skeletonData.NumUsers.ToString());
		
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
		int[] idsInDict = new int[skeletonParts.Keys.Count];
		skeletonParts.Keys.CopyTo(idsInDict, 0);
		for (int i = 0; i < idsInDict.Length; i++)
		{
			for (int j = 0; j < skeletonParts[idsInDict[i]].Length; j++)
			{
				Destroy(skeletonParts[idsInDict[i]][j]);
			}
			skeletonParts.Remove(idsInDict[i]);
		}
		skeletonParts = null;

    if (issuesProcessor != null) Destroy (issuesProcessor.gameObject);
	}
}