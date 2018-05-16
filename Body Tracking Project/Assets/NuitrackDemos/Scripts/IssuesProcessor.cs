using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using nuitrack.issues;

public class IssuesProcessor : MonoBehaviour 
{
  public class UserIssues
  {
    public bool isOccluded;
    public bool onBorderLeft;
    public bool onBorderRight;
    public bool onBorderTop;
  }

  // usually you'll have some logic to track user/users that are important 
  // for an application (your active players) and have their ids to check issues,
  // but in this example we don't have a module that tracks userIds,
  // so we just check every possible id (1 - 5 at the moment)
  int[] checkedUserIds = {1, 2, 3, 4, 5};

  public Dictionary<int, UserIssues> userIssues;

  bool 
    borderedLeft = false,
    borderedRight = false,
    borderedTop = false;

  [SerializeField]Material bordersMaterial;
  GameObject[] borders; // 0 - left, 1 - right, 2 - top

  static IssuesProcessor instance = null;

  public static IssuesProcessor Instance
  {
    get
    {
      if (instance == null)
      {
        instance = FindObjectOfType<IssuesProcessor>();
        if (instance == null)
        {
          GameObject issuesProcessorGO = new GameObject();
          instance = issuesProcessorGO.AddComponent<IssuesProcessor>();
        }
      }
      return instance;
    }
  }

	void Awake () 
  {
    if (instance == null) instance = this;
    InitProcessor();
  }

  void InitProcessor()
  {
    userIssues = new Dictionary<int, UserIssues>();
    for (int i = 0; i < checkedUserIds.Length; i++)
    {
      UserIssues tmpIssue = new UserIssues();
      tmpIssue.isOccluded = false;
      tmpIssue.onBorderLeft = false;
      tmpIssue.onBorderRight = false;
      tmpIssue.onBorderTop = false;
      userIssues.Add(checkedUserIds[i], tmpIssue);
    }

    FrameBorderIssue issue = new FrameBorderIssue(true, true, true); // just to get fov values
    const float maxDepth = 10f;

    Vector3 topLeft     = new Vector3(-maxDepth * Mathf.Tan (0.5f * issue.hFov), maxDepth * Mathf.Tan (0.5f * issue.vFov), maxDepth);
    Vector3 topRight    = new Vector3( maxDepth * Mathf.Tan (0.5f * issue.hFov), maxDepth * Mathf.Tan (0.5f * issue.vFov), maxDepth);
    Vector3 bottomLeft  = new Vector3(-maxDepth * Mathf.Tan (0.5f * issue.hFov),-maxDepth * Mathf.Tan (0.5f * issue.vFov), maxDepth);
    Vector3 bottomRight = new Vector3( maxDepth * Mathf.Tan (0.5f * issue.hFov),-maxDepth * Mathf.Tan (0.5f * issue.vFov), maxDepth);

    borders = new GameObject[3];

    for (int i = 0; i < 3; i++)
    {
      borders[i] = new GameObject();
      borders[i].transform.position = Vector3.zero;
      borders[i].transform.rotation = Quaternion.identity;
      MeshFilter meshFilter = borders[i].AddComponent<MeshFilter>();
      MeshRenderer meshRenderer = borders[i].AddComponent<MeshRenderer>();
      meshRenderer.material = bordersMaterial;

      Mesh mesh = new Mesh();

      borders[i].name = (i == 0) ? "LeftBorder" : (i == 1) ? "RightBorder" : "TopBorder";
      mesh.name = borders[i].name;

      List<Vector3> verts = new List<Vector3>();
      verts.Add (Vector3.zero);
      verts.Add ( (i == 0) ? topLeft    : (i == 1) ? bottomRight  : topRight);
      verts.Add ( (i == 0) ? bottomLeft : (i == 1) ? topRight     : topLeft);

      List<int> tris = new List<int>();
      tris.Add(0);
      tris.Add(1);
      tris.Add(2);

      mesh.SetVertices(verts);
      mesh.SetTriangles(tris, 0);
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();

      meshFilter.mesh = mesh;

      borders[i].SetActive(false);
    }

    nuitrack.Nuitrack.onIssueUpdateEvent += OnIssuesUpdate;
  }

  void DisposeProcessor()
  {
    nuitrack.Nuitrack.onIssueUpdateEvent -= OnIssuesUpdate;
    if (instance == this) instance = null;
    if (borders != null)
    {
      for (int i = 0 ; i < 3; i++)
      {
        Destroy(borders[i]);
      }
    }
  }

  void OnDestroy()
  {
    DisposeProcessor();
  }

  void OnIssuesUpdate (nuitrack.issues.IssuesData issuesData)
  {
    borderedLeft = false;
    borderedRight = false;
    borderedTop = false;

    for (int i = 0; i < checkedUserIds.Length; i++)
    {
      if (issuesData.GetUserIssue<OcclusionIssue>(checkedUserIds[i]) != null)
      {
        userIssues[checkedUserIds[i]].isOccluded = true;
      }
      else
      {
        userIssues[checkedUserIds[i]].isOccluded = false;
      }

      FrameBorderIssue frameBorderIssue = issuesData.GetUserIssue<FrameBorderIssue>(checkedUserIds[i]);

      if (frameBorderIssue != null)
      {
        userIssues[checkedUserIds[i]].onBorderLeft = frameBorderIssue.Left;
        userIssues[checkedUserIds[i]].onBorderRight = frameBorderIssue.Right;
        userIssues[checkedUserIds[i]].onBorderTop = frameBorderIssue.Top;
      }
      else
      {
        userIssues[checkedUserIds[i]].onBorderLeft = false;
        userIssues[checkedUserIds[i]].onBorderRight = false;
        userIssues[checkedUserIds[i]].onBorderTop = false;
      }
      borderedLeft  |= userIssues[checkedUserIds[i]].onBorderLeft;
      borderedRight |= userIssues[checkedUserIds[i]].onBorderRight;
      borderedTop   |= userIssues[checkedUserIds[i]].onBorderTop;
    }
  }

  void Update()
  {
    /*
    if (borders[0].activeSelf != borderedLeft)  borders[0].SetActive(borderedLeft);
    if (borders[1].activeSelf != borderedRight) borders[1].SetActive(borderedRight);
    if (borders[2].activeSelf != borderedTop)   borders[2].SetActive(borderedTop);
    */
  }
}
