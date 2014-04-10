#define ibox
#define saturation

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//special class for unit storing attacker in the past few round
//for AI so AI unit can retaliate when the attacker is out of aggro range
[System.Serializable]
public class Attacker{
	public UnitTB unit;
	public int roundSinceLastAttack=0;
	
	public Attacker(UnitTB attacker){
		unit=attacker;
		roundSinceLastAttack=0;
	}
}

public enum _AttackType{Range_Normal, Range_Counter, Melee_Normal, Melee_Counter, Misc}

public class AttackInstance{
	public _AttackType type=_AttackType.Range_Normal;
	public bool isCounterAttack=false;
	public UnitAbility unitAbility;
	public ShootObjectTB shootObj;
	
	public UnitTB srcUnit;
	public UnitTB targetUnit;
	
	public bool processed=false;
	public bool destroyed=false;
	public bool missed=false;
	public bool critical=false;
	public bool counterAttacking=false;		//indicate if an counter attack is incoming after the attack hit, this is the only thing return from the target
	public int damageDone=0;
	
	public float destroyEffectDuration=2f;
	
	
	public void Process(UnitTB unit){
		processed=true;
		
		targetUnit=unit;
		
		Tile targetTile=targetUnit.occupiedTile;
		
		//get cover bonus of the unit if cover is enabled for the game
		float coverDefBonus=0;
		float exposedCritBonus=0;
		if(GameControlTB.EnableCover()){
			coverDefBonus=targetTile.GetCoverDefendBonus(srcUnit.occupiedTile.pos);
			if(coverDefBonus<=0){
				exposedCritBonus=GameControlTB.GetExposedCritBonus();
			}
		}
		
		float attack=0;
		int damageType=0;
		if(type==_AttackType.Range_Normal || type==_AttackType.Range_Counter){
			attack=srcUnit.GetRangeAttack();
			damageType=srcUnit.damageTypeRange;
		}
		else if(type==_AttackType.Melee_Normal || type==_AttackType.Melee_Counter){
			attack=srcUnit.GetMeleeAttack();
			damageType=srcUnit.damageTypeMelee;
		}
		
		if(type==_AttackType.Range_Counter || type==_AttackType.Melee_Counter){
			isCounterAttack=true;
		}
		
		//calculate if the attack hits
		float missChance=(1-attack)+targetUnit.GetDefend()+coverDefBonus;
		if(Random.Range(0f, 1f)<Mathf.Max(0.0f, missChance)){
			if(!isCounterAttack) Debug.Log(srcUnit.unitName+" attacked "+targetUnit.unitName+" and missed");
			else Debug.Log(srcUnit.unitName+" counter-attack "+targetUnit.unitName+" and missed");
			missed=true;
			return;
		}
		
		//get the damage modifier from the damagetable based on the unit armorType and the attacking damageType
		float modifier=DamageTable.GetModifier(targetUnit.armorType, damageType);
		int dmg=0;
		
		float dmgMin=0;
		float dmgMax=0;
		float critChance=0;
		
		if(type==_AttackType.Range_Normal){
			dmgMin=srcUnit.GetRangeDamageMin();
			dmgMax=srcUnit.GetRangeDamageMax();
			critChance=srcUnit.GetRangeCritical();
		}
		else if(type==_AttackType.Melee_Normal){
			dmgMin=srcUnit.GetMeleeDamageMin();
			dmgMax=srcUnit.GetMeleeDamageMax();
			critChance=srcUnit.GetMeleeCritical();
		}
		else if(type==_AttackType.Range_Counter){
			dmgMin=(int)((float)srcUnit.GetRangeDamageMin()*srcUnit.GetCounterDmgModifier());
			dmgMax=(int)((float)srcUnit.GetRangeDamageMax()*srcUnit.GetCounterDmgModifier());
			critChance=srcUnit.GetRangeCritical();
		}
		else if(type==_AttackType.Melee_Counter){
			dmgMin=(int)((float)srcUnit.GetMeleeDamageMin()*srcUnit.GetCounterDmgModifier());
			dmgMax=(int)((float)srcUnit.GetMeleeDamageMax()*srcUnit.GetCounterDmgModifier());
			critChance=srcUnit.GetMeleeCritical();
		}
		
		//calculate if the attack crits
		if(Random.Range(0f, 1f)<=critChance+exposedCritBonus){
			dmg=(int)((float)dmgMax*Random.Range(1.5f, 2f));
			dmg=(int)((float)dmg*modifier);
			critical=true;
			if(!isCounterAttack) Debug.Log(srcUnit.unitName+" attacked "+targetUnit.unitName+" for "+dmg+" (critical)");
			else Debug.Log(srcUnit.unitName+" counter-attack "+targetUnit.unitName+" for "+dmg+" (critical)");
		}
		else{
			dmg=(int)((float)Random.Range(dmgMin, dmgMax+1)*modifier);
			if(!isCounterAttack) Debug.Log(srcUnit.unitName+" attacked "+targetUnit.unitName+" for "+dmg+" ");
			else Debug.Log(srcUnit.unitName+" counter-attack "+targetUnit.unitName+" for "+dmg+" ");
		}
		
		damageDone=dmg;
		destroyed=dmg>=targetUnit.HP ? true : false;
		destroyEffectDuration=targetUnit.destroyEffectDuration;
	}
	
	public AttackInstance Clone(){
		AttackInstance attInst=new AttackInstance();
		attInst.type=type;
		attInst.unitAbility=unitAbility;
		attInst.shootObj=shootObj;
		attInst.srcUnit=srcUnit;
		attInst.targetUnit=targetUnit;
		
		attInst.processed=processed;
		attInst.destroyed=destroyed;
		attInst.missed=missed;
		attInst.critical=critical;
		attInst.counterAttacking=counterAttacking;
		attInst.damageDone=damageDone;
		attInst.destroyEffectDuration=destroyEffectDuration;
		
		return attInst;
	}
}


public class EffectOverlay{
	public delegate void EffectOverlayHandler(EffectOverlay eff); 
	public static event EffectOverlayHandler onEffectOverlayE;
	
	public Vector3 pos;
	public string msg;
	public Color color;
	public bool useColor=false;
	
	public EffectOverlay(Vector3 p, string m){
		pos=p;
		msg=m;
		if(onEffectOverlayE!=null) onEffectOverlayE(this);
	}
	public EffectOverlay(Vector3 p, string m, Color col){
		pos=p;
		msg=m;
		color=col;
		useColor=true;
		
		if(onEffectOverlayE!=null) onEffectOverlayE(this);
	}
}


public enum _AttackMode{Melee, Range, Hybrid}


[RequireComponent (typeof (UnitTBAudio))]
[RequireComponent (typeof (UnitTBAnimation))]

public class UnitTB : MonoBehaviour {

	/*
	//animation testing
	void Update(){
		if(Input.GetMouseButtonDown(0)){
			animationTB.PlayHit();
		}
		else if(Input.GetMouseButtonDown(1)){
			animationTB.PlayRangeAttack();
		}
		else if(Input.GetKeyDown(KeyCode.H)){
			animationTB.PlayMove();
		}
		else if(Input.GetKeyDown(KeyCode.J)){
			animationTB.PlayIdle();
		}
	}
	*/

	public delegate void UnitSelectedHandler(UnitTB unit);
	public static event UnitSelectedHandler onUnitSelectedE;
	
	public delegate void UnitDeselectedHandler();
	public static event UnitDeselectedHandler onUnitDeselectedE;
	
	//to clear actionInProgress flag on GameControlTB, listen by CameraControl for actionCam event as well
	public delegate void ActionCompletedHandler(UnitTB unit);
	public static event ActionCompletedHandler onActionCompletedE;
	
	//when unit stop at a new position, for fog for war
	public delegate void NewPositionHandler(UnitTB unit);
	public static event NewPositionHandler onNewPositionE;
	
	//call when all available action is performed, for switching to new turn and next unit in turn
	public delegate void TurnDepletedHandler();
	public static event TurnDepletedHandler onTurnDepletedE;
	
	public delegate void UnitDestroyedHandler(UnitTB unit); 
	public static event UnitDestroyedHandler onUnitDestroyedE;
	
	//for UI unit overlay
	public delegate void EffectAppliedHandler(UnitTB unit);
	public static event EffectAppliedHandler onEffectAppliedE;
	
	//for UI unit overlay
	public delegate void EffectExpiredHandler(UnitTB unit);
	public static event EffectExpiredHandler onEffectExpiredE;
	
	//unit attribute ************************************
	public Texture icon;
	public string iconName="";
	
	[HideInInspector] public int prefabID=0;
	public string unitName="unit";
	public string desp="Description for unit....";
	public int factionID=0;
	public Faction faction;
	public int pointCost=5;
	public int pointReward;
	//public int level=1;
	
	
	//HP & AP stats ************************************
	public int fullHP=10;
	public int HP=10;
	public int fullAP=5;
	public int AP=0;
	
	//public int HPGain=0;
	//public int APGain=1;
	public int HPGainMin=0;
	public int HPGainMax=0;
	public int APGainMin=0;
	public int APGainMax=0;
	
	
	//general stats ************************************
	public int turnPriority=5;			//use to calculate which unit get to move first, turnPriority
	public int movementRange=3;
	public int sight=5;
	
	public int movePerTurn=1;
	public int moveRemain=1;
	public int attackPerTurn=1;
	public int attackRemain=1;
	public int counterPerTurn=1;
	public int counterAttackRemain=1;
	
	
	//offsensive stat ************************************
	public _AttackMode attackMode;
	public int damageType=0;
	public int damageTypeMelee=0;
	public int damageTypeRange=0;
	
	public int attackRangeMelee=1;
	public int attackRangeMin=0;
	public int attackRangeMax=4;
	//public int damage=2;
	public int rangeDamageMin=2;
	public int rangeDamageMax=6;
	public int meleeDamageMin=2;
	public int meleeDamageMax=6;
	
	public float counterDmgModifier=1f;
	
	public float attRange=0.7f;
	public float attMelee=0.9f;
	public float criticalRange=0.1f;
	public float criticalMelee=0.1f;
	
	
	//defensive stats ************************************
	public int armorType=0;
	public float defend=0.1f;
	public float critDef=0;
	public int damageReduc=0;
	
	
	public int APCostAttack=1;
	public int APCostMove=1;
	
	
	
	UnitTBAnimation animationTB;
	UnitTBAudio audioTB;
	
	
	//public bool rotateToTarget;
	//public bool rotateToAttacker;
	
	
	//not in use, check again
	public float waitingTime=0;
	public float waitedTime=0;
	//public float turnPriority=5;	//use to store wahtever  info related to unit turn, based on turnmode
											//FactionBased (if unit not switchable) - turn in each faction
											//FactionBasedSingleUnit (if unit not switchable) - turn in each faction
											//UnitPriorityBased - not used
											//UnitPriorityBasedNoCap - turn cooldown
	public float baseTurnPriority=0; //use in UnitPriorityBasedNoCap only as base cooldown;
	
	
	public Transform targetPointT;
	
	private float rotateSpeed=20;
	private float moveSpeed=14;
	
	
	public int stun=0;
	public int attackDisabled=0;
	public int movementDisabled=0;
	public int abilityDisabled=0;
	public int defaultFactionID=-1;
	
	public List<int> abilityIDList=new List<int>();
	public List<UnitAbility> unitAbilityList=new List<UnitAbility>();
	
	//a record of every hostile unit attack this unit in the past N round
	[HideInInspector] 
	public List<Attacker> attackerList=new List<Attacker>();
	
	
	//used by AI in trigger stance, check if an AI unit has been spotted
	public bool triggered=false;
	
	
	[HideInInspector] 
	public bool moved=false;	//flag indicate if the unit has moved in current round
	[HideInInspector] 
	public bool attacked=false;	//flag indicate if the unit has performed an attack in current round
	[HideInInspector] 
	public bool abilityTriggered=false;	//flag indicate if the unit used any ability in current round
	//[HideInInspector] 
	public int actionQueued=0;	//an int indicate the unit is performing an action, 
											//increase when a new action is taking place (shoot a shootObj), decrese when it's done
											//zero when unit is idle
	
	
	public Transform turretObject;
	
	public GameObject shootObject;
	public Transform shootPoint;
	public List<Transform> shootPoints=new List<Transform>();
	
	public Tile occupiedTile;
	
	
	private bool spawnedInGame=false;
	private int spawnedLastDuration=-1;
	public void SetSpawnInGameFlag(bool flag){ spawnedInGame=flag; }
	public bool GetSpawnInGameFlag(){ return spawnedInGame; }
	public void SetSpawnDuration(int duration){ spawnedLastDuration=duration; }
	
	
	[HideInInspector] public Transform thisT;
	[HideInInspector] public GameObject thisObj;
	
	void Awake(){
		thisT=transform;
		thisObj=gameObject;
		
		animationTB=thisObj.GetComponent<UnitTBAnimation>();
		audioTB=thisObj.GetComponent<UnitTBAudio>();
		
		/*  //external code, not in used for TBTK
		repairCashUsed=repairCashUnit;
		repairMatsUsed=repairMatsUnit;
		*/
	}
	
