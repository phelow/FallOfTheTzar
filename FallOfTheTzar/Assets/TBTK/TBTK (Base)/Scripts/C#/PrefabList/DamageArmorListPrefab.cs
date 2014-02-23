using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageArmorListPrefab : MonoBehaviour {
	public List<ArmorType> armorList=new List<ArmorType>();
	public List<DamageType> damageList=new List<DamageType>();
}

//~ public class DamageType{
	//~ public int typeID=-1;
	//~ public string name="DamageType";
	//~ public string desp="";
//~ }

//~ public class ArmorType{
	//~ public int typeID=-1;
	//~ public string name="ArmorType";
	//~ public string desp="";
	//~ public List<float> modifiers=new List<float>();
//~ }
