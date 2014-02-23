using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;

public class PerkTBEditorWindow : EditorWindow {
	
	
	//public delegate void ScoreHandler(int waveID);
	//public static event ScoreHandler onScoreE;
	

	static private PerkTBEditorWindow window;
	
	private static PerkTBListPrefab prefab;
	private static List<PerkTB> perkList=new List<PerkTB>();
	private static List<int> perkIDList=new List<int>();
	
	static string[] nameList=new string[0];
	static List<UnitTB> unitList=new List<UnitTB>();//new UnitTB[0];
	
	public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (PerkTBEditorWindow)EditorWindow.GetWindow(typeof (PerkTBEditorWindow));
		window.minSize=new Vector2(615, 700);
		window.maxSize=new Vector2(615, 701);
		
		InitLabel();
		Load();
		
		EditorUnitList eUnitList=UnitTBManagerWindow.LoadUnit();
		unitList=eUnitList.prefab.unitList;
		string[] tempNameList=eUnitList.nameList;
		
		unitList.Add(null);
		nameList=new String[tempNameList.Length+1];
		for(int i=0; i<tempNameList.Length; i++){
			nameList[i]=tempNameList[i];
		}
		nameList[nameList.Length-1]="-";
	}
	
	static string[] perkTypeLabel=new string[0];
	static string[] perkTypeTooltip=new string[0];
	static string[] modTypeLabel=new string[2];
	static string[] modTypeTooltip=new string[2];
	
	protected static void InitLabel() {
		int enumLength = Enum.GetValues(typeof(_PerkTypeTB)).Length;
		perkTypeLabel=new string[enumLength];
		perkTypeTooltip=new string[enumLength];
		for(int i=0; i<perkTypeLabel.Length; i++){
			perkTypeLabel[i]=((_PerkTypeTB)i).ToString();
			perkTypeTooltip[i]=((_PerkTypeTB)i).ToString();
		}
		
		modTypeLabel[0]="direct value";
		modTypeLabel[1]="percentage";
		modTypeTooltip[0]="use the value directly by adding on to the existing value";
		modTypeTooltip[1]="use the value as a percentage modifier by multiplying the existing value";
	}
	
	public static List<PerkTB> Load(){
		GameObject obj=Resources.Load("PrefabList/PerkTBListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null) obj=CreatePrefab();
		
		prefab=obj.GetComponent<PerkTBListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<PerkTBListPrefab>();
		
		perkList=prefab.perkTBList;
		foreach(PerkTB perk in perkList){
			perkIDList.Add(perk.ID);
		}
		
		return perkList;
	}
	
	public static GameObject CreatePrefab(){
		GameObject obj=new GameObject();
		obj.AddComponent<PerkTBListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/PrefabList/PerkTBListPrefab.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
		DestroyImmediate(obj);
		AssetDatabase.Refresh ();
		return prefab;
	}
	
	
	
	int delete=-1;
	static int swapID=-1;
	static int selectedPerkID=0;
	private Vector2 scrollPos;
	
	GUIContent cont;
	GUIContent[] contL;
	
	void OnGUI () {
		if(window==null) Init();
		
		if(GUI.Button(new Rect(5, 10, 100, 30), "New Perk")){
			PerkTB perk=new PerkTB();
			perk.ID=GenerateNewID();
			perkIDList.Add(perk.ID);
			perk.name="Perk "+perkList.Count;
			perkList.Add(perk);
			
			NewSelection(perkList.Count-1);
			delete=-1;
			
			GUI.changed=true;
		}
		if(selectedPerkID>=0 && selectedPerkID<perkList.Count){
			if(GUI.Button(new Rect(115, 10, 100, 30), "Clone Perk")){
				PerkTB perk=perkList[selectedPerkID].Clone();
				perk.ID=GenerateNewID();
				perkIDList.Add(perk.ID);
				perkList.Add(perk);
				
				NewSelection(perkList.Count-1);
				delete=-1;
				
				GUI.changed=true;
			}
		}
		
		cont=new GUIContent("Reset Perk", "Reset all persistant perk progress made in gameplay");
		if(GUI.Button(new Rect(window.position.width-110, 10, 100, 30), cont)){
			ResetAllPerk();
		}
		
		
		GUI.Box(new Rect(5, 50, window.position.width-10, 185), "");
		scrollPos = GUI.BeginScrollView(new Rect(5, 55, window.position.width-12, 175), scrollPos, new Rect(5, 50, window.position.width-40, 10+((perkList.Count-1)/3)*35));
		
		int row=0;
		int column=0;
		for(int i=0; i<perkList.Count; i++){
			
			GUIStyle style=GUI.skin.label;
			style.alignment=TextAnchor.MiddleCenter;
			if(swapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			else GUI.color=new Color(.8f, .8f, .8f, 1);
			GUI.Box(new Rect(10+column*210, 50+row*30, 25, 25), "");
			if(GUI.Button(new Rect(10+column*210, 50+row*30, 25, 25), "")){
				delete=-1;
				if(swapID==i) swapID=-1;
				else if(swapID==-1) swapID=i;
				else{
					SwapPerkInList(swapID, i);
					swapID=-1;
				}
			}
			GUI.Label(new Rect(8+column*210, 50+row*30, 25, 25), i.ToString(), style);
			GUI.color=Color.white;
			style.alignment=TextAnchor.MiddleLeft;
			
			if(selectedPerkID==i) GUI.color = Color.green;
			if(perkList[selectedPerkID].prereq.Contains(i)) GUI.color=new Color(0, 1f, 1f, 1f);
			style=GUI.skin.button;
			style.fontStyle=FontStyle.Bold;
			
			GUI.SetNextControlName ("PerkButton");
			if(GUI.Button(new Rect(10+27+column*210, 50+row*30, 100, 25), perkList[i].name, style)){
				GUI.FocusControl ("PerkButton");
				NewSelection(i);
				delete=-1;
			}
			GUI.color = Color.white;
			style.fontStyle=FontStyle.Normal;
			
			if(delete!=i){
				if(GUI.Button(new Rect(10+27+102+column*210, 50+row*30, 25, 25), "X")){
					delete=i;
				}
			}
			else{
				GUI.color = Color.red;
				if(GUI.Button(new Rect(10+27+102+column*210, 50+row*30, 55, 25), "Delete")){
					CheckDeletePrereq(i);
					perkIDList.Remove(perkList[i].ID);
					perkList.RemoveAt(i);
					InitPerkUIDependency();
					if(i<=selectedPerkID) selectedPerkID-=1;
					delete=-1;
					
					GUI.changed=true;
				}
				GUI.color = Color.white;
			}
			
			column+=1;
			if(column==3){
				column=0;
				row+=1;
			}
		}
		
		GUI.EndScrollView();
		
		if(selectedPerkID>-1 && selectedPerkID<perkList.Count){
			PerkConfigurator();
		}
		
		if(GUI.changed) EditorUtility.SetDirty(prefab);
	}
	
	
	void PerkConfigurator(){
		int startY=245;
		int startX=360;
		int spaceY=18;
		
		GUIStyle style=new GUIStyle();
		style.wordWrap=true;
		
		PerkTB perk=perkList[selectedPerkID];
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Default Icon: ");
		perk.icon=(Texture)EditorGUI.ObjectField(new Rect(startX+10, startY+17, 60, 60), perk.icon, typeof(Texture), false);
		startX+=100;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Unavailable: ");
		perk.iconUnavailable=(Texture)EditorGUI.ObjectField(new Rect(startX+10, startY+17, 60, 60), perk.iconUnavailable, typeof(Texture), false);
		startX+=80;
		
		EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Unlocked: ");
		perk.iconUnlocked=(Texture)EditorGUI.ObjectField(new Rect(startX, startY+17, 60, 60), perk.iconUnlocked, typeof(Texture), false);
		
		
		if(perk.icon!=null && perk.icon.name!=perk.iconName){
			perk.iconName=perk.icon.name;
			//GUI.changed=true;
		}
		if(perk.iconUnavailable!=null && perk.iconUnavailable.name!=perk.iconUnavailableName){
			perk.iconUnavailableName=perk.iconUnavailable.name;
			//GUI.changed=true;
		}
		if(perk.iconUnlocked!=null && perk.iconUnlocked.name!=perk.iconUnlockedName){
			perk.iconUnlockedName=perk.iconUnlocked.name;
			//GUI.changed=true;
		}
		
		
		
		
		//~ startX+=100;
		//~ EditorGUI.LabelField(new Rect(startX, startY, 200, 34), "Description: ");
		//~ startY+=17;
		//~ EditorGUI.LabelField(new Rect(startX, startY, window.position.width-startX-20, 120), perkTypeTooltip[(int)perk.type], style);
		
		startX=5;
		startY=245;
		
		cont=new GUIContent("Name: ", "The name of the perk");
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), cont);
		perk.name=EditorGUI.TextField(new Rect(startX+50, startY-1, 150, 17), perk.name);
		
		
		cont=new GUIContent("cost:", "The cost to unlock the perk");
		EditorGUI.LabelField(new Rect(startX+225, startY, 180, 17), cont);
		perk.cost=EditorGUI.IntField(new Rect(startX+260, startY, 60, 17), perk.cost);
		
		
		startY+=5;
		cont=new GUIContent("Description (to be used in runtime): ", "");
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 250, 20), cont);
		perk.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 320, 50), perk.desp);
		
		//startY+=10;
		//~ int perkType=(int)perk.type;
		//~ EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "Type: ");
		//~ perkType=EditorGUI.Popup(new Rect(startX+50, startY, 150, 20), perkType, perkTypeLabel);
		//~ perk.type=(_PerkTypeTB)perkType;
		//if(perkType!=(int)perk.type) perk.SetType((_PerkTypeTB)perkType);
		
		//~ startY+=60;
		//~ cont=new GUIContent("cost:", "The cost to unlock the perk");
		//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 180, 17), cont);
		//~ perk.cost=EditorGUI.IntField(new Rect(startX+100, startY, 80, 17), perk.cost);
		
		
		startY+=60;
		
		
		cont=new GUIContent("Effect Types:", "The type of effect for this perk, determine what the perk do\nEach perk can only have up to 3 effects");
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 180, 17), cont);
		
		
		
		int tempStartY=startY;
		
		for(int n=0; n<perk.effects.Count; n++){
			PerkEffectTB effect=perk.effects[n];
			int perkType=(int)effect.type;
			cont=new GUIContent("Type:", "The type of effect");
			contL=new GUIContent[perkTypeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(perkTypeLabel[i], perkTypeTooltip[i]);
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 180, 17), cont);
			perkType=EditorGUI.Popup(new Rect(startX+50, startY, 130, 17), perkType, contL);
			effect.type=(_PerkTypeTB)perkType;
			
			cont=new GUIContent("value:", "The value for this particular effect");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 180, 17), cont);
			effect.value=EditorGUI.FloatField(new Rect(startX+100, startY, 80, 17), effect.value);
			
			int modifierFlag=0;
			if(effect.isModifier) modifierFlag=1;
			cont=new GUIContent("value type:", "how the value will be applied");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 180, 17), cont);
			contL=new GUIContent[2];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(modTypeLabel[i], modTypeTooltip[i]);
			modifierFlag=EditorGUI.Popup(new Rect(startX+100, startY, 80, 17), modifierFlag, contL);
			if(modifierFlag==1) effect.isModifier=true;
			else effect.isModifier=false;
			
			if(GUI.Button(new Rect(startX+50, startY+=spaceY, 130, 15), "Remove Effect")){
				perk.effects.RemoveAt(n);
				n-=1;
			}
			
			startY=tempStartY;
			startX+=210;
		}
		
		if(perk.effects.Count<3){
			if(GUI.Button(new Rect(startX, startY+=spaceY, 150, 20), "Add")){
				perk.effects.Add(new PerkEffectTB());
			}
		}
		
		startX=5;
		//startY+=100;
		startY=460;
		
		cont=new GUIContent("PerkPoint Required:", "The perk point required before the perk can be unlocked\neach unlocked perk gives one perk point");
		EditorGUI.LabelField(new Rect(startX, startY, 180, 17), cont);
		perk.pointReq=EditorGUI.IntField(new Rect(startX+120, startY, 60, 17), perk.pointReq);
		spaceY+=5;
		
		prereq=perk.prereq;
		cont=new GUIContent("Pre-req Perk(s): ", "perk that required to be unlocked before this perk comes available to be unlocked");
		EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, 20), cont); //startY-=5;
		//existing prereq
		for(int i=0; i<prereq.Count; i++){
			int existPrereq=prereq[i];
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, 150, 20), " - ");
			existPrereq=EditorGUI.Popup(new Rect(startX+20, startY, 160, 20), existPrereq, perkListLabel);
			if(existPrereq!=selectedPerkID) prereq[i]=existPrereq;
			if(existPrereq==perkList.Count){
				perk.prereq.RemoveAt(i);
				perkList[i].reqby.Remove(selectedPerkID);
			}
		}
		//assignNewOne
		if(prereq.Count<3){
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY-5, 150, 20), "Add new: ");
			newPrereq=EditorGUI.Popup(new Rect(startX+70, startY, 110, 20), newPrereq, perkListLabel);
			if(newPrereq<perkList.Count){
				if(!perk.prereq.Contains(newPrereq) && selectedPerkID!=newPrereq){
					perk.prereq.Add(newPrereq);
					perkList[newPrereq].reqby.Add(selectedPerkID);
				}
				newPrereq=perkList.Count;
			}
		}
		
		
		startY=460;
		startX=220;
		
		cont=new GUIContent("Apply to all unit: ", "check to if effect apply universally to all unit\notherwise specify the units to be affected");
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), cont);
		perk.applyToAllUnit=EditorGUI.Toggle(new Rect(startX+100, startY, 200, 20), perk.applyToAllUnit);
		
		if(!perk.applyToAllUnit){
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 200, 20), "Select unit to be affected: ");
			
			int index=0;
			
			for(int i=0; i<perk.unitPrefab.Count; i++){
				if(perk.unitPrefab[i]==null){
					perk.unitPrefab.RemoveAt(i);	i-=1;
				}
			}
			
			for(int i=0; i<perk.unitPrefab.Count; i++){
				for(int n=0; n<unitList.Count; n++){
					if(perk.unitPrefab[i]==unitList[n]){
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 150, 20), " - ");
						index=n;
						index = EditorGUI.Popup(new Rect(startX+15, startY, 160, 20), index, nameList);
						if(!perk.unitPrefab.Contains(unitList[index])){
							perk.unitPrefab[i]=unitList[index];
						}
						break;
					}
				}
			}
			
			index=nameList.Length-1;
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 150, 20), "Add new: ");
			index = EditorGUI.Popup(new Rect(startX+70, startY, 105, 20), index, nameList);
			if(index<nameList.Length-1){
				if(!perk.unitPrefab.Contains(unitList[index])) perk.unitPrefab.Add(unitList[index]);
			}
		}
		
		
		
		if(GUI.changed) EditorUtility.SetDirty(prefab);
	}
	
	//~ static string[] nameList=new string[0];
	//~ static List<UnitTB> unitList=new List<UnitTB>();//new UnitTB[0];
	
	//static int newPrereq=2000;
	static private List<int> prereq=new List<int>();
	
	static void NewSelection(int ID){
		selectedPerkID=ID;
		InitPerkUIDependency();
		
		//for(int i=0; i<perkList[selectedPerkID].reqby.Count; i++){
		//	int IDD=perkList[selectedPerkID].reqby[i];
		//	Debug.Log(perkList[IDD].name);
		//}
	}
	
	int GenerateNewID(){
		int newID=0;
		while(perkIDList.Contains(newID)) newID+=1;
		return newID;
	}
	
	static void SwapPerkInList(int ID1, int ID2){
		PerkTB temp=perkList[ID1].Clone();
		perkList[ID1]=perkList[ID2].Clone();
		perkList[ID2]=temp.Clone();
		InitPerkUIDependency();
		
		foreach(PerkTB perk in perkList){
			for(int i=0; i<perk.prereq.Count; i++){
				if(perk.prereq[i]==ID1) perk.prereq[i]=ID2;
				else if(perk.prereq[i]==ID2) perk.prereq[i]=ID1;
			}
		}
	}
	
	static int newPrereq=2000;
	static string[] perkListLabel=new string[0];
	protected static void InitPerkUIDependency(){
		perkListLabel=new string[perkList.Count+1];
		for(int i=0; i<perkList.Count; i++){
			if(i!=selectedPerkID){
				perkListLabel[i]=("("+i.ToString()+")"+perkList[i].name);
			}
			else perkListLabel[i]="Self";
		}
		perkListLabel[perkList.Count]="None";
		
		newPrereq=perkList.Count;
	}
	
	static void CheckDeletePrereq(int ID){
		for(int i=0; i<perkList.Count; i++){
			PerkTB perk=perkList[i];
			if(perk.prereq.Contains(ID)) perk.prereq.Remove(ID);
			for(int n=0; n<perk.prereq.Count; n++){
				if(perk.prereq[n]>ID) perk.prereq[n]-=1;
			}
		}
	}
	
	
	public static void ResetAllPerk(){
		List<PerkTB> list=Load();
		for(int i=0; i<list.Count; i++){
			list[i].unlocked=false;
		}
		Debug.Log("all perk has been set to locked");
	}
}
