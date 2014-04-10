#define ibox
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class PlayerUnits{
	[HideInInspector] public int factionID=-1;
	//[HideInInspector] 
	public List<UnitTB> starting=new List<UnitTB>();
	//[HideInInspector] public List<UnitTB> undeployed=new List<UnitTB>();
	[HideInInspector] public bool showInInspector=false;
	
	public PlayerUnits(int ID){
		factionID=ID;
	}
}

public class UnitControl : MonoBehaviour {
	
	public delegate void NewFactionPlacementHandler(PlayerUnits pUnits); 
	public static event NewFactionPlacementHandler onNewPlacementE;

	public delegate void PlacementUpdateHandler(); 
	public static event PlacementUpdateHandler onPlacementUpdateE;
	
	public delegate void NewUnitInRuntimeHandler(UnitTB unit); 
	public static event NewUnitInRuntimeHandler onNewUnitInRuntimeE;
	
	public delegate void FactionDefeatedHandler(int ID); 
	public static event FactionDefeatedHandler onFactionDefeatedE;
	
	[HideInInspector] 
	public List<PlayerUnits> playerUnits=new List<PlayerUnits>();
	
	[HideInInspector] 
	public List<UnitTB> startingUnit=new List<UnitTB>();
	[HideInInspector] public List<UnitTB> undeployedUnit=new List<UnitTB>();
	
	[HideInInspector] 
	public List<Color> factionColors=new List<Color>();
	
	[HideInInspector] 
	public List<UnitTB> allUnits=new List<UnitTB>();
	private List<UnitTB> allUnitsMoved=new List<UnitTB>();
	
	[HideInInspector] 
	public List<Faction> allFactionList=new List<Faction>();
	public static int activeFactionCount=0;	//indicate how many faction are still in the game, defeated faction excluded
	
	
	public static UnitTB selectedUnit;			//player selected unit
	public static UnitTB hoveredUnit;		
	
	public static UnitControl instance;
	
	//for unity-based turn mode, indicatring which unit turn it's current
	public static int currentUnitTurnID=-1;
	
	//for singleUnitRealtime, the highest speed among all the unit
	private int highestUnitPriority=0;
	
	//flag to indicate if waitingTime of the units should be reduced
	//used in singleUnitRealtime only
	//set to true if there are unit with the same moveOrder in the turn
	private bool holdWaitedTime=false;
	
	//public float unitMoveSpeed;
	//public static float GetUnitMoveSpeed(){
	//	return instance.unitMoveSpeed;
	//}
	
	//flag to indicate if the unit gameObject should be destroy when the unit is killed
	public bool hideUnitWhenKilled=true;
	public bool destroyUnitObject=true;
	public static bool HideUnitWhenKilled(){
		return instance.hideUnitWhenKilled;
	}
	public static bool DestroyUnitObject(){
		return instance.destroyUnitObject;
	}
	
	
	void Awake(){
		instance=this;
		
		List<Tile> allTiles=GridManager.GetAllTiles();
		foreach(Tile tile in allTiles){
			if(tile.unit!=null){
				UnitTB unit=tile.unit;
				allUnits.Add(unit);
			}
		}
		
		currentUnitTurnID=-1;
#if ibox
		if(playerUnits==null || playerUnits.Count==0){
			playerUnits=new List<PlayerUnits>();
			playerUnits.Add(new PlayerUnits(GameControlTB.instance.playerFactionID[0]));
		}
#endif
	}
	
	// Use this for initialization
	void Start () {
#if ibox
#else
		if(playerUnits==null || playerUnits.Count==0){
			playerUnits=new List<PlayerUnits>();
			playerUnits.Add(new PlayerUnits(GameControlTB.instance.playerFactionID[0]));
		}
#endif
		if(GameControlTB.LoadMode()==_LoadMode.UsePersistantData){
			if(!GlobalStatsTB.loaded){ GlobalStatsTB.Init(); }
			
			//int playerFactionCount=GlobalStatsTB.GetPlayerFactionCount();
			//for(int i=0; i<playerFactionCount; i++){
			//	playerUnits[i].starting=GlobalStatsTB.GetPlayerUnitList(i);
			//}
			
			playerUnits[0].starting=GlobalStatsTB.GetPlayerUnitList();
		}
		else if(GameControlTB.LoadMode()==_LoadMode.UseTemporaryData){
			int playerFactionCount=GlobalStatsTB.GetPlayerFactionCount();
			for(int i=0; i<playerFactionCount; i++){
				playerUnits[i].starting=GlobalStatsTB.GetTempPlayerUnitList();
			}
			
			//playerUnits[0].starting=GlobalStatsTB.GetTempPlayerUnitList();
		}
		
		#if UnityEditor
			Transform unitParent=transform.root;
			foreach(Transform child in transform.root){
				if(child.gameObject.name=="Units") unitParent=child;
			}
		#endif
		
		
		//instantiate the starting unit in playerUnits, for hotseat mode, not in use 
		
		for(int j=0; j<playerUnits.Count; j++){
			PlayerUnits pUnits=playerUnits[j];
			for(int i=0; i<pUnits.starting.Count; i++){
				if(pUnits.starting[i]!=null){
					GameObject obj=(GameObject)Instantiate(pUnits.starting[i].gameObject, new Vector3(0, 9999, 0), Quaternion.identity);
					pUnits.starting[i]=obj.GetComponent<UnitTB>();
					pUnits.starting[i].factionID=pUnits.factionID;
					
					#if UnityEditor
						obj.transform.parent=unitParent;
					#endif
				}
			}
		}
		
		//make sure none of the element is empty, shrink the list
		for(int j=0; j<playerUnits.Count; j++){
			PlayerUnits pUnits=playerUnits[j];
			for(int i=0; i<pUnits.starting.Count; i++){
				if(pUnits.starting[i]==null){
					pUnits.starting.RemoveAt(i); 	i-=1;
				}
			}
		}
		
		
		
		//instantiate the starting unit
		/*
		for(int i=0; i<startingUnit.Count; i++){
			if(startingUnit[i]!=null){
				GameObject obj=(GameObject)Instantiate(startingUnit[i].gameObject, new Vector3(0, 9999, 0), Quaternion.identity);
				startingUnit[i]=obj.GetComponent<UnitTB>();
				
				#if UnityEditor
					obj.transform.parent=unitParent;
				#endif
			}
			//make sure none of the element is empty, shrink the list
		}
		
		for(int i=0; i<startingUnit.Count; i++){
			if(startingUnit[i]==null){
				startingUnit.RemoveAt(i); 	i-=1;
			}
		}
		*/
		
		
		//external code, not TBTK related
		//if(isWarCorpAlpha) WarCorpInitRoutine();
		
		
		
		
		//if(startingUnit.Count>0){
		if(playerUnits!=null && playerUnits.Count>0 && playerUnits[0].starting.Count>0){
			if(GameControlTB.EnableUnitPlacement()){
				//to inform GridManager to show the correct placeale tile
				if(onNewPlacementE!=null) onNewPlacementE(playerUnits[facPlacementID]);
				//to update UI and other stuff
				if(onPlacementUpdateE!=null) onPlacementUpdateE();
			}
			else{
				_AutoPlaceUnit();
				StartCoroutine(UnitPlacementCompleted());
			}
		}
		else{
			StartCoroutine(UnitPlacementCompleted());
		}
	}
	
	
	IEnumerator UnitPlacementCompleted(){
		yield return null;
		GameControlTB.UnitPlacementCompleted();
	}
	

	
	void OnEnable(){
		GameControlTB.onBattleStartE += OnBattleStart;
		GameControlTB.onNewRoundE += OnNewRound;
		GameControlTB.onNextTurnE += OnNextTurn;
		GameControlTB.onBattleEndE += OnBattleEnd;
		
		UnitTB.onUnitSelectedE += OnUnitSelected;
		UnitTB.onUnitDeselectedE += OnUnitDeselected;
		UnitTB.onUnitDestroyedE += OnUnitDestroyed;
	}
	
