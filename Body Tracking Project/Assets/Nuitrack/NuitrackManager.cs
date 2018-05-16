using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InitEvent : UnityEvent<NuitrackInitState>
{
}
public class NuitrackManager : MonoBehaviour
{
	public NuitrackInitState InitState{get{return NuitrackLoader.initState;}}
    [SerializeField]bool 
    depthModuleOn = true,
    colorModuleOn = true,
    userTrackerModuleOn = true,
    skeletonTrackerModuleOn = true,
    gesturesRecognizerModuleOn = true,
    handsTrackerModuleOn = true;

    [SerializeField] bool wifiConnectFromEditor = false;

    static nuitrack.DepthSensor depthSensor;
    public static nuitrack.DepthSensor DepthSensor { get { return depthSensor; } }
    static nuitrack.ColorSensor colorSensor;
    static nuitrack.UserTracker userTracker;
    static nuitrack.SkeletonTracker skeletonTracker;
    static nuitrack.GestureRecognizer gestureRecognizer;
    static nuitrack.HandTracker handTracker;

    static nuitrack.DepthFrame depthFrame;
    public static nuitrack.DepthFrame DepthFrame {get {return depthFrame;}}
    static nuitrack.ColorFrame colorFrame;
    public static nuitrack.ColorFrame ColorFrame { get { return colorFrame; } }
    static nuitrack.UserFrame userFrame;
    public static nuitrack.UserFrame UserFrame {get {return userFrame;}}
    static nuitrack.SkeletonData skeletonData;
    public static nuitrack.SkeletonData SkeletonData{get {return skeletonData;}}
    static nuitrack.HandTrackerData handTrackerData;
    public static nuitrack.HandTrackerData HandTrackerData {get {return handTrackerData;}}

    public static event nuitrack.DepthSensor.OnUpdate onDepthUpdate;
    public static event nuitrack.ColorSensor.OnUpdate onColorUpdate;
    public static event nuitrack.UserTracker.OnUpdate onUserTrackerUpdate;
    public static event nuitrack.SkeletonTracker.OnSkeletonUpdate onSkeletonTrackerUpdate;
    public static event nuitrack.HandTracker.OnUpdate onHandsTrackerUpdate;

    public delegate void OnNewGestureHandler(nuitrack.Gesture gesture);
    public static event OnNewGestureHandler onNewGesture;

    static nuitrack.UserHands currentHands;
    public static nuitrack.UserHands СurrentHands { get { return currentHands; } }

    static NuitrackManager instance;
	NuitrackInitState initState = NuitrackInitState.INIT_NUITRACK_MANAGER_NOT_INSTALLED;
	[SerializeField]InitEvent initEvent;
    public static NuitrackManager Instance
    {
        get 
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NuitrackManager>();
                if (instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "NuitrackManager";
                    instance = container.AddComponent<NuitrackManager>();
                }
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }
  
