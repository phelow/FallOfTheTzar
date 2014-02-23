using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;



public class UnitTBEditorWindow : EditorWindow {
    
	
	static string[] nameList=new string[0];
	static List<UnitTB> unitList=new List<UnitTB>();//new UnitTB[0];
	
	static string[] damageList=new string[0];
	static string[] armorList=new string[0];
	static string[] damageTooltipList=new string[0];
	static string[] armorTooltipList=new string[0];
	
	static string[] attackModeLabel=new string[0];
	static string[] attackModeTooltipLabel=new string[0];
	
	static List<CollectibleTB> collectibleList=new List<CollectibleTB>();
	static string[] collectibleNameList=new string[0];
	
	static private UnitTBEditorWindow window;
	
	private int index=0;
	
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (UnitTBEditorWindow)EditorWindow.GetWindow(typeof (UnitTBEditorWindow));
		window.minSize=new Vector2(730, 675);
		window.maxSize=new Vector2(731, 5000);

		EditorUnitList eUnitList=UnitTBManagerWindow.LoadUnit();
		unitList=eUnitList.prefab.unitList;
		nameList=eUnitList.nameList;
		
		LoadAbility();
		LoadCollectible();
		
		LoadDamageArmor();
		
		int enumLength = Enum.GetValues(typeof(_AttackMode)).Length;
		attackModeLabel=new string[enumLength];
		attackModeTooltipLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) attackModeLabel[i]=((_AttackMode)i).ToString();
		attackModeTooltipLabel[0]="Melee attack. Can only attack enemies within adjacent tile using melee attack stats";
		attackModeTooltipLabel[1]="Range attack. Attack enemies within attack range using range attack stats";
		attackModeTooltipLabel[2]="Hybrid attack. When target is in adjacent tile, use melee attack stats, otherwise uses range attack stats";
		//for(int i=0; i<enumLength; i++) attackModeTooltipLabel[i]="";
		
		InitObjectHierarchy(0);
	}
	
	public static void LoadAbility(){
		GameObject obj=Resources.Load("PrefabList/UnitAbilityListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null) obj=UnitAbilityEditorWindow.CreatePrefab();
		
		uABPrefab=obj.GetComponent<UnitAbilityListPrefab>();
		if(uABPrefab==null) uABPrefab=obj.AddComponent<UnitAbilityListPrefab>();
		
		allUAbList=uABPrefab.unitAbilityList;
		
		List<int> tempIDList=new List<int>();
		foreach(UnitAbility uAB in allUAbList){
			tempIDList.Add(uAB.ID);
		}
		
		foreach(UnitTB unit in unitList){
			for(int i=0; i<unit.abilityIDList.Count; i++){
				if(!tempIDList.Contains(unit.abilityIDList[i])){
					unit.abilityIDList.RemoveAt(i);
					i-=1;
				}
			}
		}
	}
	
	public static void LoadCollectible(){
		EditorCollectibleList collectibleInfo=CollectibleManagerWindow.LoadCollectible();
		collectibleList=collectibleInfo.prefab.collectibleList;
		collectibleNameList=new string[collectibleInfo.nameList.Length+1];
		for(int i=0; i<collectibleInfo.nameList.Length; i++){
			collectibleNameList[i]=collectibleInfo.nameList[i];
		}
		collectibleNameList[collectibleNameList.Length-1]="None";
	}
	
	private static void LoadDamageArmor(){
		GameObject obj=Resources.Load("PrefabList/DamageArmorList", typeof(GameObject)) as GameObject;
		
		if(obj==null) return;
		
		DamageArmorListPrefab prefab=obj.GetComponent<DamageArmorListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<DamageArmorListPrefab>();
		
		armorList=new string[prefab.armorList.Count];
		armorTooltipList=new string[prefab.armorList.Count];
		damageList=new string[prefab.damageList.Count];
		damageTooltipList=new string[prefab.damageList.Count];
		
		for(int i=0; i<prefab.armorList.Count; i++){
			armorList[i]=(prefab.armorList[i].name);
			armorTooltipList[i]=prefab.armorList[i].desp;
		}
		for(int i=0; i<prefab.damageList.Count; i++){
			damageList[i]=prefab.damageList[i].name;
			damageTooltipList[i]=prefab.damageList[i].desp;
		}
	}
	
	void OnEnable(){
		UnitAbilityEditorWindow.onUnitAbilityUpdateE+=LoadAbility;
		DamageArmorTableEditor.onDamageArmorTableUpdateE+=LoadDamageArmor;
	}
	
	void OnDisable(){
		UnitAbilityEditorWindow.onUnitAbilityUpdateE-=LoadAbility;
		DamageArmorTableEditor.onDamageArmorTableUpdateE-=LoadDamageArmor;
	}
	
	static string[] hierarchyLabelList;
	static GameObject[] hierarchyObjList;
	static void InitObjectHierarchy(int index){
		HierarchyList hl=GetTransformInHierarchy(unitList[index].transform, 0);
		hierarchyLabelList=new string[hl.stringList.Count+1];
		hierarchyObjList=new GameObject[hl.transformList.Count+1];
		for(int i=1; i<hierarchyLabelList.Length; i++){
			hierarchyLabelList[i]=hl.stringList[i-1];
		}
		for(int i=1; i<hierarchyObjList.Length; i++){
			hierarchyObjList[i]=hl.transformList[i-1].gameObject;
		}
		hierarchyLabelList[0]="None";
		hierarchyObjList[0]=null;
	}
	
	
	float startX, startY, height, spaceY;//, lW;
	//~ int rscCount=1;
	float contentHeight;
	
	GUIContent cont;
	GUIContent[] contL;
	
	private Vector2 scrollPos;
	private Vector2 scrollPosAbilBox;
	private Vector2 scrollPosMisc;
	
    void OnGUI () {
		if(window==null) Init();
		
		Rect visibleRect=new Rect(0, 0, window.position.width, window.position.height);
		Rect contentRect=new Rect(0, 0, window.position.width-15, contentHeight);
		scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
		//~ scrollPos = GUI.BeginScrollView(new Rect(0, 0, window.position.width, window.position.height), scrollPos, new Rect(0, 0, Mathf.Max(window.position.width, 610+(levelCap-3)*180), 1410));
		
		GUI.changed = false;
		
		if(GUI.Button(new Rect(window.position.width-120, 5, 100, 25), "UnitManager")){
			this.Close();
			UnitTBManagerWindow.Init();
		}
		
		startX=3;
		startY=3;
		height=15;
		//~ spaceY=height+startX;
		spaceY=17;
			
		if(unitList.Count>0) {
			GUI.SetNextControlName ("UnitSelect");
			index = EditorGUI.Popup(new Rect(startX, startY, 300, height), "Unit:", index, nameList);
			if(GUI.changed){
				InitObjectHierarchy(index);
				GUI.FocusControl ("UnitSelect");
			}
			
			
			UnitTB unit=unitList[index];
			
			EditorGUI.LabelField(new Rect(340, startY, 70, height), "Icon: ");
			unit.icon=(Texture)EditorGUI.ObjectField(new Rect(380, startY, 70, 70), unit.icon, typeof(Texture), false);
		
			if(unit.icon!=null && unit.icon.name!=unit.iconName){
				unit.iconName=unit.icon.name;
				GUI.changed=true;
			}
			
			
			//******************************************************************************************************************************************//
			//general
			
			startY+=5;
			cont=new GUIContent("PointCost:", "The cost of unit");
			unit.pointCost = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.pointCost);
			cont=new GUIContent("PointReward:", "Point gain when destroy the unit");
			unit.pointReward = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.pointReward);
			
			cont=new GUIContent("DefaultFaction:", "The default factionID for the unit");
			unit.factionID = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.factionID);
			
			startY+=10;
			cont=new GUIContent("HP Full:", "The default Hit Point of the unit");
			unit.fullHP = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, (int)unit.fullHP);
			cont=new GUIContent("AP Full:", "The maximum capacity of AP");
			unit.fullAP = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.fullAP);
			
			cont=new GUIContent("HPRegen (min/max):", "Hit Point regenerated every new round/turn");
			unit.HPGainMin = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 225, height), cont, unit.HPGainMin);
			unit.HPGainMax = EditorGUI.IntField(new Rect(startX+230, startY, 70, height), unit.HPGainMax);
			
			cont=new GUIContent("APRegen (min/max):", "Action Point regenerated every new round/turn");
			unit.APGainMin = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 225, height), cont, unit.APGainMin);
			unit.APGainMax = EditorGUI.IntField(new Rect(startX+230, startY, 70, height), unit.APGainMax);
			
			//cont=new GUIContent("HPRegenPerRound:", "Hit Point regenerated every new round");
			//unit.HPGain = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.HPGain);
			//cont=new GUIContent("APRegenPerRound:", "AP regenerated every new round");
			//unit.APGain = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.APGain);
			
			startY+=5;
			cont=new GUIContent("TurnPriority:", "Determine if the unit get to move first in certain turn mode");
			unit.turnPriority = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.turnPriority);
			
			cont=new GUIContent("MovementRange:", "Movement Range, in unit of tile");
			unit.movementRange = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.movementRange);

			cont=new GUIContent("SightRange:", "Range of the unit sight, in unit of tile\nonly applicable if fog-of-war is enabled");
			unit.sight = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.sight);
			
			
			startY+=5;
			cont=new GUIContent("MovePerTurn:", "Maximum move the unit can do for a turn");
			unit.movePerTurn = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.movePerTurn);
			
			cont=new GUIContent("AttackPerTurn:", "Maximum attack the unit can do for a turn");
			unit.attackPerTurn = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.attackPerTurn);
			
			cont=new GUIContent("CounterAttackPerTurn:", "Maximum counter attack the unit can do for a turn");
			unit.counterPerTurn = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.counterPerTurn);
			
			
			
			//******************************************************************************************************************************************//
			//offensive stats
			
			startY+=10;
			int attMode=(int)unit.attackMode;
			cont=new GUIContent("AttackMode:", "Attack Mode of the unit (Range/Melee/Hybrid)");
			contL=new GUIContent[attackModeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(attackModeLabel[i], attackModeTooltipLabel[i]);
			attMode = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, height), cont, attMode, contL);
			unit.attackMode = (_AttackMode)attMode;
			
			if(damageList.Length>0){
				contL=new GUIContent[damageTooltipList.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(damageList[i], damageTooltipList[i]);
				if(unit.attackMode==_AttackMode.Melee || unit.attackMode==_AttackMode.Hybrid){
					cont=new GUIContent("DamageType(Melee):", "Damage type to be inflicted on target");
					unit.damageTypeMelee = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, height), cont, unit.damageTypeMelee, contL);
				}
				if(unit.attackMode==_AttackMode.Range || unit.attackMode==_AttackMode.Hybrid){
					cont=new GUIContent("DamageType(Range):", "Damage type to be inflicted on target");
					unit.damageTypeRange = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, height), cont, unit.damageTypeRange, contL);
				}
			}
			else{
				if(unit.damageType>0) unit.damageType=0;
				
				cont=new GUIContent("DamageType:", "No damage type has been created, use DamageArmorTableEditor to create one");
				EditorGUI.LabelField(new Rect(startX, startY+spaceY, 300, height), cont);
				EditorGUI.LabelField(new Rect(startX+150, startY+spaceY, 150, height), "Invalid");
				
				if(GUI.Button(new Rect(startX+200, startY+=spaceY, 100, height), "OpenEditor")){
					DamageArmorTableEditor.Init();
				}
			}
			
			if(unit.attackMode==_AttackMode.Hybrid){
				cont=new GUIContent("AttackRange:", "Attack Range, in unit of tile\nThe first column being the maximum range for melee attack\nThe second column being the maximum range for range attack");
				unit.attackRangeMelee = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 225, height), cont, unit.attackRangeMelee);
				unit.attackRangeMax = EditorGUI.IntField(new Rect(startX+230, startY, 70, height), unit.attackRangeMax);
			}
			else if(unit.attackMode==_AttackMode.Range){
				cont=new GUIContent("AttackRange:", "Attack Range, in unit of tile\nThe first column being the maximum range for minimum attack range\nThe second column being the maximum range for maximum attack range");
				//unit.attackRange = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.attackRange);
				unit.attackRangeMin = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 225, height), cont, unit.attackRangeMin);
				unit.attackRangeMax = EditorGUI.IntField(new Rect(startX+230, startY, 70, height), unit.attackRangeMax);
			}
			else{
				//cont=new GUIContent("AttackRange:", "Attack Range, in unit of tile");
				//EditorGUI.LabelField(new Rect(startX, startY+spaceY, 225, height), cont);
				//EditorGUI.LabelField(new Rect(startX+150, startY+=spaceY, 225, height), "1");
				
				cont=new GUIContent("AttackRange:", "Attack Range, in unit of tile");
				unit.attackRangeMelee = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 225, height), cont, unit.attackRangeMelee);
			}
			
			
			if(unit.attackMode==_AttackMode.Melee || unit.attackMode==_AttackMode.Hybrid){
				cont=new GUIContent("MeleeDmg (min/max):", "Melee damage capacity of the unit");
				unit.meleeDamageMin = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 225, height), cont, unit.meleeDamageMin);
				unit.meleeDamageMax = EditorGUI.IntField(new Rect(startX+230, startY, 70, height), unit.meleeDamageMax);
			}
			if(unit.attackMode==_AttackMode.Range || unit.attackMode==_AttackMode.Hybrid){
				cont=new GUIContent("RangeDmg (min/max):", "Range damage capacity of the unit");
				unit.rangeDamageMin = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 225, height), cont, unit.rangeDamageMin);
				unit.rangeDamageMax = EditorGUI.IntField(new Rect(startX+230, startY, 70, height), unit.rangeDamageMax);
			}
			
			cont=new GUIContent("CounterModifer:", "Damage modifier when performing counter attack");
			unit.counterDmgModifier=EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.counterDmgModifier);
			
			
			startY+=5;
			if(unit.attackMode==_AttackMode.Melee || unit.attackMode==_AttackMode.Hybrid){
				cont=new GUIContent("HitChance (Melee):", "The melee hit chance of the unit, take value from 0-1");
				unit.attMelee = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.attMelee);
			}
			if(unit.attackMode==_AttackMode.Range || unit.attackMode==_AttackMode.Hybrid){
				cont=new GUIContent("HitChance (Range):", "The range hit chance of the unit, take value from 0-1");
				unit.attRange = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.attRange);
			}
			
			
			if(unit.attackMode==_AttackMode.Melee || unit.attackMode==_AttackMode.Hybrid){
				cont=new GUIContent("CriticalChance (Melee):", "The critical chance of the unit, takes value from 0-1");
				unit.criticalMelee = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.criticalMelee);
			}
			if(unit.attackMode==_AttackMode.Range || unit.attackMode==_AttackMode.Hybrid){
				cont=new GUIContent("CriticalChance (Range):", "The critical chance of the unit, takes value from 0-1");
				unit.criticalRange = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.criticalRange);
			}
			
			
			
			
			
			
			//******************************************************************************************************************************************//
			//defensive stats
			
			startY+=10;
			
			if(armorList.Length>0){
				cont=new GUIContent("ArmorType:", "Armor type of the unit");
				contL=new GUIContent[armorTooltipList.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(armorList[i], armorTooltipList[i]);
				unit.armorType = EditorGUI.Popup(new Rect(startX, startY+=spaceY, 300, height), cont, unit.armorType, contL);
			}
			else{
				if(unit.armorType>0) unit.armorType=0;
				
				cont=new GUIContent("ArmorType:", "No armor type has been created, use DamageArmorTableEditor to create one");
				EditorGUI.LabelField(new Rect(startX, startY+spaceY, 300, height), cont);
				EditorGUI.LabelField(new Rect(startX+150, startY+spaceY, 150, height), "Invalid");
				
				if(GUI.Button(new Rect(startX+200, startY+=spaceY, 100, height), "OpenEditor")){
					DamageArmorTableEditor.Init();
				}
			}
			
			cont=new GUIContent("DodgeChance:", "The chances of the unit to dodge any normal melee/range attack, take value from 0-1");
			unit.defend = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.defend);
			
			cont=new GUIContent("CriticalImmunity:", "The critical immunity of the unit, negate attacker critical chance, take value from 0-1");
			unit.critDef = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.critDef);
			
			cont=new GUIContent("DamageReduction:", "Value to be deduct from any incoming damage");
			unit.damageReduc = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.damageReduc);
			
			
			startY+=10;
			cont=new GUIContent("APCostPerAttack:", "AP cost needed for every attack");
			unit.APCostAttack = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.APCostAttack);
			
			cont=new GUIContent("APCostPerMovement:", "AP cost needed for every movement");
			unit.APCostMove = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.APCostMove);
			
			
			//******************************************************************************************************************************************//
			//misc
			
			startY+=10;
			cont=new GUIContent("TargetPoint: ", "the dummy transform which serve as the reference position for all visual purpose for the unit. ie. effect spawn point, shootObject target point, etc.");
			unit.targetPointT=(Transform)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.targetPointT, typeof(Transform), false);
			
			startY+=10;
			cont=new GUIContent("DestroyedEffect:", "The visual effect to spawn when the unit is destroyed");
			unit.destroyedEffect=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.destroyedEffect, typeof(GameObject), false);
			cont=new GUIContent("DestroyEffectDuration:", "The duration for the all the destroyed visual effect of the unit (used for action cam)");
			unit.destroyEffectDuration = EditorGUI.FloatField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.destroyEffectDuration);
			
			startY+=10;
			cont=new GUIContent("SpawnCollectible:", "Check to have the unit spawn a collectible when it's destroyed. One of the collecible from ths list specified will be selected randomly");
			unit.spawnCollectibleUponDestroyed=EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, height), cont, unit.spawnCollectibleUponDestroyed);
			
			if(unit.spawnCollectibleUponDestroyed){
				for(int i=0; i<unit.collectibleList.Count; i++){
					int collectibleID=-1;
					if(unit.collectibleList[i]==null) collectibleID=0;
					else{
						for(int n=0; n<collectibleList.Count; n++){ if(collectibleList[n]==unit.collectibleList[i]) collectibleID=n; }
					}
					if(collectibleID==-1){
						unit.collectibleList.RemoveAt(i); i-=1;
						continue;
					}
					cont=new GUIContent(" - Collectible"+(i+1), "the collectible to be spawn the unit is killed");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
					collectibleID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), collectibleID, collectibleNameList);
					
					if(collectibleID<collectibleList.Count) unit.collectibleList[i] = collectibleList[collectibleID];
					else{
						unit.collectibleList.RemoveAt(i);
						i-=1;
					}
				}
				
				if(unit.collectibleList.Count!=collectibleList.Count){
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), " - Add new: ");
					int collectibleID=collectibleList.Count;
					collectibleID=EditorGUI.Popup(new Rect(startX+150, startY, 150, height), collectibleID, collectibleNameList);
					
					if(collectibleID<collectibleList.Count){
						if(!unit.collectibleList.Contains(collectibleList[collectibleID])){
							unit.collectibleList.Add(collectibleList[collectibleID]);
						}
					}
					
				}
			}
			
			
			startY+=10;
			if(unit.attackMode!=_AttackMode.Melee){
				cont=new GUIContent("ShootObject:", "the object to be fired by the creep range attack (must contain a ShootObject component)");
				unit.shootObject=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.shootObject, typeof(GameObject), false);
			}
			else startY+=spaceY;
			
			
			startY+=10;
			cont=new GUIContent("Unit Ability:", "Abilities for this unit. Select any ability to add to this unit, deselect to remove.");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, 54), cont);
			string label="Expand";
			if(expandAbilityPanel) label="Minimise";
			if(GUI.Button(new Rect(startX+80, startY, 70, 17), label)){
				expandAbilityPanel=!expandAbilityPanel;
			}
			startY+=25;
			
			
			
			//******************************************************************************************************************************************//
			//abilities
			
			int count=0;
			if(!expandAbilityPanel){
				foreach(int abID in unit.abilityIDList){
					foreach(UnitAbility uAB in uABPrefab.unitAbilityList){
						if(uAB.ID==abID){
							GUI.Box(new Rect(startX+count*70, startY, 60, 60), "");
							cont=new GUIContent(uAB.icon, uAB.name+" - "+uAB.desp);
							EditorGUI.LabelField(new Rect(startX+count*70+3, startY+3, 54, 54), cont);
							count+=1;
						}
					}
				}
				
				startY+=65;
			}
			else{
				
				//~ Debug.Log(window.position.width);
				Rect abilityBoxRect=new Rect(startX, startY, 720, 215);
				Rect abilityBoxcontentRect=new Rect(startX-2, startY-2, window.position.width-28, Mathf.Ceil((uABPrefab.unitAbilityList.Count/10)+1)*70);
				GUI.Box(abilityBoxRect, "");
				
				scrollPosAbilBox = GUI.BeginScrollView(abilityBoxRect, scrollPosAbilBox, abilityBoxcontentRect);
				
				float startXX=startX+8; 
				float startYY=startY+8;
				foreach(UnitAbility uAB in uABPrefab.unitAbilityList){
					cont=new GUIContent(uAB.icon, uAB.name+" - "+uAB.desp);
					
					if(unit.abilityIDList.Contains(uAB.ID)) GUI.color=Color.green;
					GUI.Box(new Rect(startXX+count*70, startYY, 60, 60), "");
					GUI.color=Color.white;
					
					if(GUI.Button(new Rect(startXX+count*70+4, startYY+4, 52, 52), cont)){
						if(!unit.abilityIDList.Contains(uAB.ID)){
							if(unit.abilityIDList.Count<6){
								unit.abilityIDList.Add(uAB.ID);
							}
							else Debug.Log("a unit cannot have more than 6 abilities");
						}
						else unit.abilityIDList.Remove(uAB.ID);
					}
					
					
					
					count+=1;
					if(count%10==0){
						startXX-=10*70;
						startYY+=70;
					}
				}
				
				GUI.EndScrollView();
				
				startY+=220;
			}
			
			
			
			
			EditorGUI.LabelField(new Rect(startX, startY+=5, 300, 25), "Unit Description (for runtime UI): ");
			unit.desp=EditorGUI.TextArea(new Rect(startX, startY+=17, window.position.width-20, 50), unit.desp);
			startY+=60;
			contentHeight=startY;
			
			
			startX=340; 
			startY=100;
			//~ Rect visibleRectMisc=new Rect(startX, startY, 340, 395);
			Rect visibleRectMisc=new Rect(startX, startY, 340, 540);
			Rect contentRectMisc=new Rect(startX, startY, 325, miscContentHeight);
			
			GUI.Box(visibleRectMisc, "");
			
			scrollPosMisc = GUI.BeginScrollView(visibleRectMisc, scrollPosMisc, contentRectMisc);
			float tempY=startY;
			
			
			startX+=10;
			
			
			if(unit.attackMode!=_AttackMode.Melee){
				int turretObjID=-1;
				if(unit.turretObject==null) turretObjID=0;
				else{
					for(int n=0; n<hierarchyObjList.Length; n++){ if(hierarchyObjList[n]==unit.turretObject.gameObject) turretObjID=n; }
				}
				cont=new GUIContent("Turret Object", "The part which rotate to face the target when shooting");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
				turretObjID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), turretObjID, hierarchyLabelList);
				if(turretObjID>0) unit.turretObject = hierarchyObjList[turretObjID].transform;
				else if(turretObjID==0) unit.turretObject = null;
				
				
				
				cont=new GUIContent("Shoot Points Count:", "number of shoot point for the unit");
				EditorGUI.LabelField(new Rect(startX, startY+spaceY, 300, height), cont);
				if(GUI.Button(new Rect(startX+150, startY+spaceY, 50, height), "+")){
					unit.shootPoints.Add(unit.transform);
				}
				if(GUI.Button(new Rect(startX+210, startY+spaceY, 50, height), "-")){
					if(unit.shootPoints.Count>1)
						unit.shootPoints.RemoveAt(unit.shootPoints.Count-1);
				}
				startY+=spaceY;
				
				if(unit.shootPoints.Count==0) unit.shootPoints.Add(unit.transform);
				
				for(int i=0; i<unit.shootPoints.Count; i++){
					int spObjID=-1;
					if(unit.shootPoints[i]==null) spObjID=0;
					else{
						for(int n=0; n<hierarchyObjList.Length; n++){ if(hierarchyObjList[n]==unit.shootPoints[i].gameObject) spObjID=n; }
					}
					cont=new GUIContent("   - "+i+":", "dummy transform which acts as a spawn position for shootObject\neach ShootPoint spawn a shootObject");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
					spObjID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), spObjID, hierarchyLabelList);
					if(spObjID>0) unit.shootPoints[i] = hierarchyObjList[spObjID].transform;
					else if(spObjID==0) unit.shootPoints[i] = null;
				}
				
				startY+=30;
			}
			else startY+=10;
			
			
			startY=UnitTBAudioConfigurator(index, startX, startY);
			
			startY+=30;
			startY=UnitTBAnimationConfigurator(index, startX, startY);
			
			startY+=20;
			miscContentHeight=startY-tempY;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) EditorUtility.SetDirty(unit);
		}
		else{
			EditorGUI.LabelField(new Rect(startX, startY, 450, height), "No Unit has been assigned, please do so in the UnitManager");
		}
		
		
		GUI.EndScrollView();
	}
	
	float miscContentHeight=0;
	
	public static UnitAbilityListPrefab uABPrefab;
	public static List<UnitAbility> allUAbList=new List<UnitAbility>();
	bool expandAbilityPanel=false;
	
	bool showAudio=false;
	float UnitTBAudioConfigurator(int ID, float startX, float startY){
		UnitTBAudio audio=unitList[ID].gameObject.GetComponent<UnitTBAudio>();
		
		//~ float startX=350;
		//float startY=105;
		float spaceY=18;
		
		string label="Audio Setting - ";
		if(showAudio) label+="Hide";
		else label+="Show";
		showAudio=EditorGUI.Foldout(new Rect(startX, startY, 300, height-3), showAudio, label);
		
		if(showAudio){
			cont=new GUIContent("   SelectSound:", "Audio clip to play when the unit is selected");
			audio.selectSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, audio.selectSound, typeof(AudioClip), false);
			
			cont=new GUIContent("   MoveSound:", "Audio clip to play when the unit is moving");
			audio.moveSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, audio.moveSound, typeof(AudioClip), false);
			
			if(audio.moveSound!=null){
				cont=new GUIContent("   LoopMoveSound:", "Check to have move sound loop when playing until unit stop\nElse the clip will be played on once when the unit start moving");
				audio.loopMoveSound=EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, 17), cont, audio.loopMoveSound);
			}
			
			cont=new GUIContent("   MeleeAttackSound:", "Audio clip to play when the unit is doing a melee attack");
			audio.meleeAttackSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, audio.meleeAttackSound, typeof(AudioClip), false);
			
			cont=new GUIContent("   RangeAttackSound:", "Audio clip to play when the unit is doing a range attack");
			audio.rangeAttackSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, audio.rangeAttackSound, typeof(AudioClip), false);
			
			cont=new GUIContent("   HitSound:", "Audio clip to play when the unit is hit");
			audio.hitSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, audio.hitSound, typeof(AudioClip), false);
			
			cont=new GUIContent("   MissedSound:", "Audio clip to play when the unit is hit");
			audio.missedSound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, audio.missedSound, typeof(AudioClip), false);
			
			cont=new GUIContent("   DestroySound:",  "Audio clip to play when the unit has been destroyed");
			audio.destroySound=(AudioClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, audio.destroySound, typeof(AudioClip), false);
		}
		
		return startY;
	}
	
	
	
	
	bool showAnimation=false;
	int count0, count1, count2, count3, count4, count5, count6, count7;
	float UnitTBAnimationConfigurator(int ID, float startX, float startY){
		UnitTBAnimation ani=unitList[ID].gameObject.GetComponent<UnitTBAnimation>();
		
		//float startX=350;
		//float startY=105;
		float spaceY=18;
		
		string label="Animation Setting - ";
		if(showAnimation) label+="Hide";
		else label+="Show";
		showAnimation=EditorGUI.Foldout(new Rect(startX, startY, 300, height-3), showAnimation, label);
		
		if(showAnimation){
			
			count0=ani.idleAniClip.Length;
			count1=ani.moveAniClip.Length;
			count2=ani.hitAniClip.Length;
			count3=ani.destroyAniClip.Length;
			count4=ani.meleeAttackAniClip.Length;
			count5=ani.rangeAttackAniClip.Length;
			
			
			int idleAniBodyID=-1;
			for(int i=0; i<hierarchyObjList.Length; i++){ if(hierarchyObjList[i]==ani.idleAniBody) idleAniBodyID=i; }
			cont=new GUIContent("   IdleAnimatedObject:", "game-object which play the idle animation");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
			idleAniBodyID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), idleAniBodyID, hierarchyLabelList);
			if(idleAniBodyID>=0) ani.idleAniBody = hierarchyObjList[idleAniBodyID];
			if(ani.moveAniBody!=null){
				cont=new GUIContent("   IdleAnimationClip:", "the number of idle animation available, a random one will be chosen to play everytime a idle animation is required");
				count0=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, 17), cont, count0);
				if(count0!=ani.idleAniClip.Length) ani.idleAniClip=MatchAnimationLength(ani.idleAniClip, count0); startY+=1;
				for(int i=0; i<ani.idleAniClip.Length; i++)
				ani.idleAniClip[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY-1, 300, 17), "    - "+i+":", ani.idleAniClip[i], typeof(AnimationClip), false);
			}
			startY+=10;
			
			
			int moveAniBodyID=-1;
			for(int i=0; i<hierarchyObjList.Length; i++){ if(hierarchyObjList[i]==ani.moveAniBody) moveAniBodyID=i; }
			cont=new GUIContent("   MoveAnimatedObject:", "game-object which play the move animation");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
			moveAniBodyID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), moveAniBodyID, hierarchyLabelList);
			if(moveAniBodyID>=0) ani.moveAniBody = hierarchyObjList[moveAniBodyID];
			//~ cont=new GUIContent("   MoveAnimatedObject:", "game-object which play the move animation");
			//~ ani.moveAniBody=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, ani.moveAniBody, typeof(GameObject), false);
			if(ani.moveAniBody!=null){
				//~ GUI.Box(new Rect(startX+12, startY+spaceY, 290, ani.moveAniClip.Length*(spaceY-1)+spaceY), "");
				cont=new GUIContent("   MoveAnimationClip:", "the number of move animation available, a random one will be chosen to play everytime a move animation is required");
				count1=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, 17), cont, count1);
				if(count1!=ani.moveAniClip.Length) ani.moveAniClip=MatchAnimationLength(ani.moveAniClip, count1); startY+=1;
				for(int i=0; i<ani.moveAniClip.Length; i++)
				ani.moveAniClip[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY-1, 300, 17), "    - "+i+":", ani.moveAniClip[i], typeof(AnimationClip), false);
			}
			startY+=10;
			
			
			int hitAniBodyID=-1;
			for(int i=0; i<hierarchyObjList.Length; i++){ if(hierarchyObjList[i]==ani.hitAniBody) hitAniBodyID=i; }
			cont=new GUIContent("   HitAnimatedObject:", "game-object which play the hit animation");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
			hitAniBodyID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), hitAniBodyID, hierarchyLabelList);
			if(hitAniBodyID>=0) ani.hitAniBody = hierarchyObjList[hitAniBodyID];
			//~ cont=new GUIContent("   HitAnimatedObject:", "game-object which play the hit animation");
			//~ ani.hitAniBody=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, ani.hitAniBody, typeof(GameObject), false);
			if(ani.hitAniBody!=null){
				//~ GUI.Box(new Rect(startX+12, startY+spaceY, 290, ani.hitAniClip.Length*(spaceY-1)+spaceY), "");
				cont=new GUIContent("   HitAnimationClip:", "the number of hit animation available, a random one will be chosen to play everytime a hit animation is required");
				count2=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, 17), cont, count2);
				if(count2!=ani.hitAniClip.Length) ani.hitAniClip=MatchAnimationLength(ani.hitAniClip, count2); startY+=1;
				for(int i=0; i<ani.hitAniClip.Length; i++)
				ani.hitAniClip[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY-1, 300, 17), "    - "+i+":", ani.hitAniClip[i], typeof(AnimationClip), false);
			}
			startY+=10;
			
			
			int destroyAniBodyID=-1;
			for(int i=0; i<hierarchyObjList.Length; i++){ if(hierarchyObjList[i]==ani.destroyAniBody) destroyAniBodyID=i; }
			cont=new GUIContent("   DestroyAni-Object:", "game-object which play the destroy animation");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
			destroyAniBodyID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), destroyAniBodyID, hierarchyLabelList);
			if(destroyAniBodyID>=0) ani.destroyAniBody = hierarchyObjList[destroyAniBodyID];
			//~ cont=new GUIContent("   DestroyAni-Object:", "game-object which play the hit animation");
			//~ ani.destroyAniBody=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, ani.destroyAniBody, typeof(GameObject), false);
			if(ani.destroyAniBody!=null){
				//~ GUI.Box(new Rect(startX+12, startY+spaceY, 290, ani.destroyAniClip.Length*(spaceY-1)+spaceY), "");
				cont=new GUIContent("   DestroyAnimationClip:", "the number of hit animation available, a random one will be chosen to play everytime a hit animation is required");
				count3=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, 17), cont, count3);
				if(count3!=ani.destroyAniClip.Length) ani.destroyAniClip=MatchAnimationLength(ani.destroyAniClip, count3); startY+=1;
				for(int i=0; i<ani.destroyAniClip.Length; i++)
				ani.destroyAniClip[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY-1, 300, 17), "    - "+i+":", ani.destroyAniClip[i], typeof(AnimationClip), false);
			}
			startY+=10;
			
			if(unitList[ID].attackMode!=_AttackMode.Range){
				int meleeAttackAniBodyID=-1;
				for(int i=0; i<hierarchyObjList.Length; i++){ if(hierarchyObjList[i]==ani.meleeAttackAniBody) meleeAttackAniBodyID=i; }
				cont=new GUIContent("   MeleeAttackAni-Obj:", "game-object which play the destroy animation");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
				meleeAttackAniBodyID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), meleeAttackAniBodyID, hierarchyLabelList);
				if(meleeAttackAniBodyID>=0) ani.meleeAttackAniBody = hierarchyObjList[meleeAttackAniBodyID];
				//~ cont=new GUIContent("   MeleeAttackAni-Object:", "game-object which play the melee attack animation");
				//~ ani.meleeAttackAniBody=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, ani.meleeAttackAniBody, typeof(GameObject), false);
				if(ani.meleeAttackAniBody!=null){
					//~ GUI.Box(new Rect(startX+12, startY+spaceY, 290, ani.moveAniClip.Length*(spaceY-1)+spaceY), "");
					cont=new GUIContent("   MeleeAttackAni-Clip:", "the number of melee attack animation available, a random one will be chosen to play everytime an attack animation is required");
					count4=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, 17), cont, count4);
					if(count4!=ani.meleeAttackAniClip.Length){
						ani.meleeAttackAniClip=MatchAnimationLength(ani.meleeAttackAniClip, count4); startY+=1;
					}
					if(count4!=ani.meleeAttackAniDelay.Length){
						ani.meleeAttackAniDelay=MatchAniDelayLength(ani.meleeAttackAniDelay, count4); startY+=1;
					}
					for(int i=0; i<count4; i++){
						startY+=spaceY;
						cont=new GUIContent("    - "+i +" (delay):", "animation clip (the delay bfore the damage is inflicted after the animation is played)");
						EditorGUI.LabelField(new Rect(startX, startY, 300, 17), cont);
						ani.meleeAttackAniClip[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+100, startY, 150, 17), ani.meleeAttackAniClip[i], typeof(AnimationClip), false);
						ani.meleeAttackAniDelay[i]=EditorGUI.FloatField(new Rect(startX+260, startY, 40, 17), ani.meleeAttackAniDelay[i]);
						//ani.meleeAttackAniDelay[i]=EditorGUI.FloatField(new Rect(startX+310, startY, 40, 17), ani.meleeAttackAniDelay[i]);
					}
				}
				startY+=10;
			}
			
			if(unitList[ID].attackMode!=_AttackMode.Melee){
				int rangeAttackAniBodyID=-1;
				for(int i=0; i<hierarchyObjList.Length; i++){ if(hierarchyObjList[i]==ani.rangeAttackAniBody) rangeAttackAniBodyID=i; }
				cont=new GUIContent("   RangeAttackAni-Obj:", "game-object which play the destroy animation");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 300, height), cont);
				rangeAttackAniBodyID = EditorGUI.Popup(new Rect(startX+150, startY, 150, height), rangeAttackAniBodyID, hierarchyLabelList);
				if(rangeAttackAniBodyID>=0) ani.rangeAttackAniBody = hierarchyObjList[rangeAttackAniBodyID];
				//~ cont=new GUIContent("   RangeAttackAni-Object:", "game-object which play the range attack animation");
				//~ ani.rangeAttackAniBody=(GameObject)EditorGUI.ObjectField(new Rect(startX, startY+=spaceY, 300, 17), cont, ani.rangeAttackAniBody, typeof(GameObject), false);
				if(ani.rangeAttackAniBody!=null){
					//~ GUI.Box(new Rect(startX+12, startY+spaceY, 290, ani.moveAniClip.Length*(spaceY-1)+spaceY), "");
					cont=new GUIContent("   RangeAttackAni-Clip:", "the number of range attack animation available, a random one will be chosen to play everytime an attack animation is required");
					count5=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, 17), cont, count5);
					if(count5!=ani.rangeAttackAniClip.Length){
						ani.rangeAttackAniClip=MatchAnimationLength(ani.rangeAttackAniClip, count5); startY+=1;
					}
					if(count5!=ani.rangeAttackAniDelay.Length){
						ani.rangeAttackAniDelay=MatchAniDelayLength(ani.rangeAttackAniDelay, count5); startY+=1;
					}
					for(int i=0; i<count5; i++){
						startY+=spaceY;
						cont=new GUIContent("    - "+i +" (delay):", "animation clip (the delay bfore the shootObject is fired after the animation is played)");
						EditorGUI.LabelField(new Rect(startX, startY, 300, 17), cont);
						ani.rangeAttackAniClip[i]=(AnimationClip)EditorGUI.ObjectField(new Rect(startX+100, startY, 150, 17), ani.rangeAttackAniClip[i], typeof(AnimationClip), false);
						ani.rangeAttackAniDelay[i]=EditorGUI.FloatField(new Rect(startX+260, startY, 40, 17), ani.rangeAttackAniDelay[i]);
						//ani.rangeAttackAniDelay[i]=EditorGUI.FloatField(new Rect(startX+310, startY, 40, 17), ani.rangeAttackAniDelay[i]);
					}
				}
				startY+=10;
			}
		}
		
		return startY;
	}
	
	AnimationClip[] MatchAnimationLength(AnimationClip[] list, int count){
		AnimationClip[] newList=new AnimationClip[count];
		for(int i=0; i<newList.Length; i++){
			if(i<list.Length) newList[i]=list[i];
		}
		return newList;
	}
		
	float[] MatchAniDelayLength(float[] list, int count){
		float[] newList=new float[count];
		for(int i=0; i<newList.Length; i++){
			if(i<list.Length) newList[i]=list[i];
		}
		return newList;
	}

	
	public static HierarchyList GetTransformInHierarchy(Transform transform, int depth){
		HierarchyList hl=new HierarchyList();
		
		hl=GetTransformInHierarchyRecursively(transform, depth);
		
		hl.transformList.Insert(0, transform);
		hl.stringList.Insert(0, "-"+transform.name);
		
		return hl;
	}
	public static HierarchyList GetTransformInHierarchyRecursively(Transform transform, int depth){
		HierarchyList hl=new HierarchyList();
		depth+=1;
		foreach(Transform t in transform){
			string label="";
			for(int i=0; i<depth; i++) label+="   ";
			
			hl.transformList.Add(t);
			hl.stringList.Add(label+"-"+t.name);
			
			HierarchyList tempHL=GetTransformInHierarchyRecursively(t, depth);
			foreach(Transform tt in tempHL.transformList){
				hl.transformList.Add(tt);
			}
			foreach(string ll in tempHL.stringList){
				hl.stringList.Add(ll);
			}
		}
		return hl;
	}
	
}


public class HierarchyList{
	public List<Transform> transformList=new List<Transform>();
	public List<string> stringList=new List<string>();
}