	// Use this for initialization
	void Start () {
		if(GameControlTB.IsPlayerFaction(factionID)) thisObj.layer=LayerManager.GetLayerUnit();
		else{
			if(GameControlTB.EnableFogOfWar()) SetToInvisible();
			else SetToVisible();
		}
		
		if(GameControlTB.IsPlayerFaction(factionID)) InitPerkBonus();
		
		HP=GetFullHP();
		if(GameControlTB.FullAPOnStart() || GameControlTB.FullAPOnNewRound()) GainAP(GetFullAP());
		
		turnPriority=Mathf.Max(1, turnPriority);
		
		InitAbility();
		
		if(movementRange<=0) moved=false;
		if(attackRangeMin<=0 && attackRangeMax<=0) attacked=false;
		
		InitShootPoint();
		
		if(turretObject==null) turretObject=thisT;
		
		ClearSelectedAbility();
	}
	
	
	
	void InitShootPoint(){
		List<Transform> tempSPList=new List<Transform>();
		for(int i=0; i<shootPoints.Count; i++){
			if(shootPoints[i]!=null) tempSPList.Add(shootPoints[i]);
		}
		shootPoints=tempSPList;
		if(shootPoints.Count==0) shootPoints.Add(thisT);
	}
	
	void InitAbility(){
		foreach(int ID in abilityIDList){
			if(ID>=0 && ID<AbilityManagerTB.unitAbilityList.Count){
				UnitAbility uAB=AbilityManagerTB.GetUnitAbility(ID);
				if(uAB!=null){
					unitAbilityList.Add(uAB);
					unitAbilityList[unitAbilityList.Count-1].factionID=factionID;
					unitAbilityList[unitAbilityList.Count-1].cooldown=0;
					
					foreach(EffectAttr effectAttr in unitAbilityList[unitAbilityList.Count-1].effectAttrs){
						if(effectAttr.type==_EffectAttrType.HPDamage && effectAttr.useDefaultDamageValue){
							effectAttr.value=Mathf.Max(meleeDamageMin, rangeDamageMin);
							effectAttr.valueAlt=Mathf.Max(meleeDamageMax, rangeDamageMax);
						}
						else if(effectAttr.type==_EffectAttrType.ChangeTargetFaction){
							effectAttr.valueAlt=factionID;
						}
					}
				}
			}
		}
	}
	
	void OnEnable(){
		GameControlTB.onBattleStartE += OnBattleStart;
		GameControlTB.onNewRoundE += OnNewRound;
		GameControlTB.onNextTurnE += OnNextTurn;
		
		UnitTB.onNewPositionE += OnCheckFogOfWar;
		UnitTB.onUnitDestroyedE += OnCheckFogOfWar;
		
		PerkManagerTB.onPerkUnlockE += OnPerkUnlock;
	}
	void OnDisable(){
		GameControlTB.onBattleStartE -= OnBattleStart;
		GameControlTB.onNewRoundE -= OnNewRound;
		GameControlTB.onNextTurnE -= OnNextTurn;
		
		UnitTB.onNewPositionE -= OnCheckFogOfWar;
		UnitTB.onUnitDestroyedE -= OnCheckFogOfWar;
		
		PerkManagerTB.onPerkUnlockE -= OnPerkUnlock;
	}
	
	void OnBattleStart(){
		if(GameControlTB.EnableFogOfWar()){
			if(factionID!=GameControlTB.GetPlayerFactionID()) AIUnitCheckFogOfWar();
		}
	}
	
	int CalculateHPDamagePerTurn(){
		int value=0;
		for(int i=0; i<activeUnitAbilityEffectList.Count; i++){
			UnitAbility ability=activeUnitAbilityEffectList[i];
			
			//scan through each effectAttr of the ability
			for(int j=0; j<ability.effectAttrs.Count; j++){
				EffectAttr effectAttr=ability.effectAttrs[j];
				if(effectAttr.type==_EffectAttrType.HPDamage){
					float val=Random.Range(effectAttr.value, effectAttr.valueAlt);
					float modifier=DamageTable.GetModifier(armorType, effectAttr.damageType);
					value+=(int)(val*modifier);
				}
			}
		}
		return -value;
	}
	int CalculateHPGainPerTurn(){
		return Random.Range(GetHPGainMin(), GetHPGainMax());
	}
	
	
	public void OnNewRound(int roundCounter){
		//return;
		
		if(spawnedInGame){
			if(spawnedLastDuration>0){
				spawnedLastDuration-=1;
				if(spawnedLastDuration==0){
					Destroyed();
				}
			}
		}
		
		ClearAllFlag();
		if(roundCounter<=1){
			if(GameControlTB.FullAPOnStart() || GameControlTB.FullAPOnNewRound()) GainAP(GetFullAP());
			return;
		}
		
		//CalculateEffect();
		
		int HPGain=CalculateHPGainPerTurn()+CalculateHPDamagePerTurn();
		if(HPGain>0) ApplyHeal(HPGain);
		else if(HPGain<0) ApplyDamage(-HPGain);
		
		//foreach(UnitAbility uAB in unitAbilityList){
		//	uAB.cooldown-=1;
		//}
		
		UpdateAttackerList();
		
		if(GameControlTB.FullAPOnNewRound()) GainAP(GetFullAP());
		else{
			int APGain=Random.Range(GetAPGainMin(), GetAPGainMax());
			if(APGain>0) GainAP(APGain);
			else if(APGain<0) GainAP(-APGain);
		}
	}
	public void ClearAllFlag(){
		moveRemain=GetMovePerTurn();
		attackRemain=GetAttackPerTurn();
		
		counterAttackRemain=GetCounterPerTurn();
		
		moved=false;
		attacked=false;
		abilityTriggered=false;
		
		actionQueued=0;
	}

	public void UpdateAttackerList(){
		for(int i=0; i<attackerList.Count; i++){
			if(attackerList[i].roundSinceLastAttack>5 || attackerList[i].unit==null){
				attackerList.RemoveAt(i);
				i+=1;
			}
			else{
				attackerList[i].roundSinceLastAttack+=1;
			}
		}
	}
	
	public void OnPerkUnlock(PerkTB perk){
		if(GameControlTB.IsPlayerFaction(factionID)){
			if(perk.applyToAllUnit){
				InitPerkBonus();
			}
			else{
				if(perk.unitPrefabID.Contains(prefabID)){
					InitPerkBonus();
				}
			}
		}
	}
	
	public void Select(){
		if(GameControlTB.IsPlayerFaction(factionID)) audioTB.PlaySelect();
		if(onUnitSelectedE!=null) onUnitSelectedE(this);
	}
	
	public void Deselect(){
		if(onUnitDeselectedE!=null) onUnitDeselectedE();
	}
	
	public bool IsControllable(){
		if(factionID==GameControlTB.GetCurrentPlayerFactionID()) return true;
		return false;
	}
	
	
	
	//function call to move the unit
	//this put the unit into action and locks any further player input until the action is complete
	public bool Move(Tile newTile){
		if(moved && stun>0) return false;
		
		if(movementDisabled>0) return false;
		
		moveRemain-=1;
		if(moveRemain<=0) moved=true;
		
		actionQueued+=1;
		
		UnitControl.MoveUnit(this);
		
		_MovementAPCostRule movementRule=GameControlTB.MovementAPCostRule();
		if(movementRule==_MovementAPCostRule.PerMove) AP-=APCostMove;
		else if(movementRule==_MovementAPCostRule.PerTile) AP-=AStar.Distance(occupiedTile, newTile)*APCostMove;
		
		//int cost=GameControlTB.GetMovementAPCost();
		//if(movementRule==_MovementAPCostRule.PerMove) AP-=cost;
		//else if(movementRule==_MovementAPCostRule.PerTile) AP-=AStar.Distance(occupiedTile, newTile)*cost;
		
		occupiedTile.ClearUnit();
		GridManager.Deselect();
		
		StartCoroutine(MoveRoutine(occupiedTile, newTile));
		
		return true;
	}
	
	//coroutine to actually move the unit, called from move
	IEnumerator MoveRoutine(Tile origin, Tile target){
		StartCoroutine(RotateTurretToOrigin());
		
		List<Tile> path=AStar.SearchWalkableTile(origin, target);
		
		while(GameControlTB.ActionCommenced()){
			yield return null;
		}
		
		audioTB.PlayMove();
		animationTB.PlayMove();
		
		while(path.Count>1){
			Quaternion wantedRot=Quaternion.LookRotation(path[1].pos-path[0].pos);
			while(true){
				thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, Time.deltaTime*rotateSpeed);
				if(Quaternion.Angle(thisT.rotation, wantedRot)<1){
					thisT.rotation=wantedRot;
					break;
				}
				yield return null;
			}
			
			path.RemoveAt(0);
			
			while(true){
				float dist=Vector3.Distance(thisT.position, path[0].pos);
				if(dist<0.1f){
					thisT.position=target.transform.position;
					break;
				}
				thisT.Translate(Vector3.forward*Mathf.Min(moveSpeed*Time.deltaTime, dist));
				yield return null;
			}
			
			thisT.position=path[0].pos;
			occupiedTile=path[0];
			
			//for fog of war
			if(GameControlTB.EnableFogOfWar()){
				if(factionID==GameControlTB.GetPlayerFactionID()){
					//let all other unit check their respective LOS against this unit
					if(onNewPositionE!=null) onNewPositionE(this);
				}
				else{
					//call to check LOS against all player unit
					AIUnitCheckFogOfWar();
				}
			}
		}
		
		occupiedTile=target;
		occupiedTile.SetUnit(this);
		
		
		yield return new WaitForSeconds(0.25f);
		actionQueued-=1;
		if(actionQueued==0){
			StartCoroutine(ActionComplete(0.1f));
		}
		
		audioTB.StopMove();
		animationTB.StopMove();
	}
	
	public void Check(){
		if(onNewPositionE!=null) onNewPositionE(this);
	}
	
	
	//function call to move the unit and attack when there's a possible target
	//this put the unit into action and locks any further player input until the action is complete
	//this is called/used by AI only
	public bool MoveAttack(Tile destinationTile){
		if(stun>0) return false;
		
		//if movement is required but disabled
		if(occupiedTile!=destinationTile && movementDisabled>0) return false;
		
		//if called to attack but attack is disabled
		if(occupiedTile==destinationTile && attackDisabled>0) return false;
		
		//moved=true;
		moveRemain-=1;
		if(moveRemain<=0) moved=true;
		
		UnitControl.MoveUnit(this);
		
		StartCoroutine(MoveAttackRoutine(occupiedTile, destinationTile, movementRange));
		
		return true;
	}
	
	//coroutine to actually move the unit and attack when a target is in range, called from MoveAttack
	//this is called/used by AI only
	IEnumerator MoveAttackRoutine(Tile origin, Tile target, int range){
		if(origin!=target)	{
			occupiedTile.ClearUnit();
			
			StartCoroutine(RotateTurretToOrigin());
			
			List<Tile> path=AStar.SearchToOccupiedTile(origin, target);
			
			if(path.Count>1){
				if(path[path.Count-1].unit!=null) path.RemoveAt(path.Count-1);	//make sure the target tile is not occupied
			}
			
			//make sure the unit doesnt move beyond the allowed range
			while(path.Count>Mathf.Max(1, range+1)){
				path.RemoveAt(path.Count-1);
			}
			
			while(GameControlTB.ActionCommenced()){
				yield return null;
			}
			
			if(path.Count>1){
				audioTB.PlayMove();
				animationTB.PlayMove();
			}
			
			actionQueued+=1;
			
			while(path.Count>1){
				Quaternion wantedRot=Quaternion.LookRotation(path[1].pos-path[0].pos);
				while(true){
					thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, Time.deltaTime*rotateSpeed);
					if(Quaternion.Angle(thisT.rotation, wantedRot)<1){
						thisT.rotation=wantedRot;
						break;
					}
					yield return null;
				}
				
				path.RemoveAt(0);
				
				while(true){
					float dist=Vector3.Distance(thisT.position, path[0].pos);
					if(dist<0.1f){
						thisT.position=target.transform.position;
						break;
					}
					thisT.Translate(Vector3.forward*Mathf.Min(moveSpeed*Time.deltaTime, dist));
					yield return null;
				}
				
				thisT.position=path[0].pos;
				occupiedTile=path[0];
				
				//for fog of war
				if(GameControlTB.EnableFogOfWar()){
					if(factionID==GameControlTB.GetPlayerFactionID()){
						//let all other unit check their respective LOS against this unit
						if(onNewPositionE!=null) onNewPositionE(this);
					}
					else{
						//call to check LOS against all player unit
						AIUnitCheckFogOfWar();
					}
				}
			}
			
			if(path.Count>1) occupiedTile=path[0];
			occupiedTile.SetUnit(this);
			
			audioTB.StopMove();
			animationTB.StopMove();
			
			actionQueued-=1;
		}
		
		//make sure there's target within the new occupied tile
		if(occupiedTile.AIHostileList.Count==0){
			occupiedTile.AIHostileList=UnitControl.GetHostileInRangeFromTile(occupiedTile, this);
		}
		
		//flag to check if any attack has been made
		bool attackMade=false;
		
		if(occupiedTile.AIHostileList.Count>0){
			UnitTB targetUnit=null;
			
			//keep looping until unit cannot attack anymore
			while(attackRemain>0){
				//get a target, will not switch until target is destroyed
				while(targetUnit==null || targetUnit.IsDestroyed()){
					int rand=Random.Range(0, occupiedTile.AIHostileList.Count);
					targetUnit=occupiedTile.AIHostileList[rand];
					if(targetUnit==null || targetUnit.IsDestroyed()) occupiedTile.AIHostileList.RemoveAt(rand);
					
					//if there's no more target, break;
					if(occupiedTile.AIHostileList.Count==0){
						targetUnit=null;
						break;
					}
				}
				
				//attack!
				if(targetUnit!=null){
					Attack(targetUnit);
					attackMade=true;
					
					//wait until the attack is completed
					while(GameControlTB.IsActionInProgress()) yield return null;
					
					//if unit is counterattacked and destroyed, break;
					if(HP<=0) break;
				}
				//if there's no target, break the loop
				else break;
				
				if(attackRemain>0) yield return new WaitForSeconds(0.25f);
			}
		}
		
		//if no attack has been made, call ActionComplete, otherwise it would have been called at attack
		if(!attackMade){
			//yield return new WaitForSeconds(0.25f);
			if(actionQueued==0){
				StartCoroutine(ActionComplete(0.1f));
			}
		}
		
		AIManager.ClearUnitInActionFlag();
	}
	

	
	
	
	//function call to attack a target(groups), could be a direct attack or a shoot action for certain UnitAbility
	//this put the unit into action and locks any further player input until the action is complete
	//can do single or multiple target, with or without 
	public bool Attack(UnitTB target){ return Attack(target.occupiedTile); }
	public bool Attack(Tile targetTile){
		List<Tile> list=new List<Tile>();	list.Add(targetTile);
		return Attack(list, null);
	}		
	public bool Attack(List<Tile> targetTileList){ return Attack(targetTileList, null); }
	public bool Attack(List<Tile> targetTileList, UnitAbility ability){
		//if unit ability is not null, there fore it's not a normal attack
		if((ability==null && attacked) || stun>0) return false;
		
		if(attackDisabled>0) return false;
		
		attackRemain-=1;
		if(attackRemain<=0) attacked=true;
		
		UnitControl.MoveUnit(this);
		
		if(Random.Range(0f, 1f)<GameControlTB.GetActionCamFrequency()){
			CameraControl.ActionCam(this.occupiedTile, targetTileList[0]);
		}
		
		_AttackAPCostRule attackRule=GameControlTB.AttackAPCostRule();
		if(attackRule==_AttackAPCostRule.PerAttack) AP-=APCostAttack;
		//if(attackRule==_AttackAPCostRule.PerAttack) AP-=GameControlTB.GetAttackAPCost();
		
		GridManager.Select(occupiedTile);
		
		AttackInstance attInstance=new AttackInstance();
		attInstance.unitAbility=ability;
		attInstance.srcUnit=this;
		
		if(attInstance.unitAbility==null){
			if(attackMode==_AttackMode.Melee){
				attInstance.type=_AttackType.Melee_Normal;
				StartCoroutine(AttackRoutineMelee(targetTileList, attInstance));
			}
			else if(attackMode==_AttackMode.Range){
				attInstance.type=_AttackType.Range_Normal;
				StartCoroutine(AttackRoutineRange(targetTileList, attInstance));
			}
			else if(attackMode==_AttackMode.Hybrid){
				int dist=AStar.Distance(occupiedTile, targetTileList[0]);
				if(dist<=attackRangeMelee){
					attInstance.type=_AttackType.Melee_Normal;
					StartCoroutine(AttackRoutineMelee(targetTileList, attInstance));
				}
				else{
					attInstance.type=_AttackType.Range_Normal;
					StartCoroutine(AttackRoutineRange(targetTileList, attInstance));
				}
			}
		}
		else{
			if(attInstance.unitAbility.shootMode!=_AbilityShootMode.None){
				attInstance.type=_AttackType.Range_Normal;
				StartCoroutine(AttackRoutineRange(targetTileList, attInstance));
			}
			else{
				attInstance.type=_AttackType.Melee_Normal;
				StartCoroutine(AttackRoutineMelee(targetTileList, attInstance));
			}
		}
		
		if(attacked){
			GridManager.ClearHostileList();
			if(moved) GridManager.Select(null);
		}
		
		return true;
	}
	
	
	
	//similar to attack(), but called when unit is performing an counter attack
	public void CounterAttack(UnitTB target){
		List<Tile> targetTileList=new List<Tile>();
		targetTileList.Add(target.occupiedTile);
		
		AttackInstance attInstance=new AttackInstance();
		attInstance.srcUnit=this;
		
		if(attackMode==_AttackMode.Melee){
			attInstance.type=_AttackType.Melee_Counter;
			StartCoroutine(AttackRoutineMelee(targetTileList, attInstance));
		}
		else if(attackMode==_AttackMode.Range){
			attInstance.type=_AttackType.Range_Counter;
			StartCoroutine(AttackRoutineRange(targetTileList, attInstance));
		}
		else if(attackMode==_AttackMode.Hybrid){
			int dist=AStar.Distance(occupiedTile, targetTileList[0]);
			if(dist<=attackRangeMelee){
				attInstance.type=_AttackType.Melee_Counter;
				StartCoroutine(AttackRoutineMelee(targetTileList, attInstance));
			}
			else{
				attInstance.type=_AttackType.Range_Counter;
				StartCoroutine(AttackRoutineRange(targetTileList, attInstance));
			}
		}
	}
	
	
	
	//coroutine to actually attack a target, called from Attack or CounterAttack
	//for range attack
	IEnumerator AttackRoutineRange(List<Tile> targetTileList, AttackInstance attInstance){
		while(GameControlTB.ActionCommenced()){
			yield return null;
		}
		
		foreach(Tile tile in targetTileList){
			if(tile.unit!=null) tile.unit.RotateToUnit(this);
		}
		
		Vector3 targetPos=Vector3.zero;
		foreach(Tile tile in targetTileList){
			targetPos+=tile.pos;
		}
		targetPos/=targetTileList.Count;
		
		Vector3 pos=turretObject.position;
		pos.y=targetPos.y;
		Quaternion wantedRot=Quaternion.LookRotation(targetPos-pos);
		
		actionQueued+=1;
		
		while(true){
			turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, Time.deltaTime*5);
			if(Quaternion.Angle(turretObject.rotation, wantedRot)<1){
				turretObject.rotation=wantedRot;
				break;
			}
			yield return null;
		}
		
		
		float delay=animationTB.PlayRangeAttack();
		yield return new WaitForSeconds(delay);
		
		if(attInstance.unitAbility!=null){
			GameObject so=shootObject;
			if(attInstance.unitAbility.shootObject!=null) so=attInstance.unitAbility.shootObject;
			
			if(attInstance.unitAbility.shootMode==_AbilityShootMode.ShootToAll){
				actionQueued+=(Mathf.Max(0, targetTileList.Count-1));
				foreach(Tile tile in targetTileList){
					for(int i=0; i<shootPoints.Count; i++){
						audioTB.PlayRangeAttack();
						Transform sp=shootPoints[i];
						GameObject obj=(GameObject)Instantiate(so, sp.position, sp.rotation);
						ShootObjectTB shootObj=obj.GetComponent<ShootObjectTB>();
						if(i==shootPoints.Count-1){
							shootObj.Shoot(tile, attInstance);
							shootObj.SetAbilityTargetTile(abilityTargetTile);
						}
						else shootObj.Shoot(tile, null);
						yield return new WaitForSeconds(0.05f);
					}

					yield return new WaitForSeconds(0.05f);
				}
			}
			else if(attInstance.unitAbility.shootMode==_AbilityShootMode.ShootToCenter){
				for(int i=0; i<shootPoints.Count; i++){
					audioTB.PlayRangeAttack();
					Transform sp=shootPoints[i];
					GameObject obj=(GameObject)Instantiate(so, sp.position, sp.rotation);
					ShootObjectTB shootObj=obj.GetComponent<ShootObjectTB>();
					
					if(i==shootPoints.Count-1){
						shootObj.Shoot(targetPos, targetTileList, attInstance);
						shootObj.SetAbilityTargetTile(abilityTargetTile);
					}
					else shootObj.Shoot(targetPos, targetTileList, null);
					yield return new WaitForSeconds(0.05f);
				}
			}
			else{
				Debug.Log("ability shoot mode error");
			}
		}
		else{
			actionQueued+=(Mathf.Max(0, targetTileList.Count-1));
			foreach(Tile tile in targetTileList){
				
				UnitTB targetUnit=tile.unit;
				if(targetUnit!=null) attInstance.Process(targetUnit);
				
				for(int i=0; i<shootPoints.Count; i++){
					audioTB.PlayRangeAttack();
					Transform sp=shootPoints[i];
					//Debug.DrawLine(sp.position, sp.position+new Vector3(0, 9, 0), Color.red, 4);
					GameObject obj=(GameObject)Instantiate(shootObject, sp.position, sp.rotation);
					ShootObjectTB shootObj=obj.GetComponent<ShootObjectTB>();
					if(i==shootPoints.Count-1){
						if(targetUnit!=null) shootObj.Shoot(targetUnit, attInstance.Clone(), attInstance.missed);
						//~ else shootObj.Shoot(tile, attInstance.Clone(), attInstance.missed);
					}
					else{
						if(targetUnit!=null) shootObj.Shoot(targetUnit, null, attInstance.missed);
						//~ else shootObj.Shoot(tile, null, attInstance.missed);
					}
					yield return new WaitForSeconds(0.05f);
				}
				yield return new WaitForSeconds(0.05f);
			}
		}
		
		yield return new WaitForSeconds(0.15f);
	}

	
	
	//coroutine to actually attack a target, called from Attack or CounterAttack
	//for melee attack (untested)
	IEnumerator AttackRoutineMelee(List<Tile> targetTileList, AttackInstance attInstance){
		while(GameControlTB.ActionCommenced()){
			yield return null;
		}
		
		
		foreach(Tile tile in targetTileList){
			if(tile.unit!=null) tile.unit.RotateToUnit(this);
		}
		
		Vector3 targetPos=Vector3.zero;
		if(targetTileList.Count>1){
			foreach(Tile tile in targetTileList){
				targetPos+=tile.pos;
			}
			targetPos/=targetTileList.Count;
		}
		else targetPos=targetTileList[0].pos;
		
		targetPos=(thisT.position+targetPos)/2;
		
		//rotate to destination
		Quaternion wantedRot=Quaternion.LookRotation(targetPos-thisT.position);
		while(true){
			thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, Time.deltaTime*rotateSpeed);
			if(Quaternion.Angle(thisT.rotation, wantedRot)<1){
				thisT.rotation=wantedRot;
				break;
			}
			yield return null;
		}
		
		actionQueued+=1;
		
		audioTB.PlayMeleeAttack();
		float delay=animationTB.PlayMeleeAttack();
		yield return new WaitForSeconds(delay);
		
		UnitTB targetUnit=null;
		if(targetTileList.Count>0){
			targetUnit=targetTileList[0].unit;
			if(targetUnit!=null){
				attInstance.Process(targetUnit);
				attInstance=targetUnit.ApplyHitEffect(attInstance);
			}
		}
		
		thisT.position=occupiedTile.thisT.position;
		
		yield return new WaitForSeconds(0.5f);
		
		actionQueued-=1;
		if(actionQueued<=0){
			actionQueued=0;
			
			if(attInstance!=null){
				
				if(attInstance.type!=_AttackType.Melee_Counter && attInstance.type!=_AttackType.Range_Counter){
					//if the attack is not counter attack, complete the action using delay based on if the target is destroyed
					if(attInstance.destroyed) StartCoroutine(ActionComplete(attInstance.destroyEffectDuration));
					else if(!attInstance.counterAttacking){
						//complete action is target is not counterAttacking
						//otherwise waiting for counter attack to complete, CounterAttackComplete will be called
						StartCoroutine(ActionComplete(0.25f));
					}
				}
				//if the attack is counter attack, tell the attacker that the counter attack has been completed
				else {
					if(targetUnit!=null) targetUnit.CounterAttackComplete(attInstance);
				}
			}
			else{
				StartCoroutine(ActionComplete(0.25f));
			}
		}
	}
	

	
	//called by shootObject when hit the target
	public void HitTarget(UnitTB target, AttackInstance attInstance){
		StartCoroutine(_HitTarget(target, attInstance));
	}
	IEnumerator _HitTarget(UnitTB target, AttackInstance attInstance){
		//HitResult hitResult=null;
		
		//apply the attackInfo to the target, get the hit result info in return
		if(target!=null){
			attInstance=target.ApplyHitEffect(attInstance);
		}
		
		yield return new WaitForSeconds(0.1f);
		actionQueued-=1;
		if(actionQueued<=0){
			actionQueued=0;
			
			if(attInstance!=null){
				//~ if(!hitResult.counterAttack){
					if(attInstance.type!=_AttackType.Melee_Counter && attInstance.type!=_AttackType.Range_Counter){
						//if the attack is not counter attack, complete the action using delay based on if the target is destroyed
						if(attInstance.destroyed) StartCoroutine(ActionComplete(attInstance.destroyEffectDuration));
						else if(!attInstance.counterAttacking){
							//complete action if target is not counterAttacking
							//otherwise waiting for counter attack to complete, CounterAttackComplete will be called
							StartCoroutine(ActionComplete(0.25f));
						}
					}
					//if the attack is counter attack, tell the attacker that the counter attack has been completed
					else target.CounterAttackComplete(attInstance);
				//~ }
			}
			else{
				StartCoroutine(ActionComplete(0.25f));
			}
		}
	}
	
	//called by attacker's target when the counter attack is completed
	public void CounterAttackComplete(AttackInstance attInstance){
		if(!attInstance.destroyed) StartCoroutine(ActionComplete(0.25f));
		else{
			StartCoroutine(ActionComplete(attInstance.destroyEffectDuration));
		}
	}
	//for unit priority based turn mode, when a unit has used up all it's action
	IEnumerator ActionComplete(float delay){
		yield return new WaitForSeconds(delay);
		//make sure actionInProgress flag is cleared
		if(onActionCompletedE!=null) onActionCompletedE(this);
		
		if(GameControlTB.IsPlayerFaction(factionID)){
			if(IsAllActionCompleted()){
				while(GameControlTB.IsActionInProgress()) yield return null;
				if(onTurnDepletedE!=null) onTurnDepletedE();
			}
			else GridManager.Select(occupiedTile);
		}
		//else{
			//no needed, this bit is handled by AIManager when a unit has finish a move
			//while(GameControlTB.IsActionInProgress()) yield return null;
			//if(onTurnDepletedE!=null) onTurnDepletedE();
		//}
	}
	public void CompleteAllAction(){
		StartCoroutine(ActionComplete(0.25f));
	}
	
	bool ContainedInAttackerList(UnitTB unit){
		for(int i=0; i<attackerList.Count; i++){
			if(attackerList[i].unit==unit) return true;
			else if(attackerList[i]==null){
				attackerList.RemoveAt(i);
				i-=1;
			}
		}
		return false;
	}
	public UnitTB GetNearestAttacker(){
		for(int i=0; i<attackerList.Count; i++){
			if(attackerList[i].unit==null){
				attackerList.RemoveAt(i);
				i-=1;
			}
		}
		
		float currentDist=0;
		UnitTB currentUnit=null;
		
		for(int i=0; i<attackerList.Count; i++){
			float dist=Vector3.Distance(attackerList[i].unit.thisT.position, thisT.position);
			if(dist<currentDist){
				currentDist=dist;
				currentUnit=attackerList[i].unit;
			}
		}
		
		return currentUnit;
	}
	
	//called by attacker to apply attackInfo when a unit is attacked and hit
	public AttackInstance ApplyHitEffect(AttackInstance attInstance){
		//HitResult hitResult=
		ApplyDamage(attInstance);
		
		if(attInstance.missed) audioTB.PlayMissed();
		else{
			audioTB.PlayHit();
			animationTB.PlayHit();
		}
		
		
		if(!ContainedInAttackerList(attInstance.srcUnit)){
			Attacker attacker=new Attacker(attInstance.srcUnit);
			attackerList.Add(attacker);
		}
		
		triggered=true;
		
		//perform counter attack if possible
		//if the rule allows it
		if(attInstance.type==_AttackType.Range_Normal || attInstance.type==_AttackType.Melee_Normal){
			if(GameControlTB.IsCounterAttackEnabled()){
				//if the target is still not destroyed and have a counter attack move remain
				if(!attInstance.destroyed && counterAttackRemain>0){
					//if the attacker is within attack distance
					float dist=AStar.Distance(occupiedTile, attInstance.srcUnit.occupiedTile);
					if(dist>=GetAttackRangeMin() && dist<=GetAttackRangeMax()){
						attInstance.counterAttacking=true;
						StartCoroutine(CounterAttackRoutine(attInstance.srcUnit));
					}
				}
			}
		}
		
		return attInstance;
	}
	
	//short delay before starting the counter attack action sequence
	IEnumerator CounterAttackRoutine(UnitTB srcUnit){
		//Debug.Log("counter");
		counterAttackRemain-=1;
		yield return new WaitForSeconds(0.5f);
		CounterAttack(srcUnit);
	}
	
	
	
	public void ApplyHeal(int val){
		float value=Mathf.Min(GetFullHP()-HP, val);
		HP=Mathf.Min(GetFullHP(), HP+=val);
		if(thisObj.layer!=LayerManager.GetLayerUnitAIInvisible() && value!=0){
			new EffectOverlay(thisT.position+new Vector3(0, 0.5f, 0), value.ToString(), new Color(.6f, 1f, 0, 1));
			//UITB.DisplayDamageOverlay(thisT.position+new Vector3(0, 0.5f, 0), value.ToString(), new Color(.6f, 1f, 0, 1));
		}
	}
	
	//function call to calculate the hit stats
	public void ApplyDamage(AttackInstance attInstance){
		if(attInstance.missed){
			if(thisObj.layer!=LayerManager.GetLayerUnitAIInvisible()){
				new EffectOverlay(thisT.position+new Vector3(0, 0.5f, 0), "missed", new Color(.6f, 1f, 0, 1));
				//UITB.DisplayDamageOverlay(thisT.position+new Vector3(0, 0.5f, 0), "missed", new Color(.6f, 1f, 0, 1));
			}
			return;
		}
		
		if(thisObj.layer!=LayerManager.GetLayerUnitAIInvisible()){
			if(attInstance.critical){
				new EffectOverlay(thisT.position+new Vector3(0, 0.5f, 0), "Critical! ("+attInstance.damageDone.ToString()+")");
				//UITB.DisplayDamageOverlay(thisT.position+new Vector3(0, 0.5f, 0), "Critical! ("+attInstance.damageDone.ToString()+")");
			}
			else{
				new EffectOverlay(thisT.position+new Vector3(0, 0.5f, 0), attInstance.damageDone.ToString());
				//UITB.DisplayDamageOverlay(thisT.position+new Vector3(0, 0.5f, 0), attInstance.damageDone.ToString());
			}
		}
		
		ApplyDamage(attInstance.damageDone);
	}
	
	//apply damage, display the overlay
	public bool ApplyDamage(int dmg){
		//new EffectOverlay(thisT.position+new Vector3(0, 0.5f, 0), dmg.ToString());
		//UITB.DisplayDamageOverlay(thisT.position+new Vector3(0, 0.5f, 0), dmg.ToString());
		return _ApplyDamage(dmg);
	}
	
	//apply damage without displaying any overlay
	public bool _ApplyDamage(int dmg){
		HP=Mathf.Max(0, HP-=dmg);
		
		if(HP<=0){
			if(onUnitDestroyedE!=null) onUnitDestroyedE(this);
			
			occupiedTile.ClearUnit();
			StartCoroutine(Destroyed());
			
			return true;
		}
		
		return false;
	}
	
	public GameObject destroyedEffect;
	public float destroyEffectDuration=2;
	
	public bool spawnCollectibleUponDestroyed=false;
	public List<CollectibleTB> collectibleList=new List<CollectibleTB>();
	
	//called when unit is destroyed
	IEnumerator Destroyed(){
		yield return new WaitForSeconds(1f);
		
		audioTB.PlayDestroy();
		float aniDuration=animationTB.PlayDestroyed();
		
		if(destroyedEffect!=null){
			GameObject obj=(GameObject)Instantiate(destroyedEffect, thisT.position, thisT.rotation);
			Destroy(obj, 10);
		}
		
		yield return new WaitForSeconds(aniDuration+0.1f);
		float totalDelay=destroyEffectDuration-aniDuration;
		
		if(UnitControl.HideUnitWhenKilled()){
			thisT.position=new Vector3(0, 999999, 0);
		}
		
		yield return new WaitForSeconds(totalDelay+0.25f);
		
		if(spawnCollectibleUponDestroyed){
			if(collectibleList.Count!=0){
				int rand=Random.Range(0, collectibleList.Count);
				GridManager.InsertCollectible(collectibleList[rand], occupiedTile);
			}
		}
		
		if(UnitControl.DestroyUnitObject()) Destroy(gameObject);
	}
	
	//function to rotate turret back to origin, called when unit is moving
	IEnumerator RotateTurretToOrigin(){
		if(turretObject==thisT) yield break;
		Quaternion wantedRot=thisT.rotation;
		while(true){
			turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, Time.deltaTime*5);
			if(Quaternion.Angle(turretObject.rotation, wantedRot)<1 || InAction()){
				turretObject.rotation=wantedRot;
				break;
			}
			yield return null;
		}
	}
	
	//rotate to face a unit;
	//called when unit is being attack to face the attacker, or when unit is attack to face the target
	public void RotateToUnit(UnitTB unit){
		//~ if(rotateToAttacker) StartCoroutine(_RotateToAttacker(attacker));
		StartCoroutine(_RotateToUnit(unit));
	}
	IEnumerator _RotateToUnit(UnitTB unit){
		Quaternion wantedRot=Quaternion.LookRotation(unit.occupiedTile.pos-occupiedTile.pos);
		while(true){
			thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, Time.deltaTime*rotateSpeed);
			if(Quaternion.Angle(thisT.rotation, wantedRot)<1){
				thisT.rotation=wantedRot;
				break;
			}
			yield return null;
		}
	}
	
	
	
	public void GainAP(int val){
		AP=Mathf.Clamp(AP+=val, 0, GetFullAP());
	}
	
	
	public void EndUnitTurn(){
		GridManager.Deselect();
		if(onTurnDepletedE!=null) onTurnDepletedE();
	}

	//function call to check if all available actions has been taken (move, attack and ability)
	public bool IsAllActionCompleted(){
		//for different ruleset in which ap is used for either movement or attack
		bool movementRuleFlag=(GameControlTB.MovementAPCostRule()!=_MovementAPCostRule.None);
		bool attackRuleFlag=(GameControlTB.AttackAPCostRule()!=_AttackAPCostRule.None);
		
		if(movementRuleFlag && attackRuleFlag && AP<=0) return true;
		if(movementRuleFlag && AP<=0) moved=true;
		if(attackRuleFlag && AP<=0) attacked=true;
		if(AP<=0 && moved && attacked) return true; 
		
		
		//typical check where ap is not used for both movement or attack
		bool allowAbAfterAttack=GameControlTB.AllowAbilityAfterAttack();
		bool allowMoveAfterAttack=GameControlTB.AllowMovementAfterAttack();
		
		bool abilityFlag=IsAllAbilityDisabled();
		
		//unit ability is disable for AI for now
		if(!GameControlTB.IsPlayerFaction(factionID)) abilityFlag=true;
		
		if(allowAbAfterAttack && allowMoveAfterAttack){
			return moved & attacked & abilityFlag;
		}
		else if(!allowAbAfterAttack && allowMoveAfterAttack){
			if(attacked){
				return moved;
			}
			else{
				return moved & attacked;
			}
		}
		else if(allowAbAfterAttack && !allowMoveAfterAttack){
			if(attacked){
				return abilityFlag;
			}
			else{
				return abilityFlag & attacked;
			}
		}
		else if(!allowAbAfterAttack && !allowMoveAfterAttack){
			return attacked;
		}
		
		return false;
	}