    void Awake()
    {
    //  NuitrackLoader.InitNuitrackLibraries();
	    initState = NuitrackLoader.InitNuitrackLibraries ();
	    {
		    if (initEvent != null)
		    {
			    initEvent.Invoke (initState);
		    }
	    }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
  
    void Start()
    {
		Application.targetFrameRate = 60;

        //	Debug.Log ("NuitrackStart");

#if UNITY_ANDROID && !UNITY_EDITOR
    if (initState == NuitrackInitState.INIT_OK)
#endif
        NuitrackInit();
    }

	bool prevSkel = false;
	bool prevHand = false;
	bool prevDepth = false;
    bool prevColor = false;
    bool prevGest = false;
	bool prevUser = false;

	void ChangeModulsState(bool skel,bool hand, bool depth, bool color, bool gest, bool user)
	{
//		Debug.Log ("" + skel + hand + depth + gest + user);
		if (skeletonTracker == null)
			return;
		if (prevSkel != skel) {
			skeletonData = null;
			prevSkel = skel;
            if (skel)
            {
                skeletonTracker.OnSkeletonUpdateEvent += HandleOnSkeletonUpdateEvent;
            }
            else
            {
                skeletonTracker.OnSkeletonUpdateEvent -= HandleOnSkeletonUpdateEvent;
            }
		}
		if (prevHand != hand) {
			handTrackerData = null;
			prevHand = hand;
			if(hand)
				handTracker.OnUpdateEvent += HandleOnHandsUpdateEvent;
			else
				handTracker.OnUpdateEvent -= HandleOnHandsUpdateEvent;
		}
		if (prevGest != gest) {
			prevGest = gest;
			if(gest)
				gestureRecognizer.OnNewGesturesEvent += OnNewGestures;
			else
				gestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
		}
		if (prevDepth != depth) {
			depthFrame = null;
			prevDepth = depth;
			if(depth)
				depthSensor.OnUpdateEvent += HandleOnDepthSensorUpdateEvent;
			else
				depthSensor.OnUpdateEvent -= HandleOnDepthSensorUpdateEvent;
		}
        if (prevColor != color)
        {
            colorFrame = null;
            prevColor = color;
            if (color)
                colorSensor.OnUpdateEvent += HandleOnColorSensorUpdateEvent;
            else
                colorSensor.OnUpdateEvent -= HandleOnColorSensorUpdateEvent;
        }
        if (prevUser != user) {
			userFrame = null;
			prevUser = user;
			if(user)
				userTracker.OnUpdateEvent += HandleOnUserTrackerUpdateEvent;
			else
				userTracker.OnUpdateEvent -= HandleOnUserTrackerUpdateEvent;
		}
	}


    void NuitrackInit()
    {
        //    CloseUserGen(); //just in case
        #if UNITY_IOS
			nuitrack.Nuitrack.Init("", nuitrack.Nuitrack.NuitrackMode.DEBUG);
        #else
        if(Application.isEditor && wifiConnectFromEditor)
            nuitrack.Nuitrack.Init("", nuitrack.Nuitrack.NuitrackMode.DEBUG);
        else
            nuitrack.Nuitrack.Init();
        #endif
        Debug.Log("Init OK");

        depthSensor = nuitrack.DepthSensor.Create();

        colorSensor = nuitrack.ColorSensor.Create();

        userTracker = nuitrack.UserTracker.Create();

        skeletonTracker = nuitrack.SkeletonTracker.Create();

        gestureRecognizer = nuitrack.GestureRecognizer.Create();

        handTracker = nuitrack.HandTracker.Create();

		nuitrack.Nuitrack.Run();
        Debug.Log("Run OK");

        ChangeModulsState (
			skeletonTrackerModuleOn,
			handsTrackerModuleOn,
			depthModuleOn,
            colorModuleOn,
            gesturesRecognizerModuleOn,
			userTrackerModuleOn
        );
    }

    void HandleOnDepthSensorUpdateEvent (nuitrack.DepthFrame frame)
    {
//        Debug.Log("Depth Update");
        depthFrame = frame;
        if (onDepthUpdate != null) onDepthUpdate(depthFrame);
    }

    void HandleOnColorSensorUpdateEvent(nuitrack.ColorFrame frame)
    {
        //        Debug.Log("Color Update");
        colorFrame = frame;
        if (onColorUpdate != null) onColorUpdate(colorFrame);
    }

    void HandleOnUserTrackerUpdateEvent (nuitrack.UserFrame frame)
    {
        userFrame = frame;
        if (onUserTrackerUpdate != null) onUserTrackerUpdate(userFrame);
    }

    void HandleOnSkeletonUpdateEvent (nuitrack.SkeletonData _skeletonData)
    {
//        Debug.Log("Skeleton Update");
        skeletonData = _skeletonData;
        if (onSkeletonTrackerUpdate != null) onSkeletonTrackerUpdate(skeletonData);
    }
  
    private void OnNewGestures(nuitrack.GestureData gestures)
    {
        if (gestures.NumGestures > 0)
        {
            if (onNewGesture != null)
            {
                for (int i = 0; i < gestures.Gestures.Length; i++)
                {
                    onNewGesture(gestures.Gestures[i]);
                }
            }
        }
    }
  
    void HandleOnHandsUpdateEvent (nuitrack.HandTrackerData _handTrackerData)
    {
        handTrackerData = _handTrackerData;
        if (onHandsTrackerUpdate != null) onHandsTrackerUpdate(handTrackerData);

        //Debug.Log ("Grabbed hands");
        if (handTrackerData == null) return;
        if (CurrentUserTracker.CurrentUser != 0)
        {
            currentHands = handTrackerData.GetUserHandsByID(CurrentUserTracker.CurrentUser);
        }
        else
        {
            currentHands = null;
        }
    }
	bool pauseState = false;
  
    void OnApplicationPause (bool pauseStatus)
    {
        if (pauseStatus)
        {
		        ChangeModulsState (
			        false,
			        false,
			        false,
			        false,
                    false,
                    false
		        );
		        pauseState = true;
        }
        else
        {
		        ChangeModulsState (
			        skeletonTrackerModuleOn,
			        handsTrackerModuleOn,
			        depthModuleOn,
                    colorModuleOn,
                    gesturesRecognizerModuleOn,
			        userTrackerModuleOn
		        );
		        pauseState = false;
        }
    }
  
  void Update()
  {
		if (Input.GetKeyDown (KeyCode.Escape)) {
					Application.Quit ();
		}

#if UNITY_ANDROID && !UNITY_EDITOR
        if (NuitrackLoader.initState == NuitrackInitState.INIT_OK)
#endif
            if (!pauseState)
            {
                nuitrack.Nuitrack.Update();
            }
    }
  
	public void DepthModuleClose ()
	{
//		Debug.Log ("changeModuls: start");
//		if (!depthModuleOn)
//			return;
		depthModuleOn = false;
		userTrackerModuleOn = false;
		ChangeModulsState (
			skeletonTrackerModuleOn,
			handsTrackerModuleOn,
			depthModuleOn,
            colorModuleOn,
            gesturesRecognizerModuleOn,
			userTrackerModuleOn
		);
//		Debug.Log ("changeModuls: end");
	}

	public void DepthModuleStart ()
	{
//		if (depthModuleOn)
//			return;
		depthModuleOn = true;
		userTrackerModuleOn = true;
		ChangeModulsState (
			skeletonTrackerModuleOn,
			handsTrackerModuleOn,
			depthModuleOn,
            colorModuleOn,
            gesturesRecognizerModuleOn,
			userTrackerModuleOn
		);

//		StartCoroutine (DMStart ());

	}

    public void CloseUserGen()
    {
        if (depthSensor != null) depthSensor.OnUpdateEvent -= HandleOnDepthSensorUpdateEvent;
        if (colorSensor != null) colorSensor.OnUpdateEvent -= HandleOnColorSensorUpdateEvent;
        if (userTracker != null) userTracker.OnUpdateEvent -= HandleOnUserTrackerUpdateEvent;
        if (skeletonTracker != null) skeletonTracker.OnSkeletonUpdateEvent -= HandleOnSkeletonUpdateEvent;
        if (gestureRecognizer != null) gestureRecognizer.OnNewGesturesEvent -= OnNewGestures;
        if (handTracker != null) handTracker.OnUpdateEvent -= HandleOnHandsUpdateEvent;
        //		Debug.Log ("preRelease");
        nuitrack.Nuitrack.Release();
        //		Debug.Log ("postRelease");
        	
        depthSensor = null;
        colorSensor = null;
        userTracker = null;
        skeletonTracker = null;
        gestureRecognizer = null;
        handTracker = null;

        depthFrame = null;
        colorFrame = null;
        userFrame = null;
        skeletonData = null;
        handTrackerData = null;
    }
  
    void OnDestroy()
    {
        CloseUserGen();
    }
}