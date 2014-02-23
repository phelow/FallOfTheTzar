using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;



public class ObstacleManagerWindow : EditorWindow {

	static private ObstacleManagerWindow window;
	
	//public delegate void ObstacleUpdateHandler(); 
	//public static event ObstacleUpdateHandler onUpdateE;
	
	private static ObstacleListPrefab prefab;
	private static List<Obstacle> obstacleList=new List<Obstacle>();
	private static List<int> obstacleIDList=new List<int>();
	
	private Obstacle newObstacle=null;
	
	private static int[] intVal;
	private static string[] coverTypeLabel;
	private static string[] tileTypeLabel;
	private static string[] obstacleTypeLabel;
	
	public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (ObstacleManagerWindow)EditorWindow.GetWindow(typeof (ObstacleManagerWindow));
		window.minSize=new Vector2(375, 449);
		window.maxSize=new Vector2(375, 800);
		
		prefab=LoadObstacle();
		obstacleList=prefab.obstacleList;
		
		foreach(Obstacle obs in obstacleList){
			obstacleIDList.Add(obs.prefabID);
		}
		
		intVal=new int[3];
		for(int i=0; i<intVal.Length; i++) intVal[i]=i;
		
		coverTypeLabel=new string[3];
		coverTypeLabel[0]="None";
		coverTypeLabel[1]="Half";
		coverTypeLabel[2]="Full";
		
		tileTypeLabel=new string[3];
		tileTypeLabel[0]="Hex";
		tileTypeLabel[1]="Square";
		tileTypeLabel[2]="Universal";
		
