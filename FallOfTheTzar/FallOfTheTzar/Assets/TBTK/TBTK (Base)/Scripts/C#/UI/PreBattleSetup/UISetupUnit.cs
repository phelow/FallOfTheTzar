using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UISetupUnit : MonoBehaviour {

	private int selectedID=0;
	
	public List<UnitTB> availableUnits=new List<UnitTB>();
	public static List<UnitTB> selectedUnits=new List<UnitTB>();
	
	private enum _Tab{Available, Selected}
	private _Tab tab=_Tab.Available;
	
	private enum _InfoTab{General, Stats}
	private _InfoTab infoTab=_InfoTab.General;
	
	private Texture texBar;
	
	// Use this for initialization
	void Start () {
		texBar=Resources.Load("Textures/Bar", typeof(Texture)) as Texture;
		LoadUnitAbility();
	}
	
	[HideInInspector] public List<UnitAbility> unitAbilityList;
	public void LoadUnitAbility(){
		GameObject obj=Resources.Load("PrefabList/UnitAbilityListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null){
			Debug.Log("load unit ability fail, make sure the resource file exists");
			return;
		}
		UnitAbilityListPrefab prefab=obj.GetComponent<UnitAbilityListPrefab>();
		unitAbilityList=prefab.unitAbilityList;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	private Vector2 scrollPosition1;
	private Vector2 scrollPosition2;
	public void DrawUnitMenu(){
		GUIStyle style=new GUIStyle();
		style.fontSize=20;	style.fontStyle=FontStyle.Bold;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
		
		int winWidth=385;
		int winHeight=550;
		
		int startX=Screen.width/2-30;
		int startY=Screen.height/2-winHeight/2;
		
		int rowLimit=5;
		
		int row=0;		int column=0;
		for(int i=0; i<3; i++) GUI.Box(new Rect(startX, startY, winWidth, winHeight), "");
		
		
		
		GUI.Label(new Rect(startX, startY+10, winWidth, winHeight), "Unit Selection", style);
		
		style.fontSize=16;	style.alignment=TextAnchor.UpperLeft;
		GUI.Label(new Rect(startX+10, startY+45, winWidth, winHeight), "Available Units:", style);
		
		startX+=10;	startY+=65;
		
		int scrollLength=(int)(Mathf.Ceil(availableUnits.Count/rowLimit+1)*70);
		if(availableUnits.Count%rowLimit==0) scrollLength-=70;
		Rect viewRect=new Rect(startX, startY, winWidth-20, 280);
		Rect scrollRect=new Rect(startX, startY, winWidth-50, scrollLength);
		GUI.Box(viewRect, "");
		scrollPosition1 = GUI.BeginScrollView (viewRect, scrollPosition1, scrollRect);
			
			startX+=5;	startY+=5;
			
			for(int i=0; i<availableUnits.Count; i++){
				if(tab==_Tab.Available && selectedID==i){
					if(GUI.Button(new Rect(startX+column*70-3, startY+row*70-3, 66, 66), availableUnits[i].icon)){
						
					}
				}
				else{
					if(GUI.Button(new Rect(startX+column*70, startY+row*70, 60, 60), availableUnits[i].icon)){
						tab=_Tab.Available;
						selectedID=i;
					}
				}
				
				column+=1;
				if(column==5){
					row+=1; column=0;
				}
			}
		
		GUI.EndScrollView();
			
		startY+=280;
		
		if(tab==_Tab.Available){
			if(GUI.Button(new Rect(startX+280, startY, 80, 30), "Select", UI.buttonStyle)){
				UnitTB unit=availableUnits[selectedID];
				if(UISetup.playerPoint>=unit.pointCost){
					UISetup.UpdatePoints(-unit.pointCost);
					selectedUnits.Add(unit);
				}
			}
		}
		else if(tab==_Tab.Selected){
			if(GUI.Button(new Rect(startX+280, startY, 80, 30), "Remove", UI.buttonStyle)){
				UISetup.UpdatePoints(selectedUnits[selectedID].pointCost);
				selectedUnits.RemoveAt(selectedID);
				if(selectedUnits.Count>0){
					selectedID=Mathf.Max(selectedID-1, 0);
				}
				else{
					selectedID=0;
					tab=_Tab.Available;
				}
			}
		}
		
		style.fontSize=16;
		GUI.Label(new Rect(startX, startY+15, winWidth, winHeight), "Selected Units:", style);
		
		startX-=5;	startY+=35;
		
		scrollLength=(int)(Mathf.Ceil(selectedUnits.Count/rowLimit+1)*70);
		if(selectedUnits.Count%rowLimit==0) scrollLength-=70;
		viewRect=new Rect(startX, startY, winWidth-20, 140);
		scrollRect=new Rect(startX, startY, winWidth-50, scrollLength);
		GUI.Box(viewRect, "");
		scrollPosition2 = GUI.BeginScrollView (viewRect, scrollPosition2, scrollRect);
			
			row=0;		column=0;
		
			startX+=5;		startY+=5;
		
			for(int i=0; i<selectedUnits.Count; i++){
				if(tab==_Tab.Selected && selectedID==i){
					if(GUI.Button(new Rect(startX+column*70-3, startY+row*70-3, 66, 66), selectedUnits[i].icon)){
						
					}
				}
				else{
					if(GUI.Button(new Rect(startX+column*70, startY+row*70, 60, 60), selectedUnits[i].icon)){
						tab=_Tab.Selected;
						selectedID=i;
					}
				}
				
				column+=1;
				if(column==5){
					row+=1; column=0;
				}
			}
		
		GUI.EndScrollView();
			
		UnitTB selectedUnit=null;
		if(tab==_Tab.Available) selectedUnit=availableUnits[selectedID];
		else if(tab==_Tab.Selected) selectedUnit=selectedUnits[selectedID];
		DrawSelectedUnit(selectedUnit);
	}
	
	
	void DrawSelectedUnit(UnitTB unit){
		GUIStyle style=new GUIStyle();
		style.fontSize=20;	style.fontStyle=FontStyle.Bold;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
		
		int winWidth=300;
		int winHeight=550;
		
		int startX=Screen.width/2-350;
		int startY=Screen.height/2-winHeight/2;
		
		for(int i=0; i<3; i++) GUI.Box(new Rect(startX, startY, winWidth, winHeight), "");
		startX+=15;	winWidth-=30;
		
		GUI.Label(new Rect(startX, startY+10, winWidth, winHeight), unit.unitName, style);
		GUI.Label(new Rect(startX, startY+12, winWidth, winHeight), "_________________________________________________", style);
		
		startY+=40;
		
		GUI.DrawTexture(new Rect(startX+10, startY, 60, 60), texBar);
		GUI.DrawTexture(new Rect(startX+13, startY+3, 54, 54), unit.icon);
		style.alignment=TextAnchor.UpperLeft;	style.fontSize=16;
		GUI.Label(new Rect(startX+80, startY+10, winWidth-80, winHeight), "Hit Point:\nAction Point:", style);
		style.alignment=TextAnchor.UpperRight;
		GUI.Label(new Rect(startX+80, startY+10, winWidth-80, winHeight), unit.fullHP+"\n"+unit.fullAP, style);
		
		startY+=70;	
		
		if(infoTab==_InfoTab.General){
			style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;	style.normal.textColor=UI.colorN;
			GUI.Label(new Rect(startX, startY, winWidth, 100), unit.desp, style);
		}
		else{
			string offText="";
			string offVText="";
			if(unit.attackMode==_AttackMode.Hybrid){
				offText="Damage (melee/range):\nHit (melee/range):\nCritical (melee/range):\nAttack Range";

				offVText+=unit.GetMeleeDamageMin()+"-"+unit.GetMeleeDamageMax()+"/"+unit.GetRangeDamageMin()+"-"+unit.GetRangeDamageMax()+"\n";
				offVText+=(unit.GetMeleeAttack()*100).ToString("f0")+"/"+(unit.GetRangeAttack()*100).ToString("f0")+"%"+"\n";
				offVText+=(unit.GetMeleeCritical()*100).ToString("f0")+"/"+(unit.GetRangeCritical()*100).ToString("f0")+"%"+"\n";
				offVText+=unit.GetAttackRangeMax().ToString("f0")+"\n";
			}
			else{
				offText="Attack Damage:\nHit Chance:\nCritical Hit Chance:\nAttack Range:";
			}

			if(unit.attackMode==_AttackMode.Melee){
				offVText+=unit.GetMeleeDamageMin()+"-"+unit.GetMeleeDamageMax()+"\n";
				offVText+=(unit.GetMeleeAttack()*100).ToString("f0")+"%"+"\n";
				offVText+=(unit.GetMeleeCritical()*100).ToString("f0")+"%"+"\n";
				
				offVText+=unit.GetAttackRangeMax().ToString("f0")+"\n";
			}
			else if(unit.attackMode==_AttackMode.Range){
				offVText+=unit.GetRangeDamageMin()+"-"+unit.GetRangeDamageMax()+"\n";
				offVText+=(unit.GetRangeAttack()*100).ToString("f0")+"%"+"\n";
				offVText+=(unit.GetRangeCritical()*100).ToString("f0")+"%"+"\n";
				
				int dist=unit.GetAttackRangeMin();
				if(dist<=0) offVText+=unit.GetAttackRangeMax().ToString("f0")+"\n";
				else offVText+=unit.GetRangeDamageMin()+"-"+unit.GetRangeDamageMax()+"\n";
			}

			string defText="Armour:\nDodge Chance:\nCritical Immunity:";
			string miscText="Mobility:\nSpeed:\nAbilities:";
			
			string defVText="";
			defVText+=unit.GetDmgReduc().ToString("f0")+"\n";
			defVText+=(unit.GetDefend()*100).ToString("f0")+"%"+"\n";
			defVText+=(unit.GetCritDef()*100).ToString("f0")+"%"+"\n";
			
			string miscVText="";
			miscVText+=unit.GetMoveRange().ToString("f0")+"\n";
			miscVText+=unit.GetTurnPriority().ToString("f0")+"\n";
			
			style.alignment=TextAnchor.UpperLeft;	style.fontSize=16;	style.normal.textColor=UI.colorH;
			GUI.Label(new Rect(startX, startY, winWidth, winHeight), "Offense:", style);
			style.fontSize=12;	style.normal.textColor=UI.colorN;
			GUI.Label(new Rect(startX, startY+18, winWidth, winHeight), offText, style);
			style.alignment=TextAnchor.UpperRight;
			GUI.Label(new Rect(startX, startY+18, winWidth, winHeight), offVText, style);
			
			startY+=90;
			
			style.alignment=TextAnchor.UpperLeft;	style.fontSize=16;	style.normal.textColor=UI.colorH;
			GUI.Label(new Rect(startX, startY, winWidth, winHeight), "Defense:", style);
			style.fontSize=12;	style.normal.textColor=UI.colorN;
			GUI.Label(new Rect(startX, startY+18, winWidth, winHeight), defText, style);
			style.alignment=TextAnchor.UpperRight;
			GUI.Label(new Rect(startX, startY+18, winWidth, winHeight), defVText, style);
			
			startY+=75;
			
			style.alignment=TextAnchor.UpperLeft;	style.fontSize=16;	style.normal.textColor=UI.colorH;
			GUI.Label(new Rect(startX, startY, winWidth, winHeight), "Misc:", style);
			style.fontSize=12;	style.normal.textColor=UI.colorN;
			GUI.Label(new Rect(startX, startY+18, winWidth, winHeight), miscText, style);
			style.alignment=TextAnchor.UpperRight;
			GUI.Label(new Rect(startX, startY+18, winWidth, winHeight), miscVText, style);
			
			startY+=65;
			
			List<int> abilityIDList=unit.abilityIDList;
			for(int i=0; i<abilityIDList.Count; i++){
				foreach(UnitAbility uAB in unitAbilityList){
					if(uAB.ID==abilityIDList[i]){
						GUI.DrawTexture(new Rect(15+startX+i*35, startY, 31, 31), texBar);
						GUI.DrawTexture(new Rect(15+startX+i*35+2, startY+2, 27, 27), uAB.icon);
						GUIContent cont=new GUIContent("", uAB.ID.ToString());
						GUI.Label(new Rect(15+startX+i*35, startY+3, 25, 25), cont);
					}
				}
			}
			
			startY+=40;
			
			if(GUI.tooltip!=""){
				int ID=int.Parse(GUI.tooltip);
				foreach(UnitAbility uAB in unitAbilityList){
					if(uAB.ID==ID){
						style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
						GUI.Label(new Rect(startX+3, startY+3, winWidth, 50), uAB.desp.ToString());
					}
				}
			}
		}
		
		string buttonText="Less Details";
		if(infoTab==_InfoTab.General) buttonText="More Details";
		bool switchFlag=GUI.Button(new Rect(startX+winWidth/2-60, Screen.height/2+winHeight/2-40, 120, 30), buttonText, UI.buttonStyle);
		
		if(switchFlag){
			if(infoTab==_InfoTab.General) infoTab=_InfoTab.Stats;
			else infoTab=_InfoTab.General;
		}
		
	}
	
}
