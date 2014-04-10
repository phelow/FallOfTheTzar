using UnityEngine;
using System.Collections;
using System.Collections.Generic;


using System;
using System.Xml;
using System.IO;


public enum _Mode{New, Load, LoadXNew}

public class GlobalStatsTB : MonoBehaviour {

	public delegate void PlayerPointChangedHandler(int point); 
	public static event PlayerPointChangedHandler onPlayerPointChangedE;
	
	public _Mode mode=_Mode.Load;
	public static bool loaded=false;
	
	//public int defaultPoint;
	//public List<UnitTB> defaultPlayerUnits=new List<UnitTB>();
	
	public static int playerPoint=20;
	
	public static List<int> playerUnitIDList=new List<int>();
	
	public static List<UnitTB> unitPrefabList=new List<UnitTB>();
	public static List<UnitTB> playerUnitList=new List<UnitTB>();
	
	public static List<UnitTB> tempPlayerUnitList=new List<UnitTB>();
	public static List<UnitTB> tempPlayerUnitList1=new List<UnitTB>();
	public static List<UnitTB> tempPlayerUnitList2=new List<UnitTB>();
	public static List<UnitTB> tempPlayerUnitList3=new List<UnitTB>();
	public static List<UnitTB> tempPlayerUnitList4=new List<UnitTB>();
	
	public static int playerFactionCount=1;
	public static int GetPlayerFactionCount(){ return playerFactionCount; }
	public static void SetPlayerFactionCount(int count){ playerFactionCount=count; }
	
	
	
	private static int defaultStartingPoint=35;
	
	public static int GetPlayerPoint(){
		return playerPoint;
	}
	public static List<UnitTB> GetPlayerUnitList(){
		List<UnitTB> newList=new List<UnitTB>();
		foreach(UnitTB unit in playerUnitList) newList.Add(unit);
		return newList;
	}
	public static List<UnitTB> GetTempPlayerUnitList(){
		List<UnitTB> newList=new List<UnitTB>();
		foreach(UnitTB unit in tempPlayerUnitList) newList.Add(unit);
		return newList;
	}
	
	
	public static void Init(){
		//Debug.Log("global stats init");
		
		if(loaded) return;
		
		GameObject obj=new GameObject();
		obj.name="GlobalStats";
		obj.AddComponent<GlobalStatsTB>();
	}
	
	void Awake(){
		//Debug.Log("global stats awake");
		
		LoadUnit();
		
		if(mode==_Mode.Load){
			LoadData();
		}
		else if(mode==_Mode.New){
			NewData();
		}
		else if(mode==_Mode.LoadXNew){
			if(!PlayerPrefs.HasKey("DataExisted")){
				NewData();
			}
			else LoadData();
		}
		
		loaded=true;
	}
	
	public void NewData(){
		//Debug.Log("data: new");
		playerPoint=defaultStartingPoint;
		playerUnitList=new List<UnitTB>();
	}
	
	public void LoadData(){
		playerPoint=PlayerPrefs.GetInt("PlayerPoint", defaultStartingPoint);
		
		int playerUnitCount=PlayerPrefs.GetInt("PlayerUnitCount", 0);
		for(int i=0; i<playerUnitCount; i++){
			playerUnitIDList.Add(PlayerPrefs.GetInt("PlayerUnit"+i.ToString(), 0));
		}
		
		foreach(int ele in playerUnitIDList){
			foreach(UnitTB unit in unitPrefabList){
				if(unit.prefabID==ele){
					playerUnitList.Add(unit);
				}
			}
		}
	}
	
	public static void LoadUnit(){
		GameObject obj=Resources.Load("UnitPrefabList", typeof(GameObject)) as GameObject;
		if(obj!=null){
			UnitListPrefab prefab=obj.GetComponent<UnitListPrefab>();
			if(prefab!=null){
				//Debug.Log(prefab.unitList.Count);
				unitPrefabList=prefab.unitList;
			}
		}
		
		/*
		Debug.Log("unitPrefabList length: "+unitPrefabList.Count);
		foreach(UnitTB unit in unitPrefabList){
			if(unit==null){
				Debug.Log("null");
			}
		}
		*/
	}
	
	//~ public static void SetAll(){
		//~ PlayerPrefs.SetInt("DataExisted", 1);
		//~ SetPlayerPoint();
		//~ SetUnit();
	//~ }
	
