#define customGui
using UnityEngine;
using System.Collections;

public class UIGameOver : MonoBehaviour {
#if customGui
	GUISkin customSkin;
#endif

	private int vicFactionID=-1;
	
	// Use this for initialization
	void Start () {
		outcomeTxt="Faction 1 Won!!";
		//~ statsTxt="15\n5\n5";
		//~ pointTxt="20";
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Z)){
			Show(0);
		}
	}
	
	private string outcomeTxt="";
	private string statsTxt="";
	private string pointTxt="";
	public void Show(int vicID){
		vicFactionID=vicID;
		
		if(GameControlTB.playerFactionExisted){
			if(GameControlTB.IsHotSeatMode()){
				if(GameControlTB.IsPlayerFaction(vicFactionID)){
					outcomeTxt="Victory!!";
				}
				else{
					outcomeTxt="Defeated...";
				}
			}
			//single player mode
			else{
				if(GameControlTB.IsPlayerFaction(vicFactionID)){
					outcomeTxt="Victory!!";
				}
				else{
					outcomeTxt="Defeated";
				}
			}
		}
		else{
			outcomeTxt="Faction "+vicFactionID+" Won!!";
		}
		
		if(!GameControlTB.IsHotSeatMode() && GameControlTB.playerFactionExisted && GameControlTB.LoadMode()==_LoadMode.UsePersistantData){
			int pointGain=GameControlTB.instance.pointGain;
			int pointReward=GameControlTB.instance.winPointReward;
			if(!GameControlTB.IsPlayerFaction(vicFactionID)) pointReward=0;
			
			int killBonus=pointGain-pointReward;
			int total=GlobalStatsTB.GetPlayerPoint();
			int initial=total-pointGain;
			
			statsTxt=killBonus+"\n"+pointReward+"\n"+initial;
			pointTxt=total.ToString();
		}
	}
	
	private int boxHeight=400;
	void OnGUI(){

		#if customGui
		customSkin = (GUISkin)Resources.Load ("NecromancerGUI");;
		GUI.skin = customSkin;
		#endif
		//~ if(vicFactionID<0) return;
		
		GUIStyle style=new GUIStyle();
		style.alignment=TextAnchor.UpperCenter;
		style.fontSize=20;	style.fontStyle=FontStyle.Bold;	style.normal.textColor=UI.colorH;
		
		int startX=Screen.width/2-100;
		int startY=Screen.height/2-150;
		int width=180; int height=30; int spaceY=35;
		
		for(int i=0; i<3; i++) GUI.Box(new Rect(startX, startY, width+20, boxHeight), "");
		
		startX+=10;	startY+=10;
		
		GUI.Label(new Rect(startX, startY, width, height), outcomeTxt, style);
		
		startY+=20;
		
		
		if(statsTxt!="" && pointTxt!=""){
			style.alignment=TextAnchor.UpperLeft;
			style.fontSize=14;	style.fontStyle=FontStyle.Bold;	style.normal.textColor=UI.colorN;
			GUI.Label(new Rect(startX, startY+35, width, 100), "Kill Bonus:\nWin Bonus:\nInitial:", style);
			GUI.Label(new Rect(startX, startY+85, width, 100), "Total:", style);
			
			style.alignment=TextAnchor.UpperRight;
			GUI.Label(new Rect(startX, startY+35, width, 100), statsTxt, style);
			GUI.Label(new Rect(startX, startY+85, width, 100), pointTxt, style);
			
			GUI.Label(new Rect(startX, startY+69, width, 100), "_____________________________________________", style);
		}
		
		startY+=120;
		if(GUI.Button(new Rect(startX, startY+=spaceY, width, height), "Next", UI.buttonStyleAlt)){
			
		}
		if(GUI.Button(new Rect(startX, startY+=spaceY, width, height), "Restart", UI.buttonStyleAlt)){
			
		}
		if(GUI.Button(new Rect(startX, startY+=spaceY, width, height), "Main Menu", UI.buttonStyleAlt)){
			
		}
		
		startY+=spaceY+10;
		boxHeight=startY-(Screen.height/2-150);
		Debug.Log(boxHeight);
	}
	
	
}
