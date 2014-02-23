using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(UISetup))]
public class UISetupEditor : Editor {

	UISetup uiSetup;
	
	string[] loadModeLabel=new string[0];
	string[] loadModeTooltip=new string[0];
	
	int[] intVal=new int[10];
	
	void Awake () {
		uiSetup=(UISetup)target;
		
		InitLabel();
	}
	
	
	void InitLabel(){
		
		int enumLength = Enum.GetValues(typeof(_LoadMode)).Length;
		loadModeLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) loadModeLabel[i]=((_LoadMode)i).ToString();
		
		loadModeTooltip=new string[enumLength];
		loadModeTooltip[0]="Use data carried from previous progress\nUpon complete this level, the data will be saved and carry to subsequent scene";
		loadModeTooltip[1]="Use temporary data set in any previous scene, if there's any\nThis data will not be carried forward.";
		loadModeTooltip[2]="Use data set in UnitControl\nThis data will not be carried forward.";
		
		for(int i=0; i<intVal.Length; i++){
			intVal[i]=i;	
		}
		
	}
	
	GUIContent cont;
	GUIContent[] contL;
	
	public override void OnInspectorGUI(){
		uiSetup = (UISetup)target;
		
		//DrawDefaultInspector();
		
		EditorGUILayout.Space();
		
		int loadMode=(int)uiSetup.loadMode;  
		cont=new GUIContent("Data Load Mode:", "Data loading mode to be used in this scene");
		contL=new GUIContent[loadModeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(loadModeLabel[i], loadModeTooltip[i]);
		loadMode = EditorGUILayout.IntPopup(cont, loadMode, contL, intVal);
		uiSetup.loadMode=(_LoadMode)loadMode;
		
		EditorGUILayout.Space();
		
		if(uiSetup.loadMode!=_LoadMode.UsePersistantData){
			cont=new GUIContent("Starting Point:", "points available for the player to purchase unit and perk");
			uiSetup.startingPlayerPoint=EditorGUILayout.IntField(cont, uiSetup.startingPlayerPoint);
		}
		
		
		
		EditorGUILayout.Space();
		
		
		cont=new GUIContent("Main Menu:", "the name of the main menu scene");
		uiSetup.mainMenu=EditorGUILayout.TextField(cont, uiSetup.mainMenu);
		
		EditorGUILayout.Space();
		
		cont=new GUIContent("Random Next Scene:", "Check to randomise which scene to load next");
		uiSetup.randomNextScene=EditorGUILayout.Toggle(cont, uiSetup.randomNextScene);
		if(!uiSetup.randomNextScene){
			cont=new GUIContent("Next Scene:", "the name of next scene to be load");
			uiSetup.nextScene=EditorGUILayout.TextField(cont, uiSetup.nextScene);
		}
		else{
			int count=uiSetup.nextScenes.Count;
			cont=new GUIContent("Next Scenes Count:", "the number of potential next scenes to be load");
			count=EditorGUILayout.IntField(cont, count);
			if(count!=uiSetup.nextScenes.Count){
				if(uiSetup.nextScenes.Count<count){
					for(int i=uiSetup.nextScenes.Count; i<count; i++){
						uiSetup.nextScenes.Add("");
					}
				}
				else{
					List<string> newList=new List<string>();
					for(int i=0; i<count; i++){
						newList.Add(uiSetup.nextScenes[i]);
					}
					uiSetup.nextScenes=newList;
				}
			}
			for(int i=0; i<uiSetup.nextScenes.Count; i++){
				uiSetup.nextScenes[i]=EditorGUILayout.TextField("                          - ", uiSetup.nextScenes[i]);
			}
		}
		
		
		
		
		if(GUI.changed){
			EditorUtility.SetDirty(uiSetup);
		}
	}
}
