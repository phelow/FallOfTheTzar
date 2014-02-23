using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public enum _StartingState{
	Default,
	Locked,
	Unlocked,
}

public enum _PerkTypeTB{
	BonusHP,
	BonusAP,
	BonusHPRegen,
	BonusAPRegen,
	BonusMoveRange,
	BonusAttackRange,
	
	BonusDamage,
	BonusDamageMin,
	BonusDamageMax,
	
	BonusRangeDamage,
	BonusRangeDamageMin,
	BonusRangeDamageMax,
	
	BonusMeleeDamage,
	BonusMeleeDamageMin,
	BonusMeleeDamageMax,
	
	BonusCounterModifier,
	
	BonusHitRange,
	BonusHitMelee,
	
	BonusCritRange,
	BonusCritMelee,
	
	BonusMove,
	BonusAttack,
	BonusCounter,
	
	BonusDodge,
	BonusCritImmune,
	BonusDamageReduc,
	
	APAttackReduc,
	APMoveReduc,
}

[System.Serializable]
public class EffectValue{
	public float value=0;
	public float mod=0;
}

[System.Serializable]
public class UnitStatModifier{
	public UnitTB unit;
	
	public List<EffectValue> stats=new List<EffectValue>();
	
	public UnitStatModifier(){
		unit=null;
		
		int enumLength = Enum.GetValues(typeof(_PerkTypeTB)).Length;
		for(int i=0; i<enumLength; i++){
			stats.Add(new EffectValue());
		}
	}
	public UnitStatModifier(UnitTB unitTB){
		unit=unitTB;
		
		int enumLength = Enum.GetValues(typeof(_PerkTypeTB)).Length;
		for(int i=0; i<enumLength; i++){
			stats.Add(new EffectValue());
		}
	}
}

[System.Serializable]
public class PerkEffectTB{
	//public 
	public _PerkTypeTB type;
	public float value=1;
	public bool isModifier=false;
	
	public PerkEffectTB Clone(){
		PerkEffectTB effect=new PerkEffectTB();
		effect.type=type;
		effect.value=value;
		effect.isModifier=isModifier;
		return effect;
	}
}


[System.Serializable]
public class PerkTB{
	public int ID=-1;
	public string name="";
	public string desp="";
	
	public Texture icon;
	public Texture iconUnavailable;
	public Texture iconUnlocked;
	
	//[HideInInspector] 
	public string iconName="";
	public string iconUnavailableName="";
	public string iconUnlockedName="";
	
	public int cost=0;
	public int pointReq=0;
	
	public List<PerkEffectTB> effects=new List<PerkEffectTB>();
	
	public List<int> prereq=new List<int>();
	public List<int> reqby=new List<int>();
	
	public bool applyToAllUnit=true;
	public List<UnitTB> unitPrefab=new List<UnitTB>();
	public List<int> unitPrefabID=new List<int>();
	
	public bool unlocked=false;
	
	public bool availableInScene=true;
	public _StartingState startingState=_StartingState.Default;
	
	public PerkTB Clone(){
		PerkTB perk=new PerkTB();
		
		perk.ID=ID;
		perk.name=name;
		perk.desp=desp;
		perk.icon=icon;
		perk.iconUnavailable=iconUnavailable;
		perk.iconUnlocked=iconUnlocked;
		//perk.type=type;
		
		perk.iconName=iconName;
		perk.iconUnavailableName=iconUnavailableName;
		perk.iconUnlockedName=iconUnlockedName;
		
		
		perk.cost=cost;
		
		//perk.effects=effects;
		for(int i=0; i<effects.Count; i++){
			perk.effects.Add(effects[i].Clone());
		}
		
		perk.prereq=prereq;
		perk.reqby=reqby;
		perk.applyToAllUnit=applyToAllUnit;
		perk.unitPrefab=unitPrefab;
		
		perk.unlocked=unlocked;
		
		perk.availableInScene=availableInScene;
		perk.startingState=startingState;
		
		return perk;
	}
	
	
	public int IsAvailable(){
		if(!PerkManagerTB.UsePersistentData()){
			if(PerkManagerTB.GetPoint()<cost){
				return 1;
				//return "insufficient point to unlock perk";
				//return false;
			}
		}
		else{
			if(GlobalStatsTB.GetPlayerPoint()<cost){
				return 1;
				//return "insufficient player point to unlock perk";
				//return false;
			}
		}
		
		if(PerkManagerTB.GetPerkPoint()<pointReq){
			return 2;
			//return  "insufficient perk point to unlock perk";
			//return false;
		}
		
		for(int i=0; i<prereq.Count; i++){
			if(!PerkManagerTB.GetPerk(prereq[i]).unlocked){
				return 3;
				//return "pre-req perk needed to unlock perk";
				//return false;
			}
		}
		
		//return true;
		//return "";
		return 0;
	}
	
}