	void OnDisable(){
		GameControlTB.onBattleStartE -= OnBattleStart;
		GameControlTB.onNewRoundE -= OnNewRound;
		GameControlTB.onNextTurnE -= OnNextTurn;
		GameControlTB.onBattleEndE -= OnBattleEnd;
		
		UnitTB.onUnitSelectedE -= OnUnitSelected;
		UnitTB.onUnitDeselectedE -= OnUnitDeselected;
		UnitTB.onUnitDestroyedE -= OnUnitDestroyed;
	}
	
	
	void OnUnitSelected(UnitTB sUnit){
		selectedUnit=sUnit;
	}
	
	void OnUnitDeselected(){
		selectedUnit=null;
	}
	
	// Update is called once per frame
	void Update () {
		/*
		if(Input.GetKeyDown(KeyCode.L)){
			foreach(UnitTB unit in allUnits){
				unit.GainAP(100);
			}
		}
		*/
		if(Input.GetKeyDown(KeyCode.F)){
			_AutoPlaceUnit();
		}

	}

	
	
	//battle start, initiated the faction info
	void OnBattleStart(){
		//store away all the unused unit
		undeployedUnit=startingUnit;
		
		foreach(UnitTB unit in undeployedUnit){
			if(unit!=null){
				//Utility.SetActive(unit.thisObj, false);
				Utility.SetActive(unit.gameObject, false);
			}
		}
		startingUnit=null;
		
		InitFaction();
		
		_TurnMode turnMode=GameControlTB.GetTurnMode();
		
		if(turnMode==_TurnMode.SingleUnitRealTime || turnMode==_TurnMode.SingleUnitRealTimeNoRound){
			highestUnitPriority=0;
			for(int i=0; i<allUnits.Count; i++){
				if(allUnits[i].GetTurnPriority()>highestUnitPriority) highestUnitPriority=allUnits[i].GetTurnPriority();
			}
			
			float highest=-1;
			for(int i=0; i<allUnits.Count; i++){
				if(allUnits[i].GetTurnPriority()>highest){
					highest=allUnits[i].GetTurnPriority();
				}
			}
			
			for(int i=0; i<allUnits.Count; i++){
				//allUnits[i].waitingTime=Mathf.Floor(highest/allUnits[i].speed);
				allUnits[i].waitingTime=(highest/allUnits[i].GetTurnPriority());
			}
		}
		
		GameControlTB.OnNewRound();
	}
	
	
	//function called for a new round event
	void OnNewRound(int roundCounter){
		//clear all the moved flag for all faction
		for(int i=0; i<allFactionList.Count; i++){
			if(allFactionList[i].allUnitList.Count>0) allFactionList[i].allUnitMoved=false;
			//allFactionList[i].numOfUnitMoved=0;
		}
		
		_TurnMode turnMode=GameControlTB.GetTurnMode();
		
		if(turnMode==_TurnMode.SingleUnitPerTurn){
			currentUnitTurnID=-1;
			ArrangeAllUnitOnTurnPriority();
		}
		else if(turnMode==_TurnMode.SingleUnitRealTime || turnMode==_TurnMode.SingleUnitRealTimeNoRound){
			//ArrangeAllUnitOnTurnPriority();
			for(int i=0; i<allUnits.Count; i++) allUnits[i].waitedTime=allUnits[i].waitingTime;
		}
		else{
			if(GameControlTB.GetMoveOrder()==_MoveOrder.Free){
				ResetFactionUnitMoveList();		//reset the yetToMove list
			}
			else if(GameControlTB.GetMoveOrder()==_MoveOrder.FixedRandom){
				if(roundCounter==1) ShuffleAllUnit();	//randomly rearrange all unit
			}
			else if(GameControlTB.GetMoveOrder()==_MoveOrder.FixedStatsBased){
				ArrangeFactionUnitOnTurnPriority();	//arrange all unit within faction based on stats
			}
		}
	}
	
	
	
//****************************************************************************************************************************
//switch to next unit
	
