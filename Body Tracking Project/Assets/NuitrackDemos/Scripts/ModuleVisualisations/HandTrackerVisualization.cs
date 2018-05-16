using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HandTrackerVisualization : MonoBehaviour 
{
	NuitrackModules nuitrackModules;
	nuitrack.HandTrackerData handTrackerData = null;
	[SerializeField]Transform handsContainer;
	[SerializeField]GameObject handUIPrefab;
	[SerializeField]float sizeNormal, sizeClick;
	[SerializeField]Color leftColor, rightColor;
	Dictionary<int, Image[]> hands;

	void Start () 
	{
		nuitrackModules = FindObjectOfType<NuitrackModules>();
		hands = new Dictionary<int, Image[]>();
	}
	
	void Update () 
	{
		if (nuitrackModules.HandTrackerData != null)
		{
			if (handTrackerData != nuitrackModules.HandTrackerData)
			{
				handTrackerData = nuitrackModules.HandTrackerData;
				ProcessHands(handTrackerData);
			}
		}
    else
    {
      HideHands();
    }
	}

  void HideHands()
  {
    foreach (KeyValuePair<int, Image[]> kvp in hands)
    {
      hands[kvp.Key][0].enabled = false;
      hands[kvp.Key][1].enabled = false;
    }
  }

	void ProcessHands(nuitrack.HandTrackerData data)
	{
		if (data.NumUsers > 0)
		{
			for (int i = 0; i < data.UsersHands.Length; i++)
			{
				int userId = data.UsersHands[i].UserId;

				if (!hands.ContainsKey(userId))
				{
					hands.Add(userId, new Image[2]);
					GameObject leftHand = (GameObject)Instantiate(handUIPrefab);
					GameObject rightHand = (GameObject)Instantiate(handUIPrefab);

					leftHand.transform.SetParent(handsContainer, false);
					rightHand.transform.SetParent (handsContainer, false);

					hands[userId][0] = leftHand.GetComponent<Image>();
					hands[userId][1] = rightHand.GetComponent<Image>();

					hands[userId][0].enabled = false;
					hands[userId][1].enabled = false;
					hands[userId][0].color = leftColor;
					hands[userId][1].color = rightColor;
				}
			}

			foreach (KeyValuePair<int, Image[]> kvp in hands)
			{
				nuitrack.UserHands userHands = data.GetUserHandsByID(kvp.Key);
				if (userHands == null)
				{
					hands[kvp.Key][0].enabled = false;
					hands[kvp.Key][1].enabled = false;
				}
				else
				{
          if ((userHands.LeftHand == null) || (userHands.LeftHand.Value.X == -1f))
					{
						hands[kvp.Key][0].enabled = false;
					}
					else
					{
						hands[kvp.Key][0].enabled = true;
						Vector2 pos = new Vector2(userHands.LeftHand.Value.X, 1f - userHands.LeftHand.Value.Y);
						hands[kvp.Key][0].rectTransform.anchorMin = pos;
						hands[kvp.Key][0].rectTransform.anchorMax = pos;
						hands[kvp.Key][0].rectTransform.sizeDelta = userHands.LeftHand.Value.Click ? new Vector2(sizeClick, sizeClick) : new Vector2(sizeNormal, sizeNormal);
					}

          if ((userHands.RightHand == null) || (userHands.RightHand.Value.X == -1f))
					{
						hands[kvp.Key][1].enabled = false;
					}
					else
					{
						hands[kvp.Key][1].enabled = true;
						Vector2 pos = new Vector2(userHands.RightHand.Value.X, 1f - userHands.RightHand.Value.Y);
						hands[kvp.Key][1].rectTransform.anchorMin = pos;
						hands[kvp.Key][1].rectTransform.anchorMax = pos;
						hands[kvp.Key][1].rectTransform.sizeDelta = userHands.RightHand.Value.Click ? new Vector2(sizeClick, sizeClick) : new Vector2(sizeNormal, sizeNormal);
					}
				}
			}
		}
		else
		{
			foreach (KeyValuePair<int, Image[]> kvp in hands)
			{
				kvp.Value[0].enabled = false;
				kvp.Value[1].enabled = false;
			}
		}
	}
}
