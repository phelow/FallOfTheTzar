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
#if DEBUG
			Debug.Log("CurrentLevel set to:"+Application.loadedLevelName);
#endif
			PlayerPrefs.SetString("CurrentLevel",Application.loadedLevelName);
			levelSet = true;
		}
	}
}
