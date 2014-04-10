using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(UnitControl))]
public class UnitControlEditor : Editor {
	
	UnitControl uc;
	GameControlTB gc;
	
	List<UnitTB> unitList=new List<UnitTB>();
	string[] unitNameList=new string[0];
	int[] intVal=new int[0];
	
	void Awake(){
		LoadUnit();
		
		uc=(UnitControl)target;
		gc=uc.gameObject.GetComponent<GameControlTB>();
		if(gc!=null) InitPlayerUnitsList();
	}
	
	void LoadUnit(){
		EditorUnitList eUnitList=UnitTBManagerWindow.LoadUnit();
		unitList=eUnitList.prefab.unitList;
		unitNameList=eUnitList.nameList;
		intVal=new int[unitList.Count];
		for(int i=0; i<intVal.Length; i++){
			intVal[i]=i;
		}
	}
	
	
	
	GUIContent cont;
	GUIContent[] contL;
	
	//bool showStartingUnitList=false;
	
	
	void InitPlayerUnitsList(){
		if(uc.playerUnits==null || uc.playerUnits.Count!=gc.playerFactionID.Count){
			List<PlayerUnits> newList=new List<PlayerUnits>();
			for(int i=0; i<gc.playerFactionID.Count; i++){
				bool match=false;
				for(int j=0; j<uc.playerUnits.Count; j++){
					if(gc.playerFactionID[i]==uc.playerUnits[j].factionID){
						newList.Add(uc.playerUnits[j]);
						match=true;
						break;
					}
				}
				if(!match){
					newList.Add(new PlayerUnits(gc.playerFactionID[i]));
				}
			}
			uc.playerUnits=newList;
		}
		
		if(uc.playerUnits.Count==0) uc.playerUnits.Add(new PlayerUnits(0));
	}
	
	
	public override void OnInspectorGUI(){
		//uc = (UnitControl)target;
		if(gc==null){
			gc=uc.gameObject.GetComponent<GameControlTB>();
			if(gc==null){
				Debug.Log("No GameControlTB component on UnitControl Object");
				return;
			}
			else{
				InitPlayerUnitsList();
			}
		}
		
		DrawDefaultInspector();
		
		
		
		EditorGUILayout.Space();
		
		EditorGUILayout.LabelField("Starting Units");
		
		for(int n=0; n<uc.playerUnits.Count; n++){
			PlayerUnits pUnits=uc.playerUnits[n];
			
			int num=pUnits.starting.Count;
		
			//~ EditorGUILayout.BeginHorizontal();
				string label="FactionID:"+pUnits.factionID.ToString()+" (show unit)";
				if(pUnits.showInInspector) label="FactionID:"+pUnits.factionID.ToString()+" (hide unit)";
				cont=new GUIContent(label, "The player faction's unit to be deployed at the start of the scene");
				pUnits.showInInspector = EditorGUILayout.Foldout(pUnits.showInInspector, cont);
			//~ EditorGUILayout.EndHorizontal();
			
			if(pUnits.showInInspector){
				num=EditorGUILayout.IntField("  Number of units:", num, GUILayout.MaxHeight(14));
				if(num!=pUnits.starting.Count) pUnits.starting=MatchStartingUnitListLength(pUnits.starting, num);
				for(int i=0; i<num; i++){
					int unitID=-1;
					for(int j=0; j<unitList.Count; j++){
						if(pUnits.starting[i]!=null){
							if(unitList[j].prefabID==pUnits.starting[i].prefabID){
								unitID=j;
								break;
							}
						}
					}
					unitID = EditorGUILayout.IntPopup("     - unit"+i+": ", unitID, unitNameList, intVal,  GUILayout.MaxHeight(13));
					
					if(!Application.isPlaying){
						if(unitID>=0) pUnits.starting[i]=unitList[unitID];
						else pUnits.starting[i]=null;
					}
				}
			}
			EditorGUILayout.Space();
		}
		
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("Faction Colors:", "optional colors assignment to faction in runtime as displayed on the overlay. First color assigned to the first faction and so on. If left unassigned, a random color will be used instead");
		EditorGUILayout.LabelField(cont, GUILayout.MaxWidth(120));
		if(GUILayout.Button("+", GUILayout.MaxWidth(50))){
			uc.factionColors.Add(Color.white);
		}
		if(GUILayout.Button("-", GUILayout.MaxWidth(50))){
			if(uc.factionColors.Count>0){
				uc.factionColors.RemoveAt(uc.factionColors.Count-1);
			}
		}
		EditorGUILayout.EndHorizontal();
		for(int i=0; i<uc.factionColors.Count; i++){
			uc.factionColors[i]=EditorGUILayout.ColorField("Faction"+i, uc.factionColors[i]);
		}
		
		
		EditorGUILayout.Space();
		
		/*
		//old code before hot seat mode, obsoleted
		int numm=uc.startingUnit.Count;
		
		string labell="Starting Units (Show)";
		if(showStartingUnitList) labell="Starting Units (Hide)";
		cont=new GUIContent(labell, "The player unit ready to be deployed at the start of the scene");
		showStartingUnitList = EditorGUILayout.Foldout(showStartingUnitList, cont);
		if(showStartingUnitList){
			numm=EditorGUILayout.IntField("  Number of units:", numm, GUILayout.MaxHeight(14));
			if(numm!=uc.startingUnit.Count) uc.startingUnit=MatchStartingUnitListLength(uc.startingUnit, numm);
			for(int i=0; i<numm; i++){
				int unitID=-1;
				for(int j=0; j<unitList.Count; j++){
					if(uc.startingUnit[i]!=null){
						if(unitList[j].prefabID==uc.startingUnit[i].prefabID){
							unitID=j;
							break;
						}
					}
				}
				unitID = EditorGUILayout.IntPopup("     - unit"+i+": ", unitID, unitNameList, intVal,  GUILayout.MaxHeight(13));
				if(unitID>=0) uc.startingUnit[i]=unitList[unitID];
				else uc.startingUnit[i]=null;
			}
		}
		*/
		
		EditorGUILayout.Space();
	}
	
	
	
	
	List<UnitTB> MatchStartingUnitListLength(List<UnitTB> oldList, int count){
		List<UnitTB> newList=new List<UnitTB>();
		for(int i=0; i<count; i++){
			if(i<oldList.Count) newList.Add(oldList[i]);
			else newList.Add(null);
		}
		return newList;
	}
}