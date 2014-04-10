using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;




public class EditorCollectibleList{
	public CollectibleListPrefab prefab;
	public string[] nameList=new string[0];
}

public class CollectibleManagerWindow : EditorWindow {

	static private CollectibleManagerWindow window;
	
	private static CollectibleListPrefab prefab;
	private static List<CollectibleTB> collectibleList=new List<CollectibleTB>();
	private static List<int> collectibleIDList=new List<int>();
	
	private CollectibleTB newCollectible=null;
	
	public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (CollectibleManagerWindow)EditorWindow.GetWindow(typeof (CollectibleManagerWindow));
		window.minSize=new Vector2(375, 449);
		window.maxSize=new Vector2(375, 800);
		
		EditorCollectibleList eCollectibleList=LoadCollectible();
		prefab=eCollectibleList.prefab;
		collectibleList=prefab.collectibleList;
		
		foreach(CollectibleTB collectible in collectibleList){
			collectibleIDList.Add(collectible.prefabID);
		}
    }
	
	
	public static EditorCollectibleList LoadCollectible(){
		GameObject obj=Resources.Load("PrefabList/CollectibleListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null) obj=CreatePrefab();
		
		CollectibleListPrefab prefab=obj.GetComponent<CollectibleListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<CollectibleListPrefab>();
		
		for(int i=0; i<prefab.collectibleList.Count; i++){
			if(prefab.collectibleList[i]==null){
				prefab.collectibleList.RemoveAt(i);
				i-=1;
			}
		}
		
		string[] nameList=new string[prefab.collectibleList.Count];
		for(int i=0; i<prefab.collectibleList.Count; i++){
			if(prefab.collectibleList[i]!=null){
				nameList[i]=prefab.collectibleList[i].collectibleName;
			}
		}
		
		EditorCollectibleList eCollectibleList=new EditorCollectibleList();
		eCollectibleList.prefab=prefab;
		eCollectibleList.nameList=nameList;
		
		return eCollectibleList;
	}
	
	private static GameObject CreatePrefab(){
		GameObject obj=new GameObject();
		obj.AddComponent<CollectibleListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/PrefabList/CollectibleListPrefab.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
		DestroyImmediate(obj);
		AssetDatabase.Refresh ();
		return prefab;
	}
	
	
	
	
	int delete=-1;
	private Vector2 scrollPos;
	
	void OnGUI () {
		if(window==null) Init();
		
		#if !UNITY_4_3
			Undo.SetSnapshotTarget(this, "UnitManagerWindow");
		#else
			Undo.RecordObject(this, "UnitManagerWindow");
		#endif
		
		int currentItemCount=collectibleList.Count;
		
		if(GUI.Button(new Rect(window.position.width-120, 5, 110, 25), "CollectibleEditor")){
			this.Close();
			CollectibleEditorWindow.Init();
		}
		
		EditorGUI.LabelField(new Rect(5, 10, 150, 17), "Add new unit:");
		newCollectible=(CollectibleTB)EditorGUI.ObjectField(new Rect(100, 10, 150, 17), newCollectible, typeof(CollectibleTB), false);
		if(newCollectible!=null){
			if(!collectibleList.Contains(newCollectible)){
				int rand=0;
				while(collectibleIDList.Contains(rand)) rand+=1;
				collectibleIDList.Add(rand);
				newCollectible.prefabID=rand;
				
				collectibleList.Add(newCollectible);
				
				GUI.changed=true;
			}
			newCollectible=null;
		}

		
		if(collectibleList.Count>0){
			GUI.Box(new Rect(5, 40, 50, 20), "ID");
			GUI.Box(new Rect(5+50-1, 40, 60+1, 20), "Icon");
			GUI.Box(new Rect(5+110-1, 40, 160+2, 20), "Name");
			GUI.Box(new Rect(5+270, 40, window.position.width-300, 20), "");
		}
		
		scrollPos = GUI.BeginScrollView(new Rect(5, 60, window.position.width-12, window.position.height-50), scrollPos, new Rect(5, 55, window.position.width-30, 15+((collectibleList.Count))*50));
		
		int row=0;
		for(int i=0; i<collectibleList.Count; i++){
			if(i%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
			else GUI.color=Color.white;
			GUI.Box(new Rect(5, 60+i*49, window.position.width-30, 50), "");
			GUI.color=Color.white;
			
			if(currentSwapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			if(GUI.Button(new Rect(19, 12+60+i*49, 30, 30), collectibleList[i].prefabID.ToString())){
				if(currentSwapID==i) currentSwapID=-1;
				else if(currentSwapID==-1) currentSwapID=i;
				else{
					SwapCollectible(i);
					GUI.changed=true;
				}
			}
			if(currentSwapID==i) GUI.color=Color.white;
			
			if(collectibleList[i]!=null){
				collectibleList[i].icon=(Texture)EditorGUI.ObjectField(new Rect(12+50, 3+60+i*49, 44, 44), collectibleList[i].icon, typeof(Texture), false);
				collectibleList[i].collectibleName=EditorGUI.TextField(new Rect(5+120, 6+60+i*49, 150, 17), collectibleList[i].collectibleName);
				
				EditorGUI.LabelField(new Rect(5+120, 6+60+i*49+20, 150, 17), "Prefab:");
				EditorGUI.ObjectField(new Rect(5+165, 6+60+i*49+20, 105, 17), collectibleList[i].gameObject, typeof(GameObject), false);
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
					collectibleIDList.Remove(collectibleList[i].prefabID);
					collectibleList.RemoveAt(i);
					delete=-1;
					//~ if(onCreepUpdateE!=null) onCreepUpdateE();
					GUI.changed=true;
				}
				GUI.color = Color.white;
			}
			
			row+=1;
		}
		
		
		GUI.EndScrollView();
		
		if(GUI.changed || currentItemCount!=collectibleList.Count){
			EditorUtility.SetDirty(prefab);
			for(int i=0; i<collectibleList.Count; i++) EditorUtility.SetDirty(collectibleList[i]);
		}
		
		#if UNITY_LE_4_3
		if (GUI.changed || currentItemCount!=collectibleList.Count){
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}
		Undo.ClearSnapshotTarget();
		#endif
	}
	
	
	
	
	private int currentSwapID=-1;
	void SwapCollectible(int ID){
		CollectibleTB collectible=collectibleList[currentSwapID];
		collectibleList[currentSwapID]=collectibleList[ID];
		collectibleList[ID]=collectible;
		
		currentSwapID=-1;
	}
	
}