public class PerkManagerTB : MonoBehaviour {
	
	public delegate void PerkUnlockHandler(PerkTB perk); 
	public static event PerkUnlockHandler onPerkUnlockE;
	
	public List<PerkTB> localPerkList=new List<PerkTB>();
	
	public static List<PerkTB> perkList=new List<PerkTB>();
	
	public static UnitStatModifier globalUnitStats=new UnitStatModifier();
	
	public static List<UnitStatModifier> unitStats=new List<UnitStatModifier>();
	public static List<UnitTB> unitList=new List<UnitTB>();
	
	//recording how many perk has been unlocked
	public static int perkPoint=0;
	public static int GetPerkPoint(){
		return perkPoint;
	}
	
	//local point to unlock perk if persistant data is not used
	public int point=0;
	public static int GetPoint(){
		if(instance==null) return 0;
		return instance.point;
	}
	
	public bool dontDestroyOnLoad=true;
	public bool usePersistentData=false;
	public static bool UsePersistentData(){
		if(instance==null) return false;
		return instance.usePersistentData;
	}
	
	public static PerkManagerTB instance;
	
	void Awake(){
		if(instance!=null) Destroy(gameObject);
		
		if(dontDestroyOnLoad){
			GameObject.DontDestroyOnLoad(gameObject);
		}
		
		instance=this;
		
		LoadPerk();
		LoadUnit();
		
		//if(!GlobalStatsTB.loaded){
		//	perkPoint=startingPerkPoint;
		//}
		
		//ResetPerk();
	}
	
	/*
	void OnGUI(){
		
		int startX=10;
		int startY=10;
		
		GUI.Label(new Rect(startX, startY, 150, 25), "perk point: "+perkPoint);
		startY+=30;
		
		for(int i=0; i<perkList.Count; i++){
			PerkTB perk=perkList[i];
			if(perk.unlocked){
				GUI.Label(new Rect(startX, startY, 100, 25), perk.name);
			}
			else{
				if(perk.IsAvailable()){
					if(GUI.Button(new Rect(startX, startY, 100, 25), perk.name)){
						UnlockPerk(perk);
					}
				}
				else{
					GUI.Box(new Rect(startX, startY, 100, 25), perk.name);
				}
			}
			startY+=30;
		}
		
		if(GUI.Button(new Rect(startX, startY+30, 100, 25), "reset all")){
			ResetPerk();
		}
	}
	*/
	
	
	public static void LoadPerk(){
		GameObject obj=Resources.Load("PrefabList/PerkTBListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null){
			Debug.Log("load perk fail, make sure the resource file exists");
			return;
		}
		
		PerkTBListPrefab prefab=obj.GetComponent<PerkTBListPrefab>();
		perkList=prefab.perkTBList;
		
		if(!instance.usePersistentData){
			for(int i=0; i<perkList.Count; i++){
				perkList[i]=perkList[i].Clone();
			}
		}
		
		//check local perk, synchronice the list, set the lock/unlock list
		for(int i=0; i<perkList.Count; i++){
			PerkTB perk=perkList[i];
			for(int j=0; j<instance.localPerkList.Count; j++){
				PerkTB localPerk=instance.localPerkList[j];
				if(perk.ID==localPerk.ID){
					if(!localPerk.availableInScene){
						perkList.RemoveAt(i);
						i-=1;
					}
					else{
						if(localPerk.startingState==_StartingState.Locked){
							perk.unlocked=false;
						}
						else if(localPerk.startingState==_StartingState.Unlocked){
							perk.unlocked=true;
						}
						else if(localPerk.startingState==_StartingState.Default){
							
						}
					}
					break;
				}
			}
		}
		
		
		//re-'unlocked' unlocked perk, to register the perk point and etc.
		for(int i=0; i<perkList.Count; i++){
			if(perkList[i].unlocked) {
				UnlockPerk(perkList[i], false);
			}
		}
	}
	
	
	
