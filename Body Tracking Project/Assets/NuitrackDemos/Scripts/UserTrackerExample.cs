using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserTrackerExample: MonoBehaviour 
{
	#region Fields

	nuitrack.DepthSensor depthSensor;
	nuitrack.UserTracker userTracker;
	nuitrack.DepthFrame depthFrame;

	[SerializeField]int hRes;
	int frameStep;
	float depthToScale;

	//visualization fields
	[SerializeField]Color[] userCols;

	[SerializeField]Mesh sampleMesh;
	[SerializeField]float meshScaling = 1f;
	[SerializeField]Material visualizationMaterial;

	int pointsPerVis, parts;

	int vertsPerMesh, trisPerMesh;
	int[] sampleTriangles;
	Vector3[] sampleVertices;
	Vector3[] sampleNormals;
	Vector2[] sampleUvs;
	
	List<int[]> triangles;
	List<Vector3[]> vertices;
	List<Vector3[]> normals;
	List<Vector2[]> uvs;
	List<Color[]> colors;

	GameObject[] visualizationParts;
	Mesh[] visualizationMeshes;

	ExceptionsLogger exceptionsLogger;
	#endregion

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
		//nuitrack initialization and creation of depth and userTracker modules:
		try
		{
      nuitrack.Nuitrack.Init(debugPath);
      depthSensor = nuitrack.DepthSensor.Create();
			depthSensor.SetMirror(false);
			nuitrack.OutputMode mode = depthSensor.GetOutputMode();

			frameStep = mode.XRes / hRes;
			if (frameStep <= 0) frameStep = 1; // frameStep should be greater then 0
      hRes = mode.XRes / frameStep;

			depthToScale = 0.9f * 2f * Mathf.Tan (0.5f * mode.HFOV) / hRes;

			InitMeshes(
				((mode.XRes / frameStep) + (mode.XRes % frameStep == 0 ? 0 : 1)) * 
				((mode.YRes / frameStep) + (mode.YRes % frameStep == 0 ? 0 : 1))
			           );
      userTracker = nuitrack.UserTracker.Create();

			//event handlers registering:
			depthSensor.OnUpdateEvent += DepthUpdate;
			userTracker.OnUpdateEvent += UserUpdate;

			nuitrack.Nuitrack.Run ();
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	#region Mesh generation and mesh update methods
	void InitMeshes(int numPoints)
	{
		vertsPerMesh = sampleMesh.vertices.Length;
		trisPerMesh = sampleMesh.triangles.Length;

		sampleVertices = sampleMesh.vertices;
		sampleTriangles = sampleMesh.triangles;
		sampleNormals = sampleMesh.normals;
		sampleUvs = sampleMesh.uv;

		for (int i = 0; i < sampleVertices.Length; i++)
		{
			sampleVertices[i] *= meshScaling;
		}

		vertices = 	new List<Vector3[]>();
		triangles = new List<int[]>();
		normals = 	new List<Vector3[]>();
		uvs = 		new List<Vector2[]>();
		colors = 	new List<Color[]>();

		pointsPerVis = 64000 / vertsPerMesh; //can't go over the limit for number of mesh vertices in one mesh
		parts = numPoints / pointsPerVis + (((numPoints % pointsPerVis) != 0) ? 1 : 0);

		visualizationParts = new GameObject[parts];
		visualizationMeshes = new Mesh[parts];


		//generation of triangle indexes, vertices, uvs and normals for all visualization parts
		for (int i = 0; i < parts; i++)
		{
			int numPartPoints = Mathf.Min (pointsPerVis, numPoints - i * pointsPerVis);

			int[] partTriangles = 		new int		[numPartPoints * trisPerMesh];
			Vector3[] partVertices = 	new Vector3	[numPartPoints * vertsPerMesh];
			Vector3[] partNormals = 	new Vector3	[numPartPoints * vertsPerMesh];
			Vector2[] partUvs = 		new Vector2	[numPartPoints * vertsPerMesh];
			Color[] partColors = 		new Color	[numPartPoints * vertsPerMesh];

			for (int j = 0; j < numPartPoints; j++)
			{
				for (int k = 0; k < trisPerMesh; k++)
				{
					partTriangles[j * trisPerMesh + k] = sampleTriangles[k] + j * vertsPerMesh;
				}
				System.Array.Copy(sampleVertices, 		0, partVertices, 	j * vertsPerMesh, vertsPerMesh);
				System.Array.Copy(sampleNormals, 		0, partNormals, 	j * vertsPerMesh, vertsPerMesh);
				System.Array.Copy(sampleUvs, 			0, partUvs, 		j * vertsPerMesh, vertsPerMesh);
			}

			triangles.Add 	(partTriangles);
			vertices.Add 	(partVertices);
			normals.Add 	(partNormals);
			uvs.Add 		(partUvs);
			colors.Add 		(partColors);

			visualizationMeshes[i] = new Mesh();
			visualizationMeshes[i].vertices = vertices[i];
			visualizationMeshes[i].triangles = triangles[i];
			visualizationMeshes[i].normals = normals[i];
			visualizationMeshes[i].uv = uvs[i];
			visualizationMeshes[i].colors = colors[i];

			visualizationMeshes[i].RecalculateBounds();
			visualizationMeshes[i].MarkDynamic();
			
			visualizationParts[i] = new GameObject();
			visualizationParts[i].name = "Visualization_" + i.ToString();
			visualizationParts[i].transform.position = Vector3.zero;
			visualizationParts[i].transform.rotation = Quaternion.identity;
			visualizationParts[i].AddComponent<MeshFilter>();
			visualizationParts[i].GetComponent<MeshFilter>().mesh = visualizationMeshes[i];
			visualizationParts[i].AddComponent<MeshRenderer>();
			visualizationParts[i].GetComponent<Renderer>().sharedMaterial = visualizationMaterial;
		}
	}

	void VerticesUpdate(int point, Vector3 pos, Color newColor)
	{
		int visPart = point / pointsPerVis;
		int ind = point % pointsPerVis;

		for (int i = 0; i < vertsPerMesh; i++)
		{
			vertices[visPart][ind * vertsPerMesh + i] = sampleVertices[i] * pos.z * depthToScale + pos;
			colors[visPart][ind * vertsPerMesh + i] = newColor;
		}
	}

	void MeshesUpdate()
	{
		for (int i = 0; i < parts; i++)
		{
			visualizationMeshes[i].vertices = vertices[i];
			visualizationMeshes[i].colors 	= colors[i];
			visualizationMeshes[i].RecalculateBounds();
		}
	}
	#endregion

	void Update () 
	{
		try
		{
			if (userTracker != null) nuitrack.Nuitrack.Update(userTracker);
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	void ProcessFrame(nuitrack.DepthFrame depthFrame, nuitrack.UserFrame userFrame)
	{
		Color pointColor;

		for (int i = 0, pointIndex = 0; i < depthFrame.Rows; i += frameStep)
		{
			for (int j = 0; j < depthFrame.Cols; j += frameStep, ++pointIndex)
			{
				nuitrack.Vector3 p = depthSensor.ConvertProjToRealCoords(j, i, depthFrame[i, j]);
				uint userId =  userFrame[i * userFrame.Rows / depthFrame.Rows,
				                         j * userFrame.Cols / depthFrame.Cols];
				pointColor = userCols[userId % userCols.Length];
				VerticesUpdate(pointIndex, new Vector3(p.X * 0.001f, p.Y * 0.001f, p.Z * 0.001f), pointColor);
			}
		}
		MeshesUpdate();
	}

	void OnDestroy()
	{
		//we should release resources after we are done:
		try
		{
			nuitrack.Nuitrack.Release();
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	#region Event handlers
	void DepthUpdate(nuitrack.DepthFrame _depthFrame)
	{
		depthFrame = _depthFrame;
	}
	
	void UserUpdate(nuitrack.UserFrame _userFrame)
	{
		ProcessFrame(depthFrame, _userFrame);
	}
	#endregion
}