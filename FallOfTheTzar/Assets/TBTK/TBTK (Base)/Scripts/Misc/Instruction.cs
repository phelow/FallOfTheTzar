using UnityEngine;
using System.Collections;

public class Instruction : MonoBehaviour {

	#if !UNITY_IPHONE && !UNITY_ANDROID
	private string camInst="";
	
	private bool show=false;
	
	// Use this for initialization
	void Start () {
		camInst="";
		camInst+="- 'w','a','s','d' key to pan the camera\n";
		camInst+="- 'q' and 'e' key to rotate the view angle\n";
		camInst+="- mouse wheel to zoom\n";
		
		camInst+="\n'Space' to Toggle unit's overlay\n";
		
		camInst+="\nRight click on any of the unit to show unit stats\n";
		
		camInst+="\nWhan an ability is selected, Right click or escape key to cancel an ability\n";
		//camInst+="\n- mouse wheel to zoom\n";
		
		camInst+="\nDuring unit placement, click on the green tile to place unit, click on placed unit to reposition it";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI(){
		//~ for(int i=0; i<2; i++) GUI.Box(new Rect(Screen.width-315, 7, 90, 41), "");
		string label="Show Instruction";
		if(show) label="Hide Instruction";
		if(GUI.Button(new Rect(Screen.width-220, 10, 120, 30), label)){
		//~ if(GUI.Button(new Rect(Screen.width-130, Screen.height*0.3f, 120, 30), label)){
			show=!show;
		}
		
		if(show) GUI.Label(new Rect(Screen.width-310, +45, 300, 500), camInst);
		//~ if(show) GUI.Label(new Rect(Screen.width-310, Screen.height*0.3f+45, 300, 500), camInst);
	}
	#endif
}