#if ibox
	public bool AreAllActionsCompleted(){ //custom version of method, slightly different, does not account for unused attacks if they are out of range
		//for different ruleset in which ap is used for either movement or attack
		bool movementRuleFlag=(GameControlTB.MovementAPCostRule()!=_MovementAPCostRule.None);
		bool attackRuleFlag=(GameControlTB.AttackAPCostRule()!=_AttackAPCostRule.None);
		
		if(movementRuleFlag && attackRuleFlag && AP<=0) return true;
		if(movementRuleFlag && AP<=0) moved=true;
		if(attackRuleFlag && AP<=0) attacked=true;
		if(AP<=0 && moved && attacked) return true; 
		
		
		//typical check where ap is not used for both movement or attack
		bool allowAbAfterAttack=GameControlTB.AllowAbilityAfterAttack();
		bool allowMoveAfterAttack=GameControlTB.AllowMovementAfterAttack();
		
		bool abilityFlag=IsAllAbilityDisabled();

		bool validTarget = false;
		
		//determine if enemies are within range
		for(int i=0; i<attackerList.Count; i++){
			if(attackerList[i].unit==null){
				attackerList.RemoveAt(i);
				i-=1;
			}
		}
		for(int i=0; i<attackerList.Count; i++){
			float dist=Vector3.Distance(attackerList[i].unit.thisT.position, thisT.position);
			if(dist<this.attackRangeMax && dist> this.attackRangeMax){
				validTarget =true;
				break;
			}
		}

		//unit ability is disable for AI for now
		if(!GameControlTB.IsPlayerFaction(factionID)) abilityFlag=true;
		
		if(allowAbAfterAttack && allowMoveAfterAttack){
			return moved & (attacked|(!validTarget&moved)) & abilityFlag;
		}
		else if(!allowAbAfterAttack && allowMoveAfterAttack){
			if((attacked|(!validTarget&moved))){
				return moved;
			}
			else{
				return moved & (attacked|(!validTarget&moved));
			}
		}
		else if(allowAbAfterAttack && !allowMoveAfterAttack){
			if((attacked|(!validTarget&moved))){
				return abilityFlag;
			}
			else{
				return abilityFlag & (attacked|(!validTarget&moved));
			}
		}
		else if(!allowAbAfterAttack && !allowMoveAfterAttack){
			return (attacked|(!validTarget&moved));
		}
		
		return false;
	}
#endif
	//function call to check if all ability is disabled
	public bool IsAllAbilityDisabled(){
		if(stun>0 || abilityDisabled>0) return true;
		
		bool flag=false;
		for(int i=0; i<unitAbilityList.Count; i++){
			if(IsAbilityAvailable(i)!=0) flag=true;
		}
		return flag;
	}
	
	
	
//*************************************************************************************************************************//
//code related to activate unit ability
	
	
	
	//called when a target for unitAbility has been selected
	//activate the ability and shoot the effect and what not
	IEnumerator UnitAbilityTargetSelectedRoutine(List<Tile> tileList){
		UnitAbility unitAbility=activeAbilityPendingTarget;
		activeAbilityPendingTarget=null;
		
		yield return null;
		
		if(unitAbility.canFail && Random.Range(0f, 1f)<unitAbility.failChance){
			new EffectOverlay(thisT.position+new Vector3(0, 0.5f, 0), "failed", new Color(.6f, 1f, 0, 1));
			//UITB.DisplayDamageOverlay(thisT.position+new Vector3(0, 0.5f, 0), "failed", new Color(.6f, 1f, 0, 1));
			GridManager.Select(occupiedTile);	//this is to reset the UI
			yield break;
		}
		
		//spawn the effect
		foreach(Tile tile in tileList){
			if(unitAbility.effectTarget!=null)
				StartCoroutine(SpawnAbilityEffect(unitAbility.effectTarget, unitAbility.effectTargetDelay, tile.pos));
			
			if(unitAbility.soundHit!=null) AudioManager.PlaySound(unitAbility.soundHit, thisT);
		}
		
		yield return new WaitForSeconds(unitAbility.delay);
		
		//'shoot' the abilty
		if(unitAbility.shootMode!=_AbilityShootMode.None){
			Attack(tileList, unitAbility);
		}
		//just apply the effect
		else if(unitAbility.shootMode==_AbilityShootMode.None){
			foreach(Tile tile in tileList){
				if(unitAbility.targetType==_AbilityTargetType.AllUnits){
					if(tile.unit!=null){
						StartCoroutine(tile.unit.ApplyAbilityDelay(unitAbility.Clone()));
					}
				}
				if(unitAbility.targetType==_AbilityTargetType.Friendly){
					if(tile.unit!=null && tile.unit.factionID==factionID){
						StartCoroutine(tile.unit.ApplyAbilityDelay(unitAbility.Clone()));
					}
				}
				else if(unitAbility.targetType==_AbilityTargetType.Hostile){
					if(tile.unit!=null && tile.unit.factionID!=factionID){
						StartCoroutine(tile.unit.ApplyAbilityDelay(unitAbility.Clone()));
					}
				}
				else if(unitAbility.targetType==_AbilityTargetType.AllTile){
					tile.ApplyAbility(unitAbility.Clone());
				}
				else if(unitAbility.targetType==_AbilityTargetType.EmptyTile){
					for(int i=0; i<unitAbility.effectAttrs.Count; i++){
						if(unitAbility.effectAttrs[i].type==_EffectAttrType.Teleport){
							thisT.position=tileList[0].pos;
							occupiedTile.unit=null;
							occupiedTile=tileList[0];
							tileList[0].unit=this;
						}
						else if(unitAbility.effectAttrs[i].type==_EffectAttrType.SpawnUnit){
							UnitControl.InsertUnit(unitAbility.effectAttrs[i].unit, tileList[0], factionID, unitAbility.effectAttrs[i].duration);
						}
						else if(unitAbility.effectAttrs[i].type==_EffectAttrType.SpawnCollectible){
							//UnitControl.InsertUnit(unitAbility.effectAttrs[i].unit, tileList[0], factionID, unitAbility.effectAttrs[i].duration);
							GridManager.InsertCollectible(unitAbility.effectAttrs[i].collectible, tileList[0]);
						}
					}
				}
			}
			
			for(int i=0; i<unitAbility.chainedAbilityIDList.Count; i++){
				UnitAbility uAB=AbilityManagerTB.GetUnitAbility(unitAbility.chainedAbilityIDList[i]);
				if(uAB.requireTargetSelection){
					activeAbilityPendingTarget=uAB;
					GridManager.SetTargetTileSelectMode(abilityTargetTile, uAB);
					GridManager.OnHoverEnter(abilityTargetTile);
					GridManager.Select(abilityTargetTile);
				}
				else{
					_ActivateAbility(uAB);
				}
			}
			
			GridManager.Select(occupiedTile);
		}
		
		yield return null;
	}
	
	//select a unitAbility
	//called when any unitAbility button is pressed
	public int ActivateAbility(int ID){
		//first check if the ability is available, return the status
		int status=IsAbilityAvailable(ID);
		if(status>0){
			return status;
		}
		
		UnitAbility unitAbility=unitAbilityList[ID];
		_ActivateAbility(unitAbility);
		
		return 0;
	}
	public int _ActivateAbility(UnitAbility unitAbility){
		//first check if the ability is available, return the status
		//~ int status=IsAbilityAvailable(ID);
		//~ if(status>0){
			//~ return status;
		//~ }
		
		if(!unitAbility.requireTargetSelection){
			AbilityActivated(unitAbility, true);
			
			if(unitAbility.canFail && Random.Range(0f, 1f)<unitAbility.failChance){
				new EffectOverlay(thisT.position+new Vector3(0, 0.5f, 0), "failed", new Color(.6f, 1f, 0, 1));
				//UITB.DisplayDamageOverlay(thisT.position+new Vector3(0, 0.5f, 0), "failed", new Color(.6f, 1f, 0, 1));
				GridManager.Select(occupiedTile);	//this is to reset the UI
				return 0;
			}
			
			List<Tile> tileList=GridManager.GetTilesWithinRange(occupiedTile, unitAbility.aoeRange);
			tileList.Add(occupiedTile);
			foreach(Tile tile in tileList){
				Debug.DrawLine(tile.pos, tile.pos+new Vector3(0, 3, 0), Color.red, 5);
				if(unitAbility.targetType==_AbilityTargetType.AllUnits){
					if(tile.unit!=null){
						StartCoroutine(tile.unit.ApplyAbilityDelay(unitAbility.Clone()));
					}
				}
				if(unitAbility.targetType==_AbilityTargetType.Friendly){
					if(tile.unit!=null && tile.unit.factionID==factionID){
						StartCoroutine(tile.unit.ApplyAbilityDelay(unitAbility.Clone()));
					}
				}
				else if(unitAbility.targetType==_AbilityTargetType.Hostile){
					if(tile.unit!=null && tile.unit.factionID!=factionID){
						StartCoroutine(tile.unit.ApplyAbilityDelay(unitAbility.Clone()));
					}
				}
				else if(unitAbility.targetType==_AbilityTargetType.AllTile){
					tile.ApplyAbility(unitAbility.Clone());
				}
			}
			
			//launch the chained ability
			for(int i=0; i<unitAbility.chainedAbilityIDList.Count; i++){
				UnitAbility uAB=AbilityManagerTB.GetUnitAbility(unitAbility.chainedAbilityIDList[i]);
				if(uAB.requireTargetSelection){
					activeAbilityPendingTarget=uAB;
					GridManager.SetTargetTileSelectMode(occupiedTile, uAB);
					GridManager.OnHoverEnter(occupiedTile);
					GridManager.Select(occupiedTile);
				}
				else{
					_ActivateAbility(uAB);
				}
			}
		}
		else{
			GridManager.SetTargetTileSelectMode(occupiedTile, unitAbility);
			activeAbilityPendingTarget=unitAbility;
		}
		
		return 0;
	}
	
	public void ClearSelectedAbility(){
		if(activeAbilityPendingTarget!=null) activeAbilityPendingTarget=null;
	}
	
	//current semi-activated ability that awaiting target from user
	[HideInInspector] private UnitAbility activeAbilityPendingTarget=null;
	[HideInInspector] private Tile abilityTargetTile=null;
	public void SetAbilityTargetTile(Tile tile){ abilityTargetTile=tile; }
	//function called to activate a pending ability when a target has been selected
	public void UnitAbilityTargetSelected(Tile tile){
		abilityTargetTile=tile;
		List<Tile> list=new List<Tile>(); list.Add(tile);
		UnitAbilityTargetSelected(list);
	}
	public void UnitAbilityTargetSelected(List<Tile> list){
		//AbilityActivated(activeAbilityPendingTarget, false);
		//StartCoroutine(UnitAbilityTargetSelectedRoutine(list));
		
		//~ UnitAbility unitAbility=activeAbilityPendingTarget;
		//~ Debug.Log(unitAbility+"   lala");
		//~ //Debug.Log(unitAbility.name+"  "+unitAbility.chainedAbilityIDList.Count);
		//~ for(int i=0; i<unitAbility.chainedAbilityIDList.Count; i++){
			//~ UnitAbility uAB=AbilityManagerTB.GetUnitAbility(unitAbility.chainedAbilityIDList[i]);
			//~ if(uAB.requireTargetSelection){
				//~ activeAbilityPendingTarget=uAB;
				//~ GridManager.SetTargetTileSelectMode(occupiedTile, uAB);
				//~ activeAbilityPendingTarget=uAB;
				//~ GridManager.OnHoverEnter(occupiedTile);
				//~ GridManager.Select(occupiedTile);
			//~ }
			//~ else{
				//~ _ActivateAbility(uAB);
			//~ }
		//~ }
		
		AbilityActivated(activeAbilityPendingTarget, false);
		StartCoroutine(UnitAbilityTargetSelectedRoutine(list));
		activeAbilityPendingTarget=null;
		
		return;
	}
	
	public UnitAbility GetActiveAbilityPendingTarget(){ return activeAbilityPendingTarget; }
	public void SetActiveAbilityPendingTarget(UnitAbility uAB){ activeAbilityPendingTarget=uAB; }
	
	
	//handle when an ability has been activated, apply the ap cost, cd, audtio effect and what not
	void AbilityActivated(UnitAbility uAB, bool reselect){
		GameControlTB.LockUnitSwitching();
		//Debug.Log("ability activated");
		
		abilityTriggered=true;
		UnitControl.MoveUnit(this);
		
		AP-=uAB.cost;
		uAB.cooldown=uAB.cdDuration;
		
		if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
			uAB.countTillNextTurn=UnitControl.activeFactionCount;
		}
		else{
			uAB.countTillNextTurn=UnitControl.GetAllUnitCount();
		}
		
		uAB.useCount+=1;
		if(uAB.effectUse!=null){
			StartCoroutine(SpawnAbilityEffect(uAB.effectUse, uAB.effectUseDelay, occupiedTile.pos));
		}
		if(uAB.soundUse!=null){
			AudioManager.PlaySound(uAB.soundUse, thisT);
		}
		if(!uAB.enableMovementAfter) moved=true;
		if(!uAB.enableAttackAfter) attacked=true;
		
		//for ability with shootObject, select() will be called in attack()
		if(reselect) GridManager.Select(occupiedTile);
	}
	
	IEnumerator SpawnAbilityEffect(GameObject effect, float delay, Vector3 pos){
		yield return new WaitForSeconds(delay);
		Instantiate(effect, pos, effect.transform.rotation);
	}
	
	//check if an ability is available
	public int IsAbilityAvailable(int ID){
		//if the ability is current selected
		if(activeAbilityPendingTarget!=null && activeAbilityPendingTarget.ID==unitAbilityList[ID].ID){
			//Debug.Log(activeAbilityPendingTarget.desp);
			return 7;
		}
		
		//if unit has exhuasted its move
		if(!GameControlTB.AllowAbilityAfterAttack() && attacked) return 6;
		
		if(stun>0) return 4;
		if(abilityDisabled>0) return 5;
		
		UnitAbility unitAbility=unitAbilityList[ID];
		if(unitAbility.useLimit>0 && unitAbility.useCount>=unitAbility.useLimit){
			return 1;
		}
		else if(AP<unitAbility.totalCost){
			return 2;
		}
		else if(unitAbility.cooldown>0){
			return 3;
		}
		
		return 0;
	}
	
	
	public IEnumerator ApplyAbilityDelay(UnitAbility unitAbility){
		if(unitAbility.canMiss){
			float missChance=unitAbility.missChance;
			//if(unitAbility.stackMissWithDodge) missChance+=defend;
			if(unitAbility.stackMissWithDodge){
				//cover bonus does not work with ability 
				//if(GameControlTB.EnableCover()){
				//	coverDefBonus=targetTile.GetCoverDefendBonus(srcUnit.occupiedTile.pos);
				//}
				missChance+=GetDefend();//+coverDefBonus;
			}
			if(Random.Range(0f, 1f)<missChance){
				//UITB.DisplayDamageOverlay(thisT.position+new Vector3(0, 0.5f, 0), "missed", new Color(.6f, 1f, 0, 1));
				new EffectOverlay(thisT.position+new Vector3(0, 0.5f, 0), "missed", new Color(.6f, 1f, 0, 1));
				yield break;
			}
		}
		
		//~ yield return new WaitForSeconds(unitAbility.delay);
		ApplyAbility(unitAbility);
		yield return null;
	}
	public void ApplyAbility(UnitAbility ability){
		foreach(EffectAttr effectAttr in ability.effectAttrs){
			ApplyAbilityEffect(effectAttr);
		}
		
		if(ability.effectType==_EffectType.Buff || ability.effectType==_EffectType.Debuff){
			int countD;
			if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
				countD=UnitControl.activeFactionCount;
			}
			else{
				countD=UnitControl.GetAllUnitCount();
			}
			
			ability.countTillNextTurn=countD;
			
			activeUnitAbilityEffectList.Add(ability);
		}
		
		if(onEffectAppliedE!=null) onEffectAppliedE(this);
	}
	
	
	//called when unit trigger a collectible
	public void ApplyCollectibleEffect(Effect effect){
		foreach(EffectAttr effectAttr in effect.effectAttrs){
			ApplyAbilityEffect(effectAttr);
		}
		
		if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
			effect.countTillNextTurn=UnitControl.activeFactionCount;
		}
		else{
			effect.countTillNextTurn=UnitControl.GetAllUnitCount();
		}
		
		activeCollectibleAbilityEffectList.Add(effect);
		if(onEffectAppliedE!=null) onEffectAppliedE(this);
	}
	
	