	//switch to next unit via event
	public static void OnNextUnit(){
		instance.OnNextTurn();
	}
	void OnNextTurn(){
		if(GameControlTB.battleEnded) return;
		
		_TurnMode turnMode=GameControlTB.GetTurnMode();
		
		if(turnMode==_TurnMode.FactionAllUnitPerTurn){
			if(GameControlTB.IsPlayerTurn()) SwitchToNextUnitInTurn();
			else AIManager.AIRoutine(GameControlTB.turnID); 
		}
		else if(turnMode==_TurnMode.FactionSingleUnitPerTurnAll){
			SwitchToNextUnitInTurn();
		}
		else if(turnMode==_TurnMode.FactionSingleUnitPerTurnSingle){
			SwitchToNextUnitInTurn();
		}
		else if(turnMode==_TurnMode.SingleUnitPerTurn){
			SwitchToNextUnitInTurn();
		}
		else if(turnMode==_TurnMode.SingleUnitRealTime || turnMode==_TurnMode.SingleUnitRealTimeNoRound){
			for(int i=0; i<allUnits.Count; i++){
				if(!holdWaitedTime) allUnits[i].waitedTime-=1;
			}
			SwitchToNextUnitInTurn();
		}
	}
	
	
	//switch to next unit via direct function call, called by GameControl in UnitActionDepleted()
	public static void SwitchToNextUnit(){
		instance.StartCoroutine(instance.DelaySwitchUnit());
	}
	IEnumerator DelaySwitchUnit(){
		//make sure current action sequence is finished
		while(GameControlTB.IsActionInProgress()) yield return null;
		SwitchToNextUnitInTurn();
	}
	
	
	//called when the current selected player unit has used up all available move
	public static void SwitchToNextUnitInTurn(){
		GridManager.Deselect();
		
		_TurnMode turnMode=GameControlTB.GetTurnMode();
		
		//select next unit for this round,  if all unit has been moved, start new round
		if(turnMode==_TurnMode.SingleUnitPerTurn){
			currentUnitTurnID+=1;
			if(currentUnitTurnID>=instance.allUnits.Count){
				currentUnitTurnID=-1;
				//if all unit has been moved, issue a new round,
				GameControlTB.OnNewRound();
			}
			else{
				//Unit with the highest turnPriority move first
				//all unit has been arrange in order using function ArrangeAllUnitToTurnPriority() at every new round, check OnNewRound
				//so we simply select next unit based on currentUnitTurnID
				selectedUnit=instance.allUnits[currentUnitTurnID];
				selectedUnit.occupiedTile.Select();
				
				GameControlTB.turnID=selectedUnit.factionID;
				
				if(!GameControlTB.IsPlayerFaction(selectedUnit.factionID)) AIManager.MoveUnit(selectedUnit);
				//if(selectedUnit.factionID!=GameControlTB.GetPlayerFactionID()) AIManager.MoveUnit(selectedUnit);
			}
		}
		else if(turnMode==_TurnMode.SingleUnitRealTime || turnMode==_TurnMode.SingleUnitRealTimeNoRound){
			
			if(turnMode==_TurnMode.SingleUnitRealTime){
				if(instance.allUnitsMoved.Count==instance.allUnits.Count){
					GameControlTB.OnNewRound();
					instance.allUnitsMoved=new List<UnitTB>();
					return;
				}
			}
			
			float lowest=100000;
			instance.holdWaitedTime=false;
			UnitTB currentSelected=null;
			
			for(int i=0; i<instance.allUnits.Count; i++){
				UnitTB unit=instance.allUnits[i];
				if(unit.waitedTime<=0){
					if(unit.waitedTime==lowest) {
						instance.holdWaitedTime=true;
					}
					if(unit.waitedTime<lowest){
						lowest=unit.waitedTime;
						currentSelected=unit;
					}
				}
			}
			
			if(currentSelected==null){
				instance.OnNextTurn();
				return;
			}
			
			selectedUnit=currentSelected;
			selectedUnit.waitedTime=selectedUnit.waitingTime;
			selectedUnit.OnNewRound(0);
			selectedUnit.occupiedTile.Select();
			if(!instance.allUnitsMoved.Contains(selectedUnit)) instance.allUnitsMoved.Add(selectedUnit);
			
			GameControlTB.turnID=selectedUnit.factionID;
			
			if(!GameControlTB.IsPlayerFaction(selectedUnit.factionID)) AIManager.MoveUnit(selectedUnit);
			//if(selectedUnit.factionID!=GameControlTB.GetPlayerFactionID()) AIManager.MoveUnit(selectedUnit);
		}
		else{
			if(turnMode==_TurnMode.FactionAllUnitPerTurn){
				//when in FactionAllUnitPerTurn
				//this section is only called when the faction in turn belongs to player
				//otherwise AIROutine is called to move all AI's units
				if(!GameControlTB.IsPlayerTurn()) return;
			}
			
			//get the faction in turn and make sure a unit has been selected successfully
			Faction faction=instance.allFactionList[GameControlTB.turnID];
			if(!instance.SelectNexUnit(faction)) return;
			
			if(turnMode!=_TurnMode.FactionAllUnitPerTurn){
				//if the selecte unit doesnt belong to player, call AI to move the unit
				if(!GameControlTB.IsPlayerFaction(selectedUnit.factionID)){
					AIManager.MoveUnit(selectedUnit);
				}
				else{
					//Debug.Log("UC unit switched, player's turn");
				}
				//if(selectedUnit.factionID!=GameControlTB.GetPlayerFactionID()) AIManager.MoveUnit(selectedUnit);
			}
		}
	}
	
	//switch to next unit in turn, return true if there's a next unit exist for the faction, false if otherwise
	bool SelectNexUnit(Faction faction){
		//make sure there is unit to be selected
		if(faction.allUnitList.Count==0){
			Debug.Log("Error, UnitControl tried to select unit from empty faction");
			faction.allUnitMoved=true;
			GameControlTB.OnEndTurn();
			return false;
		}
		
		//make sure there is unit to be selected
		if(faction.allUnitMoved){
			Debug.Log("Error, UnitControl tried to select unit from faction with all unit moved");
			GameControlTB.OnEndTurn();
			return false;
		}
		
		//if move order is free, randomly select a unit
		if(GameControlTB.GetMoveOrder()==_MoveOrder.Free){
			int rand=UnityEngine.Random.Range(0, faction.unitYetToMove.Count);
			faction.currentTurnID=faction.unitYetToMove[rand];
		}
		
		//select the unit
		selectedUnit=faction.allUnitList[faction.currentTurnID];
		selectedUnit.occupiedTile.Select();
		
		if(GameControlTB.GetMoveOrder()!=_MoveOrder.Free){
			//move the currentTurnID so in the next turn, the next unit in line will be selected
			faction.currentTurnID+=1;
			if(faction.currentTurnID>=faction.allUnitList.Count){
				faction.currentTurnID=0;
				//faction.numOfUnitMoved+=1;
				faction.allUnitMoved=true;
			}
		}
		
		return true;
	}
	
//end switch to next unit
//****************************************************************************************************************************

	
	
	

	
//***********************************************************************************************************************
//create faction, add unit to faction, etc...
	
