using UnityEngine;
using System.Collections;

public enum _DelayMode{RealTime, Round, Turn}

public class SelfDestruct : MonoBehaviour {

	public _DelayMode mode;
	public float delay=5;
	public int round=3;
	public int turn=2;
	private int countTillNextTurn=0;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnEnable(){
		if(mode==_DelayMode.RealTime) DelayDestruct();
		else if(mode==_DelayMode.Round){
			GameControlTB.onNewRoundE += OnNewRound;
		}
		else if(mode==_DelayMode.Turn){
			GameControlTB.onNextTurnE += OnNextTurn;
			UnitTB.onUnitDestroyedE += OnUnitDestroyed;
			if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
				countTillNextTurn=UnitControl.activeFactionCount;
			}
			else{
				countTillNextTurn=UnitControl.GetAllUnitCount();
			}
		}
	}
	
	/*
	void OnDisable(){
		if(mode==_DelayMode.Round){
			GameControlTB.onNewRoundE -= OnNewRound;
		}
		else if(mode==_DelayMode.Turn){
			GameControlTB.onNextTurnE -= OnNextTurn;
			UnitTB.onUnitDestroyedE += OnUnitDestroyed;
		}
	}
	*/
	
	void OnNewRound(int roundCounter){
		round-=1;
		if(round==0){
			GameControlTB.onNewRoundE -= OnNewRound;
			delay=0;
			DelayDestruct();
		}
	}
	
	void OnNextTurn(){
		countTillNextTurn-=1;
		if(countTillNextTurn==0){
			turn-=1;
			
			if(turn==0){
				GameControlTB.onNextTurnE -= OnNextTurn;
				UnitTB.onUnitDestroyedE -= OnUnitDestroyed;
				delay=0;
				DelayDestruct();
			}
			else{
				if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
					countTillNextTurn=UnitControl.activeFactionCount;
				}
				else{
					countTillNextTurn=UnitControl.GetAllUnitCount();
				}
			}
		}
	}
	void OnUnitDestroyed(UnitTB unit){
		countTillNextTurn-=1;
	}
	
	public void DelayDestruct(){
		Destroy(gameObject, delay);
	}
}