		obstacleTypeLabel=new string[2];
		obstacleTypeLabel[0]="Obstacle";
		obstacleTypeLabel[1]="Wall";
	}
	
	
	public static ObstacleListPrefab LoadObstacle(){
		GameObject obj=Resources.Load("PrefabList/ObstacleListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null) obj=CreatePrefab();
		
		ObstacleListPrefab prefab=obj.GetComponent<ObstacleListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<ObstacleListPrefab>();
		
		for(int i=0; i<prefab.obstacleList.Count; i++){
			if(prefab.obstacleList[i]==null){
				prefab.obstacleList.RemoveAt(i);
				i-=1;
			}
		}
		
		/*
		string[] nameList=new string[prefab.obstacleList.Count];
		for(int i=0; i<prefab.obstacleList.Count; i++){
			if(prefab.unitList[i]!=null){
				//nameList[i]=prefab.unitList[i].unitName;
				//prefab.unitList[i].prefabID=i;
				unitIDList.Add(prefab.unitList[i].prefabID);
			}
		}
		*/
		
		//EditorUnitList eUnitList=new EditorUnitList();
		//eUnitList.prefab=prefab;
		//eUnitList.nameList=nameList;
		
		return prefab;
	}
	
	private static GameObject CreatePrefab(){
		GameObject obj=new GameObject();
		obj.AddComponent<ObstacleListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/PrefabList/ObstacleListPrefab.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
		DestroyImmediate(obj);
		AssetDatabase.Refresh ();
		return prefab;
	}
	
	
	
	
	int delete=-1;
	private Vector2 scrollPos;
	
	void OnGUI () {
		if(window==null) Init();
		
		#if !UNITY_4_3
			Undo.SetSnapshotTarget(this, "ObstacleManagerWindow");
		#else
			Undo.RecordObject(this, "ObstacleManagerWindow");
		#endif
		
		int currentObstacleCount=obstacleList.Count;
		
		EditorGUI.LabelField(new Rect(5, 10, 150, 17), "Add obstacle:");
		newObstacle=(Obstacle)EditorGUI.ObjectField(new Rect(100, 10, 150, 17), newObstacle, typeof(Obstacle), false);
		if(newObstacle!=null){
			if(!obstacleList.Contains(newObstacle)){
				int rand=0;
				while(obstacleIDList.Contains(rand)) rand+=1;
				obstacleIDList.Add(rand);
				newObstacle.prefabID=rand;
				
				obstacleList.Add(newObstacle);
				
				GUI.changed=true;
			}
			newObstacle=null;
		}
		
		//if(obstacleList.Count>0) Texture tex=EditorUtility.GetAssetPreview(obstacleList[0].gameObject);
		//EditorGUI.ObjectField(new Rect(12+50, 3+60+i*49, 44, 44), tex, typeof(Texture), false);

		
		if(obstacleList.Count>0){
			GUI.Box(new Rect(5, 40, 50, 20), "ID");
			//GUI.Box(new Rect(5+50-1, 40, 60+1, 20), "Icon");
			GUI.Box(new Rect(5+50-1, 40, 220+2, 20), "Info");
			GUI.Box(new Rect(5+270, 40, window.position.width-300, 20), "");
		}
		
		scrollPos = GUI.BeginScrollView(new Rect(5, 60, window.position.width-12, window.position.height-50), scrollPos, new Rect(5, 55, window.position.width-30, 15+((obstacleList.Count))*110));
		
		int row=0;
		for(int i=0; i<obstacleList.Count; i++){
			if(i%2==0) GUI.color=new Color(.8f, .8f, .8f, 1);
			else GUI.color=Color.white;
			GUI.Box(new Rect(5, 60+i*109, window.position.width-30, 110), "");
			GUI.color=Color.white;
			
			if(currentSwapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			if(GUI.Button(new Rect(19, 12+60+i*109, 30, 30), obstacleList[i].prefabID.ToString())){
				if(currentSwapID==i) currentSwapID=-1;
				else if(currentSwapID==-1) currentSwapID=i;
				else{
					SwapCreep(i);
					GUI.changed=true;
				}
			}
			if(currentSwapID==i) GUI.color=Color.white;
			
			if(obstacleList[i]!=null){
				//unitList[i].icon=(Texture)EditorGUI.ObjectField(new Rect(12+50, 3+60+i*49, 44, 44), unitList[i].icon, typeof(Texture), false);
				EditorGUI.LabelField(new Rect(5+60, 6+60+i*109, 75, 17), "Name:");
				obstacleList[i].obsName=EditorGUI.TextField(new Rect(5+120, 6+60+i*109, 140, 17), obstacleList[i].obsName);
				
				
				//cont=new GUIContent("Type:", "type of unwalkable tile to be applied");
				//contL=new GUIContent[unwalkableLabel.Length];
				//for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(unwalkableLabel[i], unwalkableTooltip[i]);
				//unwalkableType = EditorGUILayout.IntPopup(cont, unwalkableType, contL, intVal);
				
				EditorGUI.LabelField(new Rect(5+60, 6+60+i*109+20, 120, 17), "Type:");
				int obsType=(int)obstacleList[i].obsType;
				obsType=EditorGUI.IntPopup(new Rect(5+120, 6+60+i*109+20, 140, 17), obsType, obstacleTypeLabel, intVal);
				obstacleList[i].obsType=(_ObsType)obsType;
				
				EditorGUI.LabelField(new Rect(5+60, 6+60+i*109+40, 120, 17), "Cover:");
				int coverType=(int)obstacleList[i].coverType;
				coverType=EditorGUI.IntPopup(new Rect(5+120, 6+60+i*109+40, 140, 17), coverType, coverTypeLabel, intVal);
				obstacleList[i].coverType=(_CoverType)coverType;
				
				EditorGUI.LabelField(new Rect(5+60, 6+60+i*109+60, 120, 17), "TileType:");
				int tileType=(int)obstacleList[i].tileType;
				tileType=EditorGUI.IntPopup(new Rect(5+120, 6+60+i*109+60, 140, 17), tileType, tileTypeLabel, intVal);
				obstacleList[i].tileType=(_ObsTileType)tileType;
				
				EditorGUI.LabelField(new Rect(5+60, 6+60+i*109+80, 75, 17), "Prefab:");
				EditorGUI.ObjectField(new Rect(5+120, 6+60+i*109+80, 140, 17), obstacleList[i].gameObject, typeof(GameObject), false);
				
				
				//EditorGUI.LabelField(new Rect(5+120, 6+60+i*49+20, 150, 17), "Prefab:");
				//EditorGUI.ObjectField(new Rect(5+165, 6+60+i*49+20, 105, 17), obstacleList[i].gameObject, typeof(GameObject), false);
			}
			
			if(delete!=i){
				if(GUI.Button(new Rect(window.position.width-55, 12+60+i*109, 25, 25), "X")){
					delete=i;
				}
			}
			else{
				GUI.color = Color.red;
				if(GUI.Button(new Rect(window.position.width-90, 12+60+i*109, 60, 25), "Remove")){
					if(currentSwapID==i) currentSwapID=-1;
					obstacleIDList.Remove(obstacleList[i].prefabID);
					obstacleList.RemoveAt(i);
					delete=-1;
					//~ if(onCreepUpdateE!=null) onCreepUpdateE();
					GUI.changed=true;
				}
				GUI.color = Color.white;
			}
			
			row+=1;
		}
		
		
		
		GUI.EndScrollView();
		
		if(GUI.changed || currentObstacleCount!=obstacleList.Count){
			EditorUtility.SetDirty(prefab);
			for(int i=0; i<obstacleList.Count; i++) EditorUtility.SetDirty(obstacleList[i]);
		}
		
		#if UNITY_LE_4_3
		if (GUI.changed || currentObstacleCount!=obstacleList.Count){
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}
		Undo.ClearSnapshotTarget();
		#endif
	}
	
	
	
	
	private int currentSwapID=-1;
	void SwapCreep(int ID){
		Obstacle obs=obstacleList[currentSwapID];
		obstacleList[currentSwapID]=obstacleList[ID];
		obstacleList[ID]=obs;
		
		currentSwapID=-1;
	}
	
}
