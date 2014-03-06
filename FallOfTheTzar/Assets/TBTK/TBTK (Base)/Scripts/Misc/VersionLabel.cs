using UnityEngine;
using System.Collections;

public class VersionLabel : MonoBehaviour {
	
	#if !UNITY_IPHONE && !UNITY_ANDROID
	public bool menu;
	public string label1 = "TBTK version1.0 Demo by K.SongTan";
	public string label2 = "http://song-gamedev.blogspot.co.uk/";
	
	// Use this for initialization
	void Start () {
		//DontDestroyOnLoad(gameObject);
		
		VersionLabel[] vLabels = FindObjectsOfType(typeof(VersionLabel)) as VersionLabel[];
		if(vLabels.Length>1) Destroy(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI(){
		GUI.depth=0;
		
		if(menu){
			GUI.Label(new Rect(Screen.width/2-105, Screen.height-40, 450, 25), label1);
			GUI.Label(new Rect(Screen.width/2-101, Screen.height-24, 450, 25), label2);
		}
		else{
			GUI.Label(new Rect(5, 5, 450, 25), label1);
			GUI.Label(new Rect(5, 20, 450, 25), label2);
		}
	}
	#endif
	
}
