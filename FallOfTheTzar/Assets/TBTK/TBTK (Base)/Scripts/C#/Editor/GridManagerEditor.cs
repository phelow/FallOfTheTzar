using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor {
	
	GridManager gm;
	GameControlTB gc;
	
	string[] tileTypeLabel=new string[2];
	string[] tileTypeTooltip=new string[2];
	
	string[] spawnQuotaLabel=new string[2];
	string[] spawnQuotaTooltip=new string[2];
	
	List<UnitTB> unitList=new List<UnitTB>();
	string[] unitNameList=new string[0];
	string[] obstacleNameList=new string[0];
	string[] collectibleNameList=new string[0];
	string[] rotationLabel=new string[0];
	int[] intVal=new int[0];
	
	static bool showFactionList=false;
	//bool showUnitPrefabList=false;
	
	static bool showIndicatorList=false;
	
	
	void Awake(){
		LoadUnit();
		LoadObstacle();
		LoadCollectible();
		
		rotationLabel=new string[7];
		rotationLabel[0]="Random";
		rotationLabel[1]="0";
		rotationLabel[2]="60";
		rotationLabel[3]="120";
		rotationLabel[4]="180";
		rotationLabel[5]="240";
		rotationLabel[6]="300";
		
		int enumLength = Enum.GetValues(typeof(_TileType)).Length;
		tileTypeLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) tileTypeLabel[i]=((_TileType)i).ToString();
		
		tileTypeTooltip=new string[enumLength];
		tileTypeTooltip[0]="Use HexTile";
		tileTypeTooltip[1]="Use SquareTile";
		
		
		enumLength = Enum.GetValues(typeof(_SpawnQuota)).Length;
		spawnQuotaLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) spawnQuotaLabel[i]=((_SpawnQuota)i).ToString();
		
		spawnQuotaTooltip=new string[enumLength];
		spawnQuotaTooltip[0]="Spawn a set number of unit for the area\nEach prefab have a spawn limit";
		spawnQuotaTooltip[1]="Spawn a varied number of unit for the area based on the point budget available";
		
		gc=(GameControlTB)FindObjectOfType(typeof(GameControlTB));
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
	
	List<Obstacle> obstacleList=new List<Obstacle>();
	void LoadObstacle(){
		ObstacleListPrefab prefab=ObstacleManagerWindow.LoadObstacle();
		
		obstacleList=new List<Obstacle>();
		obstacleNameList=new string[prefab.obstacleList.Count+2];
		
		obstacleList.Add(null);
		obstacleList.Add(null);
		obstacleNameList[0]="empty(invisible)";
		obstacleNameList[1]="empty(visible)";
		
		for(int i=0; i<prefab.obstacleList.Count; i++){
			obstacleList.Add(prefab.obstacleList[i]);
			obstacleNameList[i+2]=prefab.obstacleList[i].obsName;
		}
	}
	
	List<CollectibleTB> collectibleList=new List<CollectibleTB>();
	void LoadCollectible(){
		EditorCollectibleList eCollectibleList=CollectibleManagerWindow.LoadCollectible();
		CollectibleListPrefab prefab=eCollectibleList.prefab;
		//~ CollectibleListPrefab prefab=CollectibleManagerWindow.LoadCollectible();
		
		collectibleList=new List<CollectibleTB>();
		collectibleNameList=new string[prefab.collectibleList.Count];
		
		for(int i=0; i<prefab.collectibleList.Count; i++){
			collectibleList.Add(prefab.collectibleList[i]);
			collectibleNameList[i]=prefab.collectibleList[i].collectibleName;
		}
	}
	
	
	GUIContent cont;
	GUIContent[] contL;
	
	
	public override void OnInspectorGUI(){
		gm = (GridManager)target;
		
		//DrawDefaultInspector();
		
		EditorGUILayout.Space();
		
		
		cont=new GUIContent("Randomize Grid On Start", "check to regenerate the tiles when the scene is loaded\nthis will generate all the units as well");
		gm.randomizeGridUponStart=EditorGUILayout.Toggle(cont, gm.randomizeGridUponStart);
		
		cont=new GUIContent("Randomize Unit On Start", "check to regenerate the units on the grid when the scene is loaded\nexisting unit will be removed");
		gm.randomizeUnitUponStart=EditorGUILayout.Toggle(cont, gm.randomizeUnitUponStart);
		
		cont=new GUIContent("Show Gizmo", "check to show gizmos on scene view");
		gm.showGizmo=EditorGUILayout.Toggle(cont, gm.showGizmo);
		
		EditorGUILayout.Space();
		
		int type=(int)gm.type;  
		cont=new GUIContent("Grid Type:", "Turn mode to be used in this scene");
		contL=new GUIContent[tileTypeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(tileTypeLabel[i], tileTypeTooltip[i]);
		type = EditorGUILayout.IntPopup(cont, type, contL, intVal);
		gm.type=(_TileType)type;
		
		cont=new GUIContent("Grid Size:", "The size of a single tile of the grid");
		gm.gridSize=EditorGUILayout.FloatField(cont, gm.gridSize);
		
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("Num of Column:", "the width(x-axis) of the area to cover for the whole grid");
		gm.width=EditorGUILayout.FloatField(cont, gm.width, GUILayout.ExpandWidth(true));
		//~ gm.width=EditorGUILayout.FloatField(cont, gm.width, GUILayout.MinWidth(160));
		//~ gm.width=Mathf.Round(Mathf.Clamp(gm.width, 0, 50));
		//~ cont=new GUIContent("Actual:"+(gm.width*gm.gridSize).ToString("f2"), "after multiply the GridSize");
		//~ EditorGUILayout.LabelField(cont, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		cont=new GUIContent("Num of Row:", "the length(z-axis) of the area to cover for the whole grid");
		gm.length=EditorGUILayout.FloatField(cont, gm.length, GUILayout.ExpandWidth(true));
		//~ gm.length=EditorGUILayout.FloatField(cont, gm.length, GUILayout.MinWidth(160));
		//~ gm.length=Mathf.Round(Mathf.Clamp(gm.length, 0, 50));
		//~ cont=new GUIContent("Actual:"+(gm.length*gm.gridSize).ToString("f2"), "after multiply the GridSize");
		//~ EditorGUILayout.LabelField(cont, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		cont=new GUIContent("Area Height:", "the position of the grid on y-axis\nThis is a relative value from the GridManager's transform");
		gm.baseHeight=EditorGUILayout.FloatField(cont, gm.baseHeight);
		//cont=new GUIContent("Max Height:", "the position of the grid on y-axis\nThis is a relative value from the GridManager's transform");
		//gm.maxHeight=EditorGUILayout.FloatField(cont, gm.maxHeight);

		cont=new GUIContent("GridToTileSizeRatio:", "the size ratio of a single unit in the grid with respect to an individual tile");
		gm.gridToTileSizeRatio=EditorGUILayout.FloatField(cont, gm.gridToTileSizeRatio);
		gm.gridToTileSizeRatio=Mathf.Max(0, gm.gridToTileSizeRatio);
		
		cont=new GUIContent("UnwalkableTileRate:", "the percentage of the unwalkable tile of the whole grid");
		gm.unwalkableRate=EditorGUILayout.FloatField(cont, gm.unwalkableRate);
		gm.unwalkableRate=Mathf.Clamp(gm.unwalkableRate, 0f, 1f);
		
		if(gm.unwalkableRate>0){
			cont=new GUIContent("UseObstacle:", "the percentage of the unwalkable tile of the whole grid");
			gm.useObstacle=EditorGUILayout.Toggle(cont, gm.useObstacle);
			
			bool changedFlag=GUI.changed;
			
			
			if(gm.useObstacle){
				for(int n=0; n<obstacleList.Count; n++){
					
					GUI.changed=false;
					
					bool flag=false;
					if(n==0) flag=gm.enableInvUnwalkable;
					else if(n==1) flag=gm.enableVUnwalkable;
					else{
						if(gm.obstaclePrefabList.Contains(obstacleList[n])) flag=true;
					}
					
					cont=new GUIContent("  -  "+obstacleNameList[n]+":", "");
					flag=EditorGUILayout.Toggle(cont, flag);
					
					if(n==0) gm.enableInvUnwalkable=flag;
					else if(n==1) gm.enableVUnwalkable=flag;
					else{
						if(GUI.changed){
							if(gm.obstaclePrefabList.Contains(obstacleList[n])) 
								gm.obstaclePrefabList.Remove(obstacleList[n]);
							else gm.obstaclePrefabList.Add(obstacleList[n]);
							
							GUI.changed=false;
						}
					}
				}
			}
			
			if(gm.obstaclePrefabList.Count==0){
				if(!gm.enableInvUnwalkable && !gm.enableVUnwalkable){
					gm.enableVUnwalkable=true;
				}
			}
			
			EditorGUILayout.Space();
			if(!GUI.changed) GUI.changed=changedFlag;
		}
		else EditorGUILayout.Space();
		
		
		cont=new GUIContent("AddCollectible:", "check to add collectible items to the grid when generating the grid");
		gm.addColletible=EditorGUILayout.Toggle(cont, gm.addColletible);
		
		if(gm.addColletible){
			
			EditorGUILayout.BeginHorizontal();
			cont=new GUIContent("CollectibleCount (min/max):", "The number of collectible to be spawned on the grid");
			EditorGUILayout.LabelField(cont, GUILayout.MinWidth(170), GUILayout.MaxWidth(170));
			gm.minCollectibleCount=EditorGUILayout.IntField(gm.minCollectibleCount, GUILayout.MaxWidth(30));
			gm.maxCollectibleCount=EditorGUILayout.IntField(gm.maxCollectibleCount, GUILayout.MaxWidth(30));
			EditorGUILayout.EndHorizontal();
			
			bool changedFlag=GUI.changed;
			
			if(collectibleList.Count>0){
				for(int n=0; n<collectibleList.Count; n++){
					
					GUI.changed=false;
					
					bool flag=false;
					if(gm.collectiblePrefabList.Contains(collectibleList[n])) flag=true;
					
					cont=new GUIContent("  -  "+collectibleNameList[n]+":", "");
					flag=EditorGUILayout.Toggle(cont, flag);
					
					if(GUI.changed){
						if(gm.collectiblePrefabList.Contains(collectibleList[n])) 
							gm.collectiblePrefabList.Remove(collectibleList[n]);
						else gm.collectiblePrefabList.Add(collectibleList[n]);
						
						GUI.changed=false;
					}
				}
			}
			else{
				EditorGUILayout.LabelField("- No collectible in CollectibleManager -");
			}
				
			EditorGUILayout.Space();
			if(!GUI.changed) GUI.changed=changedFlag;
		}
		else EditorGUILayout.Space();
		
		
		//cont=new GUIContent("Tile Prefab:", "the gameObject prefab to be used as the tile");
		//gm.tilePrefab=(GameObject)EditorGUILayout.ObjectField(cont, gm.tilePrefab, typeof(GameObject), false);
		
		if(gm.type==_TileType.Hex){
			cont=new GUIContent("HexTile Prefab:", "the gameObject prefab to be used as the tile");
			gm.hexTilePrefab=(GameObject)EditorGUILayout.ObjectField(cont, gm.hexTilePrefab, typeof(GameObject), false);
		}
		else if(gm.type==_TileType.Square){
			cont=new GUIContent("SquareTile Prefab:", "the gameObject prefab to be used as the tile");
			gm.squareTilePrefab=(GameObject)EditorGUILayout.ObjectField(cont, gm.squareTilePrefab, typeof(GameObject), false);
		}
		
		
		if(gc!=null){
			if(gc.playerFactionID.Count!=gm.playerPlacementAreas.Count) gm.playerPlacementAreas=MatchRectListLength(gm.playerPlacementAreas, gc.playerFactionID.Count);
			
			for(int i=0; i<gc.playerFactionID.Count; i++){
				EditorGUILayout.Space();
				cont=new GUIContent("Player (ID:"+gc.playerFactionID[i]+") Placement Area:", "The area on the grid which player will be able to place the starting unit, as shown as the green colored square on the gizmo");
				gm.playerPlacementAreas[i]=EditorGUILayout.RectField(cont,  gm.playerPlacementAreas[i]);
			}
			
			if(gc.playerFactionID.Count==0){
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("No player faction set in GameControl");
			}
		}
		else{
			gc=(GameControlTB)FindObjectOfType(typeof(GameControlTB));
		}
		EditorGUILayout.Space();
		
		
		//cont=new GUIContent("Player Placement Area:", "The area on the grid which player will be able to place the starting unit, as shown as the green colored square on the gizmo");
		//gm.playerPlacementArea=EditorGUILayout.RectField(cont,  gm.playerPlacementArea);
		
		
		//~ showIndicatorList
		string label="Indicators (Show)";
		if(showIndicatorList) label="Indicators (Hide)";
		cont=new GUIContent(label, "list of various indicators used in run time. Optional. If left empty the defaults will be loaded");
		showIndicatorList = EditorGUILayout.Foldout(showIndicatorList, cont);
		if(showIndicatorList){
			gm.indicatorSelect=(Transform)EditorGUILayout.ObjectField("IndicatorSelect:", gm.indicatorSelect, typeof(Transform), false);
			gm.indicatorCursor=(Transform)EditorGUILayout.ObjectField("IndicatorCursor:", gm.indicatorCursor, typeof(Transform), false);
			gm.indicatorHostile=(Transform)EditorGUILayout.ObjectField("IndicatorHostile:", gm.indicatorHostile, typeof(Transform), false);
		}
		
		//~ gm.playerPlacementArea=EditorGUILayout.RectField("Player Placement Area:", gm.playerPlacementArea);
		
		int num;
		
		label="Faction (Show)";
		if(showFactionList) label="Faction (Hide)";
		cont=new GUIContent(label, "list of factions in this scene.");
		showFactionList = EditorGUILayout.Foldout(showFactionList, cont);
		if(showFactionList){
			GUILayout.BeginHorizontal();	GUILayout.Space (15);
			GUILayout.BeginVertical();
			
			cont=new GUIContent("Add Faction", "add a new faction to the list");
			if(GUILayout.Button(cont)){
				Faction fac=new Faction();
				fac.factionID=gm.factionList.Count;
				fac.color=new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1);
				gm.factionList.Add(fac);
			}
			
			
			for(int i=0; i<gm.factionList.Count; i++){
				Rect rectBase = EditorGUILayout.BeginVertical();
				GUILayout.Space (5);
				rectBase.width+=5;	rectBase.height+=0;
				GUI.Box(rectBase, "");
				
				Faction fac=gm.factionList[i];
				cont=new GUIContent("ID:", "The ID of the faction. 2 factions entry will the same ID will be recognised as 1 faction in runtime. Player's faction's ID is 0 by default");
				fac.factionID = EditorGUILayout.IntField(cont, fac.factionID);
				cont=new GUIContent("Color:", "Color of the faction. This is simply for gizmo purpose to identify different spawn area in SceneView");
				fac.color = EditorGUILayout.ColorField(cont, fac.color);
				
				cont=new GUIContent("Add Area", "add an additional spawn area for the faction");
				if(GUILayout.Button(cont)){
					FactionSpawnInfo sInfo=new FactionSpawnInfo();
					if(fac.spawnInfo.Count>0){
						sInfo.area.x=fac.spawnInfo[fac.spawnInfo.Count-1].area.x;
						sInfo.area.y=fac.spawnInfo[fac.spawnInfo.Count-1].area.y;
						sInfo.area.width=fac.spawnInfo[fac.spawnInfo.Count-1].area.width;
						sInfo.area.height=fac.spawnInfo[fac.spawnInfo.Count-1].area.height;
					}
					fac.spawnInfo.Add(sInfo);
				}
				
				GUILayout.BeginHorizontal();	GUILayout.Space (10);
				GUILayout.BeginVertical();
					
				for(int nn=0; nn<fac.spawnInfo.Count; nn++){
					FactionSpawnInfo sInfo=fac.spawnInfo[nn];
					
					Rect rectSub = EditorGUILayout.BeginVertical();
					rectSub.x-=5; rectSub.width+=10;	rectSub.height+=5;
					GUI.Box(rectSub, "");
					
					
					
					int spawnQuota=(int)sInfo.spawnQuota;
					cont=new GUIContent("SpawnQuota:", "type of spawning algorithm to be used");
					contL=new GUIContent[spawnQuotaLabel.Length];
					for(int n=0; n<contL.Length; n++) contL[n]=new GUIContent(spawnQuotaLabel[n], spawnQuotaTooltip[n]);
					spawnQuota = EditorGUILayout.IntPopup(cont, spawnQuota, contL, intVal);
					sInfo.spawnQuota=(_SpawnQuota)spawnQuota;
					
					if(sInfo.spawnQuota==_SpawnQuota.UnitBased){
						cont=new GUIContent("Number to Spawn:", "number of unit to spawn for this spawn area");
						sInfo.unitCount=EditorGUILayout.IntField(cont, sInfo.unitCount, GUILayout.MaxHeight(14));
					}
					else if(sInfo.spawnQuota==_SpawnQuota.BudgetBased){
						cont=new GUIContent("Spawn Budget:", "Point budget allocated for the units to be spawned");
						sInfo.budget=EditorGUILayout.IntField(cont, sInfo.budget, GUILayout.MaxHeight(14));
					}
					
					
					cont=new GUIContent("Spawn Area:", "The area covered in which the units will be spawned in");
					sInfo.area=EditorGUILayout.RectField(cont, sInfo.area);
					
					
					cont=new GUIContent("Rotation:", "The direction which the unit spawned in this area will be facing");
					contL=new GUIContent[rotationLabel.Length];
					for(int n=0; n<contL.Length; n++) contL[n]=new GUIContent(rotationLabel[n], "");
					sInfo.unitRotation = EditorGUILayout.IntPopup(cont, sInfo.unitRotation, contL, intVal);
					
					
					label="Unit Prefab Pool (Show)";
					if(sInfo.showUnitPrefabList) label="Unit Prefab Pool (Hide)";
					cont=new GUIContent(label, "the list possible unit to be spawned in the area specified");
					sInfo.showUnitPrefabList = EditorGUILayout.Foldout(sInfo.showUnitPrefabList, label);
					if(sInfo.showUnitPrefabList){
						num=sInfo.unitPrefabs.Length;
						num=EditorGUILayout.IntField("  num of prefab:", num, GUILayout.MaxHeight(14));
						if(num!=sInfo.unitPrefabs.Length) sInfo.unitPrefabs=MatchUnitPrefabListLength(sInfo.unitPrefabs, num);
						if(num!=sInfo.unitPrefabsMax.Length) sInfo.unitPrefabsMax=MatchUnitPrefabMaxListLength(sInfo.unitPrefabsMax, num);
						for(int n=0; n<num; n++){
							int unitID=-1;
							for(int m=0; m<unitList.Count; m++){
								if(unitList[m]==sInfo.unitPrefabs[n]){
									unitID=m;
									break;
								}
							}
							
							EditorGUILayout.BeginHorizontal();
								if(sInfo.spawnQuota==_SpawnQuota.UnitBased){
									unitID = EditorGUILayout.IntPopup("   - unit"+n+"(max): ", unitID, unitNameList, intVal,  GUILayout.MaxHeight(13));
									sInfo.unitPrefabsMax[n] = EditorGUILayout.IntField(sInfo.unitPrefabsMax[n], GUILayout.MaxWidth(25));
								}
								else if(sInfo.spawnQuota==_SpawnQuota.BudgetBased){
									unitID = EditorGUILayout.IntPopup("   - unit"+n+"(max): ", unitID, unitNameList, intVal);
								}
							EditorGUILayout.EndHorizontal();
							
							if(unitID>=0) sInfo.unitPrefabs[n]=unitList[unitID];
							else sInfo.unitPrefabs[n]=null;
						}
					}
					
					int totalUnitCount=0;
					for(int n=0; n<sInfo.unitPrefabsMax.Length; n++) totalUnitCount+=sInfo.unitPrefabsMax[n];
					
					sInfo.unitCount=Mathf.Min(totalUnitCount, sInfo.unitCount);
					
					cont=new GUIContent("Remove area", "remove this spawn area");
					if(GUILayout.Button(cont)){
						fac.spawnInfo.Remove(sInfo);
					}
					
					EditorGUILayout.EndVertical();
				}
				
				
				GUILayout.EndVertical();
				GUILayout.Space (10);
				GUILayout.EndHorizontal ();
				
				GUILayout.Space (5);
				if(GUILayout.Button("Remove Faction")){
					gm.factionList.Remove(fac);
				}
				GUILayout.Space (5);
				
				EditorGUILayout.EndVertical();
				
				GUILayout.Space (5);
			}
			GUILayout.Space (10);
			GUILayout.EndVertical();
			//~ GUILayout.Space (5);
			GUILayout.EndHorizontal ();
		}
		
		
		EditorGUILayout.Space();
			cont=new GUIContent("Generate Grid", "Procedurally generate a new grid with the configured setting\nPlease note that generate a large map (30x30 and above) might take a while");
			if(GUILayout.Button(cont)) GenerateGrid();
		
			cont=new GUIContent("Generate Unit", "Procedurally place unit on the grid with the configured setting");
			if(GUILayout.Button(cont)) GenerateUnit();
		
			//~ if(GUILayout.Button("testGenerate")) TestGenerate();
			//~ if(GUILayout.Button("testDestroy")) TestDestroy();
		EditorGUILayout.Space();
		
		
		if(GUI.changed){
			EditorUtility.SetDirty(gm);
		}
	}
	
	
	
	
	List<Tile> GenerateSquareGrid(){ return GenerateSquareGrid(gm); }
	public static List<Tile> GenerateSquareGrid(GridManager gm){
		if(gm.squareTilePrefab==null) gm.squareTilePrefab=Resources.Load("SquareTile", typeof(GameObject)) as GameObject;
		Transform parentTransform=gm.transform;
		
		int counter=0;
		float highestY=-Mathf.Infinity;		float lowestY=Mathf.Infinity;
		float highestX=-Mathf.Infinity;		float lowestX=Mathf.Infinity;
		float gridSizeFactor=gm.gridToTileSizeRatio*gm.gridSize;
		
		List<Tile> tileList=new List<Tile>();
		
		for(int x=0; x<gm.width; x++){
			for(int y=0; y<gm.length; y++){
				float posX=x*gridSizeFactor;
				float posY=y*gridSizeFactor;
				
				if(posY>highestY) highestY=posY;		if(posY<lowestY) lowestY=posY;
				if(posX>highestX) highestX=posX;		if(posX<lowestX) lowestX=posX;
				
				Vector3 pos=new Vector3(posX, gm.baseHeight, posY);
				GameObject obj=(GameObject)PrefabUtility.InstantiatePrefab(gm.squareTilePrefab);
				obj.name="Tile"+counter.ToString();
				
				Transform objT=obj.transform;
				objT.parent=parentTransform;
				objT.localPosition=pos;
				objT.localRotation=Quaternion.Euler(-90, 0, 0);
				objT.localScale*=gm.gridSize;
				Tile hTile=obj.GetComponent<Tile>();
				
				tileList.Add(hTile);
				
				counter+=1;
			}
		}
		
		float disY=(Mathf.Abs(highestY)-Mathf.Abs(lowestY))/2;
		float disX=(Mathf.Abs(highestX)-Mathf.Abs(lowestX))/2;
		foreach(Tile hTile in tileList){
			Transform tileT=hTile.transform;
			tileT.position+=new Vector3(-disX, 0, -disY);
			hTile.pos=tileT.position;
		}
		
		return tileList;
	}
	List<Tile> GenerateHexGrid(){ return GenerateHexGrid(gm); }
	public static List<Tile> GenerateHexGrid(GridManager gm){
		if(gm.hexTilePrefab==null) gm.hexTilePrefab=Resources.Load("HexTile", typeof(GameObject)) as GameObject;
		Transform parentTransform=gm.transform;
		
		float ratio=0.745f/0.86f;   //0.86628f
		float hR=gm.gridToTileSizeRatio*gm.gridSize;
		float wR=hR*ratio;
		
		int counter=0;
		float highestY=-Mathf.Infinity;		float lowestY=Mathf.Infinity;
		float highestX=-Mathf.Infinity;		float lowestX=Mathf.Infinity;
		
		List<Tile> tileList=new List<Tile>();
		
		for(int x=0; x<gm.width; x++){
			float offset=0.5f*(x%2);
			
			int limit=0;
			if(x%2==1) limit=(int)(gm.length/2);
			else limit=(int)(gm.length/2+gm.length%2);
			
			for(int y=0; y<limit; y++){
				float posX=x*wR;//-widthOffset;
				float posY=y*hR+hR*offset;//-lengthOffset;
				
				if(posY>highestY) highestY=posY;		if(posY<lowestY) lowestY=posY;
				if(posX>highestX) highestX=posX;		if(posX<lowestX) lowestX=posX;
				
				Vector3 pos=new Vector3(posX, gm.baseHeight, posY);
				GameObject obj=(GameObject)PrefabUtility.InstantiatePrefab(gm.hexTilePrefab);
				obj.name="Tile"+counter.ToString();
				
				Transform objT=obj.transform;
				objT.parent=parentTransform;
				objT.localPosition=pos;
				objT.localRotation=Quaternion.Euler(-90, 0, 0);
				objT.localScale*=gm.gridSize*1.1628f;
				Tile hTile=obj.GetComponent<Tile>();
				
				tileList.Add(hTile);
				
				counter+=1;
			}
		}
		
		
		float disY=(Mathf.Abs(highestY)-Mathf.Abs(lowestY))/2;
		float disX=(Mathf.Abs(highestX)-Mathf.Abs(lowestX))/2;
		foreach(Tile hTile in tileList){
			Transform tileT=hTile.transform;
			tileT.position+=new Vector3(-disX, 0, -disY);
			hTile.pos=tileT.position;
		}
		
		return tileList;
	}
	
	void GenerateGrid(){
		/*
		DateTime timeS;
		DateTime timeE;
		TimeSpan timeSpan;
		timeS=System.DateTime.Now;
		*/
		
		/*
		timeE=System.DateTime.Now;
		timeSpan=timeE-timeS;
		Debug.Log( "Time:"+timeSpan.TotalMilliseconds);
		
		return;
		*/
		
		//clear previous tile
		Tile[] allTilesInScene=(Tile[])FindObjectsOfType(typeof(Tile));
		foreach(Tile tile in allTilesInScene){
			if(tile.unit!=null) DestroyImmediate(tile.unit.gameObject);
			if(tile.obstacle!=null) DestroyImmediate(tile.obstacle.gameObject);
			if(tile.collectible!=null) DestroyImmediate(tile.collectible.gameObject);
			
			DestroyImmediate(tile.gameObject);
		}
		//Tile[] allTilesInScenes=(Tile[])FindObjectsOfType(typeof(Tile));
		
		/*
		for(int i=0; i<tileList.Count; i++){
			if(tileList[i]!=null){
				if(tileList[i].unit!=null) DestroyImmediate(tileList[i].unit.gameObject);
				DestroyImmediate(tileList[i].gameObject);
			}
		}
		tileList=new List<Tile>();
		*/
		
		List<Tile> tileList=new List<Tile>();
		if(gm.type==_TileType.Square) tileList=GenerateSquareGrid();
		else if(gm.type==_TileType.Hex) tileList=GenerateHexGrid();
		
		
		/*
		mould the grid to the height of the terrain
		foreach(Tile hTile in tileList){
			Vector3 pos=new Vector3(hTile.pos.x, hTile.pos.y+gm.maxHeight, hTile.pos.z);
			RaycastHit hit;
			
			bool flag=Physics.Raycast(pos, Vector3.down, out hit, gm.maxHeight+10);
			
			if(flag) pos.y=hit.point.y+0.1f;
			else pos.y=gm.baseHeight;
			
			hTile.pos=pos;
			hTile.transform.position=pos;
		}
		*/
		
		
		//set unwalkable tile
		int blockTile=(int)(tileList.Count*gm.unwalkableRate);
		
		if(blockTile>0){
			int count=0;
			while(count<blockTile){
				int rand=UnityEngine.Random.Range(0, tileList.Count);
				while(!tileList[rand].walkable || tileList[rand].unit!=null) rand=UnityEngine.Random.Range(0, tileList.Count);
				
				if(!gm.useObstacle){
					tileList[rand].walkable=false;
					tileList[rand].Start();
				}
				else{
					int offset=0;
					if(gm.enableInvUnwalkable) offset+=1;
					if(gm.enableVUnwalkable) offset+=1;
					
					int randT=UnityEngine.Random.Range(0, gm.obstaclePrefabList.Count+offset);
					
					if(!gm.enableInvUnwalkable && !gm.enableVUnwalkable){
						
					}
					
					if(randT<offset){
						if(gm.enableInvUnwalkable && randT==0){
							tileList[rand].SetToUnwalkable(false);
						}
						else{
							tileList[rand].SetToUnwalkable(true);
						}
					}
					else{
						GameObject obsObj=(GameObject)PrefabUtility.InstantiatePrefab(gm.obstaclePrefabList[randT-offset].gameObject);
						obsObj.transform.localScale*=gm.gridSize;
						obsObj.transform.parent=tileList[rand].transform;
						obsObj.transform.localPosition=Vector3.zero;
						obsObj.GetComponent<Collider>().enabled=false;
						Obstacle obs=obsObj.GetComponent<Obstacle>();
						tileList[rand].obstacle=obs;
						obs.occupiedTile=tileList[rand];
						tileList[rand].SetToUnwalkable(false);
					}							
				}
				
				count+=1;
			}
		}
		
		for(int i=0; i<tileList.Count; i++) tileList[i].gameObject.layer=8;
		
		//set neighbour
		for(int i=0; i<tileList.Count; i++){
			Tile hT=tileList[i];
			Vector3 pos=hT.transform.position;
			Collider[] cols=Physics.OverlapSphere(pos, gm.gridSize*gm.gridToTileSizeRatio*0.6f);
			List<Tile> neighbour=new List<Tile>();
			foreach(Collider col in cols){
				Tile hTile=col.gameObject.GetComponent<Tile>();
				if(hTile!=null && hT!=hTile){
					neighbour.Add(hTile);
				}
			}
			hT.SetNeighbours(neighbour);
		}
		
		if(gm.addColletible && gm.collectiblePrefabList.Count>0){
			int ccount=UnityEngine.Random.Range(gm.minCollectibleCount, gm.maxCollectibleCount);
			
			if(ccount>0){
			
				List<Tile> emptyTiles=new List<Tile>();
				for(int i=0; i<tileList.Count; i++){
					if(tileList[i].walkable) emptyTiles.Add(tileList[i]);
				}
				for(int i=0; i<ccount; i++){
					int rand=UnityEngine.Random.Range(0, emptyTiles.Count);
					
					int randID=UnityEngine.Random.Range(0, gm.collectiblePrefabList.Count);
					GameObject colObj=(GameObject)PrefabUtility.InstantiatePrefab(gm.collectiblePrefabList[randID].gameObject);
					colObj.transform.parent=emptyTiles[rand].transform;
					colObj.transform.localPosition=Vector3.zero;
					colObj.transform.rotation=Quaternion.Euler(0, UnityEngine.Random.Range(0, 6)*60, 0);
					
					CollectibleTB col=colObj.GetComponent<CollectibleTB>();
					emptyTiles[rand].collectible=col;
					col.occupiedTile=emptyTiles[rand];
					
					emptyTiles.RemoveAt(rand);
					if(emptyTiles.Count==0) break;
				}
				
			}
		}
		
		GenerateUnit();
		
		gm.GenerateGrid(false);
		
		/*
		timeE=System.DateTime.Now;
		timeSpan=timeE-timeS;
		//Debug.Log(counter+" tiles has been generated    Time:"+timeSpan.TotalMilliseconds);
		*/
	}
	
	
	public void GenerateUnit(){
		Tile[] allTilesInScene=(Tile[])FindObjectsOfType(typeof(Tile));
		List<Tile> tileList=new List<Tile>();
		foreach(Tile tile in allTilesInScene){
			tileList.Add(tile);
		}
		
		//clear all the unit on the grid
		for(int i=0; i<tileList.Count; i++){
			if(tileList[i].unit!=null){
				DestroyImmediate(tileList[i].unit.gameObject);
				tileList[i].unit=null;
			}
		}
		
		//first get the TBTK/Units
		Transform unitParent=null;
		foreach(Transform child in gm.transform.parent){
			if(child.gameObject.name=="Units"){
				unitParent=child;
				break;
			}
		}
		
		int count=0;
		
		foreach(Faction fac in gm.factionList){
			foreach(FactionSpawnInfo sInfo in fac.spawnInfo){
				//make sure there's no empty element in the prefab list
				bool unitPrefabFilled=true;
				if(sInfo.unitPrefabs.Length>0){
					for(int i=0; i<sInfo.unitPrefabs.Length; i++){
						if(sInfo.unitPrefabs[i]==null){
							unitPrefabFilled=false;
						}
					}
				}
				else unitPrefabFilled=false;
				
				if(unitPrefabFilled){
					if(sInfo.unitCount>0){
						List<Tile> tempList=gm.GetTileWithinRect(sInfo.area);
						
						for(int i=0; i<tempList.Count; i++){
							Tile tile=tempList[i];
							if(tile.unit!=null || !tile.walkable || tile.collectible!=null){
								tempList.RemoveAt(i);
								i-=1;
							}
						}
						
						if(sInfo.spawnQuota==_SpawnQuota.UnitBased){
							int facUnitCount=Mathf.Min(sInfo.unitCount, tempList.Count);
							
							count=0;
							while(count<facUnitCount){
								int rand=UnityEngine.Random.Range(0, tempList.Count);
								Tile hTile=tempList[rand];
								
								int randUnit=UnityEngine.Random.Range(0, sInfo.unitPrefabs.Length);
								GameObject unitObj=(GameObject)PrefabUtility.InstantiatePrefab(sInfo.unitPrefabs[randUnit].gameObject);
								
								unitObj.transform.position=hTile.transform.position;
								if(unitParent!=null) unitObj.transform.parent=unitParent;
								
								int rotationOption=sInfo.unitRotation-1;
								if(rotationOption==-1){
									unitObj.transform.rotation=Quaternion.Euler(0, UnityEngine.Random.Range(0, 6)*60, 0);
								}
								else{
									unitObj.transform.rotation=Quaternion.Euler(0, rotationOption*60, 0);
								}
								
								UnitTB unit=unitObj.GetComponent<UnitTB>();
								unit.factionID=fac.factionID;
								unit.occupiedTile=hTile;
								hTile.unit=unit;
								unitObj.name="Gen"+unit.factionID+"_"+unit.unitName+"_"+count.ToString();
								
								EditorUtility.SetDirty(hTile);
								EditorUtility.SetDirty(unit);
								
								tempList.RemoveAt(rand);
								if(tempList.Count==0) break;
								
								count+=1;
							}
						}
						else{
							int budget=sInfo.budget;
							int lowestUnitPoint=99999999;
							for(int i=0; i<sInfo.unitPrefabs.Length; i++){
								if(sInfo.unitPrefabs[i].pointCost<lowestUnitPoint) lowestUnitPoint=sInfo.unitPrefabs[i].pointCost;
							}
							while(budget>=lowestUnitPoint){
								int rand=UnityEngine.Random.Range(0, tempList.Count);
								Tile hTile=tempList[rand];
								
								int randUnit=UnityEngine.Random.Range(0, sInfo.unitPrefabs.Length);
								GameObject unitObj=(GameObject)PrefabUtility.InstantiatePrefab(sInfo.unitPrefabs[randUnit].gameObject);
								
								unitObj.transform.position=hTile.transform.position;
								if(unitParent!=null) unitObj.transform.parent=unitParent;
								
								int rotationOption=sInfo.unitRotation-1;
								if(rotationOption==-1){
									unitObj.transform.rotation=Quaternion.Euler(0, UnityEngine.Random.Range(0, 6)*60, 0);
								}
								else{
									unitObj.transform.rotation=Quaternion.Euler(0, rotationOption*60, 0);
								}
								
								UnitTB unit=unitObj.GetComponent<UnitTB>();
								unit.factionID=fac.factionID;
								unit.occupiedTile=hTile;
								hTile.unit=unit;
								unitObj.name="Gen"+unit.factionID+"_"+unit.unitName+"_"+count.ToString();
								
								EditorUtility.SetDirty(hTile);
								EditorUtility.SetDirty(unit);
								
								tempList.RemoveAt(rand);
								if(tempList.Count==0) break;
								
								budget-=Mathf.Max(1, unit.pointCost);
							}
						}
					}
				}
				else{
					Debug.Log("unit assignment error for faction: "+fac.factionID);
				}
			}
		}
	}
	
	
	
	UnitTB[] MatchUnitPrefabListLength(UnitTB[] oldList, int count){
		UnitTB[] newList=new UnitTB[count];
		for(int i=0; i<count; i++){
			if(i<oldList.Length) newList[i]=oldList[i];
		}
		return newList;
	}
	
	
	int[] MatchUnitPrefabMaxListLength(int[] oldList, int count){
		int[] newList=new int[count];
		for(int i=0; i<count; i++){
			if(i<oldList.Length) newList[i]=oldList[i];
			else newList[i]=2;
		}
		return newList;
	}
	
	List<Rect> MatchRectListLength(List<Rect> oldList, int count){
		List<Rect> newList=new List<Rect>();
		for(int i=0; i<count; i++){
			if(i<oldList.Count) newList.Add(oldList[i]);
			else newList.Add(new Rect());
		}
		return newList;
	}
	
	public float GetTileSize(){
		return gm.gridSize*gm.gridToTileSizeRatio;
	}
}