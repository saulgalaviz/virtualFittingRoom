using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SkeletonVisualization2 : MonoBehaviour 
{
  [SerializeField]GameObject jointPrefab, connectionPrefab;
  NuitrackModules nuitrackModules;

  nuitrack.JointType[] jointsInfo = new nuitrack.JointType[]
  {
    nuitrack.JointType.Head,
    nuitrack.JointType.Neck,
    nuitrack.JointType.LeftCollar,
    nuitrack.JointType.Torso,
    nuitrack.JointType.Waist,
    nuitrack.JointType.LeftShoulder,
    nuitrack.JointType.RightShoulder,
    nuitrack.JointType.LeftElbow,
    nuitrack.JointType.RightElbow,
    nuitrack.JointType.LeftWrist,
    nuitrack.JointType.RightWrist,
    nuitrack.JointType.LeftHand,
    nuitrack.JointType.RightHand,
    nuitrack.JointType.LeftHip,
    nuitrack.JointType.RightHip,
    nuitrack.JointType.LeftKnee,
    nuitrack.JointType.RightKnee,
    nuitrack.JointType.LeftAnkle,
    nuitrack.JointType.RightAnkle
  };

  nuitrack.JointType[,] connectionsInfo = new nuitrack.JointType[,]
  {
    {nuitrack.JointType.Neck,           nuitrack.JointType.Head},
    {nuitrack.JointType.LeftCollar,     nuitrack.JointType.Neck},
    {nuitrack.JointType.LeftCollar,     nuitrack.JointType.LeftShoulder},
    {nuitrack.JointType.LeftCollar,     nuitrack.JointType.RightShoulder},
    {nuitrack.JointType.LeftCollar,     nuitrack.JointType.Torso},
    {nuitrack.JointType.Waist,          nuitrack.JointType.Torso},
    {nuitrack.JointType.Waist,          nuitrack.JointType.LeftHip},
    {nuitrack.JointType.Waist,          nuitrack.JointType.RightHip},
    {nuitrack.JointType.LeftShoulder,   nuitrack.JointType.LeftElbow},
    {nuitrack.JointType.LeftElbow,      nuitrack.JointType.LeftWrist},
    {nuitrack.JointType.LeftWrist,      nuitrack.JointType.LeftHand},
    {nuitrack.JointType.RightShoulder,  nuitrack.JointType.RightElbow},
    {nuitrack.JointType.RightElbow,     nuitrack.JointType.RightWrist},
    {nuitrack.JointType.RightWrist,     nuitrack.JointType.RightHand},
    {nuitrack.JointType.LeftHip,        nuitrack.JointType.LeftKnee},
    {nuitrack.JointType.LeftKnee,       nuitrack.JointType.LeftAnkle},
    {nuitrack.JointType.RightHip,       nuitrack.JointType.RightKnee},
    {nuitrack.JointType.RightKnee,      nuitrack.JointType.RightAnkle}
  };

  Dictionary<int, Dictionary<nuitrack.JointType, GameObject>> joints = new Dictionary<int, Dictionary<nuitrack.JointType, GameObject>>();
  Dictionary<int, GameObject[]> connections = new Dictionary<int, GameObject[]>();
  Dictionary<int, GameObject> skeletonsRoots = new Dictionary<int, GameObject>();

  IssuesProcessor issuesProcessor;

  void Start () 
  {
    issuesProcessor = IssuesProcessor.Instance;
    nuitrackModules = FindObjectOfType<NuitrackModules>();
  }

  void Update()
  {
    ProcessSkeletons(nuitrackModules.SkeletonData);
  }

  void HideAllSkeletons()
  {
    
    int[] skelIds = new int[skeletonsRoots.Keys.Count];
    skeletonsRoots.Keys.CopyTo(skelIds, 0);

    for (int i = 0; i < skelIds .Length; i++)
    {
      skeletonsRoots[skelIds[i]].SetActive(false);
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

    int[] skelIds = new int[skeletonsRoots.Keys.Count];
    skeletonsRoots.Keys.CopyTo(skelIds, 0);

    for (int i = 0; i < skelIds.Length; i++)
    {
      if (skeletonData.GetSkeletonByID(skelIds[i]) == null)
      {
        skeletonsRoots[skelIds[i]].SetActive(false);
      }
    }

    foreach (nuitrack.Skeleton skeleton in skeletonData.Skeletons)
    {
      if (!skeletonsRoots.ContainsKey(skeleton.ID)) // if don't have gameObjects for skeleton ID, create skeleton gameobjects (root, joints and connections)
      {
        GameObject skelRoot = new GameObject();
        skelRoot.name = "Root_" + skeleton.ID.ToString();

        skeletonsRoots.Add(skeleton.ID, skelRoot);

        Dictionary<nuitrack.JointType, GameObject> skelJoints = new Dictionary<nuitrack.JointType, GameObject>();


        for (int i = 0; i < jointsInfo.Length; i++)
        {
          GameObject joint = (GameObject)Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
          skelJoints.Add(jointsInfo[i], joint);
          joint.transform.parent = skelRoot.transform;
          joint.SetActive(false);
        }

        joints.Add(skeleton.ID, skelJoints);

        GameObject[] skelConnections = new GameObject[connectionsInfo.GetLength(0)];

        for (int i = 0; i < skelConnections.Length; i++)
        {
          GameObject conn = (GameObject)Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity);
          skelConnections[i] = conn;
          conn.transform.parent = skelRoot.transform;
          conn.SetActive(false);
        }

        connections.Add(skeleton.ID, skelConnections);
      }

      if (!skeletonsRoots[skeleton.ID].activeSelf) skeletonsRoots[skeleton.ID].SetActive(true);

      for (int i = 0; i < jointsInfo.Length; i++)
      {
        nuitrack.Joint j = skeleton.GetJoint(jointsInfo[i]);
        if (j.Confidence > 0.5f)
        {
          if (!joints[skeleton.ID][jointsInfo[i]].activeSelf) joints[skeleton.ID][jointsInfo[i]].SetActive(true);

          joints[skeleton.ID][jointsInfo[i]].transform.position = 0.001f * new Vector3(j.Real.X, j.Real.Y, j.Real.Z);

          //skel.Joints[i].Orient.Matrix:
          // 0,       1,      2, 
          // 3,       4,      5,
          // 6,       7,      8
          // -------
          // right(X),  up(Y),    forward(Z)
          
          //Vector3 jointRight =  new Vector3(  j.Orient.Matrix[0],  j.Orient.Matrix[3],  j.Orient.Matrix[6] );
          Vector3 jointUp =     new Vector3(    j.Orient.Matrix[1],  j.Orient.Matrix[4],  j.Orient.Matrix[7] );
          Vector3 jointForward =  new Vector3(  j.Orient.Matrix[2],  j.Orient.Matrix[5],  j.Orient.Matrix[8] );
          joints[skeleton.ID][jointsInfo[i]].transform.rotation = Quaternion.LookRotation(jointForward, jointUp);
        }
          else
        {
          if (joints[skeleton.ID][jointsInfo[i]].activeSelf) joints[skeleton.ID][jointsInfo[i]].SetActive(false);
        }
      }

      for (int i = 0; i < connectionsInfo.GetLength(0); i++)
      {
        if (joints[skeleton.ID][connectionsInfo[i, 0]].activeSelf && joints[skeleton.ID][connectionsInfo[i, 1]].activeSelf)
        {
          if (!connections[skeleton.ID][i].activeSelf) connections[skeleton.ID][i].SetActive(true);

          Vector3 diff = joints[skeleton.ID][connectionsInfo[i, 1]].transform.position - joints[skeleton.ID][connectionsInfo[i, 0]].transform.position;

          connections[skeleton.ID][i].transform.position = joints[skeleton.ID][connectionsInfo[i, 0]].transform.position;
          connections[skeleton.ID][i].transform.rotation = Quaternion.LookRotation(diff);
          connections[skeleton.ID][i].transform.localScale = new Vector3(1f, 1f, diff.magnitude);
        }
        else
        {
          if (connections[skeleton.ID][i].activeSelf) connections[skeleton.ID][i].SetActive(false);
        }
      }
    }
  }

  void OnDestroy()
  {
    int[] idsInDict = new int[skeletonsRoots.Count];
    skeletonsRoots.Keys.CopyTo(idsInDict, 0);

    for (int i = 0; i < idsInDict.Length; i++)
    {
      Destroy(skeletonsRoots[idsInDict[i]]);
    }

    skeletonsRoots = new Dictionary<int, GameObject>();
    joints = new Dictionary<int, Dictionary<nuitrack.JointType, GameObject>>();
    connections = new Dictionary<int, GameObject[]>();

    if (issuesProcessor != null) Destroy (issuesProcessor.gameObject);
  }
}