#pragma warning disable 0414 // private field assigned but not used.

using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(GridPainter))]
public class GridPainterEditor : Editor {
	
	public static bool paintEnabled=false;
	
	public static int paintMode=0;
	
	public static int tileMode=0;
	
	
	public static int tilePlacementID=0;
	public static int HPGainModifier=0;
	public static int APGainModifier=0;
	public static int damageModifier=0;
	public static int attRangeModifier=0;
	public static float attackModifier=0;
	public static float defendModifier=0;
	public static float criticalModifier=0;
	public static float critDefModifier=0;
	
	public int rotationOption=-2;
	public string[] rotationLabel;
	
	public string[] paintModeLabel;
	public string[] paintModeTooltip;
	
	public string[] tileModeLabel;
	public string[] tileModeTooltip;
	
	public string[] stateLabel;
	public string[] stateTooltip;
	
	public string[] unitNameList;
	public string[] collectibleNameList;
	public string[] obstacleNameList;
	public int[] intVal;
	
	
	public static bool useDefaultFactionID=true;
	//~ public bool clearUnit=false;
	//~ public bool clearCollectible=false;
	//public static bool visibleUnwalkable=true;
	
	 public static int collectibleID=0;
	 public static int unitID=0;
	 public static int unitFactionID=1;
	 public static int state=0;
	 public static int stateAlt=2;
	
	
	private List<Tile> allTiles=new List<Tile>();
	public List<UnitTB> allUnits=new List<UnitTB>();
	
	int obstacleType=0;
	
	int tileLayer;
	
	public GridManager gridManager;
	
	void Awake(){
		GridPainter p = (GridPainter)target;
		
		gridManager=p.gameObject.GetComponent<GridManager>();
		if(gridManager==null){
			Debug.Log("Error. No GridManager was found. GridPainter must have a GridManager associate with it");
			DestroyImmediate(this);
		}
		
		paintModeLabel=new string[3];
		paintModeLabel[0]="Tile";
		paintModeLabel[1]="Unit";
		paintModeLabel[2]="Collectible";
		paintModeTooltip=new string[3];
		paintModeTooltip[0]="Edit various tile status on the grid\nSelect a sub-category for more information";
		paintModeTooltip[1]="Edit unit on the grid\nLeft-click on any tile in the SceneView to deploy the selected unit with specified factionID\nRight-click on any tile with a unit to remove the it";
		paintModeTooltip[2]="Edit collectible on the grid\nLeft-click on any tile in the SceneView to deploy selected collectible item\nRight-click on any tile with an collectible item to remove the it";
		
		stateLabel=new string[4];
		stateLabel[0]="Default";
		stateLabel[1]="UnitPlacement";
		stateLabel[2]="Unwalkable";
		stateLabel[3]="Wall";
		stateTooltip=new string[4];
		stateTooltip[0]="Set tile to default";
		stateTooltip[1]="Set tile to as available for player unit placement";
		stateTooltip[2]="Set tile to unwalkable";
		stateTooltip[3]="Set wall to certain section of the tile";
		
		
		tileModeLabel=new string[9];
		tileModeLabel[0]="State";
		tileModeLabel[1]="HP Gain";
		tileModeLabel[2]="AP Gain";
		tileModeLabel[3]="Damage";
		tileModeLabel[4]="Attack Range";
		tileModeLabel[5]="Hit";
		tileModeLabel[6]="Dodge";
		tileModeLabel[7]="Critical";
		tileModeLabel[8]="Critical Immunity";
		tileModeTooltip=new string[9];
		tileModeTooltip[0]="Set the state of the tiles\nLeft-click on any tile in the SceneView to set it to selected Tile State";
		tileModeTooltip[1]="Set the HPGainModifier of the tiles\nLeft-click on any tile in the SceneView to set the specified value fo the tile\n+ve value grant extra HP per round (as specified), -ve value do otherwise";
		tileModeTooltip[2]="Set the APGainModifier of the tiles\nLeft-click on any tile in the SceneView to set the specified value fo the tile\n+ve value grant extra AP per round (as specified), -ve value do otherwise";
		tileModeTooltip[3]="Set the damageModifier of the tiles\nLeft-click on any tile in the SceneView to set the specified value fo the tile\n+ve value grant damage bonus (as specified), -ve value do otherwise";
		tileModeTooltip[4]="Set the attRangeModifier of the tiles\nLeft-click on any tile in the SceneView to set the specified value fo the tile\n+ve value grant extra attack range bonus (for range unit only), -ve value do otherwise";
		tileModeTooltip[5]="Set the hitChanceModifier of the tiles\nLeft-click on any tile in the SceneView to set the specified value fo the tile\n+ve value grant attack bonus, -ve value do otherwise";
		tileModeTooltip[6]="Set the dodgeChanceModifier of the tiles\nLeft-click on any tile in the SceneView to set the specified value fo the tile\n+ve value grant defend bonus, -ve value do otherwise";
		tileModeTooltip[7]="Set the criticalModifier of the tiles\nLeft-click on any tile in the SceneView to set the specified value fo the tile\n+ve value grant critical bonus, -ve value do otherwise";
		tileModeTooltip[8]="Set the criticalDefModifier of the tiles\nLeft-click on any tile in the SceneView to set the specified value fo the tile\n+ve value grant critical immunity bonus, -ve value do otherwise";
		
		
		InitRotationLabel();
		
		LoadUnit();
		LoadCollectible();
		LoadObstacle();
		
		InitIntVal();
		
		Tile[] allTilesInScene=(Tile[])FindObjectsOfType(typeof(Tile));
		allTiles=new List<Tile>();
		foreach(Tile tile in allTilesInScene){
			allTiles.Add(tile);
		}
		
		gridManager=(GridManager)GameObject.FindObjectOfType(typeof(GridManager));
		if(gridManager==null) Debug.Log("error, no GridManager found");
		foreach(Tile tile in allTiles){
			if(tile.unit!=null){
				allUnits.Add(tile.unit);
			}
		}
	}
	
	List<UnitTB> unitList=new List<UnitTB>();
	void LoadUnit(){
		EditorUnitList eUnitList=UnitTBManagerWindow.LoadUnit();
		unitList=eUnitList.prefab.unitList;
		unitNameList=eUnitList.nameList;
	}
	
