#define glow
using UnityEngine;
using System.Collections;

public class UIHUD : MonoBehaviour {
#if glow
	public bool glow = false;
	//for manipulating gui colors
	public Color c;
	public Color cc;
#endif
	
	private bool isPlayerTurn=false;
	
	private bool perkManagerExist=false;
	
	// Use this for initialization
	void Start () {
		PerkManagerTB perkManager=(PerkManagerTB)FindObjectOfType(typeof(PerkManagerTB));
		perkManagerExist=perkManager==null ? false : true;

#if glow 
		//cache the color values
		c = GUI.color;//cache the color
		cc = GUI.contentColor;
#endif

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
			glow = false;
			isPlayerTurn=true;
			BroadcastMessage("resetBool");
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
#if glow
	public void turnoffGlow(){
		glow =false;
	}

	public void toggleGlow(){
		glow = true;

	}

#endif
	void PlayerHUD(){
		//TODO: Make this glow when all the units have moved
			//The glow booolean is working, the next step is to make the button glow
#if glow
		if (!glow){
			GUI.color = c;
			GUI.contentColor = cc;
#endif
			if(GUI.Button(new Rect(Screen.width-100, Screen.height-65, 60, 60), "Next\nTurn", UI.buttonStyle)){
#if glow
				GUI.color = Color.gray;
				GUI.contentColor = Color.white;
				glow = false;
#endif
				OnEndTurnButton();
			}
#if glow
		}
		else{
			GUI.contentColor = Color.red;
			GUI.color = Color.yellow;
			if(GUI.Button(new Rect(Screen.width-100, Screen.height-65, 60, 60), "Next\nTurn", UI.buttonStyle)){
				glow = false;
				OnEndTurnButton();
			}

			GUI.color = c;
			GUI.contentColor = cc;
		}
#endif
		if(perkManagerExist){
			if(GUI.Button(new Rect(5, Screen.height-65, 60, 60), "Perk\nMenu", UI.buttonStyle)){
				OnPerkMenu();
			}
		}
	}
}
