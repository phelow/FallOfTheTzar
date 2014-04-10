#pragma warning disable 0649 // variable declared but value not assigned
//#pragma warning disable 0168 // variable declared but not used.
//#pragma warning disable 0219 // variable assigned but not used.
//#pragma warning disable 0414 // private field assigned but not used.


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum _TurnMode{
	FactionAllUnitPerTurn, 				//each faction take turn to move all units in each round
	FactionSingleUnitPerTurnSingle,	//each faction take turn to move a single unit in each round
	FactionSingleUnitPerTurnAll, 		//each faction take turn to move a single unit in each turn, when all unit is moved, the round is completed
	SingleUnitPerTurn,					//all units (regardless of faction) take turn to move according to the stats, when all unit is moves, the round is completed
	//SingleUnitPerTurnNoRound			//not in use
	
	
	SingleUnitRealTime,				//all units (regardless of faction) take turn to move according to the stats, faster unit may move more, round is complete when all unit is moved
	SingleUnitRealTimeNoRound
}

public enum _MoveOrder{
	Free, 				//unit switching is enabled
	FixedRandom, 		//random fix an order and follow the order throughout
	FixedStatsBased	//arrange the order based on unit's stats
}

public enum _LoadMode{UsePersistantData, UseTemporaryData, UseCurrentData}


public enum _CounterAttackRule{None, flexible, Always}
public enum _MovementAPCostRule{None, PerMove, PerTile}
public enum _AttackAPCostRule{None, PerAttack}

//not in used
/*
[System.Serializable]
public class Resource{
	public int ID=-1;
	public string name="resource";
	public Texture icon;
	public int value=100;
}

*/

//not in used
public enum _ObjectiveType{
	Default, 		//clear all AI to win
	FlagGrab,	//secure a key item to win
	Defend, 		//survive for however many round
	Escort 		//get an additional unit from a to b
}


[RequireComponent (typeof (UnitControl))]
[RequireComponent (typeof (DamageTable))]
[RequireComponent (typeof (AbilityManagerTB))]
[RequireComponent (typeof (AIManager))]
public class GameControlTB : MonoBehaviour {

	public delegate void BattleStartHandler(); 
	public static event BattleStartHandler onBattleStartE;
	
	public delegate void BattleEndHandler(int vicFactionID); 
	public static event BattleEndHandler onBattleEndE;
	
	public delegate void NextTurnHandler(); 
	public static event NextTurnHandler onNextTurnE;
	
	public delegate void NewRoundHandler(int round); 
	public static event NewRoundHandler onNewRoundE;
	
	public delegate void GameMessageHandler(string msg); 
	public static event GameMessageHandler onGameMessageE;
	
	#if UNITY_IPHONE || UNITY_ANDROID
	//an event handler to let the ui know weather to show the unit info button, true if there's a unit, else false.
	//for NGUI free only
	public delegate void UnitInfoHandler(Tile tile); 
	public static event UnitInfoHandler onUnitInfoE;
	#endif
	
	public _TurnMode turnMode;
	public _MoveOrder moveOrder;
	
	public static bool battleEnded=false;
	
	private static bool actionInProgress=false;
	
	//for local multiplayer, not fully implemented yet
	public bool hotseat=false;
	public List<int> playerFactionID=new List<int>();
	public static List<int> playerFactionTurnID=new List<int>();
	public List<int> _playerFactionTurnID=new List<int>();
	//public int playerFactionID=0;
	//public static int playerFactionTurnID=-1;
	public static bool playerFactionExisted=false;
	
	
	public static int turnID=-1;
	public static int turnIDLoop=0;	//to know when all faction has been cycled, this value should never exceed totalFactionInGame
	
	public static int totalFactionInGame=2;
	
	//a counter indicate the number of round played
	public static int roundCounter=0;
	private float newRoundCD=1.0f;
	
	public _LoadMode loadMode=_LoadMode.UseCurrentData;
	public int winPointReward=20;
	[HideInInspector] public int pointGain=0;
	
	public string nextScene="";
	public string mainMenu="";
	
	public bool enablePerkMenu=true;
	public static bool EnablePerkMenu(){ 
		if(instance==null) return false;
		return instance.enablePerkMenu;
	}
	
	public bool enableUnitPlacement=true;
	
	public bool enableCounterAttack;	//is counter-attack enabled in the game
	public bool fullAPOnStart=true;
	public bool fullAPOnNewRound=false;	//restore full ap to all unit at a new round
	
	public _MovementAPCostRule movementAPCostRule;	//
	//public int movementAPCost=1;
	
	public _AttackAPCostRule attackAPCostRule;
	//public int attackAPCost=1;
	
	
	public bool enableCover=false;
	public float coverBonusHalf=0.25f;
	public float coverBonusFull=0.5f;
	public float exposedCritBonus=0.3f;
	public bool enableFogOfWar=false;
	
	
	public bool allowMovementAfterAttack=false;
	public bool allowAbilityAfterAttack=false;
	
	
	//frequency of actionCam, 0-disabled, 1-always on
	public float actionCamFrequency=0.5f;
	
	private static bool unitSwitchingLocked=false;//to prevent unit switching in factionbasedSingleUnit turnMode after a unit has moved/attacked/used ability
	public bool allowUnitSwitching=true;	//for unit switching in factionbasedSingleUnit turnMode
	
	//for external use
	public _ObjectiveType objectiveType;
	
	public static GameControlTB instance;
	
	void Awake(){
		instance=this;
		
		turnID=-1;
		turnIDLoop=1;
		
		AbilityManagerTB.LoadUnitAbility();
		
		roundCounter=0;
		battleEnded=false;
		
		if(playerFactionID.Count==0) playerFactionID.Add(0);
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	void OnEnable(){
		UnitTB.onActionCompletedE += OnActionCompleted;
		UnitTB.onTurnDepletedE += UnitActionDepleted;
		UnitTB.onUnitDestroyedE+=OnUnitDestroyed;
	}
	
	void OnDisable(){
		UnitTB.onActionCompletedE -= OnActionCompleted;
		UnitTB.onTurnDepletedE -= UnitActionDepleted;
		UnitTB.onUnitDestroyedE-=OnUnitDestroyed;
	}
	
	void OnUnitDestroyed(UnitTB unit){
		if(playerFactionExisted && !hotseat){
			if(!playerFactionID.Contains(unit.factionID)){
				GainPoint(unit.pointReward);
			}
		}
	}
	
	public static void GainPoint(int val){
		if(instance==null) return;
		instance.pointGain+=val;
	}
	
	
	//function call to set actionInProgress flag to true
	//when true, all user input are disabled
	public static bool ActionCommenced(){
		LockUnitSwitching();
		if(actionInProgress) return false;
		actionInProgress=true;
		return true;
	}
	
	//function call to set actionInProgress flag to false
	//re-enable user input
	public static void OnActionCompleted(UnitTB unit){
		actionInProgress=false;
	}
	
	//note: switching to coroutine will break unitSwitchingLocked
	IEnumerator _OnActionCompleted(){
		yield return new WaitForSeconds(0);
		actionInProgress=false;
	}
	
	
	//unit action depleted event handler
	//called when a unit has used all it's action
	public void UnitActionDepleted(){
		if(instance.turnMode==_TurnMode.FactionAllUnitPerTurn){
			//if it's player faction and there are unmoved unit within the faction
			if(playerFactionTurnID.Contains(turnID) && !UnitControl.AllUnitInFactionMoved(turnID)){
				UnitControl.SwitchToNextUnit();
			}
			return;
		}
		OnEndTurn();
	}
	
	//handler when a unit/faction completed it's turn
	public static void OnEndTurn(){
		instance.StartCoroutine(instance._OnEndTurn());
	}
	IEnumerator _OnEndTurn(){
		yield return null;
		
		if(GetMoveOrder()==_MoveOrder.Free) unitSwitchingLocked=false;
		
		if(instance.turnMode==_TurnMode.FactionAllUnitPerTurn){
			if(GetMoveOrder()==_MoveOrder.Free){
				MoveToNextTurn();
			}
			else{
				if(!UnitControl.AllUnitInFactionMoved(turnID)){
					UnitControl.OnNextUnit();
				}
				else{
					MoveToNextTurn();
				}
			}
		}
		else if(instance.turnMode==_TurnMode.FactionSingleUnitPerTurnSingle){
			MoveToNextTurn();
		}
		else if(instance.turnMode==_TurnMode.FactionSingleUnitPerTurnAll){
			if(UnitControl.AllUnitInAllFactionMoved()){
				GridManager.Deselect();
				ResetTurnID();
				OnNewRound();
			}
			else{
				//first check if all the unit has been moved, if yes, switch to next faction
				NextTurnID(); 	if(turnID>=totalFactionInGame) turnID=0;
				int counter=0;
				while(UnitControl.AllUnitInFactionMoved(turnID)){
					NextTurnID(); 	if(turnID>=totalFactionInGame) turnID=0;
					//if all the faction has been looped through
					if((counter+=1)>totalFactionInGame){
						Debug.Log("error, no available faction");
						//return;
						yield break;
					}
				}
				//UnitControl.OnNextUnit();
				instance.StartCoroutine(instance.OnNextTurn());
			}
		}
		else if(instance.turnMode==_TurnMode.SingleUnitPerTurn){
			instance.StartCoroutine(instance.OnNextTurn());
		}
		else if(instance.turnMode==_TurnMode.SingleUnitRealTime || instance.turnMode==_TurnMode.SingleUnitRealTimeNoRound){
			instance.StartCoroutine(instance.OnNextTurn());
		}
	}
	
	
	//a short cut function derives from OnEndTurn() which actually end the current turn
	public static void MoveToNextTurn(){
		GridManager.Deselect();	//probably not needed,
		
		NextTurnID();
		if(turnIDLoop>totalFactionInGame){
			turnIDLoop=1;
			OnNewRound();
			return;
		}
		
		while(!UnitControl.IsFactionStillActive(turnID)){
			NextTurnID();
			if(turnIDLoop>totalFactionInGame){
				turnIDLoop=1;
				OnNewRound();
				return;
			}
		}
		
		instance.StartCoroutine(instance.OnNextTurn());
	}
	
	
	//called by UnitControl to start a new round, fire newRound event
	public static void OnNewRound(){
		if(roundCounter==0) ResetTurnID();
		roundCounter+=1;
		unitSwitchingLocked=false;
		instance.StartCoroutine(instance._OnNewRound());
	}
	IEnumerator _OnNewRound(){
		yield return null;
		if(battleEnded) yield break;
		
		//delay a bit before the game is allowed to progress
		yield return new WaitForSeconds(newRoundCD*0.25f);
		actionInProgress=true;
		
		if(onGameMessageE!=null) onGameMessageE("Round "+roundCounter);
		
		if(onNewRoundE!=null) onNewRoundE(roundCounter);
		
		//delay a bit before the game is allowed to progress
		yield return new WaitForSeconds(newRoundCD*0.85f);
		actionInProgress=false;
		StartCoroutine(OnNextTurn());
		yield return null;
	}
	
	
	//called by UnitControl to start switch turn to next faction, fire newTurn event
	IEnumerator OnNextTurn(){
		yield return null;
		if(onNextTurnE!=null) onNextTurnE();
	}

	
	
	
	//battle ended
	public static void BattleEnded(int vicFactionID){
		battleEnded=true;
		
		if(instance.playerFactionID.Contains(vicFactionID)){
		//if(vicFactionID==GetPlayerFactionID()){
			GainPoint(instance.winPointReward);
			
			//if using persistent data, save the point gain in this level
			if(instance.loadMode==_LoadMode.UsePersistantData){
				//instance.pointGain+=instance.winPointReward;
				
				GlobalStatsTB.GainPlayerPoint(instance.pointGain);
			}
		}
		
		if(onBattleEndE!=null) onBattleEndE(vicFactionID);
	}	
	
	
	//reset turnID, called when battle start
	public static void ResetTurnID(){
		//if there's a valid ID, assign to it
		int factionID=UnitControl.GetPlayerFactionTurnID();
		if(factionID>=0) turnID=factionID;
		else turnID=0;
		
		turnIDLoop=1;
	}
	//forward to next turnID
	public static void NextTurnID(){
		turnID+=1; 	turnIDLoop+=1;
		if(turnID>=totalFactionInGame) turnID=0;
	}
	
	
//*****************************************************************************************************************************
//player faction ID related function
	
	public static bool IsPlayerTurn(){
		if(playerFactionExisted){
			if(instance.turnMode==_TurnMode.SingleUnitPerTurn){
				if(IsPlayerFaction(UnitControl.selectedUnit.factionID)) return true;
			}
			else{
				if(playerFactionTurnID.Contains(turnID)) return true;
			}
		}
		return false;
	}
	
	public static bool IsHotSeatMode(){
		return instance.hotseat;
	}
	//for checking if a unit belongs to a player's faction
	public static bool IsPlayerFaction(int ID){
		if(instance.playerFactionID.Contains(ID)) return true;
		return false;
	}
	public static List<int> GetPlayerFactionIDS(){
		return instance.playerFactionID;
	}
	//get the player factionID that is in current turn
	public static int GetCurrentPlayerFactionID(){
		for(int i=0; i<playerFactionTurnID.Count; i++){
			if(playerFactionTurnID[i]==turnID){
				return instance.playerFactionID[i];
			}
		}
		return -1;
	}
	//default get playerFactionID function
	public static int GetPlayerFactionID(){
		//if(playerFactionExisted) return instance.playerFactionID[0];
		//else return -1;
		return instance.playerFactionID[0];
	}
	
	
	

//*****************************************************************************************************************************
//UI & input related code

	// Update is called once per frame
	void Update () {
		//for touch input on mobile device
		#if UNITY_IPHONE || UNITY_ANDROID
			if(Input.touchCount==1){
				Touch touch=Input.touches[0];
				//if(touch.phase == TouchPhase.Ended){
				if(touch.phase == TouchPhase.Began){
					if(!IsCursorOnUI(Input.mousePosition)){
						Ray ray = Camera.main.ScreenPointToRay(touch.position);
						RaycastHit hit;
						LayerMask mask=1<<LayerManager.GetLayerTile();
						if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
							Tile tile=hit.collider.gameObject.GetComponent<Tile>();
							/**/
							//if this is the second tap on the tile
							if(tile==lastTileTouched){
								lastTileTouched=null;
								tile.OnTouchMouseDown();
							}
							//if the tile is a new one
							else{
								//clear any effect off previously selected tile
								if(lastTileTouched!=null) lastTileTouched.OnTouchMouseExit();
								//if the tile contain friendly unit, select it directly
								if(tile.unit!=null && tile.unit.factionID==0){
									lastTileTouched=tile;
									tile.OnTouchMouseEnter();
									tile.OnTouchMouseDown();
								}
								//a new tile with no friendly unit,  just call mouse tnter
								else{
									lastTileTouched=tile;
									tile.OnTouchMouseEnter();
								}
							}
							//*/
						}
						else{
							if(lastTileTouched!=null){
								lastTileTouched.OnTouchMouseExit();
								lastTileTouched=null;
							}
						}
						
						if(onUnitInfoE!=null) onUnitInfoE(lastTileTouched);
					}
				}
			}
		#else	
			//enable this section and disable OnMouseEnter, OnMouseExit, OnMouseDown, and OnMouseOver in Tile.cs to emulate touch device input scheme on desktop
			/*
			if(Input.GetMouseButtonUp(0)){
				if(!IsCursorOnUI(Input.mousePosition)){
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hit;
					LayerMask mask=1<<LayerManager.GetLayerTile();
					if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
						Tile tile=hit.collider.gameObject.GetComponent<Tile>();
						//if this is the second tap on the tile
						if(tile==lastTileTouched){
							lastTileTouched.OnTouchMouseExit();
							lastTileTouched=null;
							tile.OnTouchMouseDown();
						}
						//if the tile is a new one
						else{
							//clear any effect off previously selected tile
							if(lastTileTouched!=null) lastTileTouched.OnTouchMouseExit();
							//if the tile contain friendly unit, select it directly
							if(tile.unit!=null && tile.unit.factionID==0){
								lastTileTouched=tile;
								tile.OnTouchMouseEnter();
								tile.OnTouchMouseDown();
							}
							//a new tile with no friendly unit,  just call mouse tnter
							else{
								lastTileTouched=tile;
								tile.OnTouchMouseEnter();
							}
						}
					}
					else{
						if(lastTileTouched!=null){
							lastTileTouched.OnTouchMouseExit();
							lastTileTouched=null;
						}
					}
					
					if(onUnitInfoE!=null) onUnitInfoE(lastTileTouched);
				}
			}
			*/
		#endif
	}
	
	//this variable is assigned by UITB.cs in runtime
	public Camera uiCam;
	//check if any of the mouse click, touch input has landed on UI element
	public static bool IsCursorOnUI(Vector3 point){
		if( instance.uiCam != null ){
			// pos is the Vector3 representing the screen position of the input
			Ray inputRay = instance.uiCam.ScreenPointToRay( point );    
			RaycastHit hit;

			LayerMask mask=1<<LayerManager.GetLayerUI();
			if( Physics.Raycast( inputRay, out hit, Mathf.Infinity, mask ) ){
				// UI was hit
				return true;
			}
		}
		return false;
	}
	public static bool IsObjectOnUI(Vector3 pos){
		Camera mainCam=Camera.main;
		
		if( instance.uiCam != null && mainCam != null){
			// pos is the Vector3 representing the screen position of the input
			Ray inputRay = instance.uiCam.ScreenPointToRay( mainCam.WorldToScreenPoint(pos) );    
			RaycastHit hit;

			LayerMask mask=1<<LayerManager.GetLayerUI();
			if( Physics.Raycast( inputRay, out hit, Mathf.Infinity, mask ) ){
				// UI was hit
				return true;
			}
		}
		return false;
	}
	public static void SetUICam(Camera cam){
		instance.uiCam=cam;
	}
	
	//for touch input on mobile device
	private Tile lastTileTouched;
	public static Tile GetLastTileTouched(){
		return instance.lastTileTouched;
	}
	

	
	
	
//*****************************************************************************************************************************
//utility function
	
	//function called to indicate the unit placement is completed, so the battle can started
	//called from UITB
	public static void UnitPlacementCompleted(){
		if(onBattleStartE!=null) onBattleStartE();
	}
	
	
	//event for UI to display a game event related message
	public static void DisplayMessage(string msg){
		if(onGameMessageE!=null) onGameMessageE(msg);
	}
	
	//scene flow related function
	public static void LoadNextScene(){
		Time.timeScale=1;	//make sure the timeScale is reset, in case the function is called when the level is paused;
		if(instance.nextScene!="")	Application.LoadLevel(instance.nextScene);
	}
	public static void LoadMainMenu(){
		Time.timeScale=1;	//make sure the timeScale is reset, in case the function is called when the level is paused;
		if(instance.mainMenu!="")	Application.LoadLevel(instance.mainMenu);
	}
	public static bool TogglePause(){
		if(Time.timeScale>=1) Time.timeScale=0;
		else Time.timeScale=1;
		
		return Time.timeScale == 0 ? true : false;
	}
	public static bool IsPaused(){
		return Time.timeScale == 0 ? true : false;
	}
	
	
//*****************************************************************************************************************************
//public function to get various flag/assignment about rules, mode
	
	public static _TurnMode GetTurnMode(){
		return instance.turnMode;
	}
	public static _MoveOrder GetMoveOrder(){
		return instance.moveOrder;
	}
	public static bool EnableUnitPlacement(){
		return instance.enableUnitPlacement;
	}
	public static bool EnableCover(){
		return instance.enableCover;
	}
	public static float GetCoverHalf(){
		return instance.coverBonusHalf;
	}
	public static float GetCoverFull(){
		return instance.coverBonusFull;
	}
	public static float GetExposedCritBonus(){
		return instance.exposedCritBonus;
	}
	public static bool EnableFogOfWar(){
		return instance.enableFogOfWar;
	}
	public static bool IsCounterAttackEnabled(){
		return instance.enableCounterAttack;
	}
	public static bool FullAPOnStart(){
		return instance.fullAPOnStart;
	}
	public static bool FullAPOnNewRound(){
		return instance.fullAPOnNewRound;
	}
	public static _MovementAPCostRule MovementAPCostRule(){
		return instance.movementAPCostRule;
	}
	public static _AttackAPCostRule AttackAPCostRule(){
		return instance.attackAPCostRule;
	}
	public static bool AllowMovementAfterAttack(){
		return instance.allowMovementAfterAttack;
	}
	public static bool AllowAbilityAfterAttack(){
		return instance.allowAbilityAfterAttack;
	}
	public static _LoadMode LoadMode(){
		return instance.loadMode;
	}
	public static bool AllowUnitSwitching(){
		//return instance.allowUnitSwitching && !instance.unitSwitchingLocked;
		return !unitSwitchingLocked;
	}
	public static void LockUnitSwitching(){	//obsolete?
		if(instance.turnMode==_TurnMode.FactionSingleUnitPerTurnAll 
			|| instance.turnMode==_TurnMode.FactionSingleUnitPerTurnSingle)
			unitSwitchingLocked=true;
	}
	public static float GetActionCamFrequency(){
		return instance.actionCamFrequency;
	}
	
	public static bool IsActionInProgress(){
		return actionInProgress || CameraControl.ActionCamInAction();
	}
	
	//check if the game is currently in unit placement phase (game not started yet)
	public static bool IsUnitPlacementState(){
		if(turnID<0) return true;
		return false;
	}
	
	
	
	//for debug, draw a box on screen when actionInProgress flag is true
	/*
	void OnGUI(){
		if(actionInProgress){
			GUI.Box(new Rect(100, 100, Screen.width-200, Screen.height-200), "");
		}
		
		GUI.Label(new Rect(50, 50, 300, 100), "RoundCounter: "+roundCounter);
		GUI.Label(new Rect(50, 75, 300, 100), "TurnID: "+turnID+"     loop:"+turnIDLoop);
		GUI.Label(new Rect(50, 100, 300, 100), "switching locked: "+unitSwitchingLocked);
		//GUI.Label(new Rect(50, 125, 300, 100), "unit: "+UnitControl.selectedUnit+"   "+UnitControl.selectedUnit.factionID);
	}
	*/
	

}
