using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum _SpawnQuota{
	UnitBased, 		//based on unit count and individual unit limit
	BudgetBased,	//based on a fixed budget and unit cost
}

[System.Serializable]
public class Faction{
	public int factionID=-1;
	public string factionName="faction";
	public Texture icon;
	public Color color;
	public List<UnitTB> allUnitList=new List<UnitTB>();
	public bool isPlayerControl=false;
	public bool allUnitMoved=false;
	public int currentTurnID=0; //for certain turnMode, cycle between faction of this faction
	
	//total of unit moved so far in a single round
	//public int numOfUnitMoved=0;
	
	//an int list indicating which of the unit in allUnitList is not moved
	public List<int> unitYetToMove=new List<int>();
	
	public List<FactionSpawnInfo> spawnInfo=new List<FactionSpawnInfo>();
}

[System.Serializable]
public class FactionSpawnInfo{
	public Rect area;
	public _SpawnQuota spawnQuota;
	public int budget;
	public int unitCount=0;
	public UnitTB[] unitPrefabs=new UnitTB[0];
	public int[] unitPrefabsMax=new int[0];
	public List<Tile> spawnTileList=new List<Tile>();
	public bool showUnitPrefabList;
	public int unitRotation;
	
	public FactionSpawnInfo(){
		area.width=1;
		area.height=1;
	}
}

public class FactionListPrefab : MonoBehaviour {
	public List<Faction> factionList=new List<Faction>();
}