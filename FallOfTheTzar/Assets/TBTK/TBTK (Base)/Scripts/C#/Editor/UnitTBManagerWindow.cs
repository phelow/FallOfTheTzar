using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;




public class EditorUnitList{
	public UnitListPrefab prefab;
	public string[] nameList=new string[0];
}

public class UnitTBManagerWindow : EditorWindow {

	static private UnitTBManagerWindow window;
	
	private static UnitListPrefab prefab;
	private static List<UnitTB> unitList=new List<UnitTB>();
	private static List<int> unitIDList=new List<int>();
	
	private UnitTB newUnit=null;
	
	public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (UnitTBManagerWindow)EditorWindow.GetWindow(typeof (UnitTBManagerWindow));
		window.minSize=new Vector2(375, 449);
		window.maxSize=new Vector2(375, 800);
		
		
		EditorUnitList eUnitList=LoadUnit();
		prefab=eUnitList.prefab;
		unitList=prefab.unitList;
		
		foreach(UnitTB unit in unitList){
			unitIDList.Add(unit.prefabID);
		}
    }
	
	
	public static EditorUnitList LoadUnit(){
		GameObject obj=Resources.Load("PrefabList/UnitListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null) obj=CreatePrefab();
		
		UnitListPrefab prefab=obj.GetComponent<UnitListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<UnitListPrefab>();
		
		for(int i=0; i<prefab.unitList.Count; i++){
			if(prefab.unitList[i]==null){
				prefab.unitList.RemoveAt(i);
				i-=1;
			}
		}
		
		string[] nameList=new string[prefab.unitList.Count];
		for(int i=0; i<prefab.unitList.Count; i++){
			if(prefab.unitList[i]!=null){
				nameList[i]=prefab.unitList[i].unitName;
				//prefab.unitList[i].prefabID=i;
				unitIDList.Add(prefab.unitList[i].prefabID);
			}
		}
		
		EditorUnitList eUnitList=new EditorUnitList();
		eUnitList.prefab=prefab;
		eUnitList.nameList=nameList;
		
		return eUnitList;
	}
	
	private static GameObject CreatePrefab(){
		GameObject obj=new GameObject();
		obj.AddComponent<UnitListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/PrefabList/UnitListPrefab.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
		DestroyImmediate(obj);
		AssetDatabase.Refresh ();
		return prefab;
	}
	
	
	
	
	int delete=-1;
	private Vector2 scrollPos;
	
	void OnGUI () {
		if(window==null) Init();
		
		Undo.SetSnapshotTarget(this, "UnitManagerWindow");
		
		int currentUnitCount=unitList.Count;
		
		if(GUI.Button(new Rect(window.position.width-110, 5, 100, 25), "UnitEditor")){
			this.Close();
			UnitTBEditorWindow.Init();
		}
		
		EditorGUI.LabelField(new Rect(5, 10, 150, 17), "Add new unit:");
		newUnit=(UnitTB)EditorGUI.ObjectField(new Rect(100, 10, 150, 17), newUnit, typeof(UnitTB), false);
		if(newUnit!=null){
			if(!unitList.Contains(newUnit)){
				int rand=0;
				while(unitIDList.Contains(rand)) rand+=1;
				unitIDList.Add(rand);
				newUnit.prefabID=rand;
				
				newUnit.unitName=newUnit.gameObject.name;
				unitList.Add(newUnit);
				
				GUI.changed=true;
			}
			newUnit=null;
		}

		
		if(unitList.Count>0){
			GUI.Box(new Rect(5, 40, 50, 20), "ID");
			GUI.Box(new Rect(5+50-1, 40, 60+1, 20), "Icon");
			GUI.Box(new Rect(5+110-1, 40, 160+2, 20), "Name");
			GUI.Box(new Rect(5+270, 40, window.position.width-300, 20), "");
		}
		
		scrollPos = GUI.BeginScrollView(new Rect(5, 60, window.position.width-12, window.position.height-50), scrollPos, new Rect(5, 55, window.position.width-30, 15+((unitList.Count))*50));
		
		int row=0;
		for(int i=0; i<unitList.Count; i++){
			if(i%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
			else GUI.color=Color.white;
			GUI.Box(new Rect(5, 60+i*49, window.position.width-30, 50), "");
			GUI.color=Color.white;
			
			if(currentSwapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			if(GUI.Button(new Rect(19, 12+60+i*49, 30, 30), unitList[i].prefabID.ToString())){
				if(currentSwapID==i) currentSwapID=-1;
				else if(currentSwapID==-1) currentSwapID=i;
				else{
					SwapCreep(i);
					GUI.changed=true;
				}
			}
			if(currentSwapID==i) GUI.color=Color.white;
			
			if(unitList[i]!=null){
				unitList[i].icon=(Texture)EditorGUI.ObjectField(new Rect(12+50, 3+60+i*49, 44, 44), unitList[i].icon, typeof(Texture), false);
				unitList[i].unitName=EditorGUI.TextField(new Rect(5+120, 6+60+i*49, 150, 17), unitList[i].unitName);
				
				if(unitList[i].icon!=null && unitList[i].icon.name!=unitList[i].iconName){
					unitList[i].iconName=unitList[i].icon.name;
					GUI.changed=true;
				}
				
				EditorGUI.LabelField(new Rect(5+120, 6+60+i*49+20, 150, 17), "Prefab:");
				EditorGUI.ObjectField(new Rect(5+165, 6+60+i*49+20, 105, 17), unitList[i].gameObject, typeof(GameObject), false);
			}
			
			if(delete!=i){
				if(GUI.Button(new Rect(window.position.width-55, 12+60+i*49, 25, 25), "X")){
					delete=i;
				}
			}
			else{
				GUI.color = Color.red;
				if(GUI.Button(new Rect(window.position.width-90, 12+60+i*49, 60, 25), "Remove")){
					if(currentSwapID==i) currentSwapID=-1;
					unitIDList.Remove(unitList[i].prefabID);
					unitList.RemoveAt(i);
					delete=-1;
					//~ if(onCreepUpdateE!=null) onCreepUpdateE();
					GUI.changed=true;
				}
				GUI.color = Color.white;
			}
			
			row+=1;
		}
		
		
		GUI.EndScrollView();
		
		if(GUI.changed || currentUnitCount!=unitList.Count){
			EditorUtility.SetDirty(prefab);
			for(int i=0; i<unitList.Count; i++) EditorUtility.SetDirty(unitList[i]);
		}
		
		if (GUI.changed || currentUnitCount!=unitList.Count){
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}
		Undo.ClearSnapshotTarget();
		
	}
	
	
	
	
	private int currentSwapID=-1;
	void SwapCreep(int ID){
		UnitTB unit=unitList[currentSwapID];
		unitList[currentSwapID]=unitList[ID];
		unitList[ID]=unit;
		
		currentSwapID=-1;
	}
	
}
