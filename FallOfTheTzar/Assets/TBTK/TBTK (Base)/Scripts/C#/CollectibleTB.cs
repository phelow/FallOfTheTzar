using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//~ public enum _CollectibleType{HPGain, APGain, PointGain, EnergyGain, Bomb, XPGain, ResourceGain};

public class CollectibleTB : MonoBehaviour {

	//~ public _CollectibleType type;
	
	public int prefabID=-1;
	public string collectibleName="collectible";
	public Texture icon;
	
	[HideInInspector] public GameObject thisObj;
	[HideInInspector] public Transform thisT;
	
	[HideInInspector] public Tile occupiedTile;
	
	public AudioClip triggerAudio;
	public GameObject triggerEffect;
	
	public float value=1f;
	
	public bool enableAOE=false;
	public int aoeRange=1;
	//~ public List<Effect> effect=new List<Effect>();
	public Effect effect=new Effect();
	
	//~ public Ability ability;
	
	void Awake(){
		thisObj=gameObject;
		thisT=transform;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void Init(){
		
	}
	
	
	
	public void Trigger(UnitTB unit){
		
		foreach(EffectAttr effectAttr in effect.effectAttrs){
			if(effectAttr.type==_EffectAttrType.PointGain){
				GameControlTB.GainPoint((int)effectAttr.value);
			}
		}
		
		if(enableAOE && aoeRange>1){
			List<Tile> list=GridManager.GetTilesWithinRange(occupiedTile, aoeRange);
			foreach(Tile tile in list){
				if(tile.unit!=null){
					tile.unit.ApplyCollectibleEffect(effect);
				}
			}
		}
		else unit.ApplyCollectibleEffect(effect);
		
		
		if(triggerEffect!=null){
			Instantiate(triggerEffect, occupiedTile.pos, Quaternion.identity);
		}
		
		if(triggerAudio!=null){
			AudioManager.PlaySound(triggerAudio, thisT.position);
		}
		
		Destroy(thisObj);
	}
}


		

/*
		foreach(EffectAttr effectAttr in effect.effectAttrs){
			if(effectAttr.type==_AbilityEffectType.HPHeal){
				if(enableAOE && aoeRange>1) AOE(effectAttr);
				else unit.ApplyHeal((int)effectAttr.value);
			}
			if(effectAttr.type==_AbilityEffectType.HPDamage){
				if(enableAOE && aoeRange>1) AOE(effectAttr);
				else unit.ApplyDamage((int)effectAttr.value);
			}
			else if(effectAttr.type==_AbilityEffectType.APGain){
				if(enableAOE && aoeRange>1) AOE(effectAttr);
				else unit.GainAP((int)effectAttr.value);
			}
			else if(effectAttr.type==_AbilityEffectType.APDamage){
				if(enableAOE && aoeRange>1) AOE(effectAttr);
				else unit.ApplyHeal((int)effectAttr.value);
			}
			else if(effectAttr.type==_AbilityEffectType.Damage){
				if(enableAOE && aoeRange>1) AOE(effectAttr);
				else unit.ApplyHeal((int)effectAttr.value);
			}
			else if(effectAttr.type==_AbilityEffectType.MovementRange){
				if(enableAOE && aoeRange>1) AOE(effectAttr);
				else unit.ApplyHeal((int)effectAttr.value);
			}
			else if(effectAttr.type==_AbilityEffectType.AttackRange){
				if(enableAOE && aoeRange>1) AOE(effectAttr);
				else unit.ApplyHeal((int)effectAttr.value);
			}
			else if(effectAttr.type==_AbilityEffectType.Speed){
				if(enableAOE && aoeRange>1) AOE(effectAttr);
				else unit.ApplyHeal((int)effectAttr.value);
			}
			else if(effectAttr.type==_AbilityEffectType.Attack){
				
			}
			else if(effectAttr.type==_AbilityEffectType.Defend){
				
			}
			else if(effectAttr.type==_AbilityEffectType.CriticalChance){
				
			}
			else if(effectAttr.type==_AbilityEffectType.CriticalImmunity){
				
			}
			else if(effectAttr.type==_AbilityEffectType.ExtraAttack){
				
			}
			else if(effectAttr.type==_AbilityEffectType.ExtraCounterAttack){
				
			}
			else if(effectAttr.type==_AbilityEffectType.Stun){
				
			}
			else if(effectAttr.type==_AbilityEffectType.DisableAttack){
				
			}
			else if(effectAttr.type==_AbilityEffectType.DisableMovement){
				
			}
			else if(effectAttr.type==_AbilityEffectType.DisableAbility){
				
			}
			else if(effectAttr.type==_AbilityEffectType.PointGain){
				
			}
		}
		*/
