using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class AIMoveInfo{
	public List<Tile> tileList=new List<Tile>();
	public List<UnitTB> targetList=new List<UnitTB>();
}

public enum _AIStance{
	Passive,		//AI faction will not do anything until it detect hostile
	Trigger, 		//AI faction will not do anything until it's being spoted
	Active		//AI faction will actively move around seeking out target
}

public class AIManager : MonoBehaviour {

	public static AIManager instance;
	
	public _AIStance aIStance;
	public static _AIStance GetAIStance(){
		if(instance==null) return _AIStance.Passive;
		return instance.aIStance;
	}
	
	//set to true when a unit is asked to move
	//when unit completes it's move, the unit instance will call function to set this back to false
	//for _AIRoutine only
	public bool unitInAction=false;
	public static void ClearUnitInActionFlag(){
		instance.unitInAction=false;
	}
	
	// Use this for initialization
	void Awake () {
		instance=this;
	}
	
	
	//just for testing,
	IEnumerator SkippedAIRoutine(){
		yield return new WaitForSeconds(1f);
		GameControlTB.OnEndTurn();
	}
	
	public static void AIRoutine(int factionID){
		instance.StartCoroutine(instance._AIRoutine(factionID));
		
		//for testing
		//Debug.Log("all AI routine");
		//instance.StartCoroutine(instance.SkippedAIRoutine(null));
	}
	
	public static void MoveUnit(UnitTB unit){
		//Debug.Log("move "+unit);
		instance.StartCoroutine(instance._AIRoutineSingleUnit(unit));
		
		//for testing
		//Debug.Log("single AI routine");
		//instance.StartCoroutine(instance.SkippedAIRoutine(null));
	}
	
	
	//AI routine to move a single unit
	IEnumerator _AIRoutineSingleUnit(UnitTB unit){
		while(GameControlTB.IsActionInProgress()) yield return null;
		yield return new WaitForSeconds(1f);
		
		unitInAction=true;
		instance.StartCoroutine(instance._MoveUnit(unit));
		
		//wait a frame for the unit to start moving, set the inAction flag, etc..
		yield return null;
		
		while(unit.InAction() || GameControlTB.IsActionInProgress() || unitInAction){
			yield return null;
		}
		
		GameControlTB.OnEndTurn();
	}
	

	//AI routine for faction based turn mode, AI will move all available unit for the faction
	//called by UnitControl in OnNextTurn
	IEnumerator _AIRoutine(int factionID){
		while(GameControlTB.IsActionInProgress()) yield return null;
		
		yield return new WaitForSeconds(1f);
		
		Faction faction=UnitControl.GetFactionInTurn(factionID);
		List<UnitTB> allUnit=faction.allUnitList;
		
		int counter=0;
		
		if(GameControlTB.GetMoveOrder()==_MoveOrder.Free){
			int count=allUnit.Count;
			
			//create a list (1, 2, 3..... ) to the size of the unit, this is for unit shuffling
			List<int> IDList=new List<int>();
			for(int i=0; i<count; i++) IDList.Add(i);
			
			for(int i=0; i<count; i++){
				//randomly select a unit to move
				int rand=UnityEngine.Random.Range(0, IDList.Count);
				
				/*
				//if(counter%2==0) rand=0;
				//else rand=IDList.Count-1;
				
				string label="rand: "+rand+" ("+IDList.Count+")";
				for(int nn=0; nn<IDList.Count; nn++){
					label+="     "+IDList[nn];
				}
				Debug.Log(label);
				*/
				
				int ID=IDList[rand];
				UnitTB unit=allUnit[ID];
				//remove the unit from allUnit list so that it wont be selected again
				IDList.RemoveAt(rand);
				
				counter+=1;
				
				//move the unit
				unitInAction=true;
				StartCoroutine(_MoveUnit(unit));
				
				//wait a frame for the unit to start moving, set the inAction flag, etc..
				yield return null;
				
				//wait while the unit is in action (making its move)
				//and while the game event is in progress (counter attack, death effect/animation, etc.)
				//and while the unit localUnitInactionFlag hasn't been cleared
				while(unit.InAction() || GameControlTB.IsActionInProgress() || unitInAction){
					yield return null;
				}
				
				//for counter-attack enabled, in case the unit is destroyed in after being countered
				if(unit.IsDestroyed()){
					//label="";
					for(int n=rand; n<IDList.Count; n++){
						if(IDList[n]>=ID){
							//label+="   "+IDList[n];
							IDList[n]-=1;
						}
					}
					//Debug.Log("unit destroyed    "+label);
				}
				
				/*
				label="rand: "+rand+" ("+IDList.Count+")";
				for(int nn=0; nn<IDList.Count; nn++){
					label+="     "+IDList[nn];
				}
				Debug.Log(label);
				*/
				
				if(GameControlTB.battleEnded) yield break;
			}
		}
		else if(GameControlTB.GetMoveOrder()==_MoveOrder.FixedRandom || GameControlTB.GetMoveOrder()==_MoveOrder.FixedStatsBased){
			
			for(int i=0; i<allUnit.Count; i++){
				UnitTB unit=allUnit[i];
				
				//move the unit
				unitInAction=true;
				StartCoroutine(_MoveUnit(unit));
				
				yield return null;
				
				//wait while the unit is in action (making its move)
				//and while the game event is in progress (counter attack, death effect/animation, etc.)
				while(unit.InAction() || GameControlTB.IsActionInProgress() || unitInAction){
					yield return null;
				}
				
				if(GameControlTB.battleEnded) yield break;
			}
		}
		
		faction.allUnitMoved=true;
		
		yield return new WaitForSeconds(0.5f);
		
		EndTurn();
	}
	
	public void EndTurn(){
		if(!GameControlTB.battleEnded) GameControlTB.OnEndTurn();
	}
	
	
	
	//LOS and fog of war is not considered
	//that has already been built into various function called on UnitControl and GridManager
	public Tile Analyse(UnitTB unit){
		
		//analyse current tile first
		List<UnitTB> currentHostilesList;
		if(!GameControlTB.EnableFogOfWar()) currentHostilesList=UnitControl.GetHostileInRangeFromTile(unit.occupiedTile, unit);
		else currentHostilesList=UnitControl.GetVisibleHostileInRangeFromTile(unit.occupiedTile, unit);
		unit.occupiedTile.AIHostileList=currentHostilesList;
		
		if(GameControlTB.EnableCover()){
			if(currentHostilesList.Count!=0){
				int count=0;
				for(int n=0; n<currentHostilesList.Count; n++){
					if((int)unit.occupiedTile.GetCoverType(currentHostilesList[n].thisT.position)>0){
						count+=1;
					}
				}
				if(count==currentHostilesList.Count) return unit.occupiedTile;
			}
		}
		else{
			if(currentHostilesList.Count!=0) return unit.occupiedTile;
		}
		
		
		//get all possible target
		List<UnitTB> allHostiles=new List<UnitTB>();
		if(GameControlTB.EnableFogOfWar()) allHostiles=UnitControl.GetAllHostileWithinFactionSight(unit.factionID);
		else allHostiles=UnitControl.GetAllHostile(unit.factionID);
		
		
		//if there's no potential target at all
		if(allHostiles.Count==0){
			if(unit.attackerList.Count>0){
				return unit.attackerList[0].unit.occupiedTile;
			}
			else{
				//if stance is set to passive, stay put
				if(aIStance==_AIStance.Passive) return unit.occupiedTile;
			}
		}
		
		//get all walkable
		List<Tile> walkableTiles=GridManager.GetWalkableTilesWithinRange(unit.occupiedTile, unit.GetMoveRange());
		
		//get tiles with target in range from the walkables
		List<Tile> offensiveTiles=new List<Tile>();
		for(int i=0; i<walkableTiles.Count; i++){
			walkableTiles[i].AIHostileList=new List<UnitTB>();
			bool visibleOnly=GameControlTB.EnableFogOfWar();
			List<UnitTB> hostilesList=UnitControl.GetHostileInRangeFromTile(walkableTiles[i], unit, visibleOnly);
			walkableTiles[i].AIHostileList=hostilesList;
			if(hostilesList.Count>0){
				offensiveTiles.Add(walkableTiles[i]);
			}
		}
		
		//cross search any walkables with any tiles with target
		List<Tile> offensiveWalkableTiles=new List<Tile>();
		for(int i=0; i<offensiveTiles.Count; i++){
			if(walkableTiles.Contains(offensiveTiles[i])){
				offensiveWalkableTiles.Add(offensiveTiles[i]);
			}
		}
		
		//if the game uses cover
		if(GameControlTB.EnableCover()){
			//calculating general direction of enemy, to know which direction to take cover from
			//Vector3 hostileAveragePos=Vector3.zero;
			//for(int i=0; i<allHostiles.Count; i++){
			//	hostileAveragePos+=allHostiles[i].occupiedTile.pos;
			//}
			//hostileAveragePos/=allHostiles.Count;
			
			
			//get offensive tile with cover from all hostile
			List<Tile> offensiveTilesWithAllCover=new List<Tile>();
			List<Tile> offensiveTilesWithPartialCover=new List<Tile>();
			for(int i=0; i<offensiveWalkableTiles.Count; i++){
				Tile tile=offensiveWalkableTiles[i];
				int count=0;
				for(int n=0; n<tile.AIHostileList.Count; n++){
					if((int)tile.GetCoverType(tile.AIHostileList[n].thisT.position)>0){
						count+=1;
					}
				}
				if(count==tile.AIHostileList.Count) offensiveTilesWithAllCover.Add(tile);
				else if(count>0) offensiveTilesWithPartialCover.Add(tile);
			}
			
			if(offensiveTilesWithAllCover.Count>0){
				int rand=UnityEngine.Random.Range(0, offensiveTilesWithAllCover.Count);
				return(offensiveTilesWithAllCover[rand]);
			}
			else if(offensiveTilesWithAllCover.Count>0){
				int rand=UnityEngine.Random.Range(0, offensiveTilesWithPartialCover.Count);
				return(offensiveTilesWithPartialCover[rand]);
			}
			
			
			//get any tile with possible cover from the walkables
			List<Tile> walkableTilesWithCover=new List<Tile>();
			for(int i=0; i<walkableTiles.Count; i++){
				if(walkableTiles[i].GotCover()){
					walkableTilesWithCover.Add(walkableTiles[i]);
				}
			}
			
			//cross-search offense and walkable tile with cover
			//to get tiles which offer cover and the unit can attack from
			List<Tile> offensiveTilesWithCover=new List<Tile>();
			for(int i=0; i<offensiveTiles.Count; i++){
				if(walkableTilesWithCover.Contains(offensiveTiles[i])){
					offensiveTilesWithCover.Add(offensiveTiles[i]);
				}
			}
			
			//if there's any tile that offer cover and target at the same time, use the tile
			if(offensiveTilesWithCover.Count>0){
				int rand=UnityEngine.Random.Range(0, offensiveTilesWithCover.Count);
				return offensiveTilesWithCover[rand];
			}
			
			//if there's no tile that offer cover and target at the same time
			//just use any walkable tile with cover instead
			if(walkableTilesWithCover.Count>0){
				int rand=UnityEngine.Random.Range(0, walkableTilesWithCover.Count);
				return walkableTilesWithCover[rand];
			}
		}
		
		//if cover system is not used, or there's no cover in any walkable tiles
		//if there's walkable that the unit can attack from, uses that
		if(offensiveWalkableTiles.Count>0){
			int rand=UnityEngine.Random.Range(0, offensiveWalkableTiles.Count);
			return offensiveWalkableTiles[rand];
		}
		
		//check if the unit has been attacked, if yes, retaliate
		UnitTB attacker=unit.GetNearestAttacker();
		if(attacker!=null) return attacker.occupiedTile;
		
		
		//no hostile unit within range
		if(aIStance==_AIStance.Active || aIStance==_AIStance.Trigger){
			//no hostile detected at all, just move randomly
			if(allHostiles.Count==0){
				if(walkableTiles.Count>0){
					int rand=UnityEngine.Random.Range(0, walkableTiles.Count);
					return walkableTiles[rand];
				}
			}
			//get the nearest target and move towards it
			else{
				int nearest=999999999;
				int nearestID=0;
				for(int i=0; i<allHostiles.Count; i++){
					int dist=GridManager.Distance(unit.occupiedTile, allHostiles[i].occupiedTile);
					if(dist<nearest){
						nearest=dist;
						nearestID=i;
					}
				}
				return allHostiles[nearestID].occupiedTile;
			}
		}
		
		return null;
	}
	
	//for LOS checking along a path, 
	//return true if the unit will be seen by hostile faction if the unit moves along the path, false if otherwise
	bool CheckEncounterOnPath(Tile srcTile, Tile targetTile){
		List<Tile> path=AStar.SearchWalkableTile(srcTile, targetTile);
		List<UnitTB> allPlayerUnit=UnitControl.GetAllUnitsOfFaction(0);
		
		bool visibleToPlayer=false;
		
		for(int i=0; i<path.Count; i++){
			Tile tile=path[0];
			for(int j=0; j<allPlayerUnit.Count; j++){
				if(GridManager.IsInLOS(tile, allPlayerUnit[j].occupiedTile)){
					int dist=GridManager.Distance(tile, allPlayerUnit[j].occupiedTile);
					if(dist<=allPlayerUnit[j].GetSight()){
						visibleToPlayer=true;
						break;
					}
				}
			}
		}
		return visibleToPlayer;
	}
	
	
	//execute move/attack for a single unit
	IEnumerator _MoveUnit(UnitTB unit){
		//make sure no event is taking place
		while(GameControlTB.IsActionInProgress()) yield return null;
			
		if(!unit.IsStunned()){
			
			/*
			DateTime timeS;
			DateTime timeE;
			TimeSpan timeSpan;
			timeS=System.DateTime.Now;
			*/
			
			Tile destinationTile=null;
			if(aIStance==_AIStance.Trigger){
				if(unit.triggered) destinationTile=Analyse(unit);
			}
			else{
				destinationTile=Analyse(unit);
			}
			
			//if there's target in current tile, just attack it
			if(destinationTile==unit.occupiedTile){
				if(destinationTile.AIHostileList.Count!=0){
					//unit.Attack(destinationTile.AIHostileList[UnityEngine.Random.Range(0, destinationTile.AIHostileList.Count)]);
					//unit will simply attack instead of move
					if(!unit.MoveAttack(unit.occupiedTile)) NoActionForUnit(unit);
				}
				else{
					unitInAction=false;
				}
				yield break;
			}
			
			if(destinationTile!=null){
				if(GameControlTB.EnableFogOfWar()){
					bool visibleToPlayer=CheckEncounterOnPath(unit.occupiedTile, destinationTile);
					if(visibleToPlayer){
						if(!unit.MoveAttack(destinationTile)) NoActionForUnit(unit);
					}
					else{
						if(destinationTile.AIHostileList.Count==0){
							MoveUnitToTileInstant(unit, destinationTile);
							unit.CompleteAllAction();
						}
						else{
							if(!unit.MoveAttack(destinationTile)) NoActionForUnit(unit);
						}
					}
				}
				else{
					if(!unit.MoveAttack(destinationTile)) NoActionForUnit(unit);
				}
			}
			else{
				//no action is available for the unit, simply registred it as moved
				NoActionForUnit(unit);
			}
			
			/*
			timeE=System.DateTime.Now;
			timeSpan=timeE-timeS;
			Debug.Log( "Time:"+timeSpan.TotalMilliseconds);
			*/
		}
		else{
			NoActionForUnit(unit);
		}
	}
	
	void NoActionForUnit(UnitTB unit){
		UnitControl.MoveUnit(unit);
		unit.CompleteAllAction();
		unitInAction=false;
	}
	
	void MoveUnitToTileInstant(UnitTB unit, Tile tile){
		List<Tile> path=AStar.SearchToOccupiedTile(unit.occupiedTile, tile);
		
		//make sure the target tile is not occupied
		if(path.Count>0){
			if(path[path.Count-1].unit!=null) path.RemoveAt(path.Count-1);
		}
		
		//make sure the unit doesnt move beyond the allowed range
		while(path.Count>Mathf.Max(1, unit.GetMoveRange()+1)){
			path.RemoveAt(path.Count-1);
		}
		
		if(path.Count>0){
			Tile destinationTile=path[path.Count-1];
			
			unit.occupiedTile.unit=null;
			unit.occupiedTile=destinationTile;
			destinationTile.unit=unit;
			unit.thisT.position=destinationTile.pos;
		}
		
		unitInAction=false;
	}
	
}














	//for fog of war mode, return a list of hostile unit which is visible by all unit
	//public List<UnitTB> allVisibleHostile=new List<UnitTB>();
	
	//~ public void GetVisibleHostileUnit(int factionID){
		//allVisibleHostile=UnitControl.GetAllHostileUnitWithinFactionSight(factionID);
	//~ }
	
	//~ public UnitTB GetNearestUnitFromFactionVisible(UnitTB srcUnit){
		//~ UnitTB targetUnit=null;
		//~ float currentNearest=Mathf.Infinity;
		//~ for(int i=0; i<allVisibleHostile.Count; i++){
			//~ float dist=Vector3.Distance(srcUnit.occupiedTile.pos, allVisibleHostile[i].occupiedTile.pos);
			//~ if(dist<currentNearest){
				//~ targetUnit=allVisibleHostile[i];
				//~ currentNearest=dist;
			//~ }
		//~ }
		//~ return targetUnit;
	//~ }
	
	//~ public UnitTB GetNearestUnitFromUnitVisible(UnitTB srcUnit, List<UnitTB> visibleList){
		//~ UnitTB targetUnit=null;
		//~ float currentNearest=Mathf.Infinity;
		//~ for(int i=0; i<visibleList.Count; i++){
			//~ float dist=Vector3.Distance(srcUnit.occupiedTile.pos, visibleList[i].occupiedTile.pos);
			//~ if(dist<currentNearest){
				//~ targetUnit=visibleList[i];
				//~ currentNearest=dist;
			//~ }
		//~ }
		//~ return targetUnit;
	//~ }
	