	public static void LoadUnit(){
		GameObject obj=Resources.Load("PrefabList/UnitListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null){
			Debug.Log("load unit fail, make sure the resource file exists");
			return;
		}
		
		UnitListPrefab prefab=obj.GetComponent<UnitListPrefab>();
		unitList=prefab.unitList;
		
		for(int i=0; i<unitList.Count; i++){
			unitStats.Add(new UnitStatModifier(unitList[i]));
		}
		
		globalUnitStats=new UnitStatModifier(null);
	}
	
	public static void SavePerk(){
		GameObject obj=Resources.Load("PrefabList/PerkTBListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null){
			Debug.Log("load perk fail, make sure the resource file exists");
			return;
		}
		
		PerkTBListPrefab prefab=obj.GetComponent<PerkTBListPrefab>();
		prefab.perkTBList=perkList;
	}
	
	public static void ResetPerk(){
		for(int i=0; i<perkList.Count; i++){
			perkList[i].unlocked=false;
		}
	}
	
	public static PerkTB GetPerk(int ID){
		for (int i=0; i<perkList.Count; i++){
			if(perkList[i].ID==ID){
				return perkList[i].Clone();
			}
		}
		return null;
	}
	
	public static List<PerkTB> GetAllPerks(){
		return perkList;
	}
	
	public static bool UnlockPerk(int ID){
		return UnlockPerk(perkList[ID], true);
	}
	public static bool UnlockPerk(int ID, bool flag){
		return UnlockPerk(perkList[ID], flag);
	}
	public static bool UnlockPerk(PerkTB perk){
		return UnlockPerk(perk, true);
	}
	public static bool UnlockPerk(PerkTB perk, bool flag){
		if(perk.unlocked){
			Debug.Log("attemp to unlock unlocked perk");
			return false;
		}
		
		perk.unlocked=true;
		
		perkPoint+=1;
		
		if(flag){
			if(!instance.usePersistentData) instance.point-=perk.cost;
			else GlobalStatsTB.GainPlayerPoint(-perk.cost);
		}
		
		if(perk.applyToAllUnit){
			for(int i=0; i<perk.effects.Count; i++){
				PerkEffectTB effect=perk.effects[i];
				
				int effectID=(int)effect.type;
				
				if(effect.isModifier) globalUnitStats.stats[effectID].mod+=effect.value;
				else globalUnitStats.stats[effectID].value+=effect.value;
			}
		}
		else{
			for(int i=0; i<unitStats.Count; i++){
				if(perk.unitPrefab.Contains(unitStats[i].unit)){
					
					perk.unitPrefabID.Add(unitStats[i].unit.prefabID);
					
					for(int j=0; j<perk.effects.Count; j++){
						PerkEffectTB effect=perk.effects[j];
						
						int effectID=(int)effect.type;
						
						if(effect.isModifier) unitStats[i].stats[effectID].mod+=effect.value;
						else unitStats[i].stats[effectID].value+=effect.value;
					}
					
					break;
				}
			}
		}
		
		if(onPerkUnlockE!=null) onPerkUnlockE(perk);
		
		return true;
	}
	
	
	public static UnitStatModifier GetSpecificUnitStat(int prefabID){
		for(int i=0; i<unitStats.Count; i++){
			if(unitStats[i].unit.prefabID==prefabID){
				//Debug.Log(unitStats[i].unit.unitName+"    "+prefabID);
				return unitStats[i];
			}
		}
		Debug.Log("error, no unit prefab found");
		return null;
	}
	
	
	
	
	
	
	//******************************************************************************************************************************
	//get specific stats bonus
	
	public static int GetPerkHPBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusHP;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.HP*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.HP*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkAPBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusAP;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.AP*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.AP*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkHPRegenBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusHPRegen;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=(.5f*(uStats.unit.HPGainMin+uStats.unit.HPGainMax))*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=(.5f*(uStats.unit.HPGainMin+uStats.unit.HPGainMax))*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkAPRegenBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusAPRegen;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=(.5f*(uStats.unit.APGainMin+uStats.unit.APGainMax))*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=(.5f*(uStats.unit.APGainMin+uStats.unit.APGainMax))*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkMoveRangeBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusMoveRange;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.movementRange*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.movementRange*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkAttackRangeBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusAttackRange;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.attackRangeMax*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.attackRangeMax*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	
	public static int GetPerkRangeDamageBonus(int prefabID, int dmg){
		float total=0;	int statID=(int)_PerkTypeTB.BonusDamage;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=dmg*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=dmg*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		statID=(int)_PerkTypeTB.BonusRangeDamage;
		uStats=GetSpecificUnitStat(prefabID);
		total+=dmg*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=dmg*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkMeleeDamageBonus(int prefabID, int dmg){
		float total=0;	int statID=(int)_PerkTypeTB.BonusDamage;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=dmg*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=dmg*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		statID=(int)_PerkTypeTB.BonusMeleeDamage;
		uStats=GetSpecificUnitStat(prefabID);
		total+=dmg*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=dmg*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkMinRangeDamageBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusDamageMin;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.rangeDamageMin*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.rangeDamageMin*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		statID=(int)_PerkTypeTB.BonusRangeDamageMin;
		uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.rangeDamageMin*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.rangeDamageMin*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkMaxRangeDamageBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusDamageMax;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.rangeDamageMax*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.rangeDamageMax*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		statID=(int)_PerkTypeTB.BonusRangeDamageMax;
		uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.rangeDamageMax*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.rangeDamageMax*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkMinMeleeDamageBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusDamageMin;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.meleeDamageMin*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.meleeDamageMin*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		statID=(int)_PerkTypeTB.BonusMeleeDamageMin;
		uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.meleeDamageMin*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.meleeDamageMin*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static int GetPerkMaxMeleeDamageBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusDamageMax;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.meleeDamageMax*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.meleeDamageMax*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		statID=(int)_PerkTypeTB.BonusMeleeDamageMax;
		uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.meleeDamageMax*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.meleeDamageMax*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	public static float GetPerkCounterModifierBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusCounterModifier;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.counterDmgModifier*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.counterDmgModifier*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return total;
	}

	
	
	public static float GetPerkRangeAttackBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusHitRange;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.attRange*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.attRange*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return total;
	}
	
	public static float GetPerkMeleeAttackBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusHitMelee;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.attMelee*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.attMelee*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return total;
	}
	
	public static float GetPerkRangeCriticalBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusCritRange;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.criticalRange*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.criticalRange*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return total;
	}
	
	public static float GetPerkMeleeCriticalBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusCritMelee;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.criticalMelee*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.criticalMelee*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return total;
	}

	
	
	public static int GetPerkMoveCountBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusMove;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.movePerTurn*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.movePerTurn*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	public static int GetPerkAttackCountBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusAttack;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.attackPerTurn*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.attackPerTurn*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	public static int GetPerkCounterCountBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusCounter;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.counterPerTurn*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.counterPerTurn*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	
	
	
	public static float GetPerkDefendBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusDodge;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.defend*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.defend*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return total;
	}
	public static float GetPerkCritImmuneBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusCritImmune;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.critDef*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.critDef*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return total;
	}
	public static float GetPerkDamageReductionBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.BonusDamageReduc;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total+=uStats.unit.damageReduc*uStats.stats[statID].mod+uStats.stats[statID].value;
		total+=uStats.unit.damageReduc*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return total;
	}
	
	
	public static int GetPerkAttackAPReductionBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.APAttackReduc;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total-=uStats.unit.APCostAttack*uStats.stats[statID].mod+uStats.stats[statID].value;
		total-=uStats.unit.APCostAttack*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}
	public static int GetPerkMoveAPReductionBonus(int prefabID){
		float total=0;	int statID=(int)_PerkTypeTB.APMoveReduc;
		
		UnitStatModifier uStats=GetSpecificUnitStat(prefabID);
		total-=uStats.unit.APCostMove*uStats.stats[statID].mod+uStats.stats[statID].value;
		total-=uStats.unit.APCostMove*globalUnitStats.stats[statID].mod+globalUnitStats.stats[statID].value;
		
		return (int)total;
	}

}
