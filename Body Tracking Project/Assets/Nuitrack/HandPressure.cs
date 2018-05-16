using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class PressureBone
{
    public Transform bone;
    public Vector3 minAngle, maxAngle;
}

public class HandPressure : MonoBehaviour {

    [SerializeField][Range(0, 1)]
    float pressure;
    float pressSpeed = 20;
    [SerializeField] PressureBone[] bones;
    [SerializeField] bool rightHand = true;
    nuitrack.HandContent hand;

    float minPressure = .5f, maxPressure = 1.0f;

    void Start () {

    }

	void Update () {
        //Debug.Log(NuitrackManager.СurrentHands);

        if(Application.platform != RuntimePlatform.WindowsEditor)
        {
            if (NuitrackManager.СurrentHands != null)
            {
                if (rightHand)
                {
                    if (NuitrackManager.СurrentHands.RightHand != null)
                    {
                        hand = (nuitrack.HandContent)NuitrackManager.СurrentHands.RightHand;

                        pressure = Mathf.Lerp(pressure, hand.Pressure / 100.0f, pressSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    if (NuitrackManager.СurrentHands.LeftHand != null)
                    {
                        hand = (nuitrack.HandContent)NuitrackManager.СurrentHands.LeftHand;
                        pressure = Mathf.Lerp(pressure, hand.Pressure / 100.0f, pressSpeed * Time.deltaTime);
                    }
                }
            }
        }

        //pressure = Mathf.InverseLerp(minPressure, maxPressure, pressure);

        if (pressure > maxPressure) maxPressure = pressure;
        if (pressure < minPressure) minPressure = pressure;

        for (int i = 0; i < bones.Length; i++)
        {
            bones[i].bone.localEulerAngles = Vector3.Lerp(bones[i].minAngle, bones[i].maxAngle, Mathf.InverseLerp(minPressure, maxPressure, pressure));
        }
    }
}