//*************************************************************************************************************************//
//code related to apply and calulate effect on the unit, both collecible and ability
	
	//called when a unit is destroyed, to reduce the count on ability/effect duration
	public void ReduceUnitAbilityCountTillNextDown(){
		for(int i=0; i<unitAbilityList.Count; i++){
			UnitAbility uAB=unitAbilityList[i];
			if(uAB.cooldown>0){
				uAB.countTillNextTurn-=1;
				if(uAB.countTillNextTurn<=0) uAB.cooldown-=1;
			}
		}
		
		for(int i=0; i<activeUnitAbilityEffectList.Count; i++){
			activeUnitAbilityEffectList[i].countTillNextTurn-=1;
		}
		for(int i=0; i<activeCollectibleAbilityEffectList.Count; i++){
			activeCollectibleAbilityEffectList[i].countTillNextTurn-=1;
		}
	}
	
	//********************************************************************************************************************
//to restructure how ability duration is tracked, not in used
	
	//public bool showDebug=false;
	//public int countTillNextTurn=0;
	public void OnNextTurn(){
		CalculateEffect();
		
		for(int i=0; i<unitAbilityList.Count; i++){
			UnitAbility uAB=unitAbilityList[i];
			if(uAB.cooldown>0){
				uAB.countTillNextTurn-=1;
				if(uAB.countTillNextTurn==0) uAB.cooldown-=1;
			}
		}
		
		return;
		
		
		
		//~ if(GameControlTB.GetTurnMode()!=_TurnMode.FactionAllUnitPerTurn){
			//~ countTillNextTurn-=1;
			//~ if(countTillNextTurn<=0){
				//~ CalculateEffect();
				//~ //StartCoroutine(_OnNextTurn());
				//~ countTillNextTurn=UnitControl.GetAllUnitCount();
			//~ }
		//~ }
		//~ else{
			//~ countTillNextTurn-=1;
			//~ if(countTillNextTurn<=0){
				//~ CalculateEffect();
				//~ //StartCoroutine(_OnNextTurn());
				//~ countTillNextTurn=GameControlTB.totalFactionInGame;
			//~ }
		//~ }
	}

	
//*****************************************************************************************************************************
	
	
	//the unit ability currently active on the unit
	public List<UnitAbility> activeUnitAbilityEffectList=new List<UnitAbility>();
	//the effect currently active on the unit courtesy of collectible
	public List<Effect> activeCollectibleAbilityEffectList=new List<Effect>();
	//call in every new round, process all the effects
	void CalculateEffect(){
		//loop through the active unit ability list
		for(int i=0; i<activeUnitAbilityEffectList.Count; i++){
			UnitAbility ability=activeUnitAbilityEffectList[i];
			ability.countTillNextTurn-=1;
			if(ability.countTillNextTurn<=0){
				if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
					ability.countTillNextTurn=UnitControl.activeFactionCount;
				}
				else{
					ability.countTillNextTurn=UnitControl.GetAllUnitCount();
				}
				
				//scan through each effectAttr of the ability
				for(int j=0; j<ability.effectAttrs.Count; j++){
					EffectAttr effectAttr=ability.effectAttrs[j];
					bool expired=CalculateAbilityEffect(effectAttr);
					if(expired){
						ability.effectAttrs.RemoveAt(j);
						j-=1;
					}
				}
				
				//if no more effectAttr is taking place, remove the ability
				if(ability.effectAttrs.Count==0){
					activeUnitAbilityEffectList.RemoveAt(i);
					i-=1;
					if(onEffectExpiredE!=null) onEffectExpiredE(this);
				}
				
			}
		}
		
		
		//loop through the active collectible effect list
		for(int i=0; i<activeCollectibleAbilityEffectList.Count; i++){
			Effect effect=activeCollectibleAbilityEffectList[i];
			
			effect.countTillNextTurn-=1;
			if(effect.countTillNextTurn<=0){
				if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
					effect.countTillNextTurn=UnitControl.activeFactionCount;
				}
				else{
					effect.countTillNextTurn=UnitControl.GetAllUnitCount();
				}
				
				//scan through each effectAttr of the ability
				for(int j=0; j<effect.effectAttrs.Count; j++){
					EffectAttr effectAttr=effect.effectAttrs[j];
					bool expired=CalculateAbilityEffect(effectAttr);
					if(expired){
						effect.effectAttrs.RemoveAt(j);
						j-=1;
					}
				}
				
				//if no more effectAttr is taking place, remove the ability
				if(effect.effectAttrs.Count==0){
					activeUnitAbilityEffectList.RemoveAt(i);
					i-=1;
					if(onEffectExpiredE!=null) onEffectExpiredE(this);
				}
				
			}
		}
	}

	//function call to process effectAttr, edit the modifier value and check duration
	bool CalculateAbilityEffect(EffectAttr effect){
		effect.duration-=1;
		
		if(effect.duration<=0){
			if(effect.type==_EffectAttrType.HPDamage){
				//calculation goes directly in CalculateHPDamagePerTurn()
			}
			else if(effect.type==_EffectAttrType.HPGain){
				HPGainMin+=(int)effect.value;
				HPGainMax+=(int)effect.valueAlt;
			}
			else if(effect.type==_EffectAttrType.APDamage){
				APGainMin+=(int)effect.value;
				APGainMax+=(int)effect.valueAlt;
			}
			else if(effect.type==_EffectAttrType.APGain){
				APGainMin-=(int)effect.value;
				APGainMax-=(int)effect.valueAlt;
			}
			else if(effect.type==_EffectAttrType.Damage){
				rangeDamageMin-=(int)effect.value;
				rangeDamageMax-=(int)effect.value;
				meleeDamageMin-=(int)effect.value;
				meleeDamageMax-=(int)effect.value;
			}
			else if(effect.type==_EffectAttrType.MovementRange){
				movementRange-=(int)effect.value;
			}
			else if(effect.type==_EffectAttrType.AttackRange){
				attackRangeMax-=(int)effect.value;
			}
			else if(effect.type==_EffectAttrType.TurnPriority){
				turnPriority-=(int)effect.value;
			}
			else if(effect.type==_EffectAttrType.HitChance){
				attRange-=effect.value;
				attMelee-=effect.value;
			}
			else if(effect.type==_EffectAttrType.DodgeChance){
				defend-=effect.value;
			}
			else if(effect.type==_EffectAttrType.CriticalChance){
				criticalRange-=effect.value;
				criticalMelee-=effect.value;
			}
			else if(effect.type==_EffectAttrType.CriticalImmunity){
				critDef-=effect.value;
			}
			else if(effect.type==_EffectAttrType.ExtraAttack){
				attackPerTurn-=(int)effect.value;
				attackRemain-=(int)effect.value;
			}
			else if(effect.type==_EffectAttrType.ExtraCounterAttack){
				counterPerTurn-=(int)effect.value;
				counterAttackRemain-=(int)effect.value;
			}
			else if(effect.type==_EffectAttrType.Stun){
				stun-=1;
			}
			else if(effect.type==_EffectAttrType.DisableAttack){
				attackDisabled-=1;
			}
			else if(effect.type==_EffectAttrType.DisableMovement){
				movementDisabled-=1;
			}
			else if(effect.type==_EffectAttrType.DisableAbility){
				abilityDisabled-=1;
			}
			else if(effect.type==_EffectAttrType.ChangeTargetFaction){
				//factionChangeDuration-=1;
				UnitControl.ChangeUnitFaction(this, defaultFactionID);
				defaultFactionID=-1;
			}
			
			return true;
		}
		
		return false;
	}
	

	
	//function call to apply effectAttr, apply the effect value and edit the modifier(subject to effectAttr duration)
	void ApplyAbilityEffect(EffectAttr effect){
		if(effect.type==_EffectAttrType.HPDamage){
			//first round, any dot after the first round is calculated and applied in CalculateHPDamagePerTurn()
			float val=Random.Range(effect.value, effect.valueAlt);
			float modifier=DamageTable.GetModifier(armorType, effect.damageType);
			ApplyDamage((int)(val*modifier));
		}
		else if(effect.type==_EffectAttrType.HPGain){
			HPGainMin+=(int)effect.value;
			HPGainMax+=(int)effect.valueAlt;
			ApplyHeal((int)Random.Range(effect.value, effect.valueAlt));
		}
		else if(effect.type==_EffectAttrType.APDamage){
			APGainMin-=(int)effect.value;
			APGainMax-=(int)effect.valueAlt;
			GainAP(-(int)Random.Range(effect.value, effect.valueAlt));
		}
		else if(effect.type==_EffectAttrType.APGain){
			APGainMin+=(int)effect.value;
			APGainMax+=(int)effect.valueAlt;
			GainAP((int)Random.Range(effect.value, effect.valueAlt));
		}
		else if(effect.type==_EffectAttrType.Damage){
			rangeDamageMin+=(int)effect.value;
			rangeDamageMax+=(int)effect.value;
			meleeDamageMin+=(int)effect.value;
			meleeDamageMax+=(int)effect.value;
		}
		else if(effect.type==_EffectAttrType.MovementRange){
			movementRange-=(int)effect.value;
		}
		else if(effect.type==_EffectAttrType.AttackRange){
			attackRangeMax+=(int)effect.value;
		}
		else if(effect.type==_EffectAttrType.TurnPriority){
			turnPriority+=(int)effect.value;
		}
		else if(effect.type==_EffectAttrType.HitChance){
			attRange+=effect.value;
			attMelee+=effect.value;
		}
		else if(effect.type==_EffectAttrType.DodgeChance){
			defend+=effect.value;
		}
		else if(effect.type==_EffectAttrType.CriticalChance){
			criticalMelee+=effect.value;
			criticalRange+=effect.value;
		}
		else if(effect.type==_EffectAttrType.CriticalImmunity){
			critDef+=effect.value;
		}
		else if(effect.type==_EffectAttrType.ExtraAttack){
			attackPerTurn+=(int)effect.value;
			attackRemain+=(int)effect.value;
		}
		else if(effect.type==_EffectAttrType.ExtraCounterAttack){
			counterPerTurn+=(int)effect.value;
			counterAttackRemain+=(int)effect.value;
		}
		else if(effect.type==_EffectAttrType.Stun){
			stun+=1;
		}
		else if(effect.type==_EffectAttrType.DisableAttack){
			attackDisabled+=1;
		}
		else if(effect.type==_EffectAttrType.DisableMovement){
			movementDisabled+=1;
		}
		else if(effect.type==_EffectAttrType.DisableAbility){
			abilityDisabled+=1;
		}
		else if(effect.type==_EffectAttrType.ChangeTargetFaction){
			if(defaultFactionID<0){
				//~ if(Random.Range(0f, 1f)<effect.value){
					defaultFactionID=factionID;
					//factionChangeDuration=effect.duration;
					UnitControl.ChangeUnitFaction(this, (int)effect.valueAlt);
				//~ }
			}
		}
	}
	

//*************************************************************************************************************************//
//code related fog of war system, wip
	
	//public List<Tile> currentTileInLOS=new List<Tile>();
	
	//call to check LOS against the unit that moved
	void OnCheckFogOfWar(UnitTB unit){
		if(factionID==unit.factionID) return;
		
		if(GameControlTB.EnableFogOfWar()){
			if(unit.factionID==GameControlTB.GetPlayerFactionID()){
				AIUnitCheckFogOfWar();
			}
		}
	}
	
	//call by AIUnit to check LOS against all player unit
	//if this unit is visible to any one of them, set the unit to visible
	void AIUnitCheckFogOfWar(){
		//Debug.Log(factionID+"   "+GameControlTB.GetPlayerFactionID()+"  "+gameObject);
		if(factionID==GameControlTB.GetPlayerFactionID()) return;
		
		List<UnitTB> list=UnitControl.GetAllUnitsOfFaction(GameControlTB.GetPlayerFactionID());
		bool flag=false;
		foreach(UnitTB unit in list){
			if(unit.HP>0){
				int dist=GridManager.Distance(unit.occupiedTile, occupiedTile);
				flag=GridManager.IsInLOS(unit.thisT.position, occupiedTile.pos) & dist<=unit.sight;
				
				if(flag){
					//Debug.Log("spoted  "+unit.occupiedTile);
					//Debug.DrawLine(unit.occupiedTile.pos, occupiedTile.pos, Color.red, 3);
					break;
				}
			}
		}
		
		if(flag) SetToVisible();
		else SetToInvisible();
	}
	
	public void SetToVisible(){
		if(factionID==GameControlTB.GetPlayerFactionID()){
			if(thisObj.layer!=LayerManager.GetLayerUnit()){
				thisObj.layer=LayerManager.GetLayerUnit();
				Utility.SetLayerRecursively(thisT, LayerManager.GetLayerUnit());
			}
		}
		else{
			if(thisObj.layer!=LayerManager.GetLayerUnitAI()){
				thisObj.layer=LayerManager.GetLayerUnitAI();
				Utility.SetLayerRecursively(thisT, LayerManager.GetLayerUnitAI());
			}
		}
		if(AIManager.GetAIStance()==_AIStance.Trigger){
			triggered=true;
		}
	}
	public void SetToInvisible(){
		if(thisObj.layer!=LayerManager.GetLayerUnitAIInvisible()){
			thisObj.layer=LayerManager.GetLayerUnitAIInvisible();
			Utility.SetLayerRecursively(thisT, LayerManager.GetLayerUnitAIInvisible());
		}
	}
	
	public bool IsVisibleToPlayer(){
		if(factionID==GameControlTB.GetPlayerFactionID()) return true;
		else{
			if(thisObj.layer==LayerManager.GetLayerUnitAI()) return true;
			else return false;
		}
	}
	
	
	
	
