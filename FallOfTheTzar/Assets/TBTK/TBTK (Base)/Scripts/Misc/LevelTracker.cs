using UnityEngine;
using System.Collections;

public class LevelTracker : MonoBehaviour {
	bool levelSet = false;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(!levelSet){
			Debug.Log("CurrentLevel set to:"+Application.loadedLevelName);
			PlayerPrefs.SetString("CurrentLevel",Application.loadedLevelName);
			levelSet = true;
		}
	}
}
