using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using nuitrack;

public class BackTextureCreator : MonoBehaviour {

	[SerializeField] bool userColorizeEnable = false;
	Texture2D tex;
	Texture2D userTex;
	[SerializeField] GameObject wall;
	Color32[] newTexture32;
	Color[] newTexture;
	float gray;

    DepthFrame depthFrame;

	public Texture GetRGBTexture{ 
		get { 
			return (Texture)tex; 
		}
	}
	public Texture GetUserTexture{ 
		get { 
			return (Texture)userTex; 
		}
	}
	public delegate void newBackGroundCreate(Texture txtr,Texture userTxtr);
	static public event newBackGroundCreate newTextureEvent;

	Dictionary<ushort, Color> UsersColor;

	void Start () {
        NuitrackManager.onDepthUpdate += DepthUpdate;
        NuitrackManager.onColorUpdate += ColorUpdate;
        if (userColorizeEnable)
			NuitrackManager.onUserTrackerUpdate += ColorizeUser;
		UsersColor = new Dictionary<ushort,Color> ();
		UsersColor.Add (0, new Color(0,0,0,0));
		UsersColor.Add (1, Color.red);
		UsersColor.Add (2, Color.red);
		UsersColor.Add (3, Color.red);
		UsersColor.Add (4, Color.red);
		UsersColor.Add (5, Color.red);
	}
	void OnDestroy()
	{
		NuitrackManager.onColorUpdate -= ColorUpdate;
        NuitrackManager.onDepthUpdate -= DepthUpdate;
        if (userColorizeEnable)
			NuitrackManager.onUserTrackerUpdate -= ColorizeUser;
	}

    //void SegmentationTextureWriting(UserFrame frame)
	void ColorizeUser(UserFrame frame)
	{
        //Debug.Log ("colorize");
		int cols = frame.Cols;
		int rows = frame.Rows;
		if ((newTexture == null) || (newTexture.Length != (cols * rows)) ) 
		{
			newTexture = new Color[cols * rows];

			if(userTex == null)
				userTex = new Texture2D (cols, rows, TextureFormat.ARGB32, false);

            //if(wall!= null)
            //wall.GetComponent<MeshRenderer> ().material.mainTexture = userTex;
		}
		Color pix;
		int userId = CurrentUserTracker.CurrentUser;
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				if (frame [i, j] == userId)
					pix = UsersColor [frame [i, j]];
				else
					pix = UsersColor [0];
				newTexture [i * cols + (cols - 1 - j)] = pix;
			}
		}
		userTex.SetPixels (newTexture);
		userTex.Apply ();

	}

    void DepthUpdate(DepthFrame frame)
    {
        depthFrame = frame;
    }

	void ColorUpdate(ColorFrame frame)
	{
		int cols = frame.Cols;
		int rows = frame.Rows;
        
		if ((newTexture32 == null) || (newTexture32.Length != (cols * rows)) ) 
		{
			newTexture32 = new Color32[cols * rows];
            //if (tex != null) {Destroy(tex); tex = null;}
            //tex = new Texture2D (cols, rows, TextureFormat.ARGB32, false);
			if (tex == null)
				tex = new Texture2D (cols, rows, TextureFormat.ARGB32, false);
			if(wall!= null)
				wall.GetComponent<MeshRenderer> ().material.mainTexture = tex;
		}
		Color32 pix;
		for (int i = 0, ptr = 0; i < rows; i++, ptr += cols) {
			for (int j = 0; j < cols; j++) {
				
				try{
                    if(frame != null)
					    pix = new Color32 (frame[i, j].Red, frame[i, j].Green, frame[i, j].Blue, 255);
                    else
                    {
                        int depth = depthFrame[i, j] / 64;
                        pix = new Color32((byte)depth, (byte)depth, (byte)depth, 255);
                    }
                    newTexture32 [ptr + (cols - 1 - j)] = pix;
				}
				catch {
					Debug.LogError ("index out of frame" + cols + " " + rows);
					return;
				}
			}
		}
		tex.SetPixels32 (newTexture32);
		tex.Apply ();
		if (newTextureEvent != null)
			newTextureEvent ((Texture)tex,(Texture)userTex);
	}
}