//***************************************************************************************************************************************
//perk related variable and function
	
	private float bonusHP=0;
	private float bonusAP=0;
	private float bonusHPGain=0;
	private float bonusAPGain=0;
	//private float bonusAPGainMin=0;
	//private float bonusHPGainMin=0;
	//private float bonusAPGainMax=0;
	//private float bonusHPGainMax=0;
	private float bonusTurnPriority=0;
	private float bonusMoveRange=0;
	private float bonusSight=0;
	private float bonusMovePerTurn=0;
	private float bonusAttackPerTurn=0;
	private float bonusCounterPerTurn=0;
	
	private float bonusAttackRange=0;
	private float bonusDmgRMin=0;
	private float bonusDmgRMax=0;
	private float bonusDmgMMin=0;
	private float bonusDmgMMax=0;
	private float bonusDmgCounter=0;
	
	private float bonusAttR=0;
	private float bonusAttM=0;
	private float bonusCritR=0;
	private float bonusCritM=0;
	
	private float bonusDefend=0;
	private float bonusCritDef=0;
	private float bonusDmgReduc=0;
	private float bonusAPCostAttack=0;
	private float bonusAPCostMove=0;
	
	void InitPerkBonus(){
		if(PerkManagerTB.instance==null) return;
		
		bonusHP=PerkManagerTB.GetPerkHPBonus(prefabID);
		bonusAP=PerkManagerTB.GetPerkAPBonus(prefabID);
		
		bonusHPGain=PerkManagerTB.GetPerkHPRegenBonus(prefabID);
		bonusAPGain=PerkManagerTB.GetPerkAPRegenBonus(prefabID);
		
		bonusTurnPriority=0;
		bonusMoveRange=PerkManagerTB.GetPerkMoveRangeBonus(prefabID);
		bonusSight=0;
		
		bonusMovePerTurn=PerkManagerTB.GetPerkMoveCountBonus(prefabID);
		bonusAttackPerTurn=PerkManagerTB.GetPerkAttackCountBonus(prefabID);
		bonusCounterPerTurn=PerkManagerTB.GetPerkCounterCountBonus(prefabID);
		
		bonusAttackRange=PerkManagerTB.GetPerkAttackRangeBonus(prefabID);
		
		int bonusDmgR=PerkManagerTB.GetPerkRangeDamageBonus(prefabID, (rangeDamageMin+rangeDamageMax)/2);
		int bonusDmgM=PerkManagerTB.GetPerkRangeDamageBonus(prefabID, (meleeDamageMin+meleeDamageMax)/2);
		
		bonusDmgRMin=PerkManagerTB.GetPerkMinRangeDamageBonus(prefabID)+bonusDmgR;
		bonusDmgRMax=PerkManagerTB.GetPerkMaxRangeDamageBonus(prefabID)+bonusDmgR;
		bonusDmgMMin=PerkManagerTB.GetPerkMinMeleeDamageBonus(prefabID)+bonusDmgM;
		bonusDmgMMax=PerkManagerTB.GetPerkMaxMeleeDamageBonus(prefabID)+bonusDmgM;
		
		bonusDmgCounter+=PerkManagerTB.GetPerkCounterModifierBonus(prefabID);
		
		bonusAttR=PerkManagerTB.GetPerkRangeAttackBonus(prefabID);
		bonusAttM=PerkManagerTB.GetPerkMeleeAttackBonus(prefabID);
		bonusCritR=PerkManagerTB.GetPerkRangeCriticalBonus(prefabID);
		bonusCritM=PerkManagerTB.GetPerkMeleeCriticalBonus(prefabID);
		
		bonusDefend=PerkManagerTB.GetPerkDefendBonus(prefabID);
		bonusCritDef=PerkManagerTB.GetPerkCritImmuneBonus(prefabID);
		bonusDmgReduc=PerkManagerTB.GetPerkDamageReductionBonus(prefabID);
		
		bonusAPCostAttack=PerkManagerTB.GetPerkAttackAPReductionBonus(prefabID);
		bonusAPCostMove=PerkManagerTB.GetPerkMoveAPReductionBonus(prefabID);
		
		//modify local value
		/*
		fullHP+=(int)bonusHP;
		fullAP+=(int)bonusAP;
		
		HPGain+=(int)bonusHPGain;
		APGain+=(int)bonusAPGain;
		
		movementRange+=(int)PerkManagerTB.GetPerkMoveRangeBonus(prefabID);
		attackRangeMax+=(int)PerkManagerTB.GetPerkAttackRangeBonus(prefabID);
		
		rangeDamageMin+=(int)bonusDmgRMin+bonusDmgR;
		rangeDamageMax+=(int)bonusDmgRMax+bonusDmgR;
		meleeDamageMin+=(int)bonusDmgMMin+bonusDmgM;
		meleeDamageMax+=(int)bonusDmgMMax+bonusDmgM;
		
		counterDmgModifier+=bonusDmgCounter;
		
		attRange+=(int)bonusAttR;
		attMelee+=(int)bonusAttM;
		criticalRange+=bonusCritR;
		criticalMelee+=bonusCritM;
		
		movePerTurn+=(int)bonusMovePerTurn;
		attackPerTurn+=(int)bonusAttackPerTurn;
		counterPerTurn+=(int)bonusCounterPerTurn;
		
		defend+=bonusDefend;
		critDef+=bonusCritDef;
		damageReduc+=(int)bonusDmgReduc;
		
		APCostPerAttack+=(int)bonusMoveAPCost;
		APCostPerMove+=(int)bonusAttAPCost;
		*/
	}
	
//end perk related function
//****************************************************************************************************************************************//
	
	
	
	
	