	List<CollectibleTB> collectibleList=new List<CollectibleTB>();
	void LoadCollectible(){
		EditorCollectibleList eCollectibleList=CollectibleManagerWindow.LoadCollectible();
		collectibleList=eCollectibleList.prefab.collectibleList;
		collectibleNameList=eCollectibleList.nameList;
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
			//~ obstacleList.Add(prefab.obstacleList[i].gameObject);
			//~ obstacleNameList[i+1]=prefab.obstacleList[i].obsName;
		}
	}
	
	void InitRotationLabel(){
		if(gridManager.type==_TileType.Square){
			rotationLabel=new string[4];
			rotationLabel[0]="0";
			rotationLabel[1]="90";
			rotationLabel[2]="180";
			rotationLabel[3]="270";
			rotationOption=0;
		}
		else if(gridManager.type==_TileType.Hex){
			rotationLabel=new string[6];
			rotationLabel[0]="0";
			rotationLabel[1]="60";
			rotationLabel[2]="120";
			rotationLabel[3]="180";
			rotationLabel[4]="240";
			rotationLabel[5]="300";
			rotationOption=0;
		}
	}
	
	void InitIntVal(){
		int max=Mathf.Max(unitList.Count, collectibleList.Count);
		max=Mathf.Max(obstacleList.Count, max);
		intVal=new int[Mathf.Max(3, max)];
		for(int i=0; i<intVal.Length; i++){
			intVal[i]=i;
		}
	}
	
	/*
	void OnEnable(){
		//ObstacleManagerWindow.onUpdateE += OnObstacleUpdate;
	}
	void OnDisable(){
		//ObstacleManagerWindow.onUpdateE -= OnObstacleUpdate;
	}
	void OnObstacleUpdate(){
		LoadObstacle();
		InitIntVal();
	}
	*/
	
	//rect to draw the hightlight for the selected item (unit/collectible)
	Rect selectedItemRect;
	
	GUIContent cont;
	GUIContent[] contL;
	
	Rect lastRect;
	float editorWidth;
	
	public override void OnInspectorGUI(){
		if(rotationLabel.Length==6 && gridManager.type!=_TileType.Hex) InitRotationLabel();
		if(rotationLabel.Length==4 && gridManager.type!=_TileType.Square) InitRotationLabel();
		
		EditorGUILayout.Space();
		
		//cont=new GUIContent("enabled:", "Check to enable painting in the SceneView");
		//paintEnabled = EditorGUILayout.Toggle(cont, paintEnabled);
		//EditorGUILayout.Space();
		
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
			if(paintEnabled) GUI.color=new Color(0, 1f, 1f, 1f);
			if(GUILayout.Button("Enable", GUILayout.ExpandWidth(true), GUILayout.Height(25))){
			//~ if(GUILayout.Button("Enable", GUILayout.Width(100), GUILayout.Height(25))){
				paintEnabled=true;
			}
			if(!paintEnabled) GUI.color=Color.red;
			else GUI.color=Color.white;
			if(GUILayout.Button("Disable", GUILayout.ExpandWidth(true), GUILayout.Height(25))){
			//~ if(GUILayout.Button("Disable", GUILayout.Width(100), GUILayout.Height(25))){
				paintEnabled=false;
			}
			GUI.color=Color.white;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		
		
		
		/*
		cont=new GUIContent("Mode:", "The edit mode currently selected\nDifferent edit mode edit different element of the grid");
		contL=new GUIContent[paintModeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(paintModeLabel[i], paintModeTooltip[i]);
		int lastMode=paintMode;
		paintMode = EditorGUILayout.IntPopup(cont, paintMode, contL, intVal);
		if(lastMode!=paintMode) GUI.changed=true;
		EditorGUILayout.Space();
		*/
		
		if(paintEnabled){
			float width=(editorWidth-2*4)/3;
			
			EditorGUILayout.BeginHorizontal();
				contL=new GUIContent[paintModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(paintModeLabel[i], paintModeTooltip[i]);
				
				for(int i=0; i<3; i++){
					if(paintMode==i) GUI.color=Color.green;
					//~ if(GUILayout.Button(contL[i], GUILayout.Width(81))){
					if(GUILayout.Button(contL[i], GUILayout.Width(width), GUILayout.Height(20))){
						paintMode=i;
						//GUI.changed=true;
					}
					GUI.color=Color.white;
				}
			EditorGUILayout.EndHorizontal();
				
			EditorGUILayout.Space();
			
			if(paintMode==0){
				/*
				cont=new GUIContent("Edit Feature:", "The specific attribute of the tile to modify");
				contL=new GUIContent[tileModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(tileModeLabel[i], tileModeTooltip[i]);
				//int lastTileMode=tileMode;
				tileMode = EditorGUILayout.IntPopup(cont, tileMode, contL, intVal);
				//if(lastTileMode!=tileMode) GUI.changed=true;
				*/
				
				cont=new GUIContent("Edit Feature:", "The specific attribute of the tile to modify");
				EditorGUILayout.LabelField(cont);
				
				float numInRow=(int)(editorWidth/75);
				
				contL=new GUIContent[tileModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(tileModeLabel[i], tileModeTooltip[i]);
				EditorGUILayout.BeginHorizontal();
					for(int i=0; i<tileModeLabel.Length; i++){
						
						if(i%numInRow==0){
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
						}
						
						//a hack to space the button
						//EditorGUILayout.LabelField("", GUILayout.MaxWidth(5));
						
						if(tileMode==i) GUI.color=Color.green;
						
						//cont=new GUIContent(unitList[i].icon, unitList[i].unitName);
						//~ if(GUILayout.Button(contL[i], GUILayout.MaxWidth(60))){
						//~ if(GUILayout.Button(contL[i], GUILayout.MaxWidth(130))){
						if(GUILayout.Button(contL[i], GUILayout.MinWidth(60), GUILayout.ExpandWidth(true))){
							tileMode=i;
						}
					
						GUI.color=Color.white;
					}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				
				
				if(tileMode==0){
					/*
					cont=new GUIContent("Tile State:", "The tile state currently selected\nTile left-clicked on in the SceneView will be set to this state");
					contL=new GUIContent[stateLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(stateLabel[i], stateTooltip[i]);
					state = EditorGUILayout.IntPopup(cont, state, contL, intVal);
					
					cont=new GUIContent("Tile State Alt: ", "Secondary tile state currently selected\nTile right-clicked on in the SceneView will be set to this state");
					stateAlt = EditorGUILayout.IntPopup(cont, stateAlt, contL, intVal);
					*/
					
					
					cont=new GUIContent("Tile State:", "The tile state currently selected\nTile left-clicked on in the SceneView will be set to this state");
					EditorGUILayout.LabelField(cont);
					
					EditorGUILayout.BeginHorizontal();
						contL=new GUIContent[stateLabel.Length];
						for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(stateLabel[i], stateTooltip[i]);
						
						for(int i=0; i<4; i++){
							if(i%2==0){
								EditorGUILayout.EndHorizontal();
								EditorGUILayout.BeginHorizontal();
							}
							
							if(state==i) GUI.color=Color.green;
							//~ if(GUILayout.Button(contL[i], GUILayout.ExpandWidth(true), GUILayout.Height(20))){
							if(GUILayout.Button(contL[i], GUILayout.Height(20))){
								state=i;
								obstacleType=1;
							}
							GUI.color=Color.white;
						}
					EditorGUILayout.EndHorizontal();
					
					
					/*
					cont=new GUIContent("Unwalkable Type:", "type of unwalkable tile to be applied");
					contL=new GUIContent[obstacleNameList.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(obstacleNameList[i], obstacleNameList[i]);
					obstacleType = EditorGUILayout.IntPopup(cont, obstacleType, contL, intVal);
					*/
					
					EditorGUILayout.Space();
					
					
					/*
					GUI.color=Color.green;
					Rect r=selectedItemRect;
					r.x-=5; r.y-=5; r.width+=10; r.height+=10;
					GUI.Box(r, "");
					GUI.color=Color.white;
					*/
					
					if(state==1){
						EditorGUILayout.Space();
						EditorGUILayout.BeginHorizontal();
							cont=new GUIContent("Placement FactionID:", "The factionID of units intended to be placed in the specified tile");
							//EditorGUILayout.LabelField(cont);
							tilePlacementID=EditorGUILayout.IntField(cont, tilePlacementID);
						EditorGUILayout.EndHorizontal();
					}
					if(state==2){
						EditorGUILayout.Space();
						
						cont=new GUIContent("Unwalkable Type:", "type of unwalkable tile to be applied");
						EditorGUILayout.LabelField(cont);
						
						EditorGUILayout.BeginHorizontal();
							for(int i=0; i<obstacleList.Count; i++){
								bool flag1=obstacleList[i]==null;
								bool flag2=false; bool flag3=false;
								if(!flag1){
									flag2=(int)gridManager.type==(int)obstacleList[i].tileType;
									flag2|=obstacleList[i].tileType==_ObsTileType.Universal;
									flag3=obstacleList[i].obsType==_ObsType.Obstacle;
								}
								else{
									flag2=true; flag3=true;
								}
								if(flag2 && flag3){
									if(i%2==0){
										EditorGUILayout.EndHorizontal();
										//EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();
									}
									
									//a hack to space the button
									//EditorGUILayout.LabelField("", GUILayout.MaxWidth(5));
									
									if(obstacleType==i) GUI.color=Color.green;
									
									//~ cont=new GUIContent(obstacleList[i].obsName+"\n"+obstacleList[i].type, obstacleList[i].obsName);
									if(i<2) cont=new GUIContent(obstacleNameList[i], "");
									else cont=new GUIContent(obstacleList[i].obsName, "");
									//~ if(GUILayout.Button(cont, GUILayout.MaxWidth(125))){
									//~ if(GUILayout.Button(cont, GUILayout.MaxWidth(50), GUILayout.MaxHeight(50))){
									if(GUILayout.Button(cont, GUILayout.ExpandWidth(true))){
										obstacleType=i;
									}
									
									GUI.color=Color.white;
									
									if(obstacleType==i && Event.current.type==EventType.repaint){
										selectedItemRect=GUILayoutUtility.GetLastRect();
										Repaint();
									}
								}
							}
						EditorGUILayout.EndHorizontal();
						//EditorGUILayout.Space();
							
						
						EditorGUILayout.Space();
						cont=new GUIContent("Item Rotation:", "The rotation of the item to be placed");
						EditorGUILayout.LabelField(cont);
							
						EditorGUILayout.BeginHorizontal();
							if(rotationOption==-2) GUI.color=Color.green;
							if(GUILayout.Button("Cursor Based", GUILayout.ExpandWidth(true))){
								rotationOption=-2;
							}
							
							if(rotationOption==-1) GUI.color=Color.green;
							else GUI.color=Color.white;
							
							if(GUILayout.Button("Random", GUILayout.ExpandWidth(true))){
								rotationOption=-1;
							}
							GUI.color=Color.white;
						EditorGUILayout.EndHorizontal();
						
						width=(editorWidth-(rotationLabel.Length)*4)/rotationLabel.Length;
					
						EditorGUILayout.BeginHorizontal();
							for(int i=0; i<rotationLabel.Length; i++){
								if(rotationOption==i) GUI.color=Color.green;
								if(GUILayout.Button(rotationLabel[i], GUILayout.Width(width))){
									rotationOption=i;
								}
								GUI.color=Color.white;
							}
						EditorGUILayout.EndHorizontal();
							
						/*
						if(obstacleType==0){
							cont=new GUIContent("Visible Unwalkable: ", "Uncheck to make the turn off the renderer on the tile set to unwalkable\nThis will make the tile invisible");
							visibleUnwalkable = EditorGUILayout.Toggle("Visible Unwalkable: ", visibleUnwalkable);
						}
						*/
						
						EditorGUILayout.Space();
						EditorGUILayout.Space();
						cont=new GUIContent("Obstacle Manager", "Open Obstacle Manager");
						if(GUILayout.Button(cont)){ ObstacleManagerWindow.Init(); }
						
					}
					else if(state==3){
						EditorGUILayout.Space();
						
						cont=new GUIContent("Wall Prefab:", "prefabs of wall to be applied");
						EditorGUILayout.LabelField(cont);
						
						EditorGUILayout.BeginHorizontal();
							for(int i=1; i<obstacleList.Count; i++){
								bool flag1=obstacleList[i]==null;
								bool flag2=false; bool flag3=false;
								if(!flag1){
									flag2=(int)gridManager.type==(int)obstacleList[i].tileType;
									flag2|=obstacleList[i].tileType==_ObsTileType.Universal;
									flag3=obstacleList[i].obsType==_ObsType.Wall;
								}
								else{
									flag2=true; flag3=true;
								}
								if(flag2 && flag3){
									if((i-1)%2==0){
										EditorGUILayout.EndHorizontal();
										//EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();
									}
									
									//a hack to space the button
									//EditorGUILayout.LabelField("", GUILayout.MaxWidth(5));
									
									if(obstacleType==i) GUI.color=Color.green;
									
									//~ cont=new GUIContent(obstacleList[i].obsName+"\n"+obstacleList[i].type, obstacleList[i].obsName);
									if(i==1) cont=new GUIContent("no visible wall", "");
									else cont=new GUIContent(obstacleList[i].obsName, "");
									//~ if(GUILayout.Button(cont, GUILayout.MaxWidth(125))){
									//~ if(GUILayout.Button(cont, GUILayout.MaxWidth(50), GUILayout.MaxHeight(50))){
									if(GUILayout.Button(cont, GUILayout.ExpandWidth(true))){
										obstacleType=i;
									}
									
									GUI.color=Color.white;
									
									if(obstacleType==i && Event.current.type==EventType.repaint){
										selectedItemRect=GUILayoutUtility.GetLastRect();
										Repaint();
									}
								}
							}
						EditorGUILayout.EndHorizontal();
					}
				}
				else if(tileMode==1){
					cont=new GUIContent("Value:", "The value to set of HPGainModifier to be set on the tile");
					HPGainModifier=EditorGUILayout.IntField(cont, HPGainModifier);
				}
				else if(tileMode==2){
					cont=new GUIContent("Value:", "The value to set of APGainModifier to be set on the tile");
					APGainModifier=EditorGUILayout.IntField(cont, APGainModifier);
				}
				else if(tileMode==3){
					cont=new GUIContent("Value:", "The value to set of damageModifier to be set on the tile");
					damageModifier=EditorGUILayout.IntField(cont, damageModifier);
				}
				else if(tileMode==4){
					cont=new GUIContent("Value:", "The value to set of attRangeModifier to be set on the tile");
					attRangeModifier=EditorGUILayout.IntField(cont, attRangeModifier);
				}
				else if(tileMode==5){
					cont=new GUIContent("Value:", "The value to set of attackModifier to be set on the tile");
					attackModifier=EditorGUILayout.FloatField(cont, attackModifier);
				}
				else if(tileMode==6){
					cont=new GUIContent("Value:", "The value to set of defendModifier to be set on the tile");
					defendModifier=EditorGUILayout.FloatField(cont, defendModifier);
				}
				else if(tileMode==7){
					cont=new GUIContent("Value:", "The value to set of criticalModifier to be set on the tile");
					criticalModifier=EditorGUILayout.FloatField(cont, criticalModifier);
				}
				else if(tileMode==8){
					cont=new GUIContent("Value:", "The value to set of critDefModifier to be set on the tile");
					critDefModifier=EditorGUILayout.FloatField(cont, critDefModifier);
				}
				
				if(tileMode>0){
					EditorGUILayout.LabelField("Left click on a tile to set to specfied value");
					EditorGUILayout.LabelField("Right click on a tile to set to default value(0)");
				}
				
			}
			else if(paintMode==1){
				
				/*
				cont=new GUIContent("Unit:", "Current selected unit to be deployed on the grid");
				contL=new GUIContent[unitNameList.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(unitNameList[i], unitNameList[i]);
				unitID = EditorGUILayout.IntPopup(cont, unitID, contL, intVal);
				*/
				
				EditorGUILayout.BeginHorizontal();
				cont=new GUIContent("Use default faction ID:", "Check to use default factionID assign to each unit in UnitEditor");
				EditorGUILayout.LabelField(cont, GUILayout.ExpandWidth(true));
				useDefaultFactionID=EditorGUILayout.Toggle(useDefaultFactionID);
				EditorGUILayout.EndHorizontal();
				
				if(!useDefaultFactionID){
					cont=new GUIContent("Overwrite faction ID:", "The factionID of the unit being deployed\nUnits will same factionID will fight on the same side");
					unitFactionID=EditorGUILayout.IntField(cont, unitFactionID);
				}
				
				/*
				GUIStyle helpStyle = new GUIStyle(GUI.skin.box);
				helpStyle.wordWrap = true;
				helpStyle.alignment = TextAnchor.UpperLeft;
				Color c = Color.black;
				c.a = 0.75f;
				helpStyle.normal.textColor = c;
				
				string label=" - Select a unit from the list below:\n";
				label+=" - Left Click on any tile in SceneView to place the unit\n";
				label+=" - Right click on any tile contain a unit to remove the it";
				EditorGUILayout.LabelField(label, helpStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				*/
				
				//hight the selected item
				GUI.color=Color.green;
				Rect r=selectedItemRect;
				r.x-=5; r.y-=5; r.width+=10; r.height+=10;
				GUI.Box(r, "");
				GUI.color=Color.white;
				
				float numInRow=(int)(editorWidth/62);
				
				EditorGUILayout.BeginHorizontal();
					for(int i=0; i<unitList.Count; i++){
						
						if(i%numInRow==0){
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();
						}
						
						//a hack to space the button
						EditorGUILayout.LabelField("", GUILayout.MaxWidth(5));
						
						string tooltip="";
						if(useDefaultFactionID) tooltip+="factionID: "+unitList[i].factionID+"\n";
						tooltip+=unitList[i].desp;
						cont=new GUIContent(unitList[i].icon, tooltip);
						if(GUILayout.Button(cont, GUILayout.MaxWidth(50), GUILayout.MaxHeight(50))){
							unitID=i;
						}
						
						if(unitID==i && Event.current.type==EventType.repaint){
							selectedItemRect=GUILayoutUtility.GetLastRect();
							Repaint();
						}
						
						
					}
					EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
					
					
				EditorGUILayout.Space();
				cont=new GUIContent("Unit Rotation:", "The rotation of the item to be placed");
				EditorGUILayout.LabelField(cont);
					
				EditorGUILayout.BeginHorizontal();
					if(rotationOption==-2) GUI.color=Color.green;
					if(GUILayout.Button("Cursor Based", GUILayout.ExpandWidth(true))){
						rotationOption=-2;
					}
					
					if(rotationOption==-1) GUI.color=Color.green;
					else GUI.color=Color.white;
					
					if(GUILayout.Button("Random", GUILayout.ExpandWidth(true))){
						rotationOption=-1;
					}
					GUI.color=Color.white;
				EditorGUILayout.EndHorizontal();
				
				width=(editorWidth-(rotationLabel.Length-1)*4)/rotationLabel.Length;
			
				EditorGUILayout.BeginHorizontal();
					for(int i=0; i<rotationLabel.Length; i++){
						if(rotationOption==i) GUI.color=Color.green;
						if(GUILayout.Button(rotationLabel[i], GUILayout.Width(width))){
							rotationOption=i;
						}
						GUI.color=Color.white;
					}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				
				
				
				cont=new GUIContent("Unit Manager", "Open Unit Manager");
				if(GUILayout.Button(cont)){ UnitTBManagerWindow.Init(); }
			}
			else if(paintMode==2){
				/*
				cont=new GUIContent("Collectible:", "Current selected collectible to be deployed on the grid");
				contL=new GUIContent[collectibleNameList.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(collectibleNameList[i], collectibleNameList[i]);
				collectibleID = EditorGUILayout.IntPopup(cont, collectibleID, contL, intVal);
				*/
				
				/*
				GUIStyle helpStyle = new GUIStyle(GUI.skin.box);
				helpStyle.wordWrap = true;
				helpStyle.alignment = TextAnchor.UpperLeft;
				Color c = Color.black;
				c.a = 0.75f;
				helpStyle.normal.textColor = c;
				
				string label="Select a collectible from the list below:\n";
				label+="and click on any tile in SceneView to place the item\n";
				label+="Right click on any tile contain a collectible item to remove the item";
				EditorGUILayout.LabelField(label, helpStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				*/
				
				GUI.color=Color.green;
				Rect r=selectedItemRect;
				r.x-=5; r.y-=5; r.width+=10; r.height+=10;
				GUI.Box(r, "");
				GUI.color=Color.white;
				
				float numInRow=(int)(editorWidth/62);
				
				EditorGUILayout.BeginHorizontal();
					for(int i=0; i<collectibleList.Count; i++){
						if(i%numInRow==0){
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();
						}
						
						//a hack to space the button
						EditorGUILayout.LabelField("", GUILayout.MaxWidth(5));
						
						cont=new GUIContent(collectibleList[i].icon, collectibleList[i].name);
						if(GUILayout.Button(cont, GUILayout.MaxWidth(50), GUILayout.MaxHeight(50))){
							unitID=i;
						}
						
						if(unitID==i && Event.current.type==EventType.repaint){
							selectedItemRect=GUILayoutUtility.GetLastRect();
							Repaint();
						}
					}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
					
					
				EditorGUILayout.Space();
				cont=new GUIContent("Item Rotation:", "The rotation of the item to be placed");
				EditorGUILayout.LabelField(cont);
					
				EditorGUILayout.BeginHorizontal();
					if(rotationOption==-2) GUI.color=Color.green;
					if(GUILayout.Button("Cursor Based", GUILayout.ExpandWidth(true))){
						rotationOption=-2;
					}
					
					if(rotationOption==-1) GUI.color=Color.green;
					else GUI.color=Color.white;
					
					if(GUILayout.Button("Random", GUILayout.ExpandWidth(true))){
						rotationOption=-1;
					}
					GUI.color=Color.white;
				EditorGUILayout.EndHorizontal();
				
				width=(editorWidth-(rotationLabel.Length-1)*4)/rotationLabel.Length;
			
				EditorGUILayout.BeginHorizontal();
					for(int i=0; i<rotationLabel.Length; i++){
						if(rotationOption==i) GUI.color=Color.green;
						if(GUILayout.Button(rotationLabel[i], GUILayout.Width(width))){
							rotationOption=i;
						}
						GUI.color=Color.white;
					}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
					
				
				cont=new GUIContent("Collectible Manager", "Open Collectible Manager");
				if(GUILayout.Button(cont)){ CollectibleManagerWindow.Init(); }
			}
			EditorGUILayout.Space();
			
			//~ DrawDefaultInspector();
			
			if(unitList.Count!=unitNameList.Length){
				//InitUnit();
				LoadUnit();
				InitIntVal();
			}
			if(collectibleList.Count!=collectibleNameList.Length){
				//InitCollectible();
				LoadCollectible();
				InitIntVal();
			}
			
			
			if(Event.current.type==EventType.repaint){
				//lastRect=GUILayoutUtility.GetLastRect();
				//editorWidth=lastRect.x+lastRect.width-8;
				editorWidth=Screen.width-8;
				Repaint();
			}
		}
		
		
		if(GUI.changed) OnSceneGUI();
	}
	
	
	
	void OnSceneGUI(){
		if(Application.isPlaying){
			return;
		}
		
		foreach(UnitTB unit in allUnits){
			if(unit!=null)
				Handles.Label(unit.transform.position,  "faction:" + unit.factionID.ToString());
			//Handles.BeginGUI(new Rect(Screen.width - 100, Screen.height - 80, 90,50));
		}
		
		if(paintMode==0){
			if(tileMode==0){
				if(state==1){
					foreach(Tile tile in allTiles){
						if(tile.openForPlacement){
							Handles.Label(tile.transform.position,  tile.placementID.ToString());
						}
					}
				}
				else if(state==3){
					foreach(Tile tile in allTiles){
						//~ Handles.color = Color.green;
						//~ foreach(Tile neighbour in tile.neighbours){
							//~ Handles.DrawLine(tile.pos, neighbour.pos);
						//~ }
						Handles.color = Color.red;
						foreach(Wall wall in tile.walls){
							Handles.DrawLine(wall.tile1.pos, wall.tile2.pos);
						}
					}
				}
			}
			if(tileMode==1){
				foreach(Tile tile in allTiles){
					if(tile.HPGainModifier<0){
						Handles.color = Color.red;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(90, 0, 0), 2);
					}
					else if(tile.HPGainModifier>0){
						Handles.color = Color.green;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(-90, 0, 0), 2);
					}
					Handles.Label(tile.transform.position,  tile.HPGainModifier.ToString());
				}
				Handles.color = Color.white;
			}
			else if(tileMode==2){
				foreach(Tile tile in allTiles){
					if(tile.APGainModifier<0){
						Handles.color = Color.red;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(90, 0, 0), 2);
					}
					else if(tile.APGainModifier>0){
						Handles.color = Color.green;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(-90, 0, 0), 2);
					}
					Handles.Label(tile.transform.position,  tile.APGainModifier.ToString());
				}
				Handles.color = Color.white;
				//~ foreach(Tile tile in allTiles) Handles.Label(tile.transform.position,  tile.APGainModifier.ToString());
			}
			else if(tileMode==3){
				foreach(Tile tile in allTiles){
					if(tile.damageModifier<0){
						Handles.color = Color.red;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(90, 0, 0), 2);
					}
					else if(tile.damageModifier>0){
						Handles.color = Color.green;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(-90, 0, 0), 2);
					}
					Handles.Label(tile.transform.position,  tile.damageModifier.ToString());
				}
				Handles.color = Color.white;
				//~ foreach(Tile tile in allTiles) Handles.Label(tile.transform.position,  tile.damageModifier.ToString());
			}
			else if(tileMode==4){
				foreach(Tile tile in allTiles){
					if(tile.attRangeModifier<0){
						Handles.color = Color.red;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(90, 0, 0), 2);
					}
					else if(tile.attRangeModifier>0){
						Handles.color = Color.green;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(-90, 0, 0), 2);
					}
					Handles.Label(tile.transform.position,  tile.attRangeModifier.ToString());
				}
				Handles.color = Color.white;
				//~ foreach(Tile tile in allTiles) Handles.Label(tile.transform.position,  tile.attRangeModifier.ToString());
			}
			else if(tileMode==5){
				foreach(Tile tile in allTiles){
					if(tile.attackModifier<0){
						Handles.color = Color.red;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(90, 0, 0), 2);
					}
					else if(tile.attackModifier>0){
						Handles.color = Color.green;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(-90, 0, 0), 2);
					}
					Handles.Label(tile.transform.position,  tile.attackModifier.ToString());
				}
				Handles.color = Color.white;
				//~ foreach(Tile tile in allTiles) Handles.Label(tile.transform.position,  tile.attackModifier.ToString("f2"));
			}
			else if(tileMode==6){
				foreach(Tile tile in allTiles){
					if(tile.defendModifier<0){
						Handles.color = Color.red;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(90, 0, 0), 2);
					}
					else if(tile.defendModifier>0){
						Handles.color = Color.green;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(-90, 0, 0), 2);
					}
					Handles.Label(tile.transform.position,  tile.defendModifier.ToString());
				}
				Handles.color = Color.white;
				//~ foreach(Tile tile in allTiles) Handles.Label(tile.transform.position,  tile.defendModifier.ToString("f2"));
			}
			else if(tileMode==7){
				foreach(Tile tile in allTiles){
					if(tile.criticalModifier<0){
						Handles.color = Color.red;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(90, 0, 0), 2);
					}
					else if(tile.criticalModifier>0){
						Handles.color = Color.green;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(-90, 0, 0), 2);
					}
					Handles.Label(tile.transform.position,  tile.criticalModifier.ToString());
				}
				Handles.color = Color.white;
				//~ foreach(Tile tile in allTiles) Handles.Label(tile.transform.position,  tile.criticalModifier.ToString("f2"));
			}
			else if(tileMode==8){
				foreach(Tile tile in allTiles){
					if(tile.critDefModifier<0){
						Handles.color = Color.red;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(90, 0, 0), 2);
					}
					else if(tile.critDefModifier>0){
						Handles.color = Color.green;
						Handles.ArrowCap(0, tile.transform.position, Quaternion.Euler(-90, 0, 0), 2);
					}
					Handles.Label(tile.transform.position,  tile.critDefModifier.ToString());
				}
				Handles.color = Color.white;
				//~ foreach(Tile tile in allTiles) Handles.Label(tile.transform.position,  tile.critDefModifier.ToString("f2"));
			}
		}
		
		
		//if(!paintEnabled) return;
		
        Event current = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
     
        switch (current.type)
        {
			case EventType.MouseDown:
				Paint(current);
				break;
			
			case EventType.MouseDrag:
				Paint(current);
				break;
     
			case EventType.layout:
				HandleUtility.AddDefaultControl(controlID);
				break;
        }

		if (GUI.changed){
			HandleUtility.Repaint();
			EditorUtility.SetDirty(target);
		}
	}
	
	void Paint(Event current){
		if(!paintEnabled) return;
		
		Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
			Tile tile=hit.transform.gameObject.GetComponent<Tile>();
			if(tile!=null){
				//tile
				if(paintMode==0){
					
					if(tileMode==0){
						if(current.button == 0){
							if(state==0){
								tile.SetToDefault();
								if(tile.obstacle!=null) DestroyImmediate(tile.obstacle.gameObject);
							}
							else if(state==1){
								tile.SetToWalkable();
								tile.openForPlacement=true;
								tile.placementID=tilePlacementID;
								if(tile.obstacle!=null) DestroyImmediate(tile.obstacle.gameObject);
							}
							else if(state==2){
								if(tile.unit!=null) return;
								if(tile.collectible!=null) DestroyImmediate(tile.collectible.gameObject);
								
								if(obstacleType==0) tile.SetToUnwalkable(false);
								else if(obstacleType==1) tile.SetToUnwalkable(true);
								else if(obstacleType>1){
									if(tile.obstacle!=null) DestroyImmediate(tile.obstacle.gameObject);
								
									GameObject obsObj=(GameObject)PrefabUtility.InstantiatePrefab(obstacleList[obstacleType].gameObject);
									obsObj.transform.localScale*=gridManager.gridSize;
									obsObj.transform.parent=tile.transform;
									obsObj.transform.localPosition=Vector3.zero;
									obsObj.GetComponent<Collider>().enabled=false;
									Obstacle obs=obsObj.GetComponent<Obstacle>();
									tile.obstacle=obs;
									obs.occupiedTile=tile;
									tile.SetToUnwalkable(false);
									
									if(gridManager.type==_TileType.Hex){
										if(rotationOption==-2){
											Vector3 pos=tile.transform.position;
											Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
											float angle=((int)(Utility.VectorToAngle(dir)/60)-1)*-60;
											obsObj.transform.rotation=Quaternion.Euler(0, angle, 0);
										}
										else if(rotationOption==-1) obsObj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 6)*60, 0);
										else obsObj.transform.rotation=Quaternion.Euler(0, rotationOption*60, 0);
									}
									else if(gridManager.type==_TileType.Square){
										if(rotationOption==-2){
											Vector3 pos=tile.transform.position;
											Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
											float angle=((int)(Utility.VectorToAngle(dir)/90)-1)*-90;
											obsObj.transform.rotation=Quaternion.Euler(0, angle, 0);
										}
										else if(rotationOption==-1) obsObj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 4)*90, 0);
										else obsObj.transform.rotation=Quaternion.Euler(0, rotationOption*90, 0);
									}
								}
							}
							else if(state==3){
								Obstacle obs=obstacleList[obstacleType];
								if(gridManager.type==_TileType.Hex){
									//~ if(rotationOption==-2){
										Vector3 pos=tile.transform.position;
										Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
										//~ float angle=((int)(Utility.VectorToAngle(dir)/60)-1)*-60;
										float angle=((int)(Utility.VectorToAngle(dir)));
										
										Tile adjacentT=tile.AddWall(angle, obs);
										EditorUtility.SetDirty(tile);
										if(adjacentT!=null) EditorUtility.SetDirty(adjacentT);
									//~ }
								}
								else if(gridManager.type==_TileType.Square){
									Vector3 pos=tile.transform.position;
									Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
									//~ float angle=((int)(Utility.VectorToAngle(dir)/90)-1)*-90;
									float angle=((int)(Utility.VectorToAngle(dir)));
									Tile adjacentT=tile.AddWall(angle, obs);
									EditorUtility.SetDirty(tile);
									if(adjacentT!=null) EditorUtility.SetDirty(adjacentT);
								}
							}
						}
						else if(current.button == 1){
							if(state==3){
								if(gridManager.type==_TileType.Hex){
									//~ if(rotationOption==-2){
										Vector3 pos=tile.transform.position;
										Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
										//~ float angle=((int)(Utility.VectorToAngle(dir)/60)-1)*-60;
										float angle=((int)(Utility.VectorToAngle(dir)));
										
										Tile adjacentT=tile.RemoveWall(angle);
										EditorUtility.SetDirty(tile);
										if(adjacentT!=null) EditorUtility.SetDirty(adjacentT);
									//~ }
								}
								else if(gridManager.type==_TileType.Square){
									Vector3 pos=tile.transform.position;
									Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
									//~ float angle=((int)(Utility.VectorToAngle(dir)/90)-1)*-45;
									float angle=((int)(Utility.VectorToAngle(dir)));
									
									Tile adjacentT=tile.RemoveWall(angle);
									EditorUtility.SetDirty(tile);
									if(adjacentT!=null) EditorUtility.SetDirty(adjacentT);
								}
							}
						}
						/*
						if(current.button == 1){
							if(stateAlt==0){
								tile.SetToDefault();
								if(tile.obstacle!=null) DestroyImmediate(tile.obstacle.gameObject);
							}
							else if(stateAlt==1){
								tile.SetToWalkable();
								tile.openForPlacement=true;
								if(tile.obstacle!=null) DestroyImmediate(tile.obstacle.gameObject);
							}
							else if(stateAlt==2){
								if(tile.unit!=null) return;
								if(tile.collectible!=null) DestroyImmediate(tile.collectible.gameObject);
								tile.SetToUnwalkable(visibleUnwalkable);
								
								//~ GameObject obsObj=null;
								//~ if(unwalkableType==1){
									//~ obsObj=(GameObject)PrefabUtility.InstantiatePrefab(obstacleObjH);
								//~ }
								//~ else if(unwalkableType==2){
									//~ obsObj=(GameObject)PrefabUtility.InstantiatePrefab(obstacleObjF);
								//~ }
								
								if(obstacleType>1){
									if(tile.obstacle!=null) DestroyImmediate(tile.obstacle.gameObject);
								
									GameObject obsObj=(GameObject)PrefabUtility.InstantiatePrefab(obstacleList[obstacleType].gameObject);
									if(tile.obstacle!=null) DestroyImmediate(tile.obstacle.gameObject);
									obsObj.transform.localScale*=gridManager.gridSize;
									obsObj.transform.parent=tile.transform;
									obsObj.transform.localPosition=Vector3.zero;
									obsObj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 6)*60, 0);
									obsObj.GetComponent<Collider>().enabled=false;
									Obstacle obs=obsObj.GetComponent<Obstacle>();
									tile.obstacle=obs;
									obs.occupiedTile=tile;
									tile.SetToUnwalkable(false);
								}
							}
						}
						*/
					}
					else if(tileMode==1){
						if(current.button == 0) tile.HPGainModifier=HPGainModifier;
						else if(current.button == 1) tile.HPGainModifier=0;
					}
					else if(tileMode==2){
						if(current.button == 0) tile.APGainModifier=APGainModifier;
						else if(current.button == 1) tile.APGainModifier=0;
					}
					else if(tileMode==3){
						if(current.button == 0) tile.damageModifier=damageModifier;
						else if(current.button == 1) tile.damageModifier=0;
					}
					else if(tileMode==4){
						if(current.button == 0) tile.attRangeModifier=attRangeModifier;
						else if(current.button == 1) tile.attRangeModifier=0;
					}
					else if(tileMode==5){
						if(current.button == 0) tile.attackModifier=attackModifier;
						else if(current.button == 1) tile.attackModifier=0;
					}
					else if(tileMode==6){
						if(current.button == 0) tile.defendModifier=defendModifier;
						else if(current.button == 1) tile.defendModifier=0;
					}
					else if(tileMode==7){
						if(current.button == 0) tile.criticalModifier=criticalModifier;
						else if(current.button == 1) tile.criticalModifier=0;
					}
					else if(tileMode==8){
						if(current.button == 0) tile.critDefModifier=critDefModifier;
						else if(current.button == 1) tile.critDefModifier=0;
					}
					
				}
				//unit
				else if(paintMode==1){
					if(current.button == 1){
						if(tile.unit!=null){
							DestroyImmediate(tile.unit.gameObject);
						}
					}
					else if(current.button == 0){
						if(!tile.walkable) return;
						
						if(tile.collectible!=null){
							DestroyImmediate(tile.collectible.gameObject);
						}
						if(tile.unit!=null){
							allUnits.Remove(tile.unit);
							DestroyImmediate(tile.unit.gameObject);
						}
						
						GameObject obj=(GameObject)PrefabUtility.InstantiatePrefab(unitList[unitID].gameObject);
						
						//obj.transform.parent=tile.transform;
						//obj.transform.localPosition=Vector3.zero;
						
						obj.transform.position=tile.transform.position;
						
						//obj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 6)*60, 0);
						UnitTB unit=obj.GetComponent<UnitTB>();
						if(!useDefaultFactionID) unit.factionID=unitFactionID;
						tile.unit=unit;
						allUnits.Add(unit);
						
						//put it under TBTK/Units, if that exist
						if(tile.transform.root.gameObject.name=="TBTK"){
							foreach(Transform child in tile.transform.root){
								if(child.gameObject.name=="Units"){
									obj.transform.parent=child;
									break;
								}
							}
						}
						
						if(gridManager.type==_TileType.Hex){
							if(rotationOption==-2){
								Vector3 pos=tile.transform.position;
								Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
								float angle=((int)(Utility.VectorToAngle(dir)/60)-1)*-60;
								obj.transform.rotation=Quaternion.Euler(0, angle, 0);
							}
							else if(rotationOption<0) obj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 6)*60, 0);
							else obj.transform.rotation=Quaternion.Euler(0, rotationOption*60, 0);
						}
						else if(gridManager.type==_TileType.Square){
							if(rotationOption==-2){
								Vector3 pos=tile.transform.position;
								Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
								float angle=((int)(Utility.VectorToAngle(dir)/90)-1)*-90;
								obj.transform.rotation=Quaternion.Euler(0, angle, 0);
							}
							else if(rotationOption==-1) obj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 4)*90, 0);
							else obj.transform.rotation=Quaternion.Euler(0, rotationOption*90, 0);
						}
					}
				}
				//collectible
				else if(paintMode==2){
					if(current.button == 1){
						if(tile.collectible!=null){
							DestroyImmediate(tile.collectible.gameObject);
						}
					}
					else if(current.button == 0){
						if(!tile.walkable) return;
						
						if(tile.collectible!=null){
							DestroyImmediate(tile.collectible.gameObject);
						}
						if(tile.unit!=null){
							allUnits.Remove(tile.unit);
							DestroyImmediate(tile.unit.gameObject);
						}
						
						GameObject obj=(GameObject)PrefabUtility.InstantiatePrefab(collectibleList[collectibleID].gameObject);
						
						//obj.transform.parent=tile.transform;
						//obj.transform.localPosition=Vector3.zero;
						
						obj.transform.position=tile.transform.position;
						
						//obj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 6)*60, 0);
						CollectibleTB c=obj.GetComponent<CollectibleTB>();
						tile.collectible=c;
						c.occupiedTile=tile;
						
						if(gridManager.type==_TileType.Hex){
							if(rotationOption==-2){
								Vector3 pos=tile.transform.position;
								Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
								float angle=((int)(Utility.VectorToAngle(dir)/60)-1)*-60;
								obj.transform.rotation=Quaternion.Euler(0, angle, 0);
							}
							else if(rotationOption<0) obj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 6)*60, 0);
							else obj.transform.rotation=Quaternion.Euler(0, rotationOption*60, 0);
						}
						else if(gridManager.type==_TileType.Square){
							if(rotationOption==-2){
								Vector3 pos=tile.transform.position;
								Vector2 dir=new Vector2(hit.point.x-pos.x, hit.point.z-pos.z);
								float angle=((int)(Utility.VectorToAngle(dir)/90)-1)*-90;
								obj.transform.rotation=Quaternion.Euler(0, angle, 0);
							}
							else if(rotationOption==-1) obj.transform.rotation=Quaternion.Euler(0, Random.Range(0, 4)*90, 0);
							else obj.transform.rotation=Quaternion.Euler(0, rotationOption*90, 0);
						}
					}
				}
				
				EditorUtility.SetDirty(tile);
			}
		}
	}
	
	
	
	
	
	
	
	
	
	
	
	
	/*
	//to add and remove wall, not in used
	//existed in tile.cs
	public void RemoveNeighbour(Tile srcT, Tile tile){
		if(srcT==null || tile==null) return;
		if(srcT.neighbours.Contains(tile)){
			srcT.neighbours.Remove(tile);
			srcT.disconnectedNeighbour.Add(tile);
		}
		if(tile.neighbours.Contains(srcT)){
			tile.neighbours.Remove(srcT);
			tile.disconnectedNeighbour.Add(srcT);
		}
		else{
			Debug.Log("is not neighbour?");
		}
	}
	public void AddNeighbour(Tile srcT, Tile tile){
		if(srcT==null || tile==null) return;
		if(!srcT.neighbours.Contains(tile)){
			srcT.neighbours.Add(tile);
			srcT.disconnectedNeighbour.Remove(tile);
		}
		if(!tile.neighbours.Contains(srcT)){
			tile.neighbours.Add(srcT);
			tile.disconnectedNeighbour.Remove(srcT);
		}
	}
	
	
	public void AddWall(Tile srcT, float angle){ AddWall(srcT, angle, null); }
	public void AddWall(Tile srcT, float angle, Obstacle wallPrefab){
		
		Tile tile=null;
		for(int i=0; i<srcT.neighbours.Count; i++){
			Vector2 dir=new Vector2(srcT.neighbours[i].pos.x-srcT.pos.x, srcT.neighbours[i].pos.z-srcT.pos.z);
			
			if(srcT.type==_TileType.Hex){
				float angleN=((int)(Utility.VectorToAngle(dir)));
				if(Mathf.Abs(Mathf.DeltaAngle(angle, angleN))<30){
					tile=srcT.neighbours[i];
					break;
				}
			}
			else if(srcT.type==_TileType.Square){
				float angleN=((int)(Utility.VectorToAngle(dir)));
				if(Mathf.Abs(Mathf.DeltaAngle(angle, angleN))<45){
					tile=srcT.neighbours[i];
					break;
				}
			}
		}
		
		for(int i=0; i<srcT.walls.Count; i++){
			if(srcT.walls[i].Contains(tile, srcT)){
				Debug.Log("wall existed   "+tile+"    "+srcT);
				return;
			}
		}
		
		if(tile==null){
			Debug.Log("no neighbour    "+srcT);
			return;
		}
		
		Vector2 dirV=new Vector2(tile.pos.x-srcT.pos.x, tile.pos.z-srcT.pos.z);
		float angleW=Utility.VectorToAngle(dirV);
		Transform wallT=null;
		
		if(wallPrefab!=null){
			wallT=(Transform)Instantiate(wallPrefab.transform);
			
			Vector3 direction=tile.pos-srcT.pos;
			
			if(srcT.type==_TileType.Hex){
				float gridSize=srcT.transform.localScale.x/1.1628f;
				wallT.position=srcT.pos+(direction.normalized)*gridSize*0.5f;//+new Vector3(0, gridSize*0.3f, 0);
				wallT.localScale=new Vector3(gridSize, gridSize, gridSize)*1.2f;
			}
			else if(srcT.type==_TileType.Square){
				float gridSize=srcT.transform.localScale.x;
				wallT.position=srcT.pos+(direction.normalized)*gridSize*0.5f;//+new Vector3(0, gridSize*0.3f, 0);
				wallT.localScale=new Vector3(gridSize, gridSize, gridSize);
			}
			
			float rotOffset=90;
			wallT.rotation=Quaternion.Euler(0, -(angleW+rotOffset), 0);
			
			wallT.parent=srcT.transform;
		}
		
		srcT.walls.Add(new Wall(wallT, tile, srcT, angleW));
		tile.walls.Add(new Wall(wallT, tile, srcT, angleW+180));
		RemoveNeighbour(srcT, tile);
		
		
		
		//Debug.Log("Disconnected   "+this+"  "+tile+"    "+(angleW+rotOffset));
	}
	public void RemoveWall(Tile srcT, float angle){
		
		//if(angle<0) angle+=360;
		
		Debug.Log("remove "+angle);
		Tile tile=null;
		
		Wall wall=null;
		for(int i=0; i<srcT.walls.Count; i++){
			if(srcT.type==_TileType.Hex){
				if(Mathf.Abs(Mathf.DeltaAngle(angle, srcT.walls[i].angle))<30){
					wall=srcT.walls[i];
					srcT.walls.RemoveAt(i);
					break;
				}
			}
			else if(srcT.type==_TileType.Square){
				if(Mathf.Abs(Mathf.DeltaAngle(angle, srcT.walls[i].angle))<45){
					wall=srcT.walls[i];
					srcT.walls.RemoveAt(i);
					break;
				}
			}
		}
		
		if(wall==null) return;
		
		Tile newNeighbour=null;
		if(wall.tile1==srcT) newNeighbour=wall.tile2;
		else if(wall.tile2==srcT) newNeighbour=wall.tile1;
		AddNeighbour(srcT, newNeighbour);
		
		//newNeighbour.walls.Remove(wall);
		Debug.Log("Connected   "+this+"  "+tile);
		
		for(int i=0; i<newNeighbour.walls.Count; i++){
			if(newNeighbour.walls[i].Contains(srcT, newNeighbour)){
				newNeighbour.walls.RemoveAt(i);
				break;
			}
		}
		
		if(wall.wallObj!=null) DestroyImmediate(wall.wallObj.gameObject);
		
	}
	*/
	
	
	
	
	
	
	
	
	
	
	
	
	public float GetTileSize(){
		return gridManager.gridSize*gridManager.gridToTileSizeRatio;
	}
}