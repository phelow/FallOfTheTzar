using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


public class DamageArmorTableEditor : EditorWindow {

	public delegate void UpdateHandler(); 
	public static event UpdateHandler onDamageArmorTableUpdateE;
	
	static private DamageArmorTableEditor window;
	
	static DamageArmorListPrefab prefab;
	static List<ArmorType> armorList=new List<ArmorType>();
	static List<DamageType> damageList=new List<DamageType>();
	
    // Add menu named "TowerEditor" to the Window menu
    //[MenuItem ("TDTK/DamageArmorTable")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (DamageArmorTableEditor)EditorWindow.GetWindow(typeof (DamageArmorTableEditor));
		//~ window.minSize=new Vector2(340, 170);
		window.minSize=new Vector2(470, 300);
		//~ window.maxSize=new Vector2(471, 301);
		
		
		LoadPrefab();
    }
	
	private static void LoadPrefab(){
		GameObject obj=Resources.Load("PrefabList/DamageArmorList", typeof(GameObject)) as GameObject;
		
		if(obj==null) obj=CreatePrefab();
		
		prefab=obj.GetComponent<DamageArmorListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<DamageArmorListPrefab>();
		
		armorList=prefab.armorList;
		damageList=prefab.damageList;
		
		if(armorList==null) armorList=new List<ArmorType>();
		if(damageList==null) damageList=new List<DamageType>();
	}
	
	private static GameObject CreatePrefab(){
		GameObject obj=new GameObject();
		obj.AddComponent<DamageArmorListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/PrefabList/DamageArmorList.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
		DestroyImmediate(obj);
		AssetDatabase.Refresh ();
		return prefab;
	}
	
	Vector2 scrollPos;

	int selectedIDDamage=-1;
	int selectedIDArmor=-1;
	bool delete=false;

	void OnGUI(){
		#if !UNITY_4_3
			Undo.SetSnapshotTarget(this, "DamageArmorTableEditor");
		#else
			Undo.RecordObject(this, "DamageArmorTableEditor");
		#endif
		
		int startX=0;
		int startY=0;
		
		if(GUI.Button(new Rect(10, 10, 100, 30), "New Armor")){
			armorList.Add(new ArmorType());
			if(onDamageArmorTableUpdateE!=null) onDamageArmorTableUpdateE();
		}
		if(GUI.Button(new Rect(120, 10, 100, 30), "New Damage")){
			damageList.Add(new DamageType());
			if(onDamageArmorTableUpdateE!=null) onDamageArmorTableUpdateE();
		}
		
		
		Rect visibleRect=new Rect(10, 50, window.position.width-20, 185);
		Rect contentRect=new Rect(10, 50, 118+damageList.Count*105, 5+(armorList.Count+1)*25);
		
		GUI.Box(visibleRect, "");
		scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
		
		startY=60;
		startX=20;
		for(int i=0; i<damageList.Count; i++){
			DamageType dmgType=damageList[i];
			if(selectedIDDamage==i) GUI.color=Color.green;
			GUI.SetNextControlName ("dmg"+i);
			if(GUI.Button(new Rect(startX+=105, startY, 100, 20), dmgType.name)){
				//~ damageList.RemoveAt(i);
				selectedIDDamage=i;
				selectedIDArmor=-1;
				delete=false;
				GUI.FocusControl ("dmg"+i);
			}
			GUI.color=Color.white;
		}
		
		
		
		startY=60;
		for(int i=0; i<armorList.Count; i++){
			startX=20;
			
			ArmorType armorType=armorList[i];
			if(selectedIDArmor==i) GUI.color=Color.green;
			GUI.SetNextControlName ("armor"+i);
			if(GUI.Button(new Rect(startX, startY+=25, 100, 20), armorType.name)){
				//~ armorList.RemoveAt(i);
				selectedIDDamage=-1;
				selectedIDArmor=i;
				delete=false;
				GUI.FocusControl ("armor"+i);
			}
			GUI.color=Color.white;
			
			if(armorType.modifiers.Count!=damageList.Count){
				List<float> temp=new List<float>();
				for(int n=0; n<damageList.Count; n++){
					if(n<armorType.modifiers.Count){
						temp.Add(armorType.modifiers[n]);
					}
					else{
						temp.Add(1);
					}
				}
				armorType.modifiers=temp;
			}
			
			startX+=110;
			for(int j=0; j<damageList.Count; j++){
				armorType.modifiers[j]=EditorGUI.FloatField(new Rect(startX, startY, 90, 20), armorType.modifiers[j]);
				startX+=105;
			}
		}
		
		
		
		GUI.EndScrollView();
		
		startX=10;
		startY=250;
		if(selectedIDDamage>=0){
			DamageType dmgType=damageList[selectedIDDamage];
			GUI.Label(new Rect(startX, startY, 200, 17), "Name:");
			dmgType.name=EditorGUI.TextField(new Rect(startX+80, startY, 150, 17), dmgType.name);
			
			EditorGUI.LabelField(new Rect(startX, startY+=25, 150, 17), "Description: ");
			dmgType.desp=EditorGUI.TextArea(new Rect(startX, startY+=17, window.position.width-20, 50), dmgType.desp);
			
			string label="";
			for(int i=0; i<armorList.Count; i++){
				label+=" - cause "+(armorList[i].modifiers[selectedIDDamage]*100).ToString("f0")+"% damage to "+armorList[i].name+"\n";
			}
			GUI.Label(new Rect(startX, startY+=60, window.position.width-20, 100), label);
			
			startX=300;
			startY=250;
			if(!delete){
				if(GUI.Button(new Rect(startX, startY, 80, 20), "delete")) delete=true;
			}
			else if(delete){
				if(GUI.Button(new Rect(startX, startY, 80, 20), "cancel")) delete=false;
				GUI.color=Color.red;
				if(GUI.Button(new Rect(startX+=90, startY, 80, 20), "confirm")){
					damageList.RemoveAt(selectedIDDamage);
					selectedIDDamage=-1;
				}
				GUI.color=Color.white;
			}
		}
		else if(selectedIDArmor>=0){
			ArmorType armorType=armorList[selectedIDArmor];
			GUI.Label(new Rect(startX, startY, 200, 17), "Name:");
			armorType.name=EditorGUI.TextField(new Rect(startX+80, startY, 150, 17), armorType.name);
			
			EditorGUI.LabelField(new Rect(startX, startY+=25, 150, 17), "Description: ");
			armorType.desp=EditorGUI.TextArea(new Rect(startX, startY+=17, window.position.width-20, 50), armorType.desp);
			
			string label="";
			for(int i=0; i<damageList.Count; i++){
				label+=" - take "+(armorType.modifiers[i]*100).ToString("f0")+"% damage from "+damageList[i].name+"\n";
			}
			GUI.Label(new Rect(startX, startY+=60, window.position.width-20, 100), label);
			
			startX=300;
			startY=250;
			if(!delete){
				if(GUI.Button(new Rect(startX, startY, 80, 20), "delete")) delete=true;
			}
			else if(delete){
				if(GUI.Button(new Rect(startX, startY, 80, 20), "cancel")) delete=false;
				GUI.color=Color.red;
				if(GUI.Button(new Rect(startX+=90, startY, 80, 20), "confirm")){
					armorList.RemoveAt(selectedIDArmor);
					selectedIDArmor=-1;
				}
				GUI.color=Color.white;
			}
			
			
		}
		
		if(GUI.changed){
			if(onDamageArmorTableUpdateE!=null) onDamageArmorTableUpdateE();
			EditorUtility.SetDirty(prefab);
		}
		
		#if UNITY_LE_4_3
		if (GUI.changed){
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}
		Undo.ClearSnapshotTarget();
		#endif
	}

}

