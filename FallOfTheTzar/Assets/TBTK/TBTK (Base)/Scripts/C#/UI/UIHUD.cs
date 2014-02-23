using UnityEngine;
using System.Collections;

public class UIHUD : MonoBehaviour {

	
	private bool isPlayerTurn=false;
	
	private bool perkManagerExist=false;
	
	// Use this for initialization
	void Start () {
		PerkManagerTB perkManager=(PerkManagerTB)FindObjectOfType(typeof(PerkManagerTB));
		perkManagerExist=perkManager==null ? false : true;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	void OnEnable(){
		GameControlTB.onNextTurnE += OnNextTurn;
	}
	void OnDisable(){
		GameControlTB.onNextTurnE -= OnNextTurn;
	}
	
	void OnNextTurn(){ StartCoroutine(_OnNextTurn()); }
	IEnumerator _OnNextTurn(){
		yield return null;
		
		if(GameControlTB.GetTurnMode()==_TurnMode.SingleUnitPerTurn){
			while(UnitControl.selectedUnit==null){
				yield return null;
			}
		}
		
		if(GameControlTB.IsPlayerTurn()){
			isPlayerTurn=true;
		}
		else{
			isPlayerTurn=false;
		}
		
		yield return null;
	}
	
	
	
	void OnEndTurnButton(){
		if(GameControlTB.IsActionInProgress()) return;
		
		if(GameControlTB.GetTurnMode()!=_TurnMode.SingleUnitPerTurn){
			if(GameControlTB.GetMoveOrder()==_MoveOrder.Free){
				if(UnitControl.selectedUnit!=null){
					UnitControl.selectedUnit.moved=true;
					UnitControl.selectedUnit.attacked=true;
					UnitControl.MoveUnit(UnitControl.selectedUnit);
				}
			}
		}
		
		GameControlTB.OnEndTurn();
	}
	
	void OnPerkMenu(){
		UI.OnPerkMenu();
	}
	
	
	public void Draw(){
		if(isPlayerTurn){
			PlayerHUD();
		}
	}
	
	void PlayerHUD(){
		if(GUI.Button(new Rect(Screen.width-65, Screen.height-65, 60, 60), "Next\nTurn", UI.buttonStyle)){
			OnEndTurnButton();
		}
		
		if(perkManagerExist){
			if(GUI.Button(new Rect(5, Screen.height-65, 60, 60), "Perk\nMenu", UI.buttonStyle)){
				OnPerkMenu();
			}
		}
	}
}
