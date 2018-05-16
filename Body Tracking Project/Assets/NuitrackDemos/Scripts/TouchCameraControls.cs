using UnityEngine;
using System.Collections;

public class TouchCameraControls : MonoBehaviour 
{
	[SerializeField]Transform camera;
  [SerializeField]float minCameraDistance = 1f;
  [SerializeField]float maxCameraDistance = 10f;

  [SerializeField]Renderer pivotRenderer;
  [SerializeField]bool showPivot = true;

  Vector3 minPivotPos = new Vector3(-2.5f, -2.5f, 0f);
  Vector3 maxPivotPos = new Vector3( 2.5f,  2.5f, 5f);

  float xAngle = 0f, yAngle = 0f;

	float cameraDistance;
	bool had2touches = false;

  Vector2 mid2touches = Vector2.zero;

  //initial position camera info:

  Vector3 pivotStartPos;
  Quaternion pivotStartRot;
  float cameraStartDistance;

	void Start () 
	{
    pivotRenderer.enabled = false;
		cameraDistance = camera.localPosition.z;

    pivotStartPos = transform.position;
    pivotStartRot = transform.rotation;
    cameraStartDistance = camera.localPosition.z;
	}

	float prev2touchesDistance;

  [SerializeField]float doubleTapInterval = 0.1f;
  int taps = 0;
  float tapTimer = 0f;

  void UpdateTapCounter()
  {
    tapTimer += Time.deltaTime;
    if (tapTimer > doubleTapInterval)
    {
      taps = 0;
      tapTimer = 0f;
    }

    if (Input.touchCount == 1)
    {
      if (Input.touches[0].phase == TouchPhase.Began)
      {
        Debug.Log("Touch began, taps: " + taps.ToString());
        taps++;
        tapTimer = 0f;
      }
    }
  }

  void ResetCamera()
  {
    xAngle = 0f;
    yAngle = 0f;
    cameraDistance = cameraStartDistance;
    transform.position = pivotStartPos;
    transform.rotation = pivotStartRot;
    camera.localPosition = new Vector3(camera.localPosition.x, camera.localPosition.y, cameraStartDistance);
  }

	void Update () 
	{
    UpdateTapCounter();
    bool doubleTapped = (taps == 2);

    if (doubleTapped) ResetCamera();//TODO: recenter camera view

    if (showPivot) pivotRenderer.enabled = (Input.touchCount > 0) && (Input.touchCount < 3);

		if (Input.touchCount == 1) // rotations
		{
			had2touches = false;
			xAngle -= Input.touches[0].deltaPosition.y;
			yAngle += Input.touches[0].deltaPosition.x;

			xAngle = Mathf.Clamp (xAngle, -90f, 90f);
			while (yAngle > 360f)
			{
				yAngle -= 360f;
			}
			while (yAngle < -360f)
			{
				yAngle += 360f;
			}
			transform.rotation = Quaternion.Euler(xAngle, yAngle, 0f);
		}
		else if (Input.touchCount == 2) //scale + translation of pivot
		{
			if (!had2touches)
			{
        prev2touchesDistance = (Input.touches[0].position - Input.touches[1].position).magnitude;
        had2touches = true;

        //translation of pivot point part:
        mid2touches = 0.5f * (Input.touches[0].position + Input.touches[1].position);
      }
			else
			{
				float current2touchesDistance = (Input.touches[0].position - Input.touches[1].position).magnitude;
				cameraDistance *= prev2touchesDistance / current2touchesDistance;
				cameraDistance = Mathf.Clamp(cameraDistance, -maxCameraDistance, -minCameraDistance);
				camera.localPosition = new Vector3(0f, 0f, cameraDistance);
				prev2touchesDistance = current2touchesDistance;

        //translation of pivot point part:
        Vector2 newMid2touches = 0.5f * (Input.touches[0].position + Input.touches[1].position);
        Vector2 midDiff = newMid2touches - mid2touches;
        mid2touches = newMid2touches;

        transform.position = transform.position + (-0.005f * midDiff.x) * camera.right + (-0.005f * midDiff.y) * camera.up;
        transform.position = new Vector3(
          Mathf.Clamp(transform.position.x, minPivotPos.x, maxPivotPos.x),
          Mathf.Clamp(transform.position.y, minPivotPos.y, maxPivotPos.y),
          Mathf.Clamp(transform.position.z, minPivotPos.z, maxPivotPos.z));
			}
		}
		else
		{
			had2touches = false;
		}
	}
}
