using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public enum _EffectType{DirectPositive, DirectNegative, Buff, Debuff}

//~ public enum _AbilityTargetType{
	//~ Self,
	//~ SelfTile,
	//~ Hostile,
	//~ Friendly,
	//~ Tile
//~ }

public enum _TargetArea{Default, Line, Cone}

public enum _AbilityTargetType{
	AllUnits, Friendly, Hostile, AllTile, EmptyTile,
}

public enum _EffectAttrType{
	HPDamage,
	HPGain,
	APDamage,
	APGain,
	Damage,
	MovementRange,
	AttackRange,
	TurnPriority,
	HitChance,
	DodgeChance,
	CriticalChance,
	CriticalImmunity,
	ExtraAttack,
	ExtraCounterAttack,
	Stun,
	DisableAttack,
	DisableMovement,
	DisableAbility,
	//Panic,
	
	PointGain,
	
	Teleport,
	SpawnUnit,
	ChangeTargetFaction,
	
	SpawnCollectible,
}

public enum _AbilityShootMode{None, ShootToCenter, ShootToAll};

[System.Serializable]
public class EffectAttr{
	public _EffectAttrType type;
	public bool useDefaultDamageValue=true;
	public float value;
	public float valueAlt;
	public int duration;
	public int damageType;
	//public Texture icon;
	
	public int cd;
	
	public UnitTB unit;
	public CollectibleTB collectible;
	
	public EffectAttr Clone(){
		EffectAttr effectAttr=new EffectAttr();
		effectAttr.type=type;
		effectAttr.useDefaultDamageValue=useDefaultDamageValue;
		effectAttr.value=value;
		effectAttr.valueAlt=valueAlt;
		effectAttr.duration=duration;
		effectAttr.damageType=damageType;
		effectAttr.unit=unit;
		effectAttr.collectible=collectible;
		return effectAttr;
	}
}

[System.Serializable]
public class Effect{
	public int ID=-1;
	public string name="";
	public string desp="";
	public Texture icon;
	public string iconName="";
	
	//~ public int duration=1;
	//public bool isBuff=true;
	public _EffectType effectType;
	
	public List<EffectAttr> effectAttrs=new List<EffectAttr>();
	
	//fill up to total unit/faction count when the effect is applied, reduced at every player's turn or full unit turn cycle
	//when the value reach zero, effect duraction-=1;
	public int countTillNextTurn=1;
}


//~ public class CollectibleAbility : Ability{
	//~ public bool enableAOE=false;
	//~ public int aoeRange=1;
//~ }


[System.Serializable]
public class UnitAbility : Effect{
	public Texture iconUnavailable;
	public string iconUnavailableName="";
	
	public _AbilityTargetType targetType;
	public bool requireTargetSelection=true;
	public bool enableMovementAfter=true;
	public bool enableAttackAfter=true;
	
	public int factionID;
	
	public bool canMiss=false;
	public float missChance=0.15f;
	public bool stackMissWithDodge=true;
	public bool canFail=false;
	public float failChance=0.15f;
	
	public List<int> chainedAbilityIDList=new List<int>();
	
	public _TargetArea targetArea;
	public bool enableAOE=false;
	public int aoeRange=1;
	//~ public _AOETypeHex aoe=_AOETypeHex.None;
	public int range=3;
	public int cost=2;
	public int totalCost;
	public int cdDuration=2;
	public int cooldown=0;
	public int useLimit=-1;
	public int useCount=0;
	
	public float delay=0;
	
	public GameObject effectUse;
	public GameObject effectTarget;
	public float effectUseDelay=0;
	public float effectTargetDelay=0;
	
	public AudioClip soundUse;
	public AudioClip soundHit;
	
	public _AbilityShootMode shootMode;
	public GameObject shootObject;
	
	public UnitAbility Clone(){
		UnitAbility UAb=new UnitAbility();
		UAb.ID=ID;
		UAb.name=name;
		UAb.desp=desp;
		UAb.icon=icon;
		UAb.iconUnavailable=iconUnavailable;
		
		UAb.iconName=iconName;
		UAb.iconUnavailableName=iconUnavailableName;
		
		UAb.factionID=factionID;
		
		UAb.targetArea=targetArea;
		UAb.targetType=targetType;
		UAb.requireTargetSelection=requireTargetSelection;
		UAb.enableMovementAfter=enableMovementAfter;
		UAb.enableAttackAfter=enableAttackAfter;
		
		UAb.effectType=effectType;
		
		UAb.enableAOE=enableAOE;
		UAb.aoeRange=aoeRange;
		//~ UAb.aoe=aoe;
		UAb.range=range;
		//~ UAb.duration=duration;
		UAb.cost=cost;
		UAb.totalCost=totalCost;
		UAb.cdDuration=cdDuration;
		UAb.cooldown=cooldown;
		UAb.useLimit=useLimit;
		UAb.useCount=useCount;
		
		foreach(EffectAttr effectAttr in effectAttrs){
			UAb.effectAttrs.Add(effectAttr.Clone());
		}
		
		UAb.delay=delay;
		//~ UAb.effect=effect;
		
		UAb.effectUse=effectUse;
		UAb.effectTarget=effectTarget;
		UAb.effectUseDelay=effectUseDelay;
		UAb.effectTargetDelay=effectTargetDelay;
		
		UAb.soundUse=soundUse;
		UAb.soundHit=soundHit;
		
		UAb.shootMode=shootMode;
		UAb.shootObject=shootObject;
		
		UAb.canMiss=canMiss;
		UAb.canFail=canFail;
		UAb.missChance=missChance;
		UAb.failChance=failChance;
		
		UAb.chainedAbilityIDList=chainedAbilityIDList;
		
		return UAb;
	}
	
	//~ public UnitAbility(int id, string n, string d, int c, int cd, int lim){
		//~ ID=id;
		//~ name=n;
		//~ desp=d;
		//~ cost=c;
		//~ cooldown=cd;
		//~ limit=lim;
	//~ }
}


public class AbilityManagerTB : MonoBehaviour {

	public static List<UnitAbility> unitAbilityList=new List<UnitAbility>();
	
	//public UnitAbility ability;
	
	public static void LoadUnitAbility(){
		GameObject obj=Resources.Load("PrefabList/UnitAbilityListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null){
			Debug.Log("load unit ability fail, make sure the resource file exists");
			return;
		}
		
		UnitAbilityListPrefab prefab=obj.GetComponent<UnitAbilityListPrefab>();
		unitAbilityList=prefab.unitAbilityList;
	}
	
	
	public static UnitAbility GetUnitAbility(int ID){
		foreach(UnitAbility uAB in unitAbilityList){
			if(uAB.ID==ID){
				return uAB.Clone();
			}
		}
		return null;
	}
	
	
	public static void HealAllFriendlyUnit(int factionID){
		List<UnitTB> list=UnitControl.GetAllUnitsOfFaction(factionID);
		foreach(UnitTB unit in list){
			int val=Random.Range(2, 6);
			unit.ApplyHeal(val);
		}
	}
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
