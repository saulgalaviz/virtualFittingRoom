using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FromGithub : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void switchScene()
    {
        SceneManager.LoadScene(6);
    }

    public void quit()
    {
        Application.Quit();
    }
}
