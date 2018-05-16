using UnityEngine;
using System.Collections;

//logic (and number of tracked users) may change from app to app
//for developer's needs
//in current case it's an ID of first found skeleton from skeleton tracker
//and reset only if we have a frame with no current skeleton ID

public class CurrentUserTracker : MonoBehaviour
{
    static int currentUser;
    public static int CurrentUser  { get { return currentUser; } }

    static nuitrack.Skeleton currentSkeleton;
    public static nuitrack.Skeleton CurrentSkeleton {get {return currentSkeleton;}}

    static CurrentUserTracker instance;

    public static CurrentUserTracker Instance
    {
        get 
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CurrentUserTracker>();
                if (instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "CurrentUserTracker";
                    instance = container.AddComponent<CurrentUserTracker>();
                }

                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    void Start ()
    {
        DontDestroyOnLoad(this);
        NuitrackManager.onSkeletonTrackerUpdate += NuitrackManager_onSkeletonTrackerUpdate;
    }

    void NuitrackManager_onSkeletonTrackerUpdate (nuitrack.SkeletonData skeletonData)
    {
        if ((skeletonData == null) || (skeletonData.NumUsers == 0))
        {
            currentUser = 0;
            currentSkeleton = null;
            return; 
        }

        if (currentUser != 0)
        {
            currentSkeleton = skeletonData.GetSkeletonByID (currentUser);
            currentUser = (currentSkeleton == null) ? 0 : currentUser;
        }

        if (currentUser == 0)
        {
            currentUser = skeletonData.Skeletons[0].ID;
            currentSkeleton = skeletonData.Skeletons[0];
        }
    }
}