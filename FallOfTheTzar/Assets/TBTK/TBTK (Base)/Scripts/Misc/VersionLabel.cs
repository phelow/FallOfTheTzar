#define customSkin
using UnityEngine;
using System.Collections;

public class VersionLabel : MonoBehaviour {
	#if customSkin
	public GUISkin customSkin;
	#endif

	#if !UNITY_IPHONE && !UNITY_ANDROID
	public bool menu;
	public string label1 = "Fall of the Tzar \n Test Version 1";

	public  int WIDTH = 450;
	public  int HEIGHT = 25;
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
		#if customSkin
		GUI.skin = customSkin;
		
		#endif

		GUI.depth=0;
		
		if(menu){
			GUI.Label(new Rect(Screen.width/2-WIDTH/2, Screen.height-HEIGHT, WIDTH, HEIGHT), label1);
		}
		else{
			GUI.Label(new Rect(5, 5, WIDTH, HEIGHT), label1);
		}
	}
	#endif
	
}
