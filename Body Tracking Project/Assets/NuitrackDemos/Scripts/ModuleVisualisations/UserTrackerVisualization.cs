using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using nuitrack.issues;

public class UserTrackerVisualization: MonoBehaviour 
{
	#region Fields
  NuitrackModules nuitrackModules;
  IssuesProcessor issuesProcessor;

  nuitrack.DepthFrame depthFrame = null;
  nuitrack.ColorFrame colorFrame = null;
  nuitrack.UserFrame userFrame = null;

  [SerializeField]int hRes;
  int frameStep;
  float depthToScale;
  
  //visualization fields
  [SerializeField]Color[] userCols;
  Color[] occludedUserCols;

  [SerializeField] Color defaultColor;
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
  List<Vector2[]> uv2s;
  List<Vector2[]> uv3s;
  //List<Vector2[]> uv4s;
  List<Color[]> colors;

  Color[] userCurrentCols;

  GameObject[] visualizationParts;
  Mesh[] visualizationMeshes;

  Texture2D depthTexture, rgbTexture, segmentationTexture;
  Color[] depthColors;
  Color[] rgbColors;
  Color[] segmColors;

  ExceptionsLogger exceptionsLogger;

  bool active = false;
  bool initialized = false;

  #endregion

  public void SetActive(bool _active)
  {
    active = _active;
  }

  public void SetShaderProperties(Color newZeroColor, bool showBorders)
  {
    if (!initialized) Initialize();
    userCols[0] = newZeroColor;
    userCurrentCols[0] = newZeroColor;
    occludedUserCols[0] = newZeroColor;
    visualizationMaterial.SetColor("_SegmZeroColor", newZeroColor);
    visualizationMaterial.SetInt("_ShowBorders", showBorders ? 1 : 0);
  }

  void Initialize()
  {
    initialized = true;
    occludedUserCols = new Color[userCols.Length];
    userCurrentCols = new Color[userCols.Length];
    for (int i = 0; i < userCols.Length; i++)
    {
      userCurrentCols[i] = userCols[i];
      float[] hsv = new float[3];
      Color.RGBToHSV(userCols[i], out hsv[0], out hsv[1], out hsv[2]);
      hsv[2] *= 0.25f;
      occludedUserCols[i] = Color.HSVToRGB(hsv[0], hsv[1], hsv[2]);
      occludedUserCols[i].a = userCols[i].a;
    }
    
    issuesProcessor = IssuesProcessor.Instance;
    nuitrackModules = FindObjectOfType<NuitrackModules>();
    nuitrack.OutputMode mode = nuitrackModules.DepthSensor.GetOutputMode();
    frameStep = mode.XRes / hRes;
    if (frameStep <= 0) frameStep = 1; // frameStep should be greater then 0
    hRes = mode.XRes / frameStep;
    
    depthToScale = meshScaling * 2f * Mathf.Tan (0.5f * mode.HFOV) / hRes;
    
    InitMeshes( 
      ((mode.XRes / frameStep) + (mode.XRes % frameStep == 0 ? 0 : 1)),
      ((mode.YRes / frameStep) + (mode.YRes % frameStep == 0 ? 0 : 1)),
      mode.HFOV
    );
  }

  void Start () 
  {
    if (!initialized) Initialize();
	}
	