	//called when battle started (after unit placement is done)
	void InitFaction(){
		List<int> factionIDExisted=new List<int>();
		activeFactionCount=0;
		//get all the factionID so it can be reformat to the lowest numerical figure
		foreach(UnitTB unit in allUnits){
			if(!factionIDExisted.Contains(unit.factionID)){
				activeFactionCount+=1;
				factionIDExisted.Add(unit.factionID);
			}
		}
		
		/*
		//make sure all faction ID and are formated to lowest numerical figure (1, 2, 3....) so it sync with turnID in gameControl
		foreach(UnitTB unit in allUnits){
			for(int i=0; i<factionIDExisted.Count; i++){
				if(unit.factionID==factionIDExisted[i]){
					unit.factionID=i;
					break;
				}
			}
		}
		*/
		
		bool match=false;
		List<int> IDList=GameControlTB.GetPlayerFactionIDS();
		for(int i=0; i<IDList.Count; i++){
			if(factionIDExisted.Contains(IDList[i])) match=true;
		}
		if(!match) Debug.Log("Warning: there's no player faction");
		
		GameControlTB.totalFactionInGame=factionIDExisted.Count;
		if(GameControlTB.totalFactionInGame==1){
			Debug.Log("Warning: only 1 faction exist in this battle");
			return;
		}
		
		List<int> playerFacIDList=GameControlTB.GetPlayerFactionIDS();
		GameControlTB.playerFactionTurnID=new List<int>();
		for(int i=0; i<playerFacIDList.Count; i++){
			GameControlTB.playerFactionTurnID.Add(-1);
		}
		
		_TurnMode turnMode=GameControlTB.GetTurnMode();
		
		for(int i=0; i<GameControlTB.totalFactionInGame; i++){
			Faction faction=new Faction();
			faction.factionID=factionIDExisted[i];
			if(i<factionColors.Count){
				faction.color=factionColors[i];
			}
			else{
				faction.color=new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
			}
			
			foreach(UnitTB unit in allUnits){
				if(unit.factionID==factionIDExisted[i]){
					faction.allUnitList.Add(unit);
				}
			}
			
			for(int n=0; n<playerFacIDList.Count; n++){
				if(playerFacIDList[n]==faction.factionID){
					faction.isPlayerControl=true;
					
					//for singleUnit turnMode, turnID in gameControl is assigned based on factionID of each unit
					if((int)turnMode>2)	GameControlTB.playerFactionTurnID[n]=faction.factionID;
					//for faction turnMode, turnID in gameControl uses a chronological numeric counter
					else GameControlTB.playerFactionTurnID[n]=i;
					
					GameControlTB.playerFactionExisted=true;
				}
			}
			
			//obsoleted code
			//~ if(faction.factionID==GameControlTB.GetPlayerFactionID()){
				//~ faction.isPlayerControl=true;
				//~ GameControlTB.playerFactionTurnID=i;
				//~ GameControlTB.playerFactionExisted=true;
			//~ }
			
			allFactionList.Add(faction);
		}
		
		GameControlTB.instance._playerFactionTurnID=GameControlTB.playerFactionTurnID;
	}
	
	public static Texture GetFactionIcon(int ID){
		if(instance!=null){
			foreach(Faction faction in instance.allFactionList){
				if(faction.factionID==ID) return faction.icon;
			}
		}
		return null;
	}
	
	public static Color GetFactionColor(int ID){
		if(instance!=null){
			foreach(Faction faction in instance.allFactionList){
				if(faction.factionID==ID) return faction.color;
			}
		}
		return Color.white;
	}
//end faction related code
//***********************************************************************************************************************
	
	
	
	
	
	
//*****************************************************************************************************************************
//code related for unit placement in unit placement phase
	
	
	private int facPlacementID=0;
	public static int GetFactionPlacementID(){
		return instance.unitPlacementID;
	}
	public static PlayerUnits GetPlayerUnitsBeingPlaced(){
		return instance.playerUnits[instance.facPlacementID];
	}
	public static void NextFactionPlacementID(){
		ResetUnitPlacementID();
		if(instance.playerUnits[instance.facPlacementID].starting.Count==0){
			instance.playerUnits.RemoveAt(0);
		}
		if(onNewPlacementE!=null) onNewPlacementE(instance.playerUnits[0]);
		if(onPlacementUpdateE!=null) onPlacementUpdateE();
		//instance.facPlacementID+=1;
	}
	public static int GetPlayerUnitsRemainng(){
		return instance.playerUnits.Count;
	}
	
	private int unitPlacementID=0;
	public static int GetUnitPlacementID(){
		return instance.unitPlacementID;
	}
	public static void NextUnitPlacementID(){
		instance.unitPlacementID+=1;
		if(instance.unitPlacementID>=instance.playerUnits[instance.facPlacementID].starting.Count) instance.unitPlacementID=0;
		
		//Debug.Log("next   "+instance.unitPlacementID);
	}
	public static void PrevUnitPlacementID(){
		instance.unitPlacementID-=1;
		if(instance.unitPlacementID<0) instance.unitPlacementID=instance.playerUnits[instance.facPlacementID].starting.Count-1;
		
		//Debug.Log("prev   "+instance.unitPlacementID);
	}
	public static void ResetUnitPlacementID(){
		instance.unitPlacementID=0;
	}
	
	//place unit at a tile
	public static void PlaceUnitAt(Tile tile){
		if(instance.playerUnits.Count==0){
			return;
		}
		if(instance.playerUnits[instance.facPlacementID].starting.Count==0){
			return;
		}
		
		PlayerUnits pUnits=instance.playerUnits[instance.facPlacementID];
		
		if(pUnits.starting[instance.unitPlacementID]==null){
			pUnits.starting.RemoveAt(instance.unitPlacementID);
			if(instance.unitPlacementID>=pUnits.starting.Count){
				instance.unitPlacementID-=1;
				//Debug.Log("next   "+instance.unitPlacementID+"    "+pUnits.starting.Count);
			}
			return;
		}
		
		UnitTB unit=pUnits.starting[instance.unitPlacementID];
		unit.transform.position=tile.thisT.position;
		unit.occupiedTile=tile;
		tile.unit=unit;
		pUnits.starting.RemoveAt(instance.unitPlacementID);
		instance.allUnits.Add(unit);
		
		GridManager.PlaceUnitAt(tile);
		
		if(instance.unitPlacementID>=pUnits.starting.Count){
			instance.unitPlacementID-=1;
			//Debug.Log("next   "+instance.unitPlacementID+"    "+pUnits.starting.Count);
		}
		
		//Debug.Log(instance.unitPlacementID);

		if(onPlacementUpdateE!=null) onPlacementUpdateE();
	}
	//remove a unit from the grid
	public static void RemoveUnit(UnitTB unit){
		GridManager.RemoveUnitAt(unit.occupiedTile);
		
		instance.playerUnits[instance.facPlacementID].starting.Insert(0, unit);
		unit.occupiedTile.unit=null;
		unit.occupiedTile=null;
		unit.transform.position=new Vector3(0, 9999, 0);
		instance.allUnits.Remove(unit);
		
		if(instance.unitPlacementID<0) instance.unitPlacementID=0;
		
		if(onPlacementUpdateE!=null) onPlacementUpdateE();
	}
	
