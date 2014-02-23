using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Xml;
using System.IO;

public class DamageTable : MonoBehaviour {

	private static List<ArmorType> armorTypes=new List<ArmorType>();
	private static List<DamageType> dmgTypes=new List<DamageType>();
	
	public static List<ArmorType> GetAllArmorType(){ return armorTypes; }
	public static List<DamageType> GetAllDamageType(){ return dmgTypes; }
	
	
	// Use this for initialization
	void Awake() {
		LoadPrefab();
	}
	
	private static void LoadPrefab(){
		GameObject obj=Resources.Load("DamageArmorList", typeof(GameObject)) as GameObject;
		
		if(obj==null) return;
		
		DamageArmorListPrefab prefab=obj.GetComponent<DamageArmorListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<DamageArmorListPrefab>();
		
		armorTypes=prefab.armorList;
		dmgTypes=prefab.damageList;
	}
	

	public static float GetModifier(int armorID=0, int dmgID=0){
		armorID=Mathf.Max(0, armorID);
		dmgID=Mathf.Max(0, dmgID);
		if(armorID<armorTypes.Count && dmgID<dmgTypes.Count){
			return armorTypes[armorID].modifiers[dmgID];
		}
		else{
			return 1f;
		}
	}
	
	public static ArmorType GetArmorInfo(int ID){
		if(ID>armorTypes.Count){
			Debug.Log("ArmorType requested does not exist");
			return new ArmorType();
		}
		
		return armorTypes[ID];
	}
	
	public static DamageType GetDamageInfo(int ID){
		if(ID>dmgTypes.Count){
			Debug.Log("DamageType requested does not exist");
			return new DamageType();
		}
		
		return dmgTypes[ID];
	}
	
}

[System.Serializable]
public class DamageType{
	public int typeID=-1;
	public string name="DamageName";
	public string desp="";
}

[System.Serializable]
public class ArmorType{
	public int typeID=-1;
	public string name="ArmorName";
	public string desp="";
	public List<float> modifiers=new List<float>();
	
	public ArmorType(int ID, string n, string d, List<float> mods){
		typeID=ID;
		name=n;
		desp=d;
		modifiers=mods;
	}
	
	public ArmorType(){
		modifiers.Add(1.0f);
	}
	
	public ArmorType(int modsNum){
		for(int i=0; i<modsNum; i++){
			modifiers.Add(1f);
		}
	}
}