using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIUnitPlacement : MonoBehaviour {

	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI(){
		List<UnitTB> list=UnitControl.GetUnplacedUnit();
		
		GUIStyle style=new GUIStyle();
		style.fontStyle=FontStyle.Bold;
		
		int width=500;
		int height=180;
		
		if(list.Count>0){
			UnitTB sUnit=list[UnitControl.GetUnitPlacementID()];

			for(int i=0; i<3; i++) GUI.Box(new Rect(Screen.width/2-width/2, Screen.height-185, width, height), "");
			
			GUI.DrawTexture(new Rect(Screen.width/2-width/2+25, Screen.height-180-40, 60, 60), UI.texBar);
			GUI.DrawTexture(new Rect(Screen.width/2-width/2+25+2, Screen.height-180-40+2, 56, 56), sUnit.icon);
			
			style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
			GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-180, width, height), sUnit.unitName, style);
			
			style.fontSize=16;	style.normal.textColor=UI.colorN;	style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
			GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-150, width, height), sUnit.desp, style);
			
			style.fontSize=16; style.normal.textColor=UI.colorH; style.wordWrap=false;
			GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-30, width, height), "Units to be deployed: "+list.Count, style);
			
			
			GUI.color=Color.white;
			
			if(GUI.Button(new Rect(Screen.width/2-width/2+10, Screen.height-50, 50, 40), "<<-", UI.buttonStyle)){
				UnitControl.PrevUnitPlacementID();
			}
			if(GUI.Button(new Rect(Screen.width/2+width/2-100, Screen.height-50, 50, 40), "->>", UI.buttonStyle)){
				UnitControl.NextUnitPlacementID();
			}
			
			
			if(GUI.Button(new Rect(Screen.width-100, Screen.height-65, 60, 60), "Auto\nPlace", UI.buttonStyle)){
				UnitControl.AutoPlaceUnit();
			}
			
		}
		else{
			
			style.fontSize=16; style.normal.textColor=UI.colorH; style.alignment=TextAnchor.UpperCenter;
			GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-30, width, height), "All units had been deployed", style);
			
			if(GUI.Button(new Rect(Screen.width-100, Screen.height-65, 60, 60), "Start\nBattle", UI.buttonStyle)){
				if(UnitControl.GetPlayerUnitsRemainng()==1 && UnitControl.IsFactionAllUnitPlaced()){
					GameControlTB.UnitPlacementCompleted();
					this.enabled=false;
				}
				//if all unit for this faction is done, move to next faction
				else if(UnitControl.IsFactionAllUnitPlaced()){
					UnitControl.NextFactionPlacementID();
				}
				//if not all unit has been placed, prompt player to place the remaining unit
				else{
					//UINGUI.DisplayMessage("You must place all your unit first!");
				}
				
				
			}
		}
	}
	
}