	public static void AutoPlaceUnit(){
		instance._AutoPlaceUnit();
	}
	public void _AutoPlaceUnit(){
		List<Tile> tileList=GridManager.GetAllPlaceableTiles();
		
		for(int i=0; i<tileList.Count; i++){
			if(tileList[i].unit!=null){
				tileList.RemoveAt(i); i-=1;
			}
		}
		
		//for(int i=0; i<playerUnits.Count; i++){
			PlayerUnits pUnits=playerUnits[facPlacementID];
			
			while(pUnits.starting.Count>0){
				int ID=Random.Range(0, tileList.Count);
				PlaceUnitAt(tileList[ID]);	//tileList will be remove at GridManager when unit is placed
				if(tileList.Count<=0) break;
			}
			
		//}
	}
	public static bool IsAllFactionPlaced(){
		if(instance.playerUnits.Count==1 && instance.playerUnits[0].starting.Count==0) return true;
		return false;
	}
	public static bool IsFactionAllUnitPlaced(){
		if(instance.playerUnits[instance.facPlacementID].starting.Count==0) return true;
		return false;
	}
	
	
	
	/*
	//place unit at a tile
	public static void PlaceUnitAt(Tile tile){
		if(instance.startingUnit.Count==0) return;
		
		if(instance.startingUnit[instance.unitPlacementID]==null){
			instance.startingUnit.RemoveAt(instance.unitPlacementID);
			if(instance.unitPlacementID>=instance.startingUnit.Count){
				instance.unitPlacementID-=1;
			}
			return;
		}
		
		UnitTB unit=instance.startingUnit[instance.unitPlacementID];
		unit.transform.position=tile.thisT.position;
		unit.occupiedTile=tile;
		tile.unit=unit;
		instance.startingUnit.RemoveAt(instance.unitPlacementID);
		instance.allUnits.Add(unit);
		
		GridManager.PlaceUnitAt(tile);
		
		if(instance.unitPlacementID>=instance.startingUnit.Count){
			instance.unitPlacementID-=1;
		}
		
		//Debug.Log(instance.unitPlacementID);

		if(onPlacementUpdateE!=null) onPlacementUpdateE();
	}
	//remove a unit from the grid
	public static void RemoveUnit(UnitTB unit){
		GridManager.RemoveUnitAt(unit.occupiedTile);
		
		instance.startingUnit.Insert(0, unit);
		unit.occupiedTile.unit=null;
		unit.occupiedTile=null;
		unit.transform.position=new Vector3(0, 9999, 0);
		instance.allUnits.Remove(unit);
		
		if(instance.unitPlacementID<0) instance.unitPlacementID=0;
		
		if(onPlacementUpdateE!=null) onPlacementUpdateE();
	}
	
	public static void AutoPlaceUnit(){
		instance._AutoPlaceUnit();
	}
	public void _AutoPlaceUnit(){
		List<Tile> tileList=GridManager.GetAllPlaceableTiles();
		
		//to make sure there's sufficient tile
		//if(tileList.Count<startingUnit.Count){
		//	Debug.Log("not enough space");
		//	return;
		//}
		
		while(startingUnit.Count>0){
			int ID=Random.Range(0, tileList.Count);
			PlaceUnitAt(tileList[ID]);	//tileList will be remove at GridManager when unit is placed
			if(tileList.Count<=0) break;
		}
	}
	
	private int unitPlacementID=0;
	public static int GetUnitPlacementID(){
		return instance.unitPlacementID;
	}
	public static void NextUnitPlacementID(){
		instance.unitPlacementID+=1;
		if(instance.unitPlacementID>=instance.startingUnit.Count) instance.unitPlacementID=0;
	}
	//~ public static void ResetUnitPlacementID(){
		//~ instance.unitPlacementID=0;
	//~ }
	
	public static bool IsAllUnitPlaced(){
		if(instance.startingUnit.Count==0) return true;
		return false;
	}
	*/
	
	//to place a unit in runtime, untested, will probably cause issue with certain turnMode
	public static void InsertUnit(UnitTB unit, Tile tile, int factionID, int duration){
		if(unit==null){
			Debug.Log("no unit is specified");
			return;
		}
		
		if(tile.unit!=null){
			Debug.Log("tile is occupied");
			return;
		}
		
		GameObject unitObj=(GameObject)Instantiate(unit.gameObject);
		unit=unitObj.GetComponent<UnitTB>();
		unit.SetSpawnInGameFlag(true);
		if(duration>0) unit.SetSpawnDuration(duration);
		
		//if(unit.occupiedTile!=null){
		//	unit.occupiedTile.unit=null;
		//}
		
		unit.factionID=factionID;
		
		unit.transform.position=tile.thisT.position;
		unit.occupiedTile=tile;
		tile.unit=unit;
		
		for(int i=0; i<instance.allFactionList.Count; i++){
			Faction faction=instance.allFactionList[i];
			if(faction.factionID==factionID){
				faction.allUnitList.Add(unit);
				break;
			}
		}
		
		instance.allUnits.Add(unit);
		
		if(onNewUnitInRuntimeE!=null) onNewUnitInRuntimeE(unit);
		
		selectedUnit.occupiedTile.Select();
	}
	
	
	
	public delegate void UnitFactionChangedHandler(UnitTB unit); 
	public static event UnitFactionChangedHandler onUnitFactionChangedE;
	
	public static void ChangeUnitFaction(UnitTB scUnit, int targetFactionID){
		Faction srcFaction=null;
		Faction tgtFaction=null;
		
		for(int i=0; i<instance.allFactionList.Count; i++){
			Faction faction=instance.allFactionList[i];
			if(faction.factionID==scUnit.factionID){
				srcFaction=faction;
			}
		}
		
		for(int i=0; i<instance.allFactionList.Count; i++){
			Faction faction=instance.allFactionList[i];
			if(faction.factionID==targetFactionID){
				tgtFaction=faction;
			}
		}
		
		if(srcFaction!=null && tgtFaction!=null){
			for(int i=0; i<srcFaction.allUnitList.Count; i++){
				UnitTB unit=srcFaction.allUnitList[i];
				if(unit==scUnit){
					srcFaction.allUnitList.RemoveAt(i);
				}
			}
			tgtFaction.allUnitList.Add(scUnit);
			
			scUnit.factionID=targetFactionID;
			
			if(onUnitFactionChangedE!=null) onUnitFactionChangedE(scUnit);
		}
		else{
			Debug.Log("faction doesn't exist");
		}
	}
	
	
	
	
//end code related for unit placement in unit placement phase
//*****************************************************************************************************************************
	
	
	
	

