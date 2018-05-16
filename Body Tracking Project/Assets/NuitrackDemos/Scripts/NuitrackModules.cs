using UnityEngine;
using System;
using System.Collections;

public class NuitrackModules : MonoBehaviour 
{
	[SerializeField]GameObject depthUserVisualizationPrefab;
  [SerializeField]GameObject depthUserMeshVisualizationPrefab;
	[SerializeField]GameObject skeletonsVisualizationPrefab;
	[SerializeField]GameObject gesturesVisualizationPrefab;
	[SerializeField]GameObject handTrackerVisualizationPrefab;
  [SerializeField]GameObject issuesProcessorPrefab;

	GameObject 
		depthUserVisualization,
    depthUserMeshVisualization,
		skeletonsVisualization,
		gesturesVisualization,
		handTrackerVisualization,
    issuesProcessor;


	nuitrack.DepthSensor depthSensor = null;
    nuitrack.ColorSensor colorSensor = null;
    nuitrack.UserTracker userTracker = null;
	nuitrack.SkeletonTracker skeletonTracker = null;
	nuitrack.HandTracker handTracker = null;
	nuitrack.GestureRecognizer gestureRecognizer = null;

	nuitrack.DepthSensor depthSensorInit = null;
    nuitrack.ColorSensor colorSensorInit = null;
    nuitrack.UserTracker userTrackerInit = null;
	nuitrack.SkeletonTracker skeletonTrackerInit = null;
	nuitrack.HandTracker handTrackerInit = null;
	nuitrack.GestureRecognizer gestureRecognizerInit = null;

	public nuitrack.DepthSensor DepthSensor {get {return this.depthSensor;}}
    public nuitrack.ColorSensor ColorSensor { get { return this.colorSensor; } }
    public nuitrack.UserTracker UserTracker {get {return this.userTracker;}}
	public nuitrack.SkeletonTracker SkeletonTracker {get {return this.skeletonTracker;}}
	public nuitrack.HandTracker HandTracker {get {return this.handTracker;}}
	public nuitrack.GestureRecognizer GestureRecognizer {get {return this.gestureRecognizer;}}

	nuitrack.DepthFrame depthFrame = null;
    nuitrack.ColorFrame colorFrame = null;
    nuitrack.UserFrame userFrame = null;
	nuitrack.SkeletonData skeletonData = null;
	nuitrack.HandTrackerData handTrackerData =  null;
	nuitrack.GestureData gesturesData = null;

	public nuitrack.DepthFrame DepthFrame {get {return this.depthFrame;}}
    public nuitrack.ColorFrame ColorFrame { get { return this.colorFrame; } }
    public nuitrack.UserFrame UserFrame {get {return this.userFrame;}}
	public nuitrack.SkeletonData SkeletonData {get {return this.skeletonData;}}
	public nuitrack.HandTrackerData HandTrackerData {get {return this.handTrackerData;}}
	public nuitrack.GestureData GesturesData{get {return this.gesturesData;}}

	ExceptionsLogger exceptionsLogger;

  [SerializeField]TextMesh perfomanceInfoText;

  //const string debugPath = "/home/stranger/repo/depth_scanner/data/nuitrack/nuitrack.config";
  const string debugPath = "../../data/nuitrack.config";

  bool nuitrackInitialized = false;

  void Awake () 
  {
    exceptionsLogger = GameObject.FindObjectOfType<ExceptionsLogger>();
    NuitrackInitState state = NuitrackLoader.InitNuitrackLibraries();
    if (state != NuitrackInitState.INIT_OK)
    {
      exceptionsLogger.AddEntry("Nuitrack native libraries iniialization error: " + Enum.GetName(typeof(NuitrackInitState), state));
    }
  }

	bool prevDepth = false;
    bool prevColor = false;
    bool prevUser = false;
	bool prevSkel = false;
	bool prevHand = false;
	bool prevGesture = false;

	public void ChangeModules(bool depthOn, bool colorOn, bool userOn, bool skeletonOn, bool handsOn, bool gesturesOn)
	{
		try
		{
			//      if (depthSensor != null) depthSensor.OnUpdateEvent -= DepthUpdate;
			//      if (userTracker != null) userTracker.OnUpdateEvent -= UserUpdate;
			//      if (skeletonTracker != null) skeletonTracker.OnSkeletonUpdateEvent -= SkeletonsUpdate;
			//      if (handTracker != null) handTracker.OnUpdateEvent -= HandTrackerUpdate;
			//      if (gestureRecognizer != null) gestureRecognizer.OnNewGesturesEvent -= GesturesUpdate;

			//*
			if (!nuitrackInitialized)
			{
				nuitrack.Nuitrack.Init();
                Debug.Log("init ok");
				depthSensorInit = nuitrack.DepthSensor.Create();
                colorSensorInit = nuitrack.ColorSensor.Create();
                userTrackerInit = nuitrack.UserTracker.Create();
				skeletonTrackerInit = nuitrack.SkeletonTracker.Create();
				handTrackerInit = nuitrack.HandTracker.Create();
				gestureRecognizerInit = nuitrack.GestureRecognizer.Create();

				nuitrack.Nuitrack.Run ();
                Debug.Log("run ok");
                nuitrackInitialized = true;
			}
//			*/

			depthFrame = null;
            colorFrame = null;
            userFrame = null;
			skeletonData = null;
			handTrackerData = null;
			gesturesData = null;
			//
			//      depthSensor = null;
			//      userTracker = null;
			//      skeletonTracker = null;
			//      handTracker = null;
			//      gestureRecognizer = null;

			//if (issuesProcessor != null) Destroy(issuesProcessor);

			if (prevDepth != depthOn)
			{
				prevDepth = depthOn;
				if (depthOn)
				{
					depthSensor = depthSensorInit;
					depthSensorInit.OnUpdateEvent += DepthUpdate;
				}
				else
				{
					depthSensorInit.OnUpdateEvent -= DepthUpdate;
					depthSensor = null;
				}
			}

            if (prevColor != colorOn)
            {
                prevColor = colorOn;
                if (colorOn)
                {
                    colorSensor = colorSensorInit;
                    colorSensorInit.OnUpdateEvent += ColorUpdate;
                }
                else
                {
                    colorSensorInit.OnUpdateEvent -= ColorUpdate;
                    colorSensor = null;
                }
            }

            if (prevUser != userOn)
			{
				prevUser = userOn;
				if (userOn)
				{
					userTracker = userTrackerInit;
					userTrackerInit.OnUpdateEvent += UserUpdate;
				}

				else
				{
					userTrackerInit.OnUpdateEvent -= UserUpdate;
					userTracker = null;
				}
			}

			if (skeletonOn != prevSkel)
			{
				prevSkel = skeletonOn;
				if (skeletonOn)
				{
					skeletonTracker = skeletonTrackerInit;
					skeletonTrackerInit.OnSkeletonUpdateEvent += SkeletonsUpdate;
				}
				else
				{
					skeletonTrackerInit.OnSkeletonUpdateEvent -= SkeletonsUpdate;
					skeletonTracker = null;
				}
			}

			if (prevHand != handsOn)
			{
				prevHand = handsOn;
				if (handsOn)
				{
					handTracker = handTrackerInit;
					handTrackerInit.OnUpdateEvent += HandTrackerUpdate;
				}
				else
				{
					handTrackerInit.OnUpdateEvent -= HandTrackerUpdate;
					handTracker = null;
				}
			}

			if (prevGesture != gesturesOn)
			{
				prevGesture = gesturesOn;
				if (gesturesOn)
				{
					gestureRecognizer = gestureRecognizerInit;
					gestureRecognizerInit.OnNewGesturesEvent += GesturesUpdate;
				}
				else
				{
					gestureRecognizerInit.OnNewGesturesEvent -= GesturesUpdate;
					gestureRecognizer = null;
				}

			}
			//issuesProcessor = (GameObject)Instantiate(issuesProcessorPrefab);
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}


	public void InitModules()
	{
		try
		{
			/*
			nuitrack.Nuitrack.Init();

			depthSensorInit = nuitrack.DepthSensor.Create();
			userTrackerInit = nuitrack.UserTracker.Create();
			skeletonTrackerInit = nuitrack.SkeletonTracker.Create();
			handTrackerInit = nuitrack.HandTracker.Create();
			gestureRecognizerInit = nuitrack.GestureRecognizer.Create();

			nuitrack.Nuitrack.Run ();
			//*/
			issuesProcessor = (GameObject)Instantiate(issuesProcessorPrefab);
			depthUserVisualization = (GameObject)Instantiate(depthUserVisualizationPrefab);
			depthUserMeshVisualization = (GameObject)Instantiate(depthUserMeshVisualizationPrefab);
			skeletonsVisualization = (GameObject)Instantiate(skeletonsVisualizationPrefab);
			handTrackerVisualization = (GameObject)Instantiate(handTrackerVisualizationPrefab);
			gesturesVisualization = (GameObject)Instantiate(gesturesVisualizationPrefab);
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	public void ReleaseNuitrack()
	{
		if (depthSensorInit != null) depthSensorInit.OnUpdateEvent -= DepthUpdate;
        if (colorSensorInit != null) colorSensorInit.OnUpdateEvent -= ColorUpdate;
        if (userTrackerInit != null) userTrackerInit.OnUpdateEvent -= UserUpdate;
		if (skeletonTrackerInit != null) skeletonTrackerInit.OnSkeletonUpdateEvent -= SkeletonsUpdate;
		if (handTrackerInit != null) handTrackerInit.OnUpdateEvent -= HandTrackerUpdate;
		if (gestureRecognizerInit != null) gestureRecognizerInit.OnNewGesturesEvent -= GesturesUpdate;

    	if (issuesProcessor != null)  Destroy(issuesProcessor);
		if (depthUserVisualization != null) Destroy (depthUserVisualization);
		if (skeletonsVisualization != null) Destroy (skeletonsVisualization);
		if (handTrackerVisualization != null) Destroy (handTrackerVisualization);
		if (gesturesVisualization != null) Destroy (gesturesVisualization);

		depthSensorInit = null;
        colorSensorInit = null;
        userTrackerInit = null;
		skeletonTrackerInit = null;
		handTrackerInit = null;
		gestureRecognizerInit = null;

		nuitrack.Nuitrack.Release();
	}

	void OnDestroy()
	{
    Debug.Log("on destroy");
		ReleaseNuitrack();
	}

    void Update () 
	{
		try
		{
//      if (skeletonTracker != null)
//      {
//        nuitrack.Nuitrack.Update(skeletonTracker);
//      }
//      else if (userTracker != null)
//      {
//        nuitrack.Nuitrack.Update(userTracker);
//      }
//      else
      {
        nuitrack.Nuitrack.Update();
      }

      string processingTimesInfo = "";
      if ((userTracker != null)     && (userTracker.GetProcessingTime() > 1f))      processingTimesInfo += "User FPS: " + (1000f / userTracker.GetProcessingTime()).ToString("0") + System.Environment.NewLine;
      if ((skeletonTracker != null) && (skeletonTracker.GetProcessingTime() > 1f))  processingTimesInfo += "Skeleton FPS: " + (1000f / skeletonTracker.GetProcessingTime()).ToString("0") + System.Environment.NewLine;
      if ((handTracker != null)     && (handTracker.GetProcessingTime() > 1f))      processingTimesInfo += "Hand FPS: " + (1000f / handTracker.GetProcessingTime()).ToString("0") + System.Environment.NewLine;


      perfomanceInfoText.text = processingTimesInfo;
		}
		catch (Exception ex)
		{
			exceptionsLogger.AddEntry(ex.ToString());
		}
	}

	void DepthUpdate(nuitrack.DepthFrame _depthFrame)
	{
		depthFrame = _depthFrame;
	}

    void ColorUpdate(nuitrack.ColorFrame _colorFrame)
    {
        colorFrame = _colorFrame;
        //Debug.Log(colorFrame.Timestamp.ToString());
    }
    
    void UserUpdate(nuitrack.UserFrame _userFrame)
	{
		userFrame = _userFrame;
	}

	void SkeletonsUpdate(nuitrack.SkeletonData _skeletonData)
	{
		skeletonData = _skeletonData;
	}

	void HandTrackerUpdate (nuitrack.HandTrackerData _handTrackerData)
	{
		handTrackerData = _handTrackerData;
	}

	void GesturesUpdate (nuitrack.GestureData _gestureUpdateData)
	{
		gesturesData = _gestureUpdateData;
	}
}