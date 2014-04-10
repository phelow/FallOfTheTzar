using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(GameControlTB))]
public class GameControlEditor : Editor {

	GameControlTB gc;
	UnitControl uc;
	
	string[] turnModeLabel=new string[0];
	string[] turnModeTooltip=new string[0];
	
	string[] moveOrderLabel=new string[0];
	string[] moveOrderTooltip=new string[0];
	
	string[] loadModeLabel=new string[0];
	string[] loadModeTooltip=new string[0];
	
	string[] moveAPRuleLabel=new string[0];
	string[] moveAPRuleTooltip=new string[0];
	
	string[] attackAPRuleLabel=new string[0];
	string[] attackAPRuleTooltip=new string[0];

	
	int[] intVal=new int[10];
	
	void Awake () {
		gc=(GameControlTB)target;
		uc=gc.gameObject.GetComponent<UnitControl>();
		
		
		InitLabel();
		
		if(gc.playerFactionID==null || gc.playerFactionID.Count==0){
			gc.playerFactionID=new List<int>();
			gc.playerFactionID.Add(0);
		}
	}
	
	
	void InitLabel(){
		
		int enumLength = Enum.GetValues(typeof(_TurnMode)).Length;
		turnModeLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) turnModeLabel[i]=((_TurnMode)i).ToString();
		
		turnModeTooltip=new string[enumLength];
		turnModeTooltip[0]="Each faction take turn to move all its unit in each round";
		turnModeTooltip[1]="Each faction take turn to move a single unit in each round";
		turnModeTooltip[2]="Each faction take turn to move a single unit in each turn\nThe round is completed when all unit has moved";
		turnModeTooltip[3]="All units (regardless of faction) take turn to move according to the stats (TurnPriority)\nThe round is completed when all unit has moved";
		turnModeTooltip[4]="All units (regardless of faction) take turn to move according to the stats (TurnPriority)\nUnit with higher stats may get to move more than 1 turn in a round\nThe round is completed when all unit has moved";
		turnModeTooltip[5]="All units (regardless of faction) take turn to move according to the stats (TurnPriority)\nUnit with higher stats may get to move more more\nThere is no round mechanic";
		
		
		
		enumLength = Enum.GetValues(typeof(_MoveOrder)).Length;
		moveOrderLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) moveOrderLabel[i]=((_MoveOrder)i).ToString();
		
		moveOrderTooltip=new string[enumLength];
		moveOrderTooltip[0]="Randomise move order for the unit\nunit switching is enabled";
		moveOrderTooltip[1]="Randomise move order for the unit\nunit switching is disabled";
		moveOrderTooltip[2]="Arrange the move order based on unit's stats (TurnPriority)\nunit switching is not allowed";


		
		enumLength = Enum.GetValues(typeof(_LoadMode)).Length;
		loadModeLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) loadModeLabel[i]=((_LoadMode)i).ToString();
		
		loadModeTooltip=new string[enumLength];
		loadModeTooltip[0]="Use data carried from previous progress\nUpon complete this level, the data will be saved and carry to subsequent scene";
		loadModeTooltip[1]="Use temporary data set in any previous scene, if there's any\nThis data will not be carried forward.";
		loadModeTooltip[2]="Use data set in UnitControl\nThis data will not be carried forward.";
		
		
		
		enumLength = Enum.GetValues(typeof(_MovementAPCostRule)).Length;
		moveAPRuleLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) moveAPRuleLabel[i]=((_MovementAPCostRule)i).ToString();
		
		moveAPRuleTooltip=new string[enumLength];
		moveAPRuleTooltip[0]="Unit's movement doesn't cost any AP";
		moveAPRuleTooltip[1]="Unit's movement cost just one AP regardless of how far the unit move";
		moveAPRuleTooltip[2]="Each movement across a tile cost the unit one AP";
		
		
		enumLength = Enum.GetValues(typeof(_AttackAPCostRule)).Length;
		attackAPRuleLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) attackAPRuleLabel[i]=((_AttackAPCostRule)i).ToString();
		
		attackAPRuleTooltip=new string[enumLength];
		attackAPRuleTooltip[0]="Unit's attack doesn't cost any AP";
		attackAPRuleTooltip[1]="Each attack cost the unit one AP";
		
		for(int i=0; i<intVal.Length; i++){
			intVal[i]=i;	
		}
		
	}
	
	GUIContent cont;
	GUIContent[] contL;
	
	public override void OnInspectorGUI(){
		gc = (GameControlTB)target;
		
		//DrawDefaultInspector();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		
		int turnMode=(int)gc.turnMode;  
		cont=new GUIContent("Turn Mode:", "Turn mode to be used in this scene");
		contL=new GUIContent[turnModeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turnModeLabel[i], turnModeTooltip[i]);
		turnMode = EditorGUILayout.IntPopup(cont, turnMode, contL, intVal);
		gc.turnMode=(_TurnMode)turnMode;
		
		
		if(turnMode<=2){
			int moveOrder=(int)gc.moveOrder;  
			cont=new GUIContent("Move Order:", "Unit move order to be used in this scene");
			contL=new GUIContent[moveOrderLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(moveOrderLabel[i], moveOrderTooltip[i]);
			moveOrder = EditorGUILayout.IntPopup(cont, moveOrder, contL, intVal);
			gc.moveOrder=(_MoveOrder)moveOrder;
		}
		
		
		int loadMode=(int)gc.loadMode;  
		cont=new GUIContent("Data Load Mode:", "Data loading mode to be used in this scene");
		contL=new GUIContent[loadModeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(loadModeLabel[i], loadModeTooltip[i]);
		loadMode = EditorGUILayout.IntPopup(cont, loadMode, contL, intVal);
		gc.loadMode=(_LoadMode)loadMode;
		
		
		cont=new GUIContent("Enable Perk Menu:", "check to enable PerkMenu to be brought out in the scene");
		EditorGUILayout.BeginHorizontal("");
		//EditorGUILayout.LabelField(cont);
		gc.enablePerkMenu=EditorGUILayout.Toggle(cont, gc.enablePerkMenu);
		EditorGUILayout.EndHorizontal();
		
		
		
		
		//cont=new GUIContent("PlayerFactionID:", "Numerial ID to identify player's units\nAny unit with similar unitID will be able to control by player\nif no unit with similar unitID is in the scene, the whole game will be run by AI");
		//gc.playerFactionID[0]=EditorGUILayout.IntField(cont, gc.playerFactionID[0]);
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		cont=new GUIContent("HotSeat Mode:", "check to enable local hot seat multiplayer\nenable multiple player faction");
		EditorGUILayout.BeginHorizontal("");
		//EditorGUILayout.LabelField(cont);
		gc.hotseat=EditorGUILayout.Toggle(cont, gc.hotseat);
		EditorGUILayout.EndHorizontal();
		
		
		if(gc.hotseat){
			gc.enableFogOfWar=false;
			
			if(gc.playerFactionID.Count==1){
				int ID=gc.playerFactionID[0];
				int newID=0;
				if(ID==0) newID=1;
				
				gc.playerFactionID.Add(newID);
				uc.playerUnits.Add(new PlayerUnits(newID));
			}
			
			for(int i=0; i<gc.playerFactionID.Count; i++){
				EditorGUILayout.BeginHorizontal("");
					cont=new GUIContent("PlayerFactionID - "+i+":", "Numerial ID to identify player's units\nAny unit with similar unitID will be able to control by player\nif no unit with similar unitID is in the scene, the whole game will be run by AI");
					int ID=gc.playerFactionID[i];
					ID=EditorGUILayout.IntField(cont, ID, GUILayout.MaxHeight(16));
					if(!gc.playerFactionID.Contains(ID)){
						gc.playerFactionID[i]=ID;
						uc.playerUnits[i].factionID=ID;
					}
					if(GUILayout.Button("Remove", GUILayout.MaxHeight(16))){
						if(gc.playerFactionID.Count>2){
							gc.playerFactionID.RemoveAt(i);
							uc.playerUnits.RemoveAt(i);
						}
					}
				EditorGUILayout.EndHorizontal();
			}
			if(GUILayout.Button("Add more player", GUILayout.MaxHeight(16))){
				int newID=0;
				while(gc.playerFactionID.Contains(newID)) newID+=1;
				gc.playerFactionID.Add(newID);
				uc.playerUnits.Add(new PlayerUnits(newID));
			}
		}
		else{
			if(gc.playerFactionID.Count>1){
				//int ID=gc.playerFactionID[0];
				//gc.playerFactionID=new List<int>();
				//gc.playerFactionID.Add(ID);
				
				for(int i=0; i<gc.playerFactionID.Count-1; i++){
					gc.playerFactionID.RemoveAt(1);
					uc.playerUnits.RemoveAt(1);
				}
			}
			
			cont=new GUIContent("PlayerFactionID:", "Numerial ID to identify player's units\nAny unit with similar unitID will be able to control by player\nif no unit with similar unitID is in the scene, the whole game will be run by AI");
			gc.playerFactionID[0]=EditorGUILayout.IntField(cont, gc.playerFactionID[0]);
			uc.playerUnits[0].factionID=gc.playerFactionID[0];
		}
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		
		int moveRule=(int)gc.movementAPCostRule;  
		cont=new GUIContent("Movement AP Rule:", "The AP cost of unit's movement");
		contL=new GUIContent[moveAPRuleLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(moveAPRuleLabel[i], moveAPRuleTooltip[i]);
		moveRule = EditorGUILayout.IntPopup(cont, moveRule, contL, intVal, GUILayout.ExpandWidth(true));
		gc.movementAPCostRule=(_MovementAPCostRule)moveRule;
		
		/*
		if(gc.movementAPCostRule==_MovementAPCostRule.PerMove){
			cont=new GUIContent("AP Cost Per Move:", "AP cost for unit to move");
			gc.movementAPCost=EditorGUILayout.IntField(cont, gc.movementAPCost);
		}
		else if(gc.movementAPCostRule==_MovementAPCostRule.PerTile){
			cont=new GUIContent("AP Cost Per Move:", "AP cost for unit to move across each tile");
			gc.movementAPCost=EditorGUILayout.IntField(cont, gc.movementAPCost);
		}
		*/
		
		int attackRule=(int)gc.attackAPCostRule;  
		cont=new GUIContent("Attack AP Rule:", "The AP cost of unit's attack");
		contL=new GUIContent[attackAPRuleLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(attackAPRuleLabel[i], attackAPRuleTooltip[i]);
		attackRule = EditorGUILayout.IntPopup(cont, attackRule, contL, intVal, GUILayout.ExpandWidth(true));
		gc.attackAPCostRule=(_AttackAPCostRule)attackRule;
		
		/*
		if(gc.attackAPCostRule==_AttackAPCostRule.PerAttack){
			cont=new GUIContent("AP Cost Per Attack:", "AP cost for unit to perform each attack");
			gc.attackAPCost=EditorGUILayout.IntField(cont, gc.attackAPCost);
		}
		*/
			
		
		cont=new GUIContent("Win Point Reward:", "point reward for winning this battle");
		gc.winPointReward=EditorGUILayout.IntField(cont, gc.winPointReward);
		
		
		cont=new GUIContent("Next Scene:", "the name of next scene to be load");
		gc.nextScene=EditorGUILayout.TextField(cont, gc.nextScene);
		cont=new GUIContent("Main Menu:", "the name of the main menu scene");
		gc.mainMenu=EditorGUILayout.TextField(cont, gc.mainMenu);
		
		cont=new GUIContent("Enable Unit Placement:", "check to enable player to hand place the starting unit before the battle start");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(180));
		gc.enableUnitPlacement=EditorGUILayout.Toggle(gc.enableUnitPlacement, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		cont=new GUIContent("Enable Counter Attack:", "check to enable unit to perform counter attack in range (only when attacker is within attack range)");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(180));
		gc.enableCounterAttack=EditorGUILayout.Toggle(gc.enableCounterAttack, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		
		cont=new GUIContent("Full AP On Start:", "check to start unit at full AP");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(180));
		gc.fullAPOnStart=EditorGUILayout.Toggle(gc.fullAPOnStart, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		cont=new GUIContent("Restore AP On New Round:", "check to restore unit AP to full at every new round");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(180));
		gc.fullAPOnNewRound=EditorGUILayout.Toggle(gc.fullAPOnNewRound, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		
		cont=new GUIContent("Enable Cover System:", "check to use cover system\nunit behind an obstacle will gain bonus defense against attack from other side of cover");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(180));
		gc.enableCover=EditorGUILayout.Toggle(gc.enableCover, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		if(gc.enableCover){
			cont=new GUIContent("- Half Cover Bonus:", "hit penalty when attacking a target behind a half cover\ntakes value from 0-1");
			gc.coverBonusHalf=EditorGUILayout.FloatField(cont, gc.coverBonusHalf);
			
			cont=new GUIContent("- Full Cover Bonus:", "hit penalty when attacking a target behind a full cover\ntakes value from 0-1");
			gc.coverBonusFull=EditorGUILayout.FloatField(cont, gc.coverBonusFull);
			
			cont=new GUIContent("- Flanking Crit Bonus:", "critical bonus gain when attacking a unit with no cover\ntakes value from 0-1");
			gc.exposedCritBonus=EditorGUILayout.FloatField(cont, gc.exposedCritBonus);
			
			gc.coverBonusHalf=Mathf.Clamp(gc.coverBonusHalf, 0f, 1f);
			gc.coverBonusFull=Mathf.Clamp(gc.coverBonusFull, 0f, 1f);
		}
		
		cont=new GUIContent("Enable Fog of War:", "check to enable fog of war\nunit cannot see enemies beyond sight and out of line-of sight");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(180));
		gc.enableFogOfWar=EditorGUILayout.Toggle(gc.enableFogOfWar, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		if(gc.enableFogOfWar) gc.hotseat=false;
		

		cont=new GUIContent("Allow Movement After Attack:", "check to allow unit to move after attack");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(180));
		gc.allowMovementAfterAttack=EditorGUILayout.Toggle(gc.allowMovementAfterAttack, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		cont=new GUIContent("Allow Ability After Attack:", "check to allow unit to use ability after attack");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(180));
		gc.allowAbilityAfterAttack=EditorGUILayout.Toggle(gc.allowAbilityAfterAttack, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		
		/*
		if(gc.turnMode==_TurnMode.FactionSingleUnitPerTurnSingle){
			cont=new GUIContent("Allow Unit Switching:", "check to allow player to switch unit in play when in FactionBasedSingleUnit turn-mode");
			EditorGUILayout.BeginHorizontal("");
			EditorGUILayout.LabelField(cont, GUILayout.MinWidth(230));
			gc.allowUnitSwitching=EditorGUILayout.Toggle(gc.allowUnitSwitching);
			EditorGUILayout.EndHorizontal();
		}
		*/
		
		
		cont=new GUIContent("Action Cam Frequency:", "The rate at which action-cam will be used. value from 0-1. 0 being no action-cam, 1 being always action-cam");
		gc.actionCamFrequency=EditorGUILayout.FloatField(cont, gc.actionCamFrequency);
		
		
		if(GUI.changed){
			EditorUtility.SetDirty(gc);
		}
	}
}
