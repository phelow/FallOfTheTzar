using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIAbilityButtons : MonoBehaviour {

	private UnitTB selectedUnit;
	GUIStyle styleA;
	private int selectedID=-1;
	
	public bool IsOn(){ return selectedID<0 ? false : true; }
	
	// Use this for initialization
	void Start () {
		styleA = new GUIStyle();
		styleA.fontStyle=FontStyle.Bold;
		
		styleA.fontSize = 20;
	}
	
	void OnEnable(){
		UnitTB.onUnitSelectedE += OnUnitSelected;
	}
	void OnDisable(){
		UnitTB.onUnitSelectedE -= OnUnitSelected;
	}
	
	void OnUnitSelected(UnitTB sUnit){ StartCoroutine(_OnUnitSelected(sUnit)); }
	IEnumerator _OnUnitSelected(UnitTB sUnit){
		yield return null;
		if(sUnit.IsControllable()){
			selectedUnit=sUnit;
			selectedID=-1;
			
			UnitTB.onUnitDeselectedE += OnUnitDeselected;
			UnitTB.onUnitDestroyedE += OnUnitDestroyed;
		}
		else{
			selectedUnit=null;
		}
	}
	
	void OnUnitDestroyed(UnitTB unit){
		if(unit==selectedUnit){
			OnUnitDeselected();
		}
	}
	void OnUnitDeselected(){
		selectedUnit=null;
		UnitTB.onUnitDeselectedE -= OnUnitDeselected;
		UnitTB.onUnitDestroyedE -= OnUnitDestroyed;
	}
	
	void Update(){
		if(Input.GetMouseButtonDown(1)){
			if(selectedID>=0) ExitUnitAbilityTargetSelect();
		}
	}
	
	public void Draw(){
		if(selectedUnit!=null){
			DrawAbilityButtons();
			
			if(GUI.tooltip!=""){
				UnitAbility ability=selectedUnit.unitAbilityList[int.Parse(GUI.tooltip)];
				ShowTooltip(ability);
			}
		}
	}
	
	void DrawAbilityButtons(){
		List<UnitAbility> list=selectedUnit.unitAbilityList;
		
		float windowSize=(list.Count-1)*65;
		int startX=Screen.width/2-(int)(windowSize/2)-30;
		
		//int startX=70;
		int startY=Screen.height-65;
		int height=60; int width=60;
		
		for(int i=0; i<list.Count; i++){
			if(selectedUnit==null) return;
			
			Texture icon=null;
			int status=selectedUnit.IsAbilityAvailable(i);
			if(status==0 || status==7) icon=list[i].icon;
			else icon=list[i].iconUnavailable;

			Rect buttonRect=new Rect(startX, startY, width, height);
			if(selectedID==i) buttonRect=new Rect(startX-3, startY-3, width+6, height+6);
			
			GUIContent cont=new GUIContent(icon, i.ToString());
			GUI.skin.button.alignment = TextAnchor.MiddleRight;
			if(GUI.Button(buttonRect, cont)){
				OnAbilityButton(i);
			}
			GUI.skin.button.alignment = TextAnchor.MiddleCenter;
			
			//add an AP label
			GUI.Label(new Rect(startX+20,startY+18,width,height),""+list[i].cost,styleA);
			startX+=width+5;
		}
	}
	
	public void OnAbilityButton(int ID){
		int status=selectedUnit.ActivateAbility(ID);
		if(status>0){
			//~ if(status==1) UINGUI.DisplayMessage("Ability is used up");
			//~ if(status==2) UINGUI.DisplayMessage("Insufficient AP");
			//~ if(status==3) UINGUI.DisplayMessage("Ability is on cooldown");
			//~ if(status==4) UINGUI.DisplayMessage("Unit are stunned");
			//~ if(status==5) UINGUI.DisplayMessage("Abilities are disabled");
			//~ if(status==6) UINGUI.DisplayMessage("Unit has end it's turn");
			//~ if(status==7) ExitUnitAbilityTargetSelect();
		}
		else{
			selectedID=ID;
		}
	}
	
	public void ExitUnitAbilityTargetSelect(){
		selectedID=-1;
		UnitControl.selectedUnit.SetActiveAbilityPendingTarget(null);
		GridManager.ExitTargetTileSelectMode();
	}
	
	void ShowTooltip(UnitAbility ability){
#if mousePos
		GUIStyle style=new GUIStyle();
		style.fontStyle=FontStyle.Bold;
		
		int width=500;
		int w_offset =50;
		int height=160;
		float posY = Input.mousePosition.y;
		float posX = Input.mousePosition.x;
		
		for(int i=0; i<3; i++) GUI.Box(new Rect(posX-(width+w_offset)/2, posY-230, width+w_offset, height), "");
		
		style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
		GUI.Label(new Rect(posX-width/2, posY-240, width, height), ability.name, style);
		
		style.fontSize=16;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperRight;
		GUI.Label(new Rect(posX-width/2-5, posY-240, width, height), ability.cost+"AP", style);
		
		style.fontSize=16;	style.normal.textColor=UI.colorN;	style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
		GUI.Label(new Rect(posX-width/2, posY-190, width, height), ability.desp, style);
		
		GUI.color=Color.white;
#else
		GUIStyle style=new GUIStyle();
		style.fontStyle=FontStyle.Bold;
		
		int width=500;
		int w_offset =50;
		int height=160;
		
		
		for(int i=0; i<3; i++) GUI.Box(new Rect(Screen.width/2-(width+w_offset)/2, Screen.height-230, width+w_offset, height), "");
		
		style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
		GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-200, width, height), ability.name, style);
		
		style.fontSize=16;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperRight;
		GUI.Label(new Rect(Screen.width/2-width/2-5, Screen.height-200, width, height), ability.cost+"AP", style);
		
		style.fontSize=16;	style.normal.textColor=UI.colorN;	style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
		GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-180, width, height), ability.desp, style);
		
		GUI.color=Color.white;
#endif
	}
}