	public static void GainPlayerPoint(int point){
		playerPoint+=point;
		PlayerPrefs.SetInt("PlayerPoint", playerPoint);
		SetDataExistedFlag();
		
		if(onPlayerPointChangedE!=null) onPlayerPointChangedE(point);
	}
	public static void SetPlayerPoint(){
		PlayerPrefs.SetInt("PlayerPoint", playerPoint);
		SetDataExistedFlag();
	}
	public static void SetPlayerPoint(int point){
		playerPoint=point;
		PlayerPrefs.SetInt("PlayerPoint", playerPoint);
		SetDataExistedFlag();
		
		//if(onPlayerPointChangedE!=null) onPlayerPointChangedE(playerPoint);
	}
	
	
	public static void SetPlayerUnitList(List<UnitTB> list){
		playerUnitList=list;
		PlayerPrefs.SetInt("PlayerUnitCount", playerUnitList.Count);
		
		for(int i=0; i<playerUnitList.Count; i++){
			PlayerPrefs.SetInt("PlayerUnit"+i.ToString(), playerUnitList[i].prefabID);
		}
		
		SetDataExistedFlag();
	}
	public static void SetTempPlayerUnitList(List<UnitTB> list){
		tempPlayerUnitList=list;
	}
	
	public static void SetTempPlayerUnitList(List<PlayerUnits> list){
		for(int i=0; i<list.Count; i++){
			if(i==0) tempPlayerUnitList=list[i].starting;
			else if(i==1) tempPlayerUnitList1=list[i].starting;
			else if(i==2) tempPlayerUnitList2=list[i].starting;
			else if(i==3) tempPlayerUnitList3=list[i].starting;
			else if(i==4) tempPlayerUnitList4=list[i].starting;
		}
	}
	public static List<UnitTB> GetTempPlayerUnitList(int ID){
		if(ID==0) return tempPlayerUnitList;
		else if(ID==1) return tempPlayerUnitList1;
		else if(ID==2) return tempPlayerUnitList2;
		else if(ID==3) return tempPlayerUnitList3;
		else if(ID==4) return tempPlayerUnitList4;
		
		return tempPlayerUnitList;
	}
	
	
	/*
	public static void SaveToXML(List<PerkTB> list){
		XmlDocument xmlDoc=new XmlDocument();
		xmlDoc.LoadXml("<something></something>");
		
		for(int j=0; j<list.Count; j++){
			PerkTB perk=list[j];
			
			XmlNode docRoot = xmlDoc.DocumentElement;
			XmlElement perkEle = xmlDoc.CreateElement("Perk"+j.ToString());
		
			XmlAttribute Attr = xmlDoc.CreateAttribute(perk.name+perk.ID.ToString());
			Attr.Value = (perk.unlocked ? 1 : 0).ToString();
			perkEle.Attributes.Append(Attr);
			
			docRoot.AppendChild(perkEle);
		}
	
		xmlDoc.Save(Application.dataPath  + "/TBTK/Resources/Perk.txt");
	}
	
	public static List<PerkTB> LoadFromXML(List<PerkTB> list){
		List<string> refList=new List<string>();
		for(int i=0; i<list.Count; i++){
			refList.Add(list[i].name+list[i].ID.ToString());
		}
		
		XmlDocument xmlDoc = new XmlDocument();
		
		TextAsset perkTextAsset=Resources.Load("Perk", typeof(TextAsset)) as TextAsset;
		if(perkTextAsset!=null){
			xmlDoc.Load(new MemoryStream(perkTextAsset.bytes));
			XmlNode rootNode = xmlDoc.FirstChild;
			if (rootNode.Name == "something"){
				int perkCount=rootNode.ChildNodes.Count;
				for(int n=0; n<perkCount; n++){
					
					for(int m=0; m<rootNode.ChildNodes[n].Attributes.Count; m++){
						XmlAttribute attr=rootNode.ChildNodes[n].Attributes[m];
						for(int i=0; i<refList.Count; i++){
							if(attr.Name==refList[i]){
								if(attr.Value=="1") list[i].unlocked=true;
								break;
							}
						}
					}
				}
			}
		}
		
		return list;
	}
	*/
	
	
	
	public static void SetDataExistedFlag(){
		if(!PlayerPrefs.HasKey("DataExisted")) PlayerPrefs.SetInt("DataExisted", 1);
	}
	
	public static void ResetAll(){
		//Debug.Log("data: reset");
		PlayerPrefs.DeleteAll ();
		playerPoint=defaultStartingPoint;
		playerUnitList=new List<UnitTB>();
		playerUnitIDList=new List<int>();
	}
	
	
	
	//Update is called once per frame
	//void Update () {
	//
	//}
}
