using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class webcamera : MonoBehaviour
{

    private bool webcamAvailable;
    private WebCamTexture webcam;
    private Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter fit;

    void Start()
    {
        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.Log("Can't run camera. Not Detected.");
            webcamAvailable = false;
            return;
        }

        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                webcam = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if (webcam == null)
        {
            Debug.Log("Can't open camera.");
            return;
        }

        webcam.Play();
        background.texture = webcam;

        webcamAvailable = true;
    }

    void Update()
    {
        if (!webcamAvailable)
            return;

        float ratio = (float)webcam.width / (float)webcam.height;
        fit.aspectRatio = ratio;

        float scaleY = webcam.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -webcam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);


    }
}