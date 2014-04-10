using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

[RequireComponent (typeof (GridPainter))]
public class GridManager : MonoBehaviour{

	public delegate void HoverTileEnterHandler(Tile tile); 
	public static event HoverTileEnterHandler onHoverTileEnterE;
	
	public delegate void HoverTileExitHandler(); 
	public static event HoverTileExitHandler onHoverTileExitE;
	
	public _TileType type;
	
	public bool randomizeGridUponStart=false;
	public bool randomizeUnitUponStart=false;
	public float gridSize=2.5f;
	
	public float width=7;
	public float length=11;
	public float baseHeight=0;
	public float maxHeight=50;
	
	public float gridToTileSizeRatio=1f;
	
	public float unwalkableRate=0.0f;
	public bool useObstacle=false;
	[HideInInspector] public bool enableInvUnwalkable=false;	//invisible unwalkable
	[HideInInspector] public bool enableVUnwalkable=true;		//visible unwalkable
	[HideInInspector] public List<Obstacle> obstaclePrefabList=new List<Obstacle>();
	
	public bool addColletible=false;
	public int minCollectibleCount=1;
	public int maxCollectibleCount=3;
	[HideInInspector] public List<CollectibleTB> collectiblePrefabList=new List<CollectibleTB>();
	
	[HideInInspector] public Transform indicatorSelect;
	[HideInInspector] public Transform indicatorCursor;
	[HideInInspector] public Transform indicatorHostile;
	private Transform[] indicatorH;
	
	public static List<Tile> walkableList=new List<Tile>();
	public static List<Tile> hostileList=new List<Tile>();
	//public static List<Tile> friendlyList=new List<Tile>();		//not in used
	
	//public GameObject tilePrefab;
	public GameObject hexTilePrefab;
	public GameObject squareTilePrefab;
	[HideInInspector] public List<Faction> factionList=new List<Faction>();
	[HideInInspector] private List<Tile> allTiles=new List<Tile>();
	[HideInInspector] public List<Tile> allPlaceableTiles=new List<Tile>();
	
	public List<Transform> coverOverlayFull=new List<Transform>();
	public List<Transform> coverOverlayHalf=new List<Transform>();
	
	//public Rect playerPlacementArea;
	public List<Rect> playerPlacementAreas=new List<Rect>();
	
	public static GridManager instance;
	
	
	public void GetAllTile(){
		Tile[] allTilesInScene=(Tile[])FindObjectsOfType(typeof(Tile));
		allTiles=new List<Tile>();
		foreach(Tile tile in allTilesInScene){
			allTiles.Add(tile);
		}
	}
	
	void Awake(){
		instance=this;
		
		walkableList=new List<Tile>();
		hostileList=new List<Tile>();
		
		GetAllTile();
		
		if(randomizeGridUponStart) GenerateGrid(true);
		else{
			if(randomizeUnitUponStart) GenerateUnit();
			InitPlacementTile(false);
		}
	}
	
	void SetClear(){
		foreach(Tile tile in allTiles){
			tile.SetState(_TileState.Default);
		}
	}
	
	// Use this for initialization
	void Start() {
		InitIndicator();
		
		//get all the placeable tile
		//if(GameControlTB.EnableUnitPlacement()){
			instance.allPlaceableTiles=new List<Tile>();
			for(int i=0; i<instance.allTiles.Count; i++){
				Tile tile=instance.allTiles[i];
				if(tile.openForPlacement && tile.unit==null) instance.allPlaceableTiles.Add(tile);
			}
		//}
		
		//create cover overlay for if cover system is enabled
		if(GameControlTB.EnableCover()){
			Transform half=Resources.Load("ScenePrefab/OverlayShieldHalf", typeof(Transform)) as Transform;
			Transform full=Resources.Load("ScenePrefab/OverlayShieldFull", typeof(Transform)) as Transform;
			
			float scale=gridSize*0.06f;
			
			int limit=4;		int angle=90;
			if(type==_TileType.Hex){
				limit=6;		angle=60;
			}
			
			for(int i=0; i<limit; i++){
				coverOverlayFull.Add((Transform)Instantiate(full));
				coverOverlayHalf.Add((Transform)Instantiate(half));
				
				coverOverlayFull[i].position=new Vector3(0, 50000, 0);
				coverOverlayHalf[i].position=new Vector3(0, 50000, 0);
				
				coverOverlayFull[i].rotation=Quaternion.Euler(90, (i-1)*-angle, 0);
				coverOverlayHalf[i].rotation=Quaternion.Euler(90, (i-1)*-angle, 0);
				
				coverOverlayFull[i].localScale=new Vector3(scale, 1, scale);
				coverOverlayHalf[i].localScale=new Vector3(scale, 1, scale);
			}
		}
	}
	
	//init all the indicators
	void InitIndicator(){
		float size=gridToTileSizeRatio;
		if(type==_TileType.Square) size=gridToTileSizeRatio*0.85f;
		
		if(indicatorSelect==null){
			//load from resource if no indicator is assigned
			if(type==_TileType.Square) indicatorSelect=Resources.Load("ScenePrefab/SquareIndicators/SqIndicatorSelect", typeof(Transform)) as Transform;
			else if(type==_TileType.Hex) indicatorSelect=Resources.Load("ScenePrefab/HexIndicators/HexIndicatorSelect", typeof(Transform)) as Transform;
			
			if(indicatorSelect!=null){
				//if load success, instantiate a new instance into the scene
				indicatorSelect=(Transform)Instantiate(indicatorSelect);
				indicatorSelect.position=new Vector3(0, 9999, 0);
				//adjust the indicator size to fit the gridSize
				ParticleSystem ps=indicatorSelect.gameObject.GetComponent<ParticleSystem>();
				if(ps!=null) ps.startSize*=gridSize*size;
			}
		}
		
		if(indicatorCursor==null){
			if(type==_TileType.Square) indicatorCursor=Resources.Load("ScenePrefab/SquareIndicators/SqIndicatorCursor", typeof(Transform)) as Transform;
			else if(type==_TileType.Hex) indicatorCursor=Resources.Load("ScenePrefab/HexIndicators/HexIndicatorCursor", typeof(Transform)) as Transform;
			
			if(indicatorCursor!=null){
				indicatorCursor=(Transform)Instantiate(indicatorCursor);
				indicatorCursor.position=new Vector3(0, 9999, 0);
				ParticleSystem ps=indicatorCursor.gameObject.GetComponent<ParticleSystem>();
				if(ps!=null) ps.startSize*=gridSize*size;
			}
		}
		
		bool scalePS=false;
		if(indicatorHostile==null){
			if(type==_TileType.Square) indicatorHostile=Resources.Load("ScenePrefab/SquareIndicators/SqIndicatorHostile", typeof(Transform)) as Transform;
			else if(type==_TileType.Hex) indicatorHostile=Resources.Load("ScenePrefab/HexIndicators/HexIndicatorHostile", typeof(Transform)) as Transform;
			
			if(indicatorHostile!=null) indicatorHostile=(Transform)Instantiate(indicatorHostile);
			scalePS=true;
		}
		
		if(indicatorHostile!=null){
			indicatorHostile.position=new Vector3(0, 9999, 0);
			
			int totalUnitCount=UnitControl.GetAllUnitCount();
			if(totalUnitCount>0){
				indicatorH=new Transform[totalUnitCount+2];
				indicatorH[0]=indicatorHostile;
				for(int i=1; i<totalUnitCount+2; i++){
					indicatorH[i]=(Transform)Instantiate(indicatorHostile);
					indicatorH[i].parent=indicatorH[0].parent;
				}
				if(scalePS){
					foreach(Transform ind in indicatorH){
						ParticleSystem ps=ind.gameObject.GetComponent<ParticleSystem>();
						if(ps!=null) ps.startSize*=gridSize*size;
					}
				}
			}
		}
	}
	
	
	
	
	void GenerateSquareGrid(){
		if(squareTilePrefab==null) squareTilePrefab=Resources.Load("ScenePrefab/SquareTile", typeof(GameObject)) as GameObject;
			
		int counter=0;
		float highestY=-Mathf.Infinity;		float lowestY=Mathf.Infinity;
		float highestX=-Mathf.Infinity;		float lowestX=Mathf.Infinity;
		float gridSizeFactor=gridToTileSizeRatio*gridSize;
		
		for(int x=0; x<width; x++){
			for(int y=0; y<length; y++){
				float posX=x*gridSizeFactor;
				float posY=y*gridSizeFactor;
				
				if(posY>highestY) highestY=posY;				if(posY<lowestY) lowestY=posY;
				if(posX>highestX) highestX=posX;				if(posX<lowestX) lowestX=posX;
				
				Vector3 pos=new Vector3(posX, baseHeight, posY);
				GameObject obj=(GameObject)Instantiate(squareTilePrefab);
				obj.transform.parent=transform;
				obj.transform.localPosition=pos;
				obj.transform.localRotation=Quaternion.Euler(-90, 0, 0);
				#if UNITY_Editor	
					obj.name="Tile"+counter.ToString();
				#endif
				obj.transform.localScale*=gridSize;
				
				Tile tile=obj.GetComponent<Tile>();
				
				allTiles.Add(tile);
				
				counter+=1;
			}
		}
		
		float disY=(Mathf.Abs(highestY)-Mathf.Abs(lowestY))/2;
		float disX=(Mathf.Abs(highestX)-Mathf.Abs(lowestX))/2;
		for(int i=0; i<allTiles.Count; i++){
			Tile tile=allTiles[i];
			Transform tileT=tile.transform;
			tileT.position+=new Vector3(-disX, 0, -disY);
			tile.pos=tileT.position;
		}
	}
	void GenerateHexGrid(){
		if(hexTilePrefab==null) hexTilePrefab=Resources.Load("ScenePrefab/HexTile", typeof(GameObject)) as GameObject;
		
		float ratio=0.745f/0.86f;   //0.86628f
		float hR=gridToTileSizeRatio*gridSize;
		float wR=hR*ratio;
		
		int counter=0;
		float highestY=-Mathf.Infinity;		float lowestY=Mathf.Infinity;
		float highestX=-Mathf.Infinity;		float lowestX=Mathf.Infinity;
		
		for(int x=0; x<width; x++){
			float offset=0.5f*(x%2);
			
			int limit=0;
			if(x%2==1) limit=(int)(length/2);
			else limit=(int)(length/2+length%2);
			
			for(int y=0; y<limit; y++){
				float posX=x*wR;//-widthOffset;
				float posY=y*hR+hR*offset;//-lengthOffset;
				
				if(posY>highestY) highestY=posY;				if(posY<lowestY) lowestY=posY;
				if(posX>highestX) highestX=posX;				if(posX<lowestX) lowestX=posX;
				
				Vector3 pos=new Vector3(posX, baseHeight, posY);
				GameObject obj=(GameObject)Instantiate(hexTilePrefab);
				obj.transform.parent=transform;
				obj.transform.localPosition=pos;
				obj.transform.localRotation=Quaternion.Euler(-90, 0, 0);
				#if UNITY_Editor	
					obj.name="Tile"+counter.ToString();
				#endif
				obj.transform.localScale*=gridSize*1.1628f;
				
				Tile hTile=obj.GetComponent<Tile>();
				
				allTiles.Add(hTile);
				
				counter+=1;
			}
		}
		
		float disY=(Mathf.Abs(highestY)-Mathf.Abs(lowestY))/2;
		float disX=(Mathf.Abs(highestX)-Mathf.Abs(lowestX))/2;
		foreach(Tile hTile in allTiles){
			Transform tileT=hTile.transform;
			tileT.position+=new Vector3(-disX, 0, -disY);
			hTile.pos=tileT.position;
		}
	}
	
	
	
	//function call to generate new grid
	public void GenerateGrid(bool generateNew){
		/*
		//for benchmarking
		DateTime timeS;
		DateTime timeE;
		TimeSpan timeSpan;
		timeS=System.DateTime.Now;
		*/
		
		int count=0;
		
		//generate new tile
		if(generateNew){
			//clear previous tile
			for(int i=0; i<allTiles.Count; i++){
				if(allTiles[i]!=null){
					if(allTiles[i].unit!=null) DestroyImmediate(allTiles[i].unit.gameObject);
					else if(allTiles[i].obstacle!=null) DestroyImmediate(allTiles[i].obstacle.gameObject);
					else if(allTiles[i].collectible!=null) DestroyImmediate(allTiles[i].collectible.gameObject);
					DestroyImmediate(allTiles[i].gameObject);
				}
			}
			allTiles=new List<Tile>();
			
			
			if(type==_TileType.Square) GenerateSquareGrid();
			else if(type==_TileType.Hex) GenerateHexGrid();
			
			/*
			mould the grid to the height of the terrain
			foreach(Tile hTile in hgm.allTiles){
				Vector3 pos=new Vector3(hTile.pos.x, hTile.pos.y+hgm.maxHeight, hTile.pos.z);
				RaycastHit hit;
				
				bool flag=Physics.Raycast(pos, Vector3.down, out hit, hgm.maxHeight+10);
				
				if(flag) pos.y=hit.point.y+0.1f;
				else pos.y=hgm.baseHeight;
				
				hTile.pos=pos;
				hTile.transform.position=pos;
			}
			*/
			
			
			int blockTile=(int)(allTiles.Count*unwalkableRate);
		
			//set unwalkable tile
			if(blockTile>0){
				count=0;
				while(count<blockTile){
					int rand=UnityEngine.Random.Range(0, allTiles.Count);
					while(!allTiles[rand].walkable || allTiles[rand].unit!=null) rand=UnityEngine.Random.Range(0, allTiles.Count);
					
					if(!useObstacle){
						allTiles[rand].walkable=false;
						allTiles[rand].Start();
					}
					else{
						int offset=0;
						if(enableInvUnwalkable) offset+=1;
						if(enableVUnwalkable) offset+=1;
						
						int randT=UnityEngine.Random.Range(0, obstaclePrefabList.Count+offset);
						
						if(randT<offset){
							if(enableInvUnwalkable && randT==0){
								allTiles[rand].SetToUnwalkable(false);
							}
							else{
								allTiles[rand].SetToUnwalkable(true);
							}
						}
						else{
							GameObject obsObj=(GameObject)Instantiate(obstaclePrefabList[randT-offset].gameObject);
							obsObj.transform.localScale*=gridSize;
							obsObj.transform.parent=allTiles[rand].transform;
							obsObj.transform.localPosition=Vector3.zero;
							obsObj.GetComponent<Collider>().enabled=false;
							Obstacle obs=obsObj.GetComponent<Obstacle>();
							allTiles[rand].obstacle=obs;
							obs.occupiedTile=allTiles[rand];
							allTiles[rand].SetToUnwalkable(false);
						}							
					}
					
					count+=1;
				}
			}
			
			for(int i=0; i<allTiles.Count; i++) allTiles[i].gameObject.layer=8;
		
			//set neighbour
			for(int i=0; i<allTiles.Count; i++){
				Tile hT=allTiles[i];
				Vector3 pos=hT.transform.position;
				Collider[] cols=Physics.OverlapSphere(pos, gridSize*gridToTileSizeRatio*0.6f);
				List<Tile> neighbour=new List<Tile>();
				foreach(Collider col in cols){
					Tile hTile=col.gameObject.GetComponent<Tile>();
					if(hTile!=null && hT!=hTile){
						neighbour.Add(hTile);
					}
				}
				hT.SetNeighbours(neighbour);
			}
			
			
			if(addColletible && collectiblePrefabList.Count>0){
				int ccount=UnityEngine.Random.Range(minCollectibleCount, maxCollectibleCount);
				
				if(ccount>0){
				
					List<Tile> emptyTiles=new List<Tile>();
					for(int i=0; i<allTiles.Count; i++){
						if(allTiles[i].walkable) emptyTiles.Add(allTiles[i]);
					}
					for(int i=0; i<ccount; i++){
						int rand=UnityEngine.Random.Range(0, emptyTiles.Count);
						
						int randID=UnityEngine.Random.Range(0, collectiblePrefabList.Count);
						InsertCollectible(collectiblePrefabList[randID], emptyTiles[rand]);
						//~ GameObject colObj=(GameObject)Instantiate(collectiblePrefabList[randID].gameObject);
						//~ colObj.transform.parent=emptyTiles[rand].transform;
						//~ colObj.transform.localPosition=Vector3.zero;
						//~ colObj.transform.rotation=Quaternion.Euler(0, UnityEngine.Random.Range(0, 6)*60, 0);
						
						//~ CollectibleTB col=colObj.GetComponent<CollectibleTB>();
						//~ emptyTiles[rand].collectible=col;
						//~ col.occupiedTile=emptyTiles[rand];
						
						emptyTiles.RemoveAt(rand);
						if(emptyTiles.Count==0) break;
					}
					
				}
			}
			
			
			//place unit
			GenerateUnit();
		}
		
		InitPlacementTile(true);
		
		//set unit placement tile for single area only, obsolte and not in use
		/*
		allPlaceableTiles=new List<Tile>();
		List<Tile> playerTiles=GetTileWithinRect(playerPlacementArea);
		for(int i=0; i<playerTiles.Count; i++){
			Tile hT=playerTiles[i];
			if(hT.walkable && hT.unit==null){
				hT.openForPlacement=true;
				hT.placementID=0;
				hT.SetState(_TileState.Walkable);
				allPlaceableTiles.Add(hT);
			}
		}
		*/
		
		
		/*
		//for benchmarking
		if(generateNew){
			timeE=System.DateTime.Now;
			timeSpan=timeE-timeS;
			Debug.Log( "Time:"+timeSpan.TotalMilliseconds);
		}
		*/
		
	}
	
	
	
	//initialise all the tile for unit placement, setup the stat according from the playerPlacementAreas
	void InitPlacementTile(bool flag){
		GameControlTB gc=(GameControlTB)FindObjectOfType(typeof(GameControlTB));
		for(int n=0; n<playerPlacementAreas.Count; n++){
			allPlaceableTiles=new List<Tile>();
			List<Tile> playerTiles=GetTileWithinRect(playerPlacementAreas[n]);
			for(int i=0; i<playerTiles.Count; i++){
				Tile hT=playerTiles[i];
				if(hT.walkable && hT.unit==null){
					hT.openForPlacement=true;
					hT.placementID=gc.playerFactionID[n];
					if(flag) hT.SetState(_TileState.Walkable);
					allPlaceableTiles.Add(hT);
				}
			}
		}
	}
	
	
	//called when a unit placement phase is initiated for a faction
	void OnNewPlacement(PlayerUnits pUnits){
		allPlaceableTiles=new List<Tile>();
		for(int i=0; i<allTiles.Count; i++){
			Tile hT=allTiles[i];
			if(hT.openForPlacement && hT.placementID==pUnits.factionID){
				hT.openForPlacement=true;
				hT.placementID=pUnits.factionID;
				hT.SetState(_TileState.Walkable);
				allPlaceableTiles.Add(hT);
			}
			else hT.SetState(_TileState.Default);
		}
	}
	
	public static void InsertCollectible(CollectibleTB collectible, Tile tile){
		GameObject colObj=(GameObject)Instantiate(collectible.gameObject);
		colObj.transform.parent=tile.transform;
		colObj.transform.localPosition=Vector3.zero;
		colObj.transform.rotation=Quaternion.Euler(0, UnityEngine.Random.Range(0, 6)*60, 0);
		
		CollectibleTB col=colObj.GetComponent<CollectibleTB>();
		tile.collectible=col;
		col.occupiedTile=tile;
	}
	
	
	public void GenerateUnit(){
		//clear all the unit on the grid
		for(int i=0; i<allTiles.Count; i++){
			if(allTiles[i].unit!=null){
				DestroyImmediate(allTiles[i].unit.gameObject);
				allTiles[i].unit=null;
			}
		}
		
		//first get the TBTK/Units
		Transform unitParent=null;
		foreach(Transform child in transform.parent){
			if(child.gameObject.name=="Units"){
				unitParent=child;
				break;
			}
		}
		
		int count=0;
		
		foreach(Faction fac in factionList){
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
						List<Tile> tileList=GetTileWithinRect(sInfo.area);
						
						for(int i=0; i<tileList.Count; i++){
							Tile tile=tileList[i];
							if(tile.unit!=null || !tile.walkable || tile.collectible!=null){
								tileList.RemoveAt(i);
								i-=1;
							}
						}
						
						if(sInfo.spawnQuota==_SpawnQuota.UnitBased){
							int facUnitCount=Mathf.Min(sInfo.unitCount, tileList.Count);
							count=0;
							while(count<facUnitCount){
								int rand=UnityEngine.Random.Range(0, tileList.Count);
								Tile hTile=tileList[rand];
								
								int randUnit=UnityEngine.Random.Range(0, sInfo.unitPrefabs.Length);
								GameObject unitObj=(GameObject)Instantiate(sInfo.unitPrefabs[randUnit].gameObject);
								
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
								
								tileList.RemoveAt(rand);
								if(tileList.Count==0) break;
								
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
								int rand=UnityEngine.Random.Range(0, tileList.Count);
								Tile hTile=tileList[rand];
								
								int randUnit=UnityEngine.Random.Range(0, sInfo.unitPrefabs.Length);
								GameObject unitObj=(GameObject)Instantiate(sInfo.unitPrefabs[randUnit].gameObject);
								
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
								
								tileList.RemoveAt(rand);
								if(tileList.Count==0) break;
								
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
	
	
	void OnEnable(){
		GameControlTB.onBattleStartE += OnBattleStart;
		GameControlTB.onNextTurnE += OnNextTurn;
		
		UnitControl.onNewPlacementE += OnNewPlacement;
	}
	
	void OnDisable(){
		GameControlTB.onBattleStartE -= OnBattleStart;
		GameControlTB.onNextTurnE -= OnNextTurn;
		
		UnitControl.onNewPlacementE -= OnNewPlacement;
	}
	
	
	
	void OnBattleStart(){
		//battle start, change all tile available for placement visual to default
		for(int i=0; i<allTiles.Count; i++){
			Tile hT=allTiles[i];
			if(hT.openForPlacement){
				hT.openForPlacement=false;
				hT.SetState(_TileState.Default);
			}
		}
		
		allPlaceableTiles=new List<Tile>();
	}
	
	void OnNextTurn(){
		//on new turn, clear any current selected tile
		if(selectedTile!=null){
			Deselect();
		}
	}
	
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	
	//call to set the passed tile as current selected tile
	//this will clear any current selected tile
	private Tile selectedTile;
	public static void Select(Tile ht){
		//alternate select phase where player is selecting a target tile for unit ability
		if(targetTileSelectMode){
			if(Distance(ht, instance.selectedTile)<=currentUAB.range){
				//if no aoe, make sure the select tile is valid
				if(currentUAB.targetArea==_TargetArea.Default && currentUAB.aoeRange==0){
					if(currentUAB.targetType==_AbilityTargetType.AllUnits){
						if(ht.unit==null) GameControlTB.DisplayMessage("No target!");
						else{
							UnitControl.selectedUnit.UnitAbilityTargetSelected(ht);
							ExitTargetTileSelectMode();
						}
					}
					else if(currentUAB.targetType==_AbilityTargetType.Friendly){
						int currentFacID=UnitControl.selectedUnit.factionID;
						if(ht.unit==null) GameControlTB.DisplayMessage("No target!");
						else if(ht.unit.factionID!=currentFacID) GameControlTB.DisplayMessage("Target is hostile!");
						else{
							UnitControl.selectedUnit.UnitAbilityTargetSelected(ht);
							ExitTargetTileSelectMode();
						}
					}
					else if(currentUAB.targetType==_AbilityTargetType.Hostile){
						int currentFacID=UnitControl.selectedUnit.factionID;
						if(ht.unit==null) GameControlTB.DisplayMessage("No target!");
						else if(ht.unit.factionID==currentFacID) GameControlTB.DisplayMessage("Target is friendly!");
						else{
							UnitControl.selectedUnit.UnitAbilityTargetSelected(ht);
							ExitTargetTileSelectMode();
						}
					}
					else if(currentUAB.targetType==_AbilityTargetType.EmptyTile){
						if(ht.unit!=null) GameControlTB.DisplayMessage("Tile is occupied!");
						else{
							UnitControl.selectedUnit.UnitAbilityTargetSelected(ht);
							ExitTargetTileSelectMode();
						}
					}
					else{
						UnitControl.selectedUnit.UnitAbilityTargetSelected(ht);
						ExitTargetTileSelectMode();
					}
				}
				//otherwise just pass the whole tile group
				else{
					UnitControl.selectedUnit.SetAbilityTargetTile(ht);
					UnitControl.selectedUnit.UnitAbilityTargetSelected(currentAOETileGroup);
					ExitTargetTileSelectMode();
				}
			}
			else{
				GameControlTB.DisplayMessage("Out Of Range");
			}
			
			return;
		}
		
		//if tile doesnt contain any unit
		if(ht==null || ht.unit==null){
			return;
		}
		
		//if unit doesnt belong to player
		if(!GameControlTB.IsPlayerFaction(ht.unit.factionID)) return;
		//if selecting another player's unit (factionID will be different to the current selected unit)
//WARNING: This was commented out to prevent the game from crashing		if(ht.unit.factionID!=UnitControl.selectedUnit.factionID) return;
		//if(ht.unit.factionID!=GameControlTB.GetPlayerFactionID()) return;
		
		//deselect currently selected tile
		if(instance.selectedTile!=null) Deselect();
		
		//set tile as selected
		instance.selectedTile=ht;
		ht.SetState(_TileState.Selected);
		
		//set selected unit
		UnitTB unit =ht.unit;
		unit.Select();
		
		//set select indicator
		if(ht!=null) instance.indicatorSelect.position=ht.thisT.position;
		else instance.indicatorSelect.position=new Vector3(0, 99999, 0);
		
		
		//if unit is stunned stop here
		if(unit.IsStunned()) return;
		
		//set target and highlight hostile unit in range
		ResetHostileList(ht);
		
		//get all walkable tile and highlight them
		ResetWalkableList(ht);
		
		ResetCoverOverlay();
	}
	
	//method to deselect current selected tile
	public static void Deselect(){
		if(instance.selectedTile==null) return;
		
		ClearWalkableList();
		
		ClearHostileList();
		
		if(instance.selectedTile.unit!=null) instance.selectedTile.unit.Deselect();
		
		instance.selectedTile.SetState(_TileState.Default);
		instance.selectedTile=null;
		
		instance.indicatorSelect.position=new Vector3(0, 99999, 0);
		
		//make sure we are not in target select mode for unit ability anymore
		//targetTileSelectMode=false;
		//enableTargetTileAOE=false;
		//targetTileAOERange=1;
		OnHoverTargetTileExit();
		ExitTargetTileSelectMode();
	}
	
	//set hostile list to match current selected unit if there's any
	//otherwise clear the hostile list
	public static void ResetHostileList(Tile ht){
		UnitTB unit=ht.unit;
		
		if(unit==null){
			ClearHostileList();
			return;
		}
		
		if(GameControlTB.AttackAPCostRule()==_AttackAPCostRule.PerAttack && unit.AP<=0){
			ClearHostileList();
			return;
		}
		
		//if selected new unit belong to player's faction, show the change the tile status to match
		if(!unit.attacked && unit.attackDisabled<=0){
			//get all tile within unit attack range
			List<Tile> tempList=GetTilesWithinRange(ht, unit.GetAttackRangeMin(), unit.GetAttackRangeMax());
			foreach(Tile tile in tempList){
				//if the tile is occupied by a unit and it's from other faction
				if(tile.unit!=null && tile.unit.factionID!=unit.factionID){
					bool LOSFlag=true;
					if(GameControlTB.EnableFogOfWar()){
						int dist=GridManager.Distance(tile, ht);
						LOSFlag=IsInLOS(ht, tile) & dist<unit.GetUnitSight() & tile.unit.IsVisibleToPlayer();
					}
					
					if(LOSFlag){
						tile.SetState(_TileState.Hostile);
						tile.attackableToSelected=true;
						hostileList.Add(tile);
					}
				}
			}
			PlaceIndicatorH();
		}
	}
	
	//set walkable list to match current selected unit if there's any
	//otherwise clear the hostile list
	public static void ResetWalkableList(Tile ht){
		UnitTB unit=ht.unit;
		
		if(unit==null){
			ClearWalkableList();
			return;
		}
		
		if(unit.attacked && !GameControlTB.AllowMovementAfterAttack()) return;
		
		//if unit can move
		if(!unit.moved && unit.stun<=0 && unit.movementDisabled<=0){
			//match effective movement range to match movement rule, AP and default unit range
			int range=unit.GetUnitMoveRange();
			if(GameControlTB.MovementAPCostRule()==_MovementAPCostRule.PerTile) range=Mathf.Min(unit.AP, range) ;
			else if(GameControlTB.MovementAPCostRule()==_MovementAPCostRule.PerMove && unit.AP<1) range=0;
			
			if(range==0){
				ClearWalkableList();
				return;
			}
			
			//get all tile within the unit effective movement range
			walkableList=GridManager.GetWalkableTilesWithinRange(ht, range);
			for(int i=0; i<walkableList.Count; i++){
				Tile tile=walkableList[i];
				if(tile.unit==null){
					tile.walkableToSelected=true;
					tile.SetState(_TileState.Walkable);
				}
			}
		}
	}
	
	//function called to clear all the hostile tile list
	public static void ClearHostileList(){
		ClearAllIndicatorH();
		for(int i=0; i<hostileList.Count; i++){
			Tile tile=hostileList[i];
			tile.SetState(_TileState.Default);
			tile.attackableToSelected=false;
		}
		hostileList=new List<Tile>();
	}
	
	//function called to clear all the wakable tile list
	public static void ClearWalkableList(){
		for(int i=0; i<walkableList.Count; i++){
			Tile tile=walkableList[i];
			tile.SetState(_TileState.Default);
			tile.walkableToSelected=false;
		}
		walkableList=new List<Tile>();
	}
	
	
	//a list contain all the tile affected by the tile hovered in targetSelectmode
	private static List<Tile> currentAOETileGroup=new List<Tile>();
	//handle for alternate hover mode when the player is slecting a target tile for unit ability
	//call when the mouse enter the tile
	public static void OnHoverTargetTileSelect(Tile rootTile){
		if(GridManager.Distance(rootTile, uABScrTile)>currentUAB.range) return;
		
		currentAOETileGroup=new List<Tile>();
		
		if(currentUAB.targetArea==_TargetArea.Default){
			currentAOETileGroup=new List<Tile>();
			
			if(currentUAB.aoeRange>=1){
				currentAOETileGroup=GetAOETile(rootTile, currentUAB.aoeRange);
			}
			
			currentAOETileGroup.Add(rootTile);
		}
		else if(currentUAB.targetArea==_TargetArea.Line){
			currentAOETileGroup=GetTileInLine(uABScrTile, rootTile, currentUAB.range);
		}
		else if(currentUAB.targetArea==_TargetArea.Cone){
			if(instance.type==_TileType.Hex) currentAOETileGroup=GetTileInCone60(uABScrTile, rootTile, currentUAB.range);
			else if(instance.type==_TileType.Square) currentAOETileGroup=GetTileInLine(uABScrTile, rootTile, currentUAB.range);
		}
		
		
		//set the affected tile material to give visual feedback
		if(currentUAB.effectType==_EffectType.DirectPositive || currentUAB.effectType==_EffectType.Buff){
			foreach(Tile tile in currentAOETileGroup){
				tile.SetState(_TileState.Walkable);
			}
		}
		else if(currentUAB.effectType==_EffectType.DirectNegative || currentUAB.effectType==_EffectType.Debuff){
			foreach(Tile tile in currentAOETileGroup){
				tile.SetState(_TileState.Hostile);
			}
		}
	}
	//handle for alternate hover mode when the player is slecting a target tile for unit ability
	//call when the mouse exit the tile
	public static void OnHoverTargetTileExit(){
		foreach(Tile tile in currentAOETileGroup){
			if(tile!=null){
				//for target select mode with tile range indication
				if(tilesInAbilityRange.Contains(tile)) tile.SetState(_TileState.AbilityRange);
				else tile.SetState(_TileState.Default);
			}
		}
	}
	
	
	//called when mouse hover over a tile
	public static List<Tile> tempHostileTileList=new List<Tile>();	//hostile unit within range from the tile hovered
	public static void OnHoverEnter(Tile tile){
		instance.indicatorCursor.position=tile.thisT.position;
		
		//if user is currently selecting a target for unit ability, call the alternate hover mode
		if(targetTileSelectMode){
			OnHoverTargetTileSelect(tile);
			return;
		}
		
		//check if any unit hostile unit is within range from current pointed tile
		//this is to preview the potential target is the selected unit move into this tile
		UnitTB unit=UnitControl.selectedUnit;
		if(unit!=null && !unit.attacked && tile.walkableToSelected){
			//if(GameControlTB.IsPlayerFaction(unit.factionID)){
			//if(unit.IsControllable(GameControlTB.GetPlayerFactionID())){
			if(unit.IsControllable()){
				//get unit effective range if placed on the hovered tile
				int effRangeMin=Mathf.Max(unit.GetAttackRangeMin(), 0);
				int effRangeMax=Mathf.Max(unit.GetAttackRangeMax(), 0);
				if(unit.attackMode==_AttackMode.Melee) effRangeMax=Mathf.Min(effRangeMax, 1);
				
				if(GameControlTB.EnableCover()){
					for(int i=0; i<tile.cover.Count; i++){
						if(tile.cover[i]==_CoverType.BlockFull){
							instance.coverOverlayFull[i].position=tile.overlayPos[i];
						}
						if(tile.cover[i]==_CoverType.BlockHalf){
							instance.coverOverlayHalf[i].position=tile.overlayPos[i];
						}
					}
				}
				
				//get all the tile within range
				List<Tile> tempList=GetTilesWithinRange(tile, effRangeMin, effRangeMax);
				
				foreach(Tile hT in tempList){
					if(hT.unit!=null && hT.unit.factionID!=unit.factionID && !hT.attackableToSelected){
						bool LOSFlag=true;
						//check if fog of war is enabled
						if(GameControlTB.EnableFogOfWar() ){
							int dist=GridManager.Distance(tile, hT);
							LOSFlag=IsInLOS(tile, hT) & dist<=unit.GetUnitSight() & hT.unit.IsVisibleToPlayer();
						}
						if(LOSFlag){
							tempHostileTileList.Add(hT);
							hT.SetState(_TileState.Hostile);
						}
					}
				}
			}
		}
		
		if(onHoverTileEnterE!=null) onHoverTileEnterE(tile);
	}
	//called when mouse exit a tile
	public static void OnHoverExit(){
		//if user is currently selecting a target for unit ability, call the alternate hover mode
		if(targetTileSelectMode){
			OnHoverTargetTileExit();
			return;
		}
		
		instance.indicatorCursor.position=new Vector3(0, 99999, 0);
		
		ResetCoverOverlay();
		
		//clear potential target tile
		foreach(Tile tile in tempHostileTileList){
			if(!tile.attackableToSelected) tile.SetState(_TileState.Default);
		}
		tempHostileTileList=new List<Tile>();
		
		if(onHoverTileExitE!=null) onHoverTileExitE();
	}
	
	
	public static void PlaceUnitAt(Tile tile){
		instance.allPlaceableTiles.Remove(tile);
	}
	
	public static void RemoveUnitAt(Tile tile){
		instance.allPlaceableTiles.Add(tile);
	}
	
	public static bool AllPlaceableTileOccupied(){
		if(instance.allPlaceableTiles.Count>0) return false;
		return true;
	}
	
	//method to get all tile which is open to unit placement
	public static List<Tile> GetAllPlaceableTiles(){
		return instance.allPlaceableTiles;
		//~ List<Tile> list=new List<Tile>();
		//~ foreach(Tile tile in instance.allTiles){
			//~ if(tile.openForPlacement && tile.unit==null) list.Add(tile);
		//~ }
		//~ return list;
	}
	
	
	//get all neighbours of the a tile
	public static List<Tile> GetAllNeighbouringTile(Tile tile){
		List<Tile> list=new List<Tile>();
		list.Add(tile);
		foreach(Tile neighbour in tile.GetNeighbours()){
			list.Add(neighbour);
		}
		return list;
	}
	
	
	//for unitAbility target selection mode
	private static UnitAbility currentUAB=null;	//current selected unit ability
	private static Tile uABScrTile=null;
	private static bool targetTileSelectMode=false;
	//private static int targetTileSelectRange=5;
	//private static bool enableTargetTileAOE=false;
	//private static int targetTileAOERange=1;
	//private static _EffectType targetTileSelectEffectType;
	//private static _AbilityTargetType targetTileSelectTargetType;
	private	static List<Tile> tilesInAbilityRange=new List<Tile>();
	
	public static bool IsInTargetTileSelectMode(){
		return targetTileSelectMode;
	}
	//activate alternate tile hover/select mode for target selection of unitAbility
	//when this is called, the selection mode swtich to allow a target for the ability to be selected
	public static void SetTargetTileSelectMode(Tile tile, UnitAbility uAB){
		targetTileSelectMode=true;
		
		currentUAB=uAB;
		uABScrTile=tile;
		
		//clear all the tile in walkeble and hostile list
		foreach(Tile walkableTile in walkableList){
			walkableTile.SetState(_TileState.Default);
		}
		foreach(Tile hostileTile in hostileList){
			hostileTile.SetState(_TileState.Default);
		}
		
		if(uAB.targetArea==_TargetArea.Default){
			//highlight the tile within the ability range
			List<Tile> list=GetTilesWithinRange(tile, currentUAB.range);
			foreach(Tile tileInRange in list){
				if(tileInRange.walkable){
					if(uAB.targetType==_AbilityTargetType.AllTile){
						if(tileInRange.unit==null){
							tilesInAbilityRange.Add(tileInRange);
							tileInRange.SetState(_TileState.AbilityRange);
						}
					}
					else{
						tilesInAbilityRange.Add(tileInRange);
						tileInRange.SetState(_TileState.AbilityRange);
					}
				}
			}
		}
	}
	/*
	//obsolete
	public static void SetTargetTileSelectMode1(Tile tile, int range, _EffectType effType, _AbilityTargetType tgtType, bool aoe, int aoeRange){
		targetTileSelectMode=true;
		targetTileSelectRange=range;
		targetTileSelectEffectType=effType;
		targetTileSelectTargetType=tgtType;
		enableTargetTileAOE=aoe;
		targetTileAOERange=aoeRange;
		if(!enableTargetTileAOE) targetTileAOERange=0;
		
		//clear all the tile in walkeble and hostile list
		foreach(Tile walkableTile in walkableList){
			walkableTile.SetState(_TileState.Default);
		}
		foreach(Tile hostileTile in hostileList){
			hostileTile.SetState(_TileState.Default);
		}
		
		//highlight the tile within the ability range
		List<Tile> list=GetTilesWithinRange(tile, targetTileSelectRange);
		foreach(Tile tileInRange in list){
			if(tileInRange.walkable){
				tilesInAbilityRange.Add(tileInRange);
				tileInRange.SetState(_TileState.AbilityRange);
			}
		}
	}
	*/
	//exit alternate tile hover/select mode for target selection of unitAbility
	public static void ExitTargetTileSelectMode(){
		targetTileSelectMode=false;
		currentUAB=null;
		
		OnHoverTargetTileExit();
		
		//clear all the current highlighted tiles for ability
		foreach(Tile tile in tilesInAbilityRange){
			tile.SetState(_TileState.Default);
		}
		tilesInAbilityRange=new List<Tile>();
		
		//reset and highlight the walkeble and hostile list
		if(UnitControl.selectedUnit!=null){
			foreach(Tile walkableTile in walkableList){
				walkableTile.SetState(_TileState.Walkable);
			}
			foreach(Tile hostileTile in hostileList){
				hostileTile.SetState(_TileState.Hostile);
			}
		}
	}
	
	//coroutine which run when unitAbility target select is active
	//for unit ability which target area can be adjusted from within a single tile
	//no longer in use
	IEnumerator HoverCoroutine(){
		Vector3 mousePos=Input.mousePosition;
		Tile currentTile=null;
		while(targetTileSelectMode){
			if(Vector3.Distance(mousePos, Input.mousePosition)>5){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
					Tile tile=hit.collider.gameObject.GetComponent<Tile>();
					if(tile!=currentTile){
						currentTile=tile;
						OnHoverTargetTileExit();
						if(tile!=null) OnHoverTargetTileSelect(tile);
					}
				}
			}	
			
			yield return null;
		}
	}
	
	
	//place hostile indicator on all the possible target
	public static void PlaceIndicatorH(){
		for(int i=0; i<hostileList.Count; i++){
			instance.indicatorH[i].position=hostileList[i].transform.position;
		}
	}
	//clear all hostile indicator
	public static void ClearAllIndicatorH(){
		for(int i=0; i<instance.indicatorH.Length; i++){
			instance.indicatorH[i].position=new Vector3(0, 9999, 0);
		}
	}
	//clear hostile indicator for the tile given
	public static void ClearIndicatorH(Tile tile){
		for(int i=0; i<instance.indicatorH.Length; i++){
			if(instance.indicatorH[i].position==tile.pos){
				instance.indicatorH[i].position=new Vector3(0, 9999, 0);
			}
		}
	}
	
	
	public static void ResetCoverOverlay(){
		if(GameControlTB.EnableCover()){
			for(int i=0; i<instance.coverOverlayFull.Count; i++){
				instance.coverOverlayHalf[i].position=new Vector3(50000, 50000, 50000);
				instance.coverOverlayFull[i].position=new Vector3(50000, 50000, 50000);
			}
		}
	}
	
	
	
	
//*************************************************************************************************************************//
//code for fog of war calculation

	//for fog of war, wip
	public static bool IsInLOS(Tile targetTile, Tile srcTile){
		return IsInLOS(targetTile.pos, srcTile.pos, false);
	}
	public static bool IsInLOS(Tile targetTile, Tile srcTile, bool debugging){
		return IsInLOS(targetTile.pos, srcTile.pos, debugging);
	}
	public static bool IsInLOS(Vector3 pos1, Vector3 pos2){
		return IsInLOS(pos1, pos2, false);
	}
	public static bool IsInLOS(Vector3 pos1, Vector3 pos2, bool debugging){
		float dist=Vector3.Distance(pos2, pos1);
		Vector3 dir=(pos2-pos1).normalized;
		Vector3 dirO=new Vector3(-dir.z, 0, dir.x).normalized;
		LayerMask mask=1<<LayerManager.GetLayerObstacle();
		float debugDuration=1.5f;
		float posOffset=0.4f;
		if(instance.type==_TileType.Square) posOffset=0.6f;
		
		RaycastHit[] hits=Physics.RaycastAll(pos1, dir, dist, mask);
		bool block1=true;
		foreach(RaycastHit hit in hits){
			Obstacle obs=hit.collider.gameObject.GetComponent<Obstacle>();
			if(obs.coverType==_CoverType.BlockFull){
				block1=false;
				break;
			}
		}
		
		if(debugging){
			Vector3 pp=pos1;
			Vector3 ss=pos2;
			if(!block1) Debug.DrawLine(pp, ss, Color.red, debugDuration);
			else Debug.DrawLine(pp, ss, Color.white, debugDuration);
		}
		
		
		hits=Physics.RaycastAll(pos1+dirO*GetTileSize()*posOffset, dir, dist, mask);
		bool block2=true;
		foreach(RaycastHit hit in hits){
			Obstacle obs=hit.collider.gameObject.GetComponent<Obstacle>();
			if(obs.coverType==_CoverType.BlockFull){
				block2=false;
				break;
			}
		}
		
		if(debugging){
			Vector3 pp=pos1+dirO*GetTileSize()*posOffset;
			Vector3 ss=pos2+dirO*GetTileSize()*posOffset;
			if(!block2) Debug.DrawLine(pp, ss, Color.red, debugDuration);
			else Debug.DrawLine(pp, ss, Color.white, debugDuration);
		}
		
		
		hits=Physics.RaycastAll(pos1-dirO*GetTileSize()*posOffset, dir, dist, mask);
		bool block3=true;
		foreach(RaycastHit hit in hits){
			Obstacle obs=hit.collider.gameObject.GetComponent<Obstacle>();
			if(obs.coverType==_CoverType.BlockFull){
				block3=false;
				break;
			}
		}
		
		if(debugging){
			Vector3 pp=pos1-dirO*GetTileSize()*posOffset;
			Vector3 ss=pos2-dirO*GetTileSize()*posOffset;
			if(!block3) Debug.DrawLine(pp, ss, Color.red, debugDuration);
			else Debug.DrawLine(pp, ss, Color.white, debugDuration);
		}
		
		bool block=true;
		if(!block1 && !block2 && !block3) block=false;
		
		//Debug.Log(block1+"  "+block2+"  "+block3+"  "+block);
		
		//Color color=Color.white;
		//if(block) color=Color.red;
		//Debug.DrawLine(pos1, pos2, color, 0.1f);
		
		return block;
	}
	
	//this one is for quick testing
	public static void LOS(Tile targetTile){
		IsInLOS(targetTile, instance.allTiles[0]);
	}
	
//end fog of war
//*************************************************************************************************************************//
	
	
	
	
//*************************************************************************************************************************//
//utility function with regards to the grid
	
	
	//get 3 neighouring tiles of current tile being pointed by the mouse
	/*obsolete
	public static List<Tile> Get4NeighbouringTile(Tile tileOrigin, Vector3 pos){
		
	}
	public static List<Tile> Get4NeighbouringTile1(){
		Tile tile=null;
		Vector3 pos=Vector3.zero;
		
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
			pos=hit.point;
			tile=hit.collider.gameObject.GetComponent<Tile>();
		}
		
		List<Tile> list=new List<Tile>();
		list.Add(tile);
		
		if(tile!=null){
			if(pos.z>=tile.pos.z){
				foreach(Tile neighbour in tile.neighbours){
					if(neighbour.pos.z>tile.pos.z){
						DebugDraw.Cross(neighbour.pos, Color.white, 0.2f);
						list.Add(neighbour);
					}
				}
			}
			else{
				foreach(Tile neighbour in tile.neighbours){
					if(neighbour.pos.z<tile.pos.z){
						DebugDraw.Cross(neighbour.pos, Color.white, 0.2f);
						list.Add(neighbour);
					}
				}
			}
		}
		
		return list;
	}
	*/
	
	//get a conical aoe area from a tile
	public static List<Tile> GetTileInCone60(Tile tileOrigin, Tile targetTile, int range){
		float angle=Utility.VectorToAngle(new Vector2(targetTile.pos.x-tileOrigin.pos.x, targetTile.pos.z-tileOrigin.pos.z));
		
		if(angle>=30 && angle<90) angle=60;
		else if(angle>=90 && angle<150) angle=120;
		else if(angle>=150 && angle<210) angle=180;
		else if(angle>=210 && angle<270) angle=240;
		else if(angle>=270 && angle<330) angle=300;
		else angle=360;
		
		List<Tile> allTileWithinRange=GetAOETile(tileOrigin, range);
		List<Tile> list=new List<Tile>();
		
		for(int i=0; i<allTileWithinRange.Count; i++){
			Tile tile=allTileWithinRange[i];
			float angleT=Utility.VectorToAngle(new Vector2(tile.pos.x-tileOrigin.pos.x, tile.pos.z-tileOrigin.pos.z));
			if(angleT<30 && angle==360) angleT+=360;
			
			//Debug.DrawLine(hTile.pos, hTile.pos+new Vector3(0, 1, 0), Color.white, 0.1f);
			
			if(Mathf.Abs(angleT-angle)<33){
				//Debug.DrawLine(hTile.pos, hTile.pos+new Vector3(0, 2, 0), Color.red, 0.1f);
				list.Add(tile);
			}
		}
		
		return list;
	}
	
	//get tiles in a straight line from a tile
	public static List<Tile> GetTileInLine(Tile tileOrigin, Tile targetTile, int range){
		int angleDiv=60;
		if(instance.type==_TileType.Square) angleDiv=90;
		else if(instance.type==_TileType.Hex) angleDiv=60;
		
		Vector2 dirOrigin=new Vector2(targetTile.pos.x-tileOrigin.pos.x, targetTile.pos.z-tileOrigin.pos.z);
		float angleOrigin=0;
		if(instance.type==_TileType.Square){
			angleOrigin=Utility.VectorToAngle(dirOrigin)+45;
			if(angleOrigin>360) angleOrigin-=360;
		}
		else if(instance.type==_TileType.Hex){
			angleOrigin=Utility.VectorToAngle(dirOrigin);
		}
		angleOrigin=(int)(angleOrigin/angleDiv)*angleDiv;
		
		List<Tile> lineList=new List<Tile>();
		Tile tile=tileOrigin;
		
		int dist=0;
		while(dist<range){
			bool match=false;
			List<Tile> tileNeighbour=tile.GetNeighbours();
			for(int i=0; i<tileNeighbour.Count; i++){
				Tile neighbour=tileNeighbour[i];
				Vector2 dir=new Vector2(neighbour.pos.x-tile.pos.x, neighbour.pos.z-tile.pos.z);
				float angle=Utility.VectorToAngle(dir);
				angle=(int)(angle/angleDiv)*angleDiv;
				
				if(angle==angleOrigin){
					match=true;
					lineList.Add(neighbour);
					tile=neighbour;
					dist+=1;
					Debug.DrawLine(tile.pos, tile.pos+new Vector3(0, 2, 0), Color.red, 0.1f);
					break;
				}
			}
			if(!match) break;
		}
		
		return lineList;
	}
	
	
	//get a typical hex aoe area
	public static List<Tile> GetAOETile(Tile tile, int range){
		return GetTilesWithinRange(tile, range);
	}
	
	
	//method to get hexTile within a distance(in tile) from the rootTile
	public static List<Tile> GetTilesWithinRange(Tile rootHT, int range){
		return GetTilesWithinRange(rootHT, 0, range);
	}
	public static List<Tile> GetTilesWithinRange(Tile rootHT, int rangeMin, int rangeMax){
		if(rangeMin==0 && rangeMax==0) return new List<Tile>();
		
		List<Tile> tileNeighbours=rootHT.GetNeighbours();
		
		if(rangeMax==1){
			List<Tile> list=new List<Tile>();
			for(int i=0; i<tileNeighbours.Count; i++) list.Add(tileNeighbours[i]);
			return list;
		}
		
		//for performance benchmarking
		//DateTime timeS;
		//DateTime timeE;
		//TimeSpan timeSpan;
		//timeS=System.DateTime.Now;
		
		List<Tile> closeList=new List<Tile>();
		List<Tile> openList=new List<Tile>();
		List<Tile> newOpenList=new List<Tile>();
		
		
		foreach(Tile neighbour in tileNeighbours){
			if(!newOpenList.Contains(neighbour)){
				newOpenList.Add(neighbour);
			}
		}
		
		for(int i=0; i<rangeMax; i++){
			openList=newOpenList;
			newOpenList=new List<Tile>();
			
			foreach(Tile ht in openList){
				foreach(Tile neighbour in ht.GetNeighbours()){
					if(!closeList.Contains(neighbour) && !openList.Contains(neighbour) && !newOpenList.Contains(neighbour)){
						newOpenList.Add(neighbour);
					}
				}
			}
			
			foreach(Tile t in openList){
				if(t!=rootHT && !closeList.Contains(t)) closeList.Add(t);
			}
		}
		
		for(int i=0; i<closeList.Count; i++){
			Tile ht=closeList[i];
			if(Distance(ht, rootHT)<=rangeMin){
				closeList.RemoveAt(i);
				i-=1;
			}
		}
		
		//show how much time has been used
		//timeE=System.DateTime.Now;
		//timeSpan=timeE-timeS;
		//Debug.Log( "Time:"+timeSpan.TotalMilliseconds);
		
		return closeList;
	}
	
	
	public static List<Tile> GetWalkableTilesWithinRange(Tile rootHT, int range){
		if(range==0) return new List<Tile>();
		
		List<Tile> tileNeighbours=rootHT.GetNeighbours();
		
		if(range==1){
			List<Tile> list=new List<Tile>();
			for(int i=0; i<tileNeighbours.Count; i++){
				if(tileNeighbours[i].walkable) list.Add(tileNeighbours[i]);
			}
			return list;
		}
		
		//for performance benchmarking
		//DateTime timeS;
		//DateTime timeE;
		//TimeSpan timeSpan;
		//timeS=System.DateTime.Now;
		
		List<Tile> closeList=new List<Tile>();
		List<Tile> openList=new List<Tile>();
		List<Tile> newOpenList=new List<Tile>();
		
		foreach(Tile neighbour in tileNeighbours){
			if(neighbour.walkable && neighbour.unit==null && !newOpenList.Contains(neighbour)){
				newOpenList.Add(neighbour);
			}
		}
		
		for(int i=0; i<range; i++){
			openList=newOpenList;
			newOpenList=new List<Tile>();
			
			foreach(Tile ht in openList){
				foreach(Tile neighbour in ht.GetNeighbours()){
					if(neighbour.walkable && neighbour.unit==null && !newOpenList.Contains(neighbour)){
						//if(!neighbour.walkable) Debug.Log("tittt");
						newOpenList.Add(neighbour);
					}
				}
			}
			
			foreach(Tile t in openList){
				if(t!=rootHT && !closeList.Contains(t)){
					//if(!t.walkable) Debug.Log("tit");
					closeList.Add(t);
				}
			}
		}
		
		//show how much time has been used
		//timeE=System.DateTime.Now;
		//timeSpan=timeE-timeS;
		//Debug.Log( "Time:"+timeSpan.TotalMilliseconds);
		
		return closeList;
	}
	
	//given a rect on horizontal plane, return all the tile enclosed by it
	public List<Tile> GetTileWithinRect(Rect rect){
		if(rect.width==0 || rect.height==0) return new List<Tile>();
		
		if(rect.width<0){
			rect.width*=-1;
			rect.x-=rect.width;
		}
		if(rect.height<0){
			rect.height*=-1;
			rect.y-=rect.height;
		}
		
		//if(allTiles.Count==0){
			GetAllTile();
		//}
		
		List<Tile> tileList=new List<Tile>();
		foreach(Tile tile in allTiles){
			Vector2 pos=new Vector2(tile.transform.position.x, tile.transform.position.z);
			if(rect.Contains(pos)){
				tileList.Add(tile);
			}
		}
		return tileList;
	}
	
	//loop through all tile to get nearest tile for the position given
	public static Tile GetNearestTile(Vector3 pos){
		pos.y=instance.baseHeight;
		Tile tile=null;
		float nearest=Mathf.Infinity;
		foreach(Tile t in instance.allTiles){
			float dist=Vector3.Distance(t.pos, pos);
			if(dist<nearest){
				nearest=dist;
				tile=t;
			}
		}
		return tile;
	}
	
	//get distance between two tiles.
	public static int Distance(Tile srcTile, Tile targetTile){
		return AStar.Distance(srcTile, targetTile);
	}
	
	public static List<Tile> GetAllTiles(){
		return instance.allTiles;
	}
	
	public static float GetTileSize(){
		return instance.gridSize*instance.gridToTileSizeRatio;
	}
	
	
	
	
	public bool showGizmo=true;
	void OnDrawGizmos(){
		if(showGizmo){
			Gizmos.color=Color.white;
			for(int i=0; i<playerPlacementAreas.Count; i++){
				Vector3 p1=new Vector3(playerPlacementAreas[i].x, baseHeight, playerPlacementAreas[i].y+playerPlacementAreas[i].height);
				Vector3 p2=new Vector3(playerPlacementAreas[i].x+playerPlacementAreas[i].width, baseHeight, playerPlacementAreas[i].y+playerPlacementAreas[i].height);
				Vector3 p3=new Vector3(playerPlacementAreas[i].x+playerPlacementAreas[i].width, baseHeight, playerPlacementAreas[i].y);
				Vector3 p4=new Vector3(playerPlacementAreas[i].x, baseHeight, playerPlacementAreas[i].y);
				
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p3);
				Gizmos.DrawLine(p3, p4);
				Gizmos.DrawLine(p4, p1);
			}
			
			foreach(Faction fac in factionList){
				Gizmos.color=fac.color;
				foreach(FactionSpawnInfo sInfo in fac.spawnInfo){
					Vector3 p1=new Vector3(sInfo.area.x, baseHeight, sInfo.area.y+sInfo.area.height);
					Vector3 p2=new Vector3(sInfo.area.x+sInfo.area.width, baseHeight, sInfo.area.y+sInfo.area.height);
					Vector3 p3=new Vector3(sInfo.area.x+sInfo.area.width, baseHeight, sInfo.area.y);
					Vector3 p4=new Vector3(sInfo.area.x, baseHeight, sInfo.area.y);
					
					Gizmos.DrawLine(p1, p2);
					Gizmos.DrawLine(p2, p3);
					Gizmos.DrawLine(p3, p4);
					Gizmos.DrawLine(p4, p1);
				}
			}
		}
	}
}
