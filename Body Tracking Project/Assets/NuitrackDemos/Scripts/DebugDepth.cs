using UnityEngine;
using System.Collections;

public class DebugDepth : MonoBehaviour 
{
  [SerializeField]Material _depthMat, _segmentationMat;

  public static Material depthMat, segmentationMat;

	void Awake () 
  {
    depthMat = _depthMat;
    segmentationMat = _segmentationMat;
	}
}
