using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

public class MenuExtension : EditorWindow {
	
	
	[MenuItem ("Tools/SongGameDev/TBTK/New Scene HexGrid (Default UI)", false, -100)]
	static void NewHex () {
		AddUI(NewScene(_TileType.Hex));
	}
	
	[MenuItem ("Tools/SongGameDev/TBTK/New Scene SquareGrid (Default UI)", false, -100)]
    static void NewSquare () {
		AddUI(NewScene(_TileType.Square));
	}
	
	static void AddUI(GameObject obj){
		GameObject ui=Resources.Load("ScenePrefab/UI", typeof(GameObject)) as GameObject;
		obj=(GameObject)PrefabUtility.InstantiatePrefab(ui);	obj.transform.parent=obj.transform;
	}
	
	
	[MenuItem ("Tools/SongGameDev/TBTK/UnitManager", false, 10)]
    static void OpenUnitManagerWindow () {
    	UnitTBManagerWindow.Init();
    }
	
	[MenuItem ("Tools/SongGameDev/TBTK/UnitEditor", false, 10)]
    static void OpenUnitEditorWindow () {
    	UnitTBEditorWindow.Init();
    }
	
	[MenuItem ("Tools/SongGameDev/TBTK/UnitAbilityEditor", false, 10)]
    static void OpenUnitAbilityEditorWindow () {
    	UnitAbilityEditorWindow.Init();
    }
	
	[MenuItem ("Tools/SongGameDev/TBTK/PerkEditor", false, 10)]
    static void OpenPerkTBEditorWindow () {
    	PerkTBEditorWindow.Init();
    }
	
	[MenuItem ("Tools/SongGameDev/TBTK/CollectibleManager", false, 10)]
    static void OpenCollectibleManagerWindow () {
    	CollectibleManagerWindow.Init();
    }
	
	[MenuItem ("Tools/SongGameDev/TBTK/CollectibleEditor", false, 10)]
    static void OpenCollectibleEditorWindow () {
    	CollectibleEditorWindow.Init();
    }
	
	[MenuItem ("Tools/SongGameDev/TBTK/ObstacleManager", false, 10)]
    static void OpenObstacleManagerWindow () {
    	ObstacleManagerWindow.Init();
    }
	
	[MenuItem ("Tools/SongGameDev/TBTK/DamageArmorTable", false, 10)]
    public static void OpenDamageTable () {
    	DamageArmorTableEditor.Init();
    }
	
	
    [MenuItem ("Tools/SongGameDev/TBTK/Support Forum", false, 100)]
    static void OpenForumLink () {
    	//Application.OpenURL("http://goo.gl/ZX4OA"
			
		Application.OpenURL("http://goo.gl/MhnkCO");
    }
	
	
	
	
	
	
	
	
	
	
	
	public static GameObject NewScene (_TileType tileType) {
		EditorApplication.NewScene();
		
		GameObject rootObj=new GameObject();
		rootObj.name="TBTK";
		Transform rootT=rootObj.transform;
		
		GameObject obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="Units";
		
    	obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="GameControl";
		//obj.AddComponent<GameControlTB>();
		GameControlTB gameControl=obj.AddComponent<GameControlTB>();
		gameControl.playerFactionID.Add(0);
		
		obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="GridManager";
		GridManager gridManager=obj.AddComponent<GridManager>();
		gridManager.playerPlacementAreas.Add(new Rect());
		
		GameObject hexTile=Resources.Load("ScenePrefab/HexTile", typeof(GameObject)) as GameObject;
		gridManager.hexTilePrefab=hexTile;
		GameObject SquareTile=Resources.Load("ScenePrefab/SquareTile", typeof(GameObject)) as GameObject;
		gridManager.squareTilePrefab=SquareTile;
		GenerateHexGrid(gridManager, tileType);
		
		obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="AudioManager";
		obj.AddComponent<AudioManager>();
		//AudioManager HexGridManager=obj.AddComponent<AudioManager>();
		
		obj=new GameObject(); obj.transform.parent=rootT;
		obj.name="PerkManager";
		obj.AddComponent<PerkManagerTB>();
		//PerkManagerTB perkManager=obj.AddComponent<PerkManagerTB>();
		
		DestroyImmediate(Camera.main.gameObject);
		GameObject cam=Resources.Load("ScenePrefab/CameraControl", typeof(GameObject)) as GameObject;
		obj=(GameObject)Instantiate(cam);	obj.transform.parent=rootT;
		obj.name="CameraControl";
		//foreach(Transform child in obj.transform) child.localPosition=new Vector3(0, 20, -20);
		
		Camera.main.transform.position=new Vector3(0, 12, -12);
		Camera.main.transform.rotation=Quaternion.Euler(45, 0, 0);
		
		return rootObj;
    }
	
	
	static void GenerateHexGrid(GridManager gm, _TileType tileType){
		if(tileType==_TileType.Square){
			gm.width=8;
			gm.length=8;
		}
		
		//clear previous tile
		Tile[] allTilesInScene=(Tile[])FindObjectsOfType(typeof(Tile));
		foreach(Tile tile in allTilesInScene){
			if(tile.unit!=null) DestroyImmediate(tile.unit.gameObject);
			DestroyImmediate(tile);
		}
		/*
		for(int i=0; i<gm.allTiles.Count; i++){
			if(gm.allTiles[i]!=null){
				if(gm.allTiles[i].unit!=null) DestroyImmediate(gm.allTiles[i].unit.gameObject);
				DestroyImmediate(gm.allTiles[i].gameObject);
			}
		}
		gm.allTiles=new List<Tile>();
		*/
		
		gm.type=tileType;
		if(gm.type==_TileType.Square) GridManagerEditor.GenerateSquareGrid(gm);
		else if(gm.type==_TileType.Hex) GridManagerEditor.GenerateHexGrid(gm);
		
		allTilesInScene=(Tile[])FindObjectsOfType(typeof(Tile));
		List<Tile> tileList=new List<Tile>();
		foreach(Tile tile in allTilesInScene){
			tileList.Add(tile);
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
		
		gm.GenerateGrid(false);
	}
}


