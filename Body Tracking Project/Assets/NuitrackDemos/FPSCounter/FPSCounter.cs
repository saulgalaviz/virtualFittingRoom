using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour 
{
	[SerializeField]float measureTime = 1f;

	float min_fps;
	float avg_fps;
	TextMesh tm;

	float timer = 0f;
	int frames = 0;

	Renderer[] rends;

	bool renders_active;

	void Start()
	{
		rends = gameObject.GetComponentsInChildren<Renderer>();
		tm = gameObject.GetComponent<TextMesh>();
	}

	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.F1)) 
		{
			renders_active = !renders_active;
			foreach (Renderer r in rends)
			{
				r.enabled = renders_active;
			}
		}

		if (tm != null)
		{
      timer += Time.unscaledDeltaTime;
			++frames;

			if (min_fps == 0)
			{
        min_fps = 1f / Time.unscaledDeltaTime;
			}
			else
			{
        float fps = 1f / Time.unscaledDeltaTime;
				if (fps < min_fps) min_fps = fps;
			}

			if (timer > measureTime)
			{
				avg_fps = frames / timer;


				tm.text = /*"avg: " + */"Rendering FPS: " + avg_fps.ToString("0")/*"; min: " + min_fps.ToString("0")*/;

				frames = 0;
				min_fps = 0f;
				timer = 0f;
			}
		}
	}
}
