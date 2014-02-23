using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(PerkManagerTB))]
public class PerkManagerTBEditor : Editor {

	PerkManagerTB pm;
	
	string[] startingStateLabel=new string[0];
	string[] startingStateTooltip=new string[0];
	
	//~ string[] moveOrderLabel=new string[0];
	//~ string[] moveOrderTooltip=new string[0];
	
	//~ string[] loadModeLabel=new string[0];
	//~ string[] loadModeTooltip=new string[0];
	
	//~ string[] moveAPRuleLabel=new string[0];
	//~ string[] moveAPRuleTooltip=new string[0];
	
	//~ string[] attackAPRuleLabel=new string[0];
	//~ string[] attackAPRuleTooltip=new string[0];

	int[] intVal=new int[5];
	
	List<PerkTB> perkList=new List<PerkTB>();
	
	void Awake () {
		pm=(PerkManagerTB)target;
		
		LoadPerk();
		
		InitLabel();
	}
	
	void LoadPerk(){
		if(Application.isPlaying) return;
		
		perkList=PerkTBEditorWindow.Load();
		
		for(int i=0; i<perkList.Count; i++){
			for(int j=0; j<pm.localPerkList.Count; j++){
				if(perkList[i].ID==pm.localPerkList[j].ID){
					perkList[i].availableInScene=pm.localPerkList[j].availableInScene;
					perkList[i].startingState=pm.localPerkList[j].startingState;
					
					break;
				}
			}
		}
		
		//pm.localPerkList=perkList;
		pm.localPerkList=new List<PerkTB>();
		for(int i=0; i<perkList.Count; i++){
			pm.localPerkList.Add(perkList[i].Clone());
		}
	}
	
	
	void InitLabel(){
		
		int enumLength = Enum.GetValues(typeof(_StartingState)).Length;
		startingStateLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) startingStateLabel[i]=((_StartingState)i).ToString();
		
		startingStateTooltip=new string[enumLength];
		startingStateTooltip[0]="use persistant data carried from previous level";
		startingStateTooltip[1]="locked when start";
		startingStateTooltip[2]="unlocked when start";
		
		for(int i=0; i<intVal.Length; i++){
			intVal[i]=i;	
		}
		
	}
	
	GUIContent cont;
	GUIContent[] contL;
	
	public override void OnInspectorGUI(){
		pm = (PerkManagerTB)target;
		
		//DrawDefaultInspector();
		
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		cont=new GUIContent("Dont Destroy On Load:", "Check to use this component through out the game\nWhen checked, setting in other PerkManager wont take effect after this scene is loaded\nIntended for game where perk progress is persistant");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(230));
		pm.dontDestroyOnLoad=EditorGUILayout.Toggle(pm.dontDestroyOnLoad);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		
		cont=new GUIContent("Use Persistent Data:", "Check to make any perk progression persistant. Otherwise any progression made will be reset upon leaving the level.\nAny persistant progression can be reset using PerkEditorWindow.");
		EditorGUILayout.BeginHorizontal("");
		EditorGUILayout.LabelField(cont, GUILayout.MinWidth(230));
		pm.usePersistentData=EditorGUILayout.Toggle(pm.usePersistentData);
		EditorGUILayout.EndHorizontal();
		
		
		if(!pm.usePersistentData){
			cont=new GUIContent("Starting Point:", "The point available for player to unlock perk in this level,\nor this play through if dontDestroyOnLoad is checked");
			pm.point=EditorGUILayout.IntField(cont, pm.point);
		}
		EditorGUILayout.Space();
		
		
		cont=new GUIContent("Set All To Default", "Set all perk starting state to default\n\nAll perk will be lock/unlocked state will be carried forward from previous play through");
		if(GUILayout.Button(cont, GUILayout.MaxHeight(17))){
			for(int i=0; i<pm.localPerkList.Count; i++){
				pm.localPerkList[i].startingState=_StartingState.Default;
			}
		}
		cont=new GUIContent("Set All To Locked", "Set all perk starting state to locked\nAll perk will be locked from start, override any previous progress");
		if(GUILayout.Button("Set All To Locked", GUILayout.MaxHeight(17))){
			for(int i=0; i<pm.localPerkList.Count; i++){
				pm.localPerkList[i].startingState=_StartingState.Locked;
			}
		}
		cont=new GUIContent("Set All To Unlocked", "Set all perk starting state to unlocked\nAll perk will be unlocked from start, override any previous progress");
		if(GUILayout.Button("Set All To Unlocked", GUILayout.MaxHeight(17))){
			for(int i=0; i<pm.localPerkList.Count; i++){
				pm.localPerkList[i].startingState=_StartingState.Unlocked;
			}
		}
		
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		for(int m=0; m<pm.localPerkList.Count; m++){
			PerkTB perk=pm.localPerkList[m];
			
			GUILayout.BeginHorizontal();
					
				cont=new GUIContent(perk.icon, perk.desp);
				GUILayout.Box(cont, GUILayout.Width(35),  GUILayout.Height(35), GUILayout.MaxHeight(35));
				
				GUILayout.BeginVertical();
			
					GUILayout.BeginHorizontal();
						cont=new GUIContent(perk.name, "Check if perk is available in this scene");
						GUILayout.Label(cont, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(15));
			
						perk.availableInScene=EditorGUILayout.Toggle(perk.availableInScene, GUILayout.MaxWidth(90), GUILayout.MaxHeight(15), GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();		
			
					if(perk.availableInScene){
						int startingState=(int)perk.startingState;  
						cont=new GUIContent("StartingState:", "The starting unlocked state of the perk");
						contL=new GUIContent[startingStateLabel.Length];
						for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(startingStateLabel[i], startingStateTooltip[i]);
						startingState = EditorGUILayout.IntPopup(cont, startingState, contL, intVal, GUILayout.MaxHeight(15));
						perk.startingState=(_StartingState)startingState;
					}
					
				GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();

		}
		
		
		if(GUI.changed){
			EditorUtility.SetDirty(pm);
		}
	}
}