	//reset the yetToMove list in each faction
	//for random MoveOrder, called in every new round event
	public static void ResetFactionUnitMoveList(){
		for(int n=0; n<instance.allFactionList.Count; n++){
			Faction faction=instance.allFactionList[n];
			
			faction.unitYetToMove=new List<int>();
			for(int i=0; i<faction.allUnitList.Count; i++) faction.unitYetToMove.Add(i);
		}
	}
	//called to register that a unit has been moved
	//registered which unit has been moved for each faction so they wont been selected again in the same round
	public static void MoveUnit(UnitTB unit){
		if(GameControlTB.GetMoveOrder()!=_MoveOrder.Free) return;
		
		bool match=false;
		
		//get the matching faction
		for(int n=0; n<instance.allFactionList.Count; n++){
			if(unit.factionID==instance.allFactionList[n].factionID){
				//get the matching unit
				for(int i=0; i<instance.allFactionList[n].allUnitList.Count; i++){
					if(instance.allFactionList[n].allUnitList[i]==unit){
						//~ if(unit.IsAllActionCompleted()){
							//remove the unit ID form yetToMove list
							instance.allFactionList[n].unitYetToMove.Remove(i);
							//instance.allFactionList[n].numOfUnitMoved+=1;
							if(instance.allFactionList[n].unitYetToMove.Count==0)
								instance.allFactionList[n].allUnitMoved=true;
						//~ }
						
						match=true;
						break;
					}
				}
			}
		}
		
		if(!match) Debug.Log("Error: no unit found trying to registered a unit moved ");
	}
	
	
	
	
//*************************************************************************************************************************************
//initiating unit turn for each mode
	
	//for turnMode of FactionBasedSingleUnit, each faction takes turn to move one unit at each turn, 
	//unit with highest initiative in the faction get to move first
	//this function arrange unitList in all faction accordingly, unit will be place with their speed in descending order
	
	//for StatsBased MoveOrder
	//arrange the units of within each faction locally based on the turnPriority in descending order, unit with the highest value goes first
	public static void ArrangeFactionUnitOnTurnPriority(){
		foreach(Faction faction in instance.allFactionList){
			List<UnitTB> tempList=new List<UnitTB>();
			while(faction.allUnitList.Count>0){
				float currentHighest=-1;
				int highestID=0;
				for(int i=0; i<faction.allUnitList.Count; i++){
					if(faction.allUnitList[i].GetTurnPriority()>currentHighest){
						currentHighest=faction.allUnitList[i].GetTurnPriority();
						highestID=i;
					}
				}
				tempList.Add(faction.allUnitList[highestID]);
				faction.allUnitList.RemoveAt(highestID);
			}
			faction.allUnitList=tempList;
		}
	}
	
	
	//for SingleUnitPerTurn TurnMode
	//arrange the all unit in the game based on the turnPriority in descending order, unit with the highest value goes first
	public static void ArrangeAllUnitOnTurnPriority(){
		List<UnitTB> tempList=new List<UnitTB>();
		
		while(instance.allUnits.Count>0){
			float currentHighest=-Mathf.Infinity;
			int highestID=0;
			for(int i=0; i<instance.allUnits.Count; i++){
				if(instance.allUnits[i].GetTurnPriority()>currentHighest){
					currentHighest=instance.allUnits[i].GetTurnPriority();
					highestID=i;
				}
			}
			tempList.Add(instance.allUnits[highestID]);
			instance.allUnits.RemoveAt(highestID);
		}
		
		instance.allUnits=tempList;
	}
	
	
	//for random MoveOrder, randomly rearrange the units within all faction
	public static void ShuffleAllUnit(){
		for(int n=0; n<instance.allFactionList.Count; n++){
			Faction faction=instance.allFactionList[n];
			
			List<UnitTB> shuffledList=new List<UnitTB>();
			
			List<int> IDList=new List<int>();
			for(int i=0; i<faction.allUnitList.Count; i++) IDList.Add(i);
			
			for(int i=0; i<faction.allUnitList.Count; i++){
				int rand=UnityEngine.Random.Range(0, IDList.Count);
				int ID=IDList[rand];
				shuffledList.Add(faction.allUnitList[ID]);
				IDList.RemoveAt(rand);
			}
			
			faction.allUnitList=shuffledList;
		}
	}
	
	
	//no longer in use
	//for turnMode of UnitPriorityBasedNoCap, all unit simply take turn to move based on stats, there will be no round,
	//first calculate the unit baseTurnPriority and turn priority, then rearrange the unitList so the first unit will get to move first
	/*
	public static void ArrangeAllUnitToTurnPriorityNoCap(){
		float currentHighest=-1;
		for(int i=0; i<instance.allUnits.Count; i++){
			if(instance.allUnits[i].speed>currentHighest){
				currentHighest=instance.allUnits[i].speed;
			}
		}
		
		for(int i=0; i<instance.allUnits.Count; i++){
			instance.allUnits[i].turnPriority=currentHighest/instance.allUnits[i].speed;
			instance.allUnits[i].baseTurnPriority=instance.allUnits[i].turnPriority;
		}
		
		ArrangeAllUnitBasedOnTurnPriority();
	}
	*/
	
//end initiating unit turn for each mode
//*************************************************************************************************************************************
	
	
	
	
	void OnBattleEnd(int vicFactionID){
		//if persistant data mode is used, save the data
		if(!GameControlTB.IsHotSeatMode() && GameControlTB.playerFactionExisted){
			if(GameControlTB.LoadMode()!=_LoadMode.UseCurrentData){
				List<UnitTB> remainingUnit=new List<UnitTB>();
				if(undeployedUnit.Count>0){
					remainingUnit=undeployedUnit;
					List<UnitTB> tempList=GetAllUnitsOfFaction(GameControlTB.GetPlayerFactionIDS()[0]);
					foreach(UnitTB unit in tempList){
						if(!unit.GetSpawnInGameFlag()) remainingUnit.Add(unit);
					}
				}
				else{
					List<UnitTB> tempList=GetAllUnitsOfFaction(GameControlTB.GetPlayerFactionIDS()[0]);
					foreach(UnitTB unit in tempList) if(!unit.GetSpawnInGameFlag()) remainingUnit.Add(unit);
				}
					
				if(GameControlTB.LoadMode()!=_LoadMode.UsePersistantData)
					GlobalStatsTB.SetPlayerUnitList(remainingUnit);
				else if(GameControlTB.LoadMode()!=_LoadMode.UseTemporaryData){
					//GlobalStatsTB.SetTempPlayerUnitList(remainingUnit);
					
				}
			}
		}
	}
	
	void OnUnitDestroyed(UnitTB unit){
		if(unit==selectedUnit){
			//OnUnitDeselected();
			GridManager.Deselect();
		}
		
		//reduce currentUnitTurnID by 1 if the unit priority is earlier than this.
		//otherwise the next unit following the current unit selected will be skipped
		//for unitprioritybased turn mode only
		if(GameControlTB.GetTurnMode()==_TurnMode.SingleUnitPerTurn){
			int ID=allUnits.IndexOf(unit);
			if(ID<=currentUnitTurnID) currentUnitTurnID-=1;
		}
		
		allUnits.Remove(unit);
		
		//check if the faction still has other active unit.
		int factionID=unit.factionID;
		Faction faction=GetFaction(factionID);
		if(faction==null) Debug.Log("Error? cant find supposed faction with ID:"+factionID);
		
		//set the turn order, etc.
		bool match=false;
		if(GameControlTB.GetMoveOrder()==_MoveOrder.Free){
			for(int i=0; i<faction.allUnitList.Count; i++){
				if(faction.allUnitList[i]==unit){
					match=true;
					if(faction.unitYetToMove.Contains(i)){
						faction.unitYetToMove.Remove(i);
						if(faction.unitYetToMove.Count==0) 
							faction.allUnitMoved=true;
					}
					for(int n=0; n<faction.unitYetToMove.Count; n++){
						if(faction.unitYetToMove[n]>i){
							faction.unitYetToMove[n]-=1;
						}
					}
					break;
				}
			}
			if(!match) Debug.Log("Error? cant find unit in supposed faction");
			
			faction.allUnitList.Remove(unit);
		}
		else{
			//reduce currentUnitTurnID by 1 if the unit priority is earlier than this.
			int ID=faction.allUnitList.IndexOf(unit);
			if(ID<=faction.currentTurnID){
				faction.currentTurnID-=1;
				if(faction.currentTurnID<0)
					faction.currentTurnID=0;
			}
			
			faction.allUnitList.Remove(unit);
			
			if(CheckUnmovedUnitInFaction(faction)==0) faction.allUnitMoved=true;
		}
		
		
		//check if faction is still in play
		if(faction.allUnitList.Count==0){
			activeFactionCount-=1;
			if(onFactionDefeatedE!=null) onFactionDefeatedE(factionID);
		}
		
		//check if all faction has been defeated
		if(activeFactionCount==1){
			List<Faction> factionWithUnit=GetFactionsWithUnitRemain();
			if(factionWithUnit.Count==1){
				GameControlTB.BattleEnded(factionWithUnit[0].factionID);
				//if(factionWithUnit[0].isPlayerControl) GameControlTB.BattleEnded(true);
				//else GameControlTB.BattleEnded(false);
			}
			else if(factionWithUnit.Count<=0){
				Debug.Log("all unit is dead. error?");
			}
		}
		
		//for abilities duration tracking
		for(int i=0; i<allUnits.Count; i++){
			allUnits[i].ReduceUnitAbilityCountTillNextDown();
		}
	}
	
	
//*****************************************************************************************************************************
//Utility function
	
	//check how many unit within the faction has been moved
	public static int CheckMovedUnitInFaction(Faction faction){
		int count=0;
		for(int i=0; i<faction.allUnitList.Count; i++){
			if(faction.allUnitList[i].IsAllActionCompleted()) count+=1;
		}
		return count;
	}
	//check how many unit within the faction has not been moved
	public static int CheckUnmovedUnitInFaction(Faction faction){
		int count=0;
		for(int i=0; i<faction.allUnitList.Count; i++){
			if(!faction.allUnitList[i].IsAllActionCompleted()) count+=1;
		}
		return count;
	}
	
	//return a list of faction that still have active unit in the game
	List<Faction> GetFactionsWithUnitRemain(){
		List<Faction> factionWithUnit=new List<Faction>();
		foreach(Faction faction in allFactionList){
			if(faction.allUnitList.Count>0){
				factionWithUnit.Add(faction);
			}
		}
		return factionWithUnit;
	}
	public static int GetActiveFactionsCount(){
		return activeFactionCount;
	}
	
	//check if faction still have a unit in the game
	public static bool IsFactionStillActive(int ID){
		if(ID<0 || ID>=instance.allFactionList.Count){
			Debug.Log("Faction doesnt exist");
			return false;
		}
		if(instance.allFactionList[ID].allUnitList.Count>0) return true;
		return false;
	}
	
	//check if all units within a faction has moved
	public static bool AllUnitInFactionMoved(int factionID){
		if(instance.allFactionList[factionID].allUnitMoved) return true;
		return false;
	}
	
	//check if all units of all factions has moved
	public static bool AllUnitInAllFactionMoved(){
		for(int i=0; i<instance.allFactionList.Count; i++){
			if(!instance.allFactionList[i].allUnitMoved) return false;
		}
		return true;
	}
	
	public static Faction GetFactionInTurn(int turnID){
		return instance.allFactionList[turnID];
	}
	public static Faction GetFaction(int factionID){
		for(int i=0; i<instance.allFactionList.Count; i++){
			if(instance.allFactionList[i].factionID==factionID){
				return instance.allFactionList[i];
			}
		}
		return null;
	}
	//~ public static Faction GetPlayerFaction(){
		//~ int factionID=GameControlTB.GetPlayerFactionID();
		//~ for(int i=0; i<instance.allFactionList.Count; i++){
			//~ if(instance.allFactionList[i].factionID==factionID){
				//~ return instance.allFactionList[i];
			//~ }
		//~ }
		//~ return null;
	//~ }
	
	//return the ID of the first player faction in all faction List, first faction gets to move first
	public static int GetPlayerFactionTurnID(){
		List<int> factionIDs=GameControlTB.GetPlayerFactionIDS();
		for(int i=0; i<instance.allFactionList.Count; i++){
			if(factionIDs.Contains(instance.allFactionList[i].factionID)){
				return i;
			}
		}
		return -1;
	}
	
	
	//return all the unplaced unit during unit placement phase
	public static List<UnitTB> GetUnplacedUnit(){
		return instance.playerUnits[instance.facPlacementID].starting;
		//return instance.playerUnits[0].starting;
		//return instance.startingUnit;
	}
	//return all unit, unplaced unit not included
	public static List<UnitTB> GetAllUnit(){
		return instance.allUnits;
	}
	
	public static int GetAllUnitCount(){
		return instance.allUnits.Count;
	}
	
	public static List<UnitTB> GetAllUnitsOfFaction(int factionID){
		List<UnitTB> list=new List<UnitTB>();
		foreach(UnitTB unit in instance.allUnits){
			if(unit.factionID==factionID){
				list.Add(unit);
			}
		}
		return list;
	}
	
	//get all unit hostile to the faction with faction ID given
	public static List<UnitTB> GetAllHostile(int factionID){
		List<UnitTB> list=new List<UnitTB>();
		foreach(UnitTB unit in instance.allUnits){
			if(unit.factionID!=factionID){
				list.Add(unit);
			}
		}
		return list;
	}
	
	//called by AIManager
	public static UnitTB GetNearestHostile(UnitTB srcUnit){
		UnitTB targetUnit=null;
		float currentNearest=Mathf.Infinity;
		foreach(UnitTB unit in instance.allUnits){
			if(unit.factionID!=srcUnit.factionID){
				float dist=Vector3.Distance(srcUnit.occupiedTile.pos, unit.occupiedTile.pos);
				if(dist<currentNearest){
					targetUnit=unit;
					currentNearest=dist;
				}
			}
		}
		return targetUnit;
	}
	
	public static UnitTB GetNearestHostileFromList(UnitTB srcUnit, List<UnitTB> targets){
		UnitTB targetUnit=null;
		float currentNearest=Mathf.Infinity;
		for(int i=0; i<targets.Count; i++){
			float dist=Vector3.Distance(srcUnit.occupiedTile.pos, targets[i].occupiedTile.pos);
			if(dist<currentNearest){
				targetUnit=targets[i];
				currentNearest=dist;
			}
		}
		return targetUnit;
	}
	
	public static List<UnitTB> GetUnitInLOSFromList(UnitTB srcUnit, List<UnitTB> targets){
		List<UnitTB> visibleList=GetAllUnitsOfFaction(srcUnit.factionID);
		for(int i=0; i<targets.Count; i++){
			if(GridManager.IsInLOS(srcUnit.occupiedTile, targets[i].occupiedTile)){
				visibleList.Add(targets[i]);
			}
		}
		return visibleList;
	}
	
	public static List<UnitTB> GetAllHostileWithinFactionSight(int factionID){
		List<UnitTB> AllFriendlies=GetAllUnitsOfFaction(factionID);
		List<UnitTB> AllHostiles=GetAllHostile(factionID);
		List<UnitTB> hostilesInSight=new List<UnitTB>();
		for(int i=0; i<AllFriendlies.Count; i++){
			UnitTB friendly=AllFriendlies[i];
			for(int j=0; j<AllHostiles.Count; j++){
				UnitTB hostile=AllHostiles[j];
				if(GridManager.IsInLOS(friendly.occupiedTile, hostile.occupiedTile)){
					if(GridManager.Distance(friendly.occupiedTile, hostile.occupiedTile)<=friendly.GetSight()){
						hostilesInSight.Add(hostile);
					}
				}
			}
		}
		return hostilesInSight;
	}
	
	public static List<UnitTB> GetAllHostileWithinUnitSight(UnitTB srcUnit){
		List<UnitTB> potentialTargets=GetAllHostile(srcUnit.factionID);
		List<UnitTB> hostilesInUnitSight=new List<UnitTB>();
		for(int i=0; i<potentialTargets.Count; i++){
			if(GridManager.IsInLOS(srcUnit.occupiedTile, potentialTargets[i].occupiedTile)){
				hostilesInUnitSight.Add(potentialTargets[i]);
			}
		}
		return hostilesInUnitSight;
	}
	
	//get possible target from a specific tile for a faction
	public static List<UnitTB> GetHostileInRangeFromTile(Tile srcTile, UnitTB srcUnit){
		return GetHostileInRangeFromTile(srcTile, srcUnit, false);
	}
	public static List<UnitTB> GetVisibleHostileInRangeFromTile(Tile srcTile, UnitTB srcUnit){
		return GetHostileInRangeFromTile(srcTile, srcUnit, true);
	}
	public static List<UnitTB> GetHostileInRangeFromTile(Tile srcTile, UnitTB srcUnit, bool visibleOnly){
		int unitMinRange=srcUnit.GetUnitAttackRangeMin();
		int unitMaxRange=srcUnit.GetUnitAttackRangeMax();
		List<Tile> tilesInRange=GridManager.GetTilesWithinRange(srcTile, unitMinRange, unitMaxRange);
		
		List<UnitTB> hostilesList=new List<UnitTB>();
		
		for(int i=0; i<tilesInRange.Count; i++){
			if(tilesInRange[i].unit!=null && tilesInRange[i].unit.factionID!=srcUnit.factionID){
				if(visibleOnly){
					if(GridManager.IsInLOS(srcTile, tilesInRange[i])){
						hostilesList.Add(tilesInRange[i].unit);
					}
				}
				else{
					hostilesList.Add(tilesInRange[i].unit);
				}
			}
		}
		
		return hostilesList;
	}
	
	
	
//*************************************************************************************************************
//external code, not in used
	
	/*
	private bool isWarCorpAlpha=true;
	
	void WarCorpInitRoutine(){
		if(GameControlTB.LoadMode()==_LoadMode.UseTemporaryData){
			List<Mech> squadList=UnitManager.GetSquadList();
			for(int i=0; i<startingUnit.Count; i++){
				if(squadList[i]!=null){
					startingUnit[i].instanceID=squadList[i].instanceID;
				}
			}
			
			
			//List<Mech> squadList=UnitManager.GetSquadList();
			//for(int i=0; i<squadList.Count; i++){
			//	if(squadList[i]==null){
			//		squadList.RemoveAt(i);
			//		startingUnit.RemoveAt(i);
			//		i-=1;
			//	}
			//}
			
			
			for(int i=0; i<startingUnit.Count; i++){
				if(startingUnit[i]!=null){
					Mech mech=squadList[i];
					UnitTB unit=startingUnit[i];
					
					unit.unitName=mech.name;
					
					unit.rangeDamageMin=mech.weap.damageMin;
					unit.rangeDamageMax=mech.weap.damageMax;
					unit.criticalRange=mech.weap.critChance;
					unit.APCostAttack=mech.weap.apCost;
					
					unit.attack=mech.pilot.attack;
					unit.defend=mech.pilot.defend*0.5f;
					
					unit.HP=(int)Mathf.Round(mech.status*0.01f*unit.GetFullHP());
					
					if(mech.weap.weapClass==mech.pilot.weapClass) unit.attack+=0.05f;
					if(mech.mechClass==mech.pilot.mechClass) unit.defend+=0.05f;
				}
			}
			
			Debug.Log(squadList.Count);
		}
	}
	*/
	
//end external code
	
	
	
	
	
	public void OnDrawGizmos(){
		if(selectedUnit!=null){
			//Debug.Log(selectedUnit+"   "+selectedUnit.moved+"   "+selectedUnit.attacked+"   "+ selectedUnit.transform.position);
			//Gizmos.DrawSphere(selectedUnit.transform.position, 1);
		}
	}
}
