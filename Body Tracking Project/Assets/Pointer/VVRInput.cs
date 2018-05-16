using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VVRInput
{
    static nuitrack.PublicNativeImporter.ButtonCallback buttonsCallback;

    static Dictionary<int, buttonState> buttonsState = new Dictionary<int, buttonState>();

    [Flags]
    public enum Button
    {
        None = 0,
        Menu = 1,
        Home = 2,
        A    = 4,
        B    = 8,
        Any = ~None,
	}

    public static Vector2 GetStickPos()
    {
        float x = 0f;
        float y = 0f;
        nuitrack.PublicNativeImporter.nuitrack_getStickPosition(ref x, ref y);
        return new Vector2(x,y);
    }

    public static bool Get(Button virtualMask)
    {
        return GetResolvedButton(virtualMask);
    }

    private static bool GetResolvedButton(Button virtualMask)
    {
        if (buttonsState[(int)virtualMask].currentState == 2)
            return true;

        return false;
    }

    public static bool GetDown(Button virtualMask)
    {
        return GetResolvedButtonDown(virtualMask);
    }

    private static bool GetResolvedButtonDown(Button virtualMask)
    {
        bool down = false;

        if (buttonsState[(int)virtualMask].currentState == 2 && buttonsState[(int)virtualMask].currentState != buttonsState[(int)virtualMask].previousState)
        {
            buttonsState[(int)virtualMask].previousState = buttonsState[(int)virtualMask].currentState;
            down = true;
        }

        return down;
    }

    public static bool GetUp(Button virtualMask)
    {
        return GetResolvedButtonUp(virtualMask);
    }

    private static bool GetResolvedButtonUp(Button virtualMask)
    {
        bool up = false;

        if (buttonsState[(int)virtualMask].currentState == 1 && buttonsState[(int)virtualMask].currentState != buttonsState[(int)virtualMask].previousState)
        {
            buttonsState[(int)virtualMask].previousState = buttonsState[(int)virtualMask].currentState;
            up = true;
        }

        return up;
    }
    
    public static void Init()
    {
        //buttonsCallback = ButtonsCallback;
        //nuitrack.PublicNativeImporter.nuitrack_OnButtonUpdate(buttonsCallback);

        PointerPassing.OnPressed += ButtonPressed;
        buttonsState.Add(1, new buttonState());
        buttonsState.Add(2, new buttonState());
        buttonsState.Add(4, new buttonState());
        buttonsState.Add(8, new buttonState());
    }

    //static void ButtonsCallback(int buttonID, int eventID)
    //{
    //    buttonsState[buttonID] = eventID;
    //}

    static void ButtonPressed(int buttonID, int eventID)
    {
        buttonsState[buttonID].previousState = buttonsState[buttonID].currentState;
        buttonsState[buttonID].currentState = eventID;

        //Debug.Log(eventID + " eventID");
    }
}

class buttonState
{
    public int currentState = 0;
    public int previousState = 0;
}