//*************************************************************************************************************************//
//public function to get various member of the instance
	
	//get the default prefab value
	public int GetUnitFullHP(){ return (int)fullHP; }
	public int GetUnitFullAP(){ return (int)fullAP; }
	public int GetUnitHP(){ return (int)HP; }
	public int GetUnitAP(){ return (int)AP; }
	//public int GetUnitHPGain(){ return HPGain; }
	//public int GetUnitAPGain(){ return APGain; }
	public int GetUnitHPGainMin(){ return HPGainMin; }
	public int GetUnitAPGainMin(){ return APGainMin; }
	public int GetUnitHPGainMax(){ return HPGainMax; }
	public int GetUnitAPGainMax(){ return APGainMax; }
	
	public int GetUnitTurnPriority(){ return Mathf.Max(1, turnPriority); }
	public int GetUnitMoveRange(){ return Mathf.Max(0, movementRange); }
	public int GetUnitSight(){ return Mathf.Max(0, sight); }
	public int GetUnitMovePerTurn(){ return Mathf.Max(0, movePerTurn); }
	public int GetUnitAttackPerTurn(){ return Mathf.Max(0, attackPerTurn); }
	public int GetUnitCounterPerTurn(){ return Mathf.Max(0, counterPerTurn); }
	
	public int GetUnitAttackRangeMin(){ return attackMode==_AttackMode.Melee ?  0 : Mathf.Max(0, attackRangeMin);	}
	public int GetUnitAttackRangeMax(){ return attackMode==_AttackMode.Melee ?  attackRangeMelee : Mathf.Max(attackRangeMin, attackRangeMax); }
	//public int GetUnitAttackRangeMin(){ return attackMode==_AttackMode.Melee ?  0 : Mathf.Max(0, attackRangeMin);	}
	//public int GetUnitAttackRangeMax(){ return attackMode==_AttackMode.Melee ?  1 : Mathf.Max(attackRangeMin, attackRangeMax); }
	public int GetUnitRangeDamageMin(){ return Mathf.Max(0, rangeDamageMin); }
	public int GetUnitMeleeDamageMin(){ return Mathf.Max(0, meleeDamageMin); }
	public int GetUnitRangeDamageMax(){ return Mathf.Max(0, rangeDamageMax); }
	public int GetUnitMeleeDamageMax(){ return Mathf.Max(0, meleeDamageMax); }
	public float GetUnitCounterDmgModifier(){ return Mathf.Max(0, counterDmgModifier); }
	public float GetUnitRangeAttack(){ return Mathf.Clamp(attRange, 0f, 1f); }
	public float GetUnitMeleeAttack(){ return Mathf.Clamp(attMelee, 0f, 1f); }
	public float GetUnitRangeCritical(){ return Mathf.Clamp(criticalRange, 0f, 1f); }
	public float GetUnitMeleeCritical(){ return Mathf.Clamp(criticalMelee, 0f, 1f); }
	
	public float GetUnitDefend(){ return Mathf.Clamp(defend, 0f, 1f); }
	public float GetUnitCritDef(){ return Mathf.Clamp(critDef, 0f, 1f); }
	public float GetUnitDmgReduc(){ return Mathf.Clamp(damageReduc, 0f, 1f); }
	public int GetUnitAPCostAttack(){ return Mathf.Max(0, APCostAttack); }
	public int GetUnitAPCostMove(){ return Mathf.Max(0, APCostMove); }
	
	
	
	//get bonus value from perk
	public float GetPerkHP(){					return bonusHP;		}
	public float GetPerkAP(){					return bonusAP;		}
	public float GetPerkHPGain(){			return bonusHPGain;		}
	public float GetPerkAPGain(){			return bonusAPGain;		}
	//public float GetPerkHPGainMin(){		return 0;}//bonusHPGainMin;		}
	//public float GetPerkAPGainMin(){		return 0;}//bonusAPGainMin;		}
	//public float GetPerkHPGainMax(){		return 0;}//bonusHPGainMax;		}
	//public float GetPerkAPGainMax(){		return 0;}//bonusAPGainMax;		}
	
	public float GetPerkTurnPriority(){		return bonusTurnPriority;		}
	public float GetPerkMoveRange(){		return bonusMoveRange;		}
	public float GetPerkSight(){				return bonusSight;		}
	public float GetPerkMovePerTurn(){	return bonusMovePerTurn;		}
	public float GetPerkAttackPerTurn(){	return bonusAttackPerTurn;		}
	public float GetPerkCounterPerTurn(){return bonusCounterPerTurn;		}
	
	public float GetPerkAttackRange(){	return bonusAttackRange;	}
	public float GetPerkDmgRMin(){		return bonusDmgRMin;		}
	public float GetPerkDmgMMin(){		return bonusDmgMMin;		}
	public float GetPerkDmgRMax(){		return bonusDmgRMax;		}
	public float GetPerkDmgMMax(){		return bonusDmgMMax;	}
	public float GetPerkDmgCounter(){	return bonusDmgCounter;		}
	public float GetPerkRangeAttack(){	return bonusAttR;		}
	public float GetPerkMeleeAttack(){	return bonusAttM;		}
	public float GetPerkRangeCritical(){	return bonusCritR;		}
	public float GetPerkMeleeCritical(){	return bonusCritM;		}
	
	public float GetPerkDefend(){			return bonusDefend;		}
	public float GetPerkCritDef(){			return bonusCritDef;		}
	public float GetPerkDmgReduc(){		return bonusDmgReduc;		}
	public float GetPerkAPCostAttack(){	return bonusAPCostAttack;		}
	public float GetPerkAPCostMove(){	return bonusAPCostMove;		}
	
	
	
	//get bonus value from tile
	public float GetTileHP(){					return 0;		}
	public float GetTileAP(){					return 0;		}
	public float GetTileHPGain(){			return occupiedTile==null ?  0 : occupiedTile.HPGainModifier;		}
	public float GetTileAPGain(){			return occupiedTile==null ?  0 : occupiedTile.APGainModifier;		}
	//public float GetTileHPGainMin(){		return 0;}//occupiedTile.HPGainModifier;		}
	//public float GetTileAPGainMin(){		return 0;}//occupiedTile.APGainModifier;		}
	//public float GetTileHPGainMax(){		return 0;}//occupiedTile.HPGainModifier;		}
	//public float GetTileAPGainMax(){		return 0;}//occupiedTile.APGainModifier;		}
	
	public float GetTileTurnPriority(){		return 0;		}
	public float GetTileMoveRange(){		return 0;		}
	public float GetTileSight(){				return occupiedTile==null ?  0 : occupiedTile.sightModifier;		}
	public float GetTileMovePerTurn(){	return 0;		}
	public float GetTileAttackPerTurn(){	return 0;		}
	public float GetTileCounterPerTurn(){return 0;		}
	
	public float GetTileAttackRange(){	return occupiedTile==null ?  0 : occupiedTile.attRangeModifier;		}
	public float GetTileDmgRMin(){			return occupiedTile==null ?  0 : occupiedTile.damageModifier;		}
	public float GetTileDmgMMin(){			return occupiedTile==null ?  0 : occupiedTile.damageModifier;		}
	public float GetTileDmgRMax(){			return occupiedTile==null ?  0 : occupiedTile.damageModifier;		}
	public float GetTileDmgMMax(){		return occupiedTile==null ?  0 : occupiedTile.damageModifier;		}
	public float GetTileDmgCounter(){		return 0;		}
	public float GetTileRangeAttack(){	return occupiedTile==null ?  0 : occupiedTile.attackModifier;		}
	public float GetTileMeleeAttack(){		return occupiedTile==null ?  0 : occupiedTile.attackModifier;		}
	public float GetTileRangeCritical(){	return occupiedTile==null ?  0 : occupiedTile.criticalModifier;		}
	public float GetTileMeleeCritical(){	return occupiedTile==null ?  0 : occupiedTile.criticalModifier;		}
	
	public float GetTileDefend(){			return occupiedTile==null ?  0 : occupiedTile.defendModifier;		}
	public float GetTileCritDef(){			return occupiedTile==null ?  0 : occupiedTile.critDefModifier;		}
	public float GetTileDmgReduc(){		return 0;		}
	public float GetTileAPCostAttack(){	return 0;		}
	public float GetTileAPCostMove(){		return 0;		}
	
	
	
	//get effective value
	public int GetFullHP(){ return (int)GetUnitFullHP()+(int)GetPerkHP()+(int)GetTileHP(); }
	public int GetFullAP(){ return (int)GetUnitFullAP()+(int)GetPerkAP()+(int)GetTileAP(); }
	public int GetHPGainMin(){ return (int)GetUnitHPGainMin()+(int)GetPerkHPGain()+(int)GetTileHPGain(); }
	public int GetAPGainMin(){ return (int)GetUnitAPGainMin()+(int)GetPerkAPGain()+(int)GetTileAPGain(); }
	public int GetHPGainMax(){ return (int)GetUnitHPGainMax()+(int)GetPerkHPGain()+(int)GetTileHPGain(); }
	public int GetAPGainMax(){ return (int)GetUnitAPGainMax()+(int)GetPerkAPGain()+(int)GetTileAPGain(); }
	
	public int GetTurnPriority(){ return (int)GetUnitTurnPriority()+(int)GetPerkTurnPriority()+(int)GetTileTurnPriority(); }
	public int GetMoveRange(){ return (int)GetUnitMoveRange()+(int)GetPerkMoveRange()+(int)GetTileMoveRange(); }
	public int GetSight(){ return (int)GetUnitSight()+(int)GetPerkSight()+(int)GetTileSight(); }
	
	public int GetMovePerTurn(){ return (int)GetUnitMovePerTurn()+(int)GetPerkMovePerTurn()+(int)GetTileMovePerTurn(); }
	public int GetAttackPerTurn(){ return (int)GetUnitAttackPerTurn()+(int)GetPerkAttackPerTurn()+(int)GetTileAttackPerTurn(); }
	public int GetCounterPerTurn(){ return (int)GetUnitCounterPerTurn()+(int)GetPerkCounterPerTurn()+(int)GetTileCounterPerTurn(); }
	
	
	public int GetAttackRangeMin(){ return attackMode==_AttackMode.Melee ?  0 : Mathf.Max(0, attackRangeMin);	}
	public int GetAttackRangeMax(){ return attackMode==_AttackMode.Melee ?  attackRangeMelee : Mathf.Max(GetAttackRangeMin(), attackRangeMax+(int)GetPerkAttackRange()+(int)GetTileAttackRange()); }
	//public int GetAttackRangeMin(){ return attackMode==_AttackMode.Melee ?  0 : Mathf.Max(0, attackRangeMin);	}
	//public int GetAttackRangeMax(){ return attackMode==_AttackMode.Melee ?  1 : Mathf.Max(GetAttackRangeMin(), attackRangeMax+(int)GetPerkAttackRange()+(int)GetTileAttackRange()); }
	public int GetRangeDamageMin(){ return (int)GetUnitRangeDamageMin()+(int)GetPerkDmgRMin()+(int)GetTileDmgRMin(); }
	public int GetMeleeDamageMin(){ return (int)GetUnitMeleeDamageMin()+(int)GetPerkDmgMMin()+(int)GetTileDmgMMin(); }
	public int GetRangeDamageMax(){ return (int)GetUnitRangeDamageMax()+(int)GetPerkDmgRMax()+(int)GetTileDmgRMax(); }
	public int GetMeleeDamageMax(){ return (int)GetUnitMeleeDamageMax()+(int)GetPerkDmgMMax()+(int)GetTileDmgMMax(); }
	
	public float GetCounterDmgModifier(){ return GetUnitCounterDmgModifier()+GetPerkDmgCounter()+GetTileDmgCounter(); }
	
	public float GetRangeAttack(){ return Mathf.Clamp(GetUnitRangeAttack()+GetPerkRangeAttack()+GetTileRangeAttack(), 0f, 1f); }
	public float GetMeleeAttack(){ return Mathf.Clamp(GetUnitMeleeAttack()+GetPerkMeleeAttack()+GetTileMeleeAttack(), 0f, 1f); }
	public float GetRangeCritical(){ return Mathf.Clamp(GetUnitRangeCritical()+GetPerkRangeCritical()+GetTileRangeCritical(), 0f, 1f); }
	public float GetMeleeCritical(){ return Mathf.Clamp(GetUnitMeleeCritical()+GetPerkMeleeCritical()+GetTileMeleeCritical(), 0f, 1f); }
	


	public float GetDefend(){ return Mathf.Clamp(GetUnitDefend()+GetPerkDefend()+GetTileDefend(), 0f, 1f); }
	public float GetCritDef(){ return Mathf.Clamp(GetUnitCritDef()+GetPerkCritDef()+GetTileCritDef(), 0f, 1f); }
	public float GetDmgReduc(){ return Mathf.Clamp(GetUnitDmgReduc()+GetPerkDmgReduc()+GetTileDmgReduc(), 0f, 1f); }
		
	public int GetAPCostAttack(){ return (int)GetUnitAPCostAttack()+(int)GetPerkAPCostAttack()+(int)GetTileAPCostAttack(); }
	public int GetAPCostMove(){ return (int)GetUnitAPCostMove()+(int)GetPerkAPCostMove()+(int)GetTileAPCostMove(); }
	

	
	
	//get effective value
	//~ public int GetHPGain(){ return (int)HPGain+occupiedTile.HPGainModifier; }
	//~ public int GetAPGain(){ return (int)APGain+occupiedTile.APGainModifier; }
	//~ public int GetHPGainMin(){ return (int)HPGainMin+occupiedTile.HPGainModifier; }
	//~ public int GetAPGainMin(){ return (int)APGainMin+occupiedTile.APGainModifier; }
	//~ public int GetHPGainMax(){ return (int)HPGainMax+occupiedTile.HPGainModifier; }
	//~ public int GetAPGainMax(){ return (int)APGainMax+occupiedTile.APGainModifier; }
	//~ //public int GetDamage(){ return damage+occupiedTile.damageModifier; }
	//~ public int GetRangeDamageMin(){ return Mathf.Max(0, rangeDamageMin+occupiedTile.damageModifier); }
	//~ public int GetRangeDamageMax(){ return Mathf.Max(0, rangeDamageMax+occupiedTile.damageModifier); }
	//~ public int GetMeleeDamageMin(){ return Mathf.Max(0, meleeDamageMin+occupiedTile.damageModifier); }
	//~ public int GetMeleeDamageMax(){ return Mathf.Max(0, meleeDamageMax+occupiedTile.damageModifier); }
	//~ public int GetMovementRange(){ return Mathf.Max(0, movementRange); }//+occupiedTile.movementModifier); }
	//~ public int GetAttackRangeMin(){ return attackMode==_AttackMode.Melee ?  0 : Mathf.Max(0, attackRangeMin); }
	//~ public int GetAttackRangeMax(){ return attackMode==_AttackMode.Melee ?  1 : Mathf.Max(attackRangeMin, attackRangeMax+occupiedTile.attRangeModifier); }
	//~ public int GetSight(){ return Mathf.Max(0, sight+occupiedTile.sightModifier); }
	//~ public int GetSpeed(){ return Mathf.Max(1, turnPriority); }//+occupiedTile.speedModifier); }
	//~ public float GetAttack(){ return Mathf.Clamp(attack+occupiedTile.attackModifier, 0f, 1f); }
	//~ public float GetAttRange(){ return Mathf.Clamp(attRange+occupiedTile.attackModifier, 0f, 1f); }
	//~ public float GetAttMelee(){ return Mathf.Clamp(attMelee+occupiedTile.attackModifier, 0f, 1f); }
	//~ public float GetDefend(){ return Mathf.Clamp(defend+occupiedTile.defendModifier, 0f, 1f); }
	//~ public float GetCritical(){ return Mathf.Clamp(critical+occupiedTile.criticalModifier, 0f, 1f); }
	//~ public float GetCriticalRange(){ return Mathf.Clamp(criticalRange+occupiedTile.criticalModifier, 0f, 1f); }
	//~ public float GetCriticalMelee(){ return Mathf.Clamp(criticalMelee+occupiedTile.criticalModifier, 0f, 1f); }
	//~ public float GetCriticalDefend(){ return Mathf.Clamp(critDef+occupiedTile.critDefModifier, 0f, 1f); }
	//~ public int GetMovePerTurn(){ return Mathf.Max(0, movePerTurn); }//+occupiedTile.extraAttackModifier); }
	//~ public int GetAttackPerTurn(){ return Mathf.Max(0, attackPerTurn); }//+occupiedTile.extraAttackModifier); }
	//~ public int GetCounterPerTurn(){ return Mathf.Max(0, counterPerTurn); }//+occupiedTile.counterAttackModifier); }
	public int GetStun(){ return Mathf.Max(0, stun); }//+occupiedTile.stun); }
	public int GetAttackDisabled(){ return Mathf.Max(0, attackDisabled); }//+occupiedTile.attackDisabled); }
	public int GetMovementDisabled(){ return Mathf.Max(0, movementDisabled); }//+occupiedTile.movementDisabled); }
	public int GetAbilityDisabled(){ return Mathf.Max(0, abilityDisabled); }//+occupiedTile.abilityDisabled); }
	
	//called by attacker to check potential value of each corresponding function call
	public float GetTotalHitChanceMelee(UnitTB target){
		return Mathf.Clamp(GetMeleeAttack()-target.GetDefend(), 0, 1);
	}
	public float GetTotalHitChanceRange(UnitTB target){
		float coverBonus=0;
		if(GameControlTB.EnableCover()) coverBonus=target.occupiedTile.GetCoverDefendBonus(occupiedTile.pos);
		return Mathf.Clamp(GetRangeAttack()-(target.GetDefend()+coverBonus), 0, 1);
	}
	public float GetTotalCritChanceMelee(UnitTB target){
		return Mathf.Clamp(GetMeleeCritical()-target.GetCritDef(), 0, 1);
	}
	public float GetTotalCritChanceRange(UnitTB target){
		float critBonus=0;
		if(GameControlTB.EnableCover()){
			if(target.occupiedTile.GetCoverDefendBonus(occupiedTile.pos)<=0) critBonus=GameControlTB.GetExposedCritBonus();
		}
		return Mathf.Clamp((GetRangeCritical()+critBonus)-target.GetCritDef(), 0, 1);
	}
	
	//check if the unit is currently performing an action
	public bool InAction(){
		if(actionQueued>0) return true;
		return false;
	}
	
	public bool IsStunned(){
		if(stun>0) return true;
		return false;
	}
	
	//check if the unit has performed any move at all
	public bool MovedForTheRound(){
		if(moved || attacked || abilityTriggered) return true;
		return false;
	}
	
	//get the number of active buff effect on the unit
	public int GetActiveBuffEffectCount(){
		int num=0;
		foreach(Effect effect in activeUnitAbilityEffectList){
			if(effect.effectType==_EffectType.Buff) num+=1;
		}
		foreach(Effect effect in activeCollectibleAbilityEffectList){
			if(effect.effectType==_EffectType.Buff) num+=1;
		}
		return num;
	}
	//get the number of active debuff effect on the unit
	public int GetActiveDebuffEffectCount(){
		int num=0;
		foreach(Effect effect in activeUnitAbilityEffectList){
			if(effect.effectType==_EffectType.Debuff) num+=1;
		}
		foreach(Effect effect in activeCollectibleAbilityEffectList){
			if(effect.effectType==_EffectType.Debuff) num+=1;
		}
		return num;
	}
	//get all active effect on the target
	public List<Effect> GetAllActiveEffect(){
		List<Effect> effectList=new List<Effect>();
		foreach(Effect effect in activeUnitAbilityEffectList){
			effectList.Add(effect);
		}
		foreach(Effect effect in activeCollectibleAbilityEffectList){
			effectList.Add(effect);
		}
		return effectList;
	}
	
	public bool IsDestroyed(){
		return HP<=0 ? true : false ;
	}
	
	public Transform GetTargetT(){
		return targetPointT != null ? targetPointT : thisT;
	}
	
	

	
	
	
//for external use, not in used
//************************************************************************************************************************************
		
	
		public static bool isWarCorpAlpha=true;
		public int instanceID=-1;
		public int uID=-1;
		//[HideInInspector] 
		public float status=0;
		//[HideInInspector] 
		public float repairSpeedModifier=1;
		//[HideInInspector] 
		public bool repair=true;
		public int valueCash=10;
		public int valueMats=10;
		public float repairCashUsed=0;
		public float repairMatsUsed=0;
		public int repairCashUnit=2;
		public int repairMatsUnit=3;
		public int deploymentCost=10;
		public int cost=100;
		//public int marketQuantity=1;
		
	/*
		public _MechClass mechClass;
		
		public Pilot pilot;
		public void NewPilot(Pilot newPilot){
			pilot=newPilot;
			attack=newPilot.attack;
			defend=newPilot.defend*0.5f;
			if(weap.weapClass==pilot.weapClass)attack+=0.05f;
			if(mechClass==pilot.mechClass) defend+=0.05f;
		}
		public void RemovePilot(){
			NewPilot(Inventory.GetBotPilot());
		}
		
		public Weapon weap;
		public Weapon defaultWeap;
		public void NewWeapon(Weapon newWeap){
			weap=newWeap;
			damageRangeMin=weap.damageMin;
			damageRangeMax=weap.damageMax;
			criticalRange=weap.critChance;
			APCostPerAttack=weap.apCost;
			if(weap.weapClass==pilot.weapClass) attack+=0.05f;
		}
		public void RemoveWeapon(){
			NewWeapon(defaultWeap);
		}
		*/
	//end for external use
	
	
	
}