	#region Mesh generation and mesh update methods
  void InitMeshes(int cols, int rows, float hfov)
	{
    //depthTexture, segmentationTexture

    depthColors = new Color[cols * rows];
    rgbColors = new Color[cols * rows];
    segmColors = new Color[cols * rows];

    depthTexture = new Texture2D(cols, rows, TextureFormat.RFloat, false);
    depthTexture.filterMode = FilterMode.Point;
    depthTexture.wrapMode = TextureWrapMode.Clamp;
    depthTexture.Apply();

    rgbTexture = new Texture2D(cols, rows, TextureFormat.ARGB32, false);
    rgbTexture.filterMode = FilterMode.Point;
    rgbTexture.wrapMode = TextureWrapMode.Clamp;
    rgbTexture.Apply();
    
    segmentationTexture = new Texture2D(cols, rows, TextureFormat.ARGB32, false);
    segmentationTexture.filterMode = FilterMode.Point;
    segmentationTexture.wrapMode = TextureWrapMode.Clamp;
    segmentationTexture.Apply();

    DebugDepth.depthMat.mainTexture = depthTexture;
    DebugDepth.segmentationMat.mainTexture = rgbTexture;

    visualizationMaterial.SetTexture("_DepthTex", depthTexture);
    visualizationMaterial.SetTexture("_SegmentationTex", segmentationTexture);
    visualizationMaterial.SetTexture("_RGBTex", rgbTexture);

    int numPoints = cols * rows;

		vertsPerMesh = sampleMesh.vertices.Length;
		trisPerMesh = sampleMesh.triangles.Length;
		
		sampleVertices = sampleMesh.vertices;
    Vector4[] sampleVertsV4 = new Vector4[sampleVertices.Length];

    for (int i = 0; i < sampleVertices.Length; i++)
    {
      sampleVertices[i] *= depthToScale;
      sampleVertsV4[i] = sampleVertices[i];
      //visualizationMaterial.SetVector("_Offsets" + i.ToString(), sampleVertices[i]); //unity 5.3-
    }
    visualizationMaterial.SetVectorArray("_Offsets", sampleVertsV4); //unity 5.4+

		sampleTriangles = sampleMesh.triangles;
		sampleNormals = sampleMesh.normals;
		sampleUvs = sampleMesh.uv;

		vertices = 	new List<Vector3[]>();
		triangles = new List<int[]>();
		normals = 	new List<Vector3[]>();
		uvs = 		  new List<Vector2[]>();
    uv2s =      new List<Vector2[]>();
    uv3s =      new List<Vector2[]>();

    colors =    new List<Color[]>();
		
		pointsPerVis = 64000 / vertsPerMesh; //can't go over the limit for number of mesh vertices in one mesh
		parts = numPoints / pointsPerVis + (((numPoints % pointsPerVis) != 0) ? 1 : 0);
		
		visualizationParts = new GameObject[parts];
		visualizationMeshes = new Mesh[parts];
		
    float fX, fY;
    fX = 0.5f / Mathf.Tan(0.5f * hfov);
    fY = fX * cols / rows;

    visualizationMaterial.SetFloat("fX", fX);
    visualizationMaterial.SetFloat("fY", fY);

		//generation of triangle indexes, vertices, uvs and normals for all visualization parts

    for (int i = 0, row = 0, col = 0; i < parts; i++)
    {
      int numPartPoints = Mathf.Min (pointsPerVis, numPoints - i * pointsPerVis);
      
      int[] partTriangles =     new int     [numPartPoints * trisPerMesh];
      Vector3[] partVertices =  new Vector3 [numPartPoints * vertsPerMesh];
      Vector3[] partNormals =   new Vector3 [numPartPoints * vertsPerMesh];
      Vector2[] partUvs =       new Vector2 [numPartPoints * vertsPerMesh];
      Vector2[] partUv2s =      new Vector2 [numPartPoints * vertsPerMesh];
      Vector2[] partUv3s =      new Vector2 [numPartPoints * vertsPerMesh];
      Color[] partColors =      new Color   [numPartPoints * vertsPerMesh];
      
      for (int j = 0; j < numPartPoints; j++)
      {
        for (int k = 0; k < trisPerMesh; k++)
        {
          partTriangles[j * trisPerMesh + k] = sampleTriangles[k] + j * vertsPerMesh;
        }
        Vector2 depthTextureUV = new Vector2( ((float)col + 0.5f) / cols, ((float)row + 0.5f) / rows );
        for (int k = 0; k < vertsPerMesh; k++)
        {
          partUv2s[j * vertsPerMesh + k] = depthTextureUV;
          partUv3s[j * vertsPerMesh + k] = new Vector2(k, 0);
        }
        System.Array.Copy(sampleVertices,     0, partVertices,  j * vertsPerMesh, vertsPerMesh);
        System.Array.Copy(sampleNormals,      0, partNormals,   j * vertsPerMesh, vertsPerMesh);
        System.Array.Copy(sampleUvs,          0, partUvs,       j * vertsPerMesh, vertsPerMesh);

        col++;
        if (col == cols)
        {
          row++;
          col = 0;
        }
			}
			
      triangles.Add (partTriangles);
			vertices.Add 	(partVertices);
			normals.Add 	(partNormals);
			uvs.Add 		  (partUvs);
      uv2s.Add      (partUv2s);
      uv3s.Add      (partUv3s);
			colors.Add 		(partColors);
			
			visualizationMeshes[i] = new Mesh();
			visualizationMeshes[i].vertices = vertices[i];
			visualizationMeshes[i].triangles = triangles[i];
			visualizationMeshes[i].normals = normals[i];
			visualizationMeshes[i].uv = uvs[i];
      visualizationMeshes[i].uv2 = uv2s[i];
      visualizationMeshes[i].uv3 = uv3s[i];
			visualizationMeshes[i].colors = colors[i];
			
      Bounds meshBounds = new Bounds(500f * new Vector3(0f, 0f, 1f), 2000f * Vector3.one);
      visualizationMeshes[i].bounds = meshBounds;
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
	#endregion
	
	void Update () 
	{
		bool haveNewFrame = false;
        if ((nuitrackModules.DepthFrame != null) && active)
		{
			if (depthFrame != null)
			{
				haveNewFrame = (depthFrame != nuitrackModules.DepthFrame);
			}
			depthFrame = nuitrackModules.DepthFrame;
            colorFrame = nuitrackModules.ColorFrame;
			userFrame = nuitrackModules.UserFrame;
			if (haveNewFrame) ProcessFrame(depthFrame, colorFrame, userFrame);
		}
        else
        {
            HideVisualization();
        }
    }
  
  void HideVisualization()
  {
    for (int i = 0; i < parts; i++)
    {
      if (visualizationParts[i].activeSelf) visualizationParts[i].SetActive(false);
    }
  }

	void ProcessFrame(nuitrack.DepthFrame depthFrame, nuitrack.ColorFrame colorFrame, nuitrack.UserFrame userFrame)
	{
        for (int i = 0; i < parts; i++)
        {
          if (!visualizationParts[i].activeSelf) visualizationParts[i].SetActive(true);
        }

        Color pointColor = Color.white;

        int pointsPerVisTotal = pointsPerVis * vertsPerMesh;

        if (userFrame != null)
        {
          if (issuesProcessor.userIssues != null)
          {
            for (int i = 1; i < userCurrentCols.Length; i++)
            {
              if (issuesProcessor.userIssues.ContainsKey(i))
              {
                userCurrentCols[i] = 
                  (issuesProcessor.userIssues[i].isOccluded || 
                  issuesProcessor.userIssues[i].onBorderLeft || 
                  issuesProcessor.userIssues[i].onBorderRight || 
                  issuesProcessor.userIssues[i].onBorderTop) ?
                  occludedUserCols[i] : userCols[i];
              }
            }
          }
        }

        for (int i = 0, pointIndex = 0, rgbOffset = 0; i < depthFrame.Rows; i += frameStep)
	    {
            for (int j = 0; j < depthFrame.Cols; j += frameStep, ++pointIndex, rgbOffset += 3)
            {
                depthColors[pointIndex].r = depthFrame[i, j] / 16384f;

                uint userId = 0u; 
                if (userFrame != null) 
                {
                    userId = userFrame[i * userFrame.Rows / depthFrame.Rows,
                    j * userFrame.Cols / depthFrame.Cols];
                }

                #region RGB coloring
                //int rgbOffset = 3 * (i * depthFrame.Cols + j);
                Color rgbCol = defaultColor; 
                if(colorFrame != null)
                rgbCol = new Color32(colorFrame[i,j].Red, colorFrame[i, j].Green, colorFrame[i, j].Blue, 255);
                //new Color32(depthFrame.rgb[rgbOffset + 2], depthFrame.rgb[rgbOffset + 1], depthFrame.rgb[rgbOffset + 0], 255);
                pointColor = rgbCol;// * userCurrentCols[userId];
                if (userId == 0) pointColor *= userCols[0];
                Color segmColor = userCurrentCols[userId];
                //Debug.Log(j.ToString() + ", " + i.ToString() + " : " + rgbCol);
                #endregion
                //        pointColor = userCurrentCols[userId]; //user segmentation coloring

                rgbColors[pointIndex] = pointColor;
                segmColors[pointIndex] = segmColor;
            }
	    }
        depthTexture.SetPixels(depthColors);
        rgbTexture.SetPixels(rgbColors);
        segmentationTexture.SetPixels(segmColors);

        depthTexture.Apply();
        rgbTexture.Apply();
        segmentationTexture.Apply();
	}
	
	void OnDestroy()
	{
    if (depthTexture != null) Destroy(depthTexture);
    if (rgbTexture != null) Destroy(rgbTexture);

    DebugDepth.depthMat.mainTexture = null;
    DebugDepth.segmentationMat.mainTexture = null;

		if (visualizationParts != null)
		{
			for (int i = 0; i < visualizationParts.Length; i++)
			{
				Destroy(visualizationParts[i]);
			}
		}

    if (issuesProcessor != null) Destroy (issuesProcessor.gameObject);
	}
}