using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIPerkMenu : MonoBehaviour {

	[HideInInspector] public bool isOn=false;
	
	//private bool showPlayerPoint=true;	//this is for the setup scene where dislay of player point is not required
	//public void SetShowPlayerPoint(bool flag){ showPlayerPoint=flag; } //UpdatePoint(); }
	
	

	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void OnPerkmenu(){
		isOn=!isOn;
	}
	
	private int selectedID=0;
	
	private Vector2 scrollPosition;
	public void DrawPerkMenu(){ DrawPerkMenu(false); }
	public void DrawPerkMenu(bool preBattle){
		//GUI.depth = 0;
		GUIStyle style=new GUIStyle();
		style.fontSize=20;	style.fontStyle=FontStyle.Bold;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
		
		
		List<PerkTB> perkList=PerkManagerTB.GetAllPerks();
		PerkTB perk=null;
		
		
		int winWidth=595;
		int winHeight=470;
		
		int startX=Screen.width/2-winWidth/2;
		int startY=Screen.height/2-winHeight/2;
		
		int rowLimit=8;
		
		int row=0;		int column=0;
		for(int i=0; i<3; i++) GUI.Box(new Rect(startX, startY, winWidth, winHeight), "");
		
		GUI.Label(new Rect(startX, startY+10, winWidth, winHeight), "Perk Menu", style);
		
		
		int point=0;
		if(!PerkManagerTB.UsePersistentData()) point=PerkManagerTB.GetPoint();
		else point=GlobalStatsTB.GetPlayerPoint();
		
		style.fontSize=14;	style.fontStyle=FontStyle.Bold;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperLeft;
		if(!preBattle) GUI.Label(new Rect(startX+10, startY+5, 150, 25), "Point: "+point, style);
		GUI.Label(new Rect(startX+10, startY+20, 150, 25), "PerkPoint: "+PerkManagerTB.perkPoint, style);
		
		if(!preBattle){
			if(GUI.Button(new Rect(startX+winWidth-35, startY+5, 30, 30), "X", UI.buttonStyle)){
				OnPerkmenu();
			}
		}
		
		startX+=10;	startY+=40;
		
		int scrollLength=(int)(Mathf.Ceil(perkList.Count/8+1)*70);
		if(perkList.Count%rowLimit==0) scrollLength-=70;
		Rect viewRect=new Rect(startX, startY, winWidth-20, 280);
		Rect scrollRect=new Rect(startX, startY, winWidth-50, scrollLength);
		GUI.Box(viewRect, "");
		scrollPosition = GUI.BeginScrollView (viewRect, scrollPosition, scrollRect);
		
			startX+=5;	startY+=5;
		
			for(int ID=0; ID<perkList.Count; ID++){
				perk=perkList[ID];
				Texture icon=null;
				if(perk.unlocked) icon=perk.iconUnlocked;
				else{
					if(perk.IsAvailable()==0) icon=perk.icon;
					else icon=perk.iconUnavailable;
				}
				
				if(selectedID==ID){
					if(GUI.Button(new Rect(startX+column*70-3, startY+row*70-3, 66, 66), icon)) selectedID=ID;
				}
				else{
					if(GUI.Button(new Rect(startX+column*70, startY+row*70, 60, 60), icon)) selectedID=ID;
				}
				
				column+=1;
				if(column==8){
					row+=1; column=0;
				}
			}
		
		GUI.EndScrollView();
			
		startY+=285;
		GUI.Box(new Rect(startX-5, startY, winWidth-20, 130), "");
			
		perk=perkList[selectedID];
		int perkAvai=perk.IsAvailable();
			
		string status="";
		if(perk.unlocked) status=" (Unlocked)";
		else if(perkAvai!=0) status=" (Unavailable)";
			
		style.fontSize=17;	style.alignment=TextAnchor.UpperLeft;	style.normal.textColor=UI.colorH;
		GUI.Label(new Rect(startX, startY+=5, winWidth-30, 30), perk.name+status, style);
			
		style.fontSize=17;	style.alignment=TextAnchor.UpperRight;	style.normal.textColor=UI.colorH;
		GUI.Label(new Rect(startX, startY+=5, winWidth-30, 30), "Cost: "+perk.cost, style);
			
		style.alignment=TextAnchor.UpperLeft;	style.normal.textColor=UI.colorN; style.wordWrap=true;
		GUI.Label(new Rect(startX, startY+=25, winWidth-30, 30), perk.desp, style);
		
		
		
		string requireTxt="";
		
		
		
		//inssufficient points
		if(perkAvai==1){
			requireTxt="Require "+perk.cost+"point";
		}
		//min perk point not reach
		else if(perkAvai==2){
			requireTxt="Require "+perk.pointReq+" perk points";
		}
		//pre-req perk not unlocked
		else if(perkAvai==3){
			string text="Require: ";
			
			List<int> list=perk.prereq;
			int i=0;
			foreach(int ID in list){
				if(i>0) text+=", ";
				text+=PerkManagerTB.GetAllPerks()[ID].name;
				i+=1;
			}
			
			requireTxt=text;
		}
		
		style.fontSize=14;	style.alignment=TextAnchor.UpperLeft;	style.normal.textColor=Color.red; style.wordWrap=false;
		GUI.Label(new Rect(startX, startY+75, winWidth-30, 30), requireTxt, style);
			
			
		if(!perk.unlocked && perkAvai==0){
			if(GUI.Button(new Rect(startX+winWidth-30-120, startY+=60, 120, 30), "Unlock", UI.buttonStyle)){
				OnUnlockPerkButton(perk);
			}
		}
	}
	
	int GetPlayerPoint(){
		//~ if(UINGUISetup.IsSetupScene()){
			//~ return UINGUISetup.playerPoint;
		//~ }
		//~ else{
			if(!PerkManagerTB.UsePersistentData())  return PerkManagerTB.GetPoint();
			else return GlobalStatsTB.GetPlayerPoint();
		//~ }
	}
	
	public void OnUnlockPerkButton(PerkTB perk){
		if(perk.cost>GetPlayerPoint()){
			Debug.Log("Insufficient Points!");
			//UINGUISetup.DisplayMessage("Insufficient Points!");
			return;
		}
		
		if(PerkManagerTB.UnlockPerk(selectedID)){
			Debug.Log(perk.name+" has been unlocked");
			//UINGUISetup.DisplayMessage(selectedPerk.name+" has been unlocked");
		}
	}
	
	
}
