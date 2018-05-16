using System.Collections.Generic;
using UnityEngine;

public class PointerVisuals : MonoBehaviour {
    
    
    [Header("Controller Visuals")]
    [Header("Stick")]
    [SerializeField]Transform stick; //x z
    [SerializeField]float maxAngleStick = 16;
    float baseZStick = -90;

    [Header("Buttons")]
    [SerializeField]Transform menuButton;
    [SerializeField]Transform homeButton;
    [SerializeField]Transform buttonA;
    [SerializeField]Transform buttonB;

    Dictionary<int, Transform> buttons = new Dictionary<int, Transform>();

    void OnEnable()
    {
        PointerPassing.OnPressed += ButtonPressed;

        buttons.Add(1, menuButton);
        buttons.Add(2, homeButton);
        buttons.Add(4, buttonA);
        buttons.Add(8, buttonB);
    }

    void OnDisable()
    {
        PointerPassing.OnPressed -= ButtonPressed;
    }

    private void ButtonPressed(int buttonID, int eventID)
    {
        Transform button;

        buttons.TryGetValue(buttonID, out button);

        if (eventID == 2)
            button.localScale = Vector3.one;
        else
            button.localScale = Vector3.zero;
    }

    private void Update()
    {
        //if (VVRInput.GetDown(VVRInput.Button.A))
        //{
        //    transform.localScale *= 1.1f;
        //    Debug.Log("Press A");
        //}

        //if (VVRInput.GetUp(VVRInput.Button.B))
        //{
        //    transform.localScale /= 1.1f;
        //    Debug.Log("Press B");
        //}

        //if (VVRInput.Get(VVRInput.Button.Menu))
        //{
        //    transform.localScale *= 1.01f;
        //    Debug.Log("Press Menu");
        //}

        //if (VVRInput.Get(VVRInput.Button.Home))
        //{
        //    transform.localScale /= 1.01f;
        //    Debug.Log("Press Home");
        //}

        Vector2 stickPos = VVRInput.GetStickPos();
        stick.localEulerAngles = new Vector3(stickPos.x * maxAngleStick, 0, baseZStick + stickPos.y * maxAngleStick);
    }
}
