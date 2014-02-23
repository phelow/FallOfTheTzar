using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(AIManager))]
public class AIManagerEditor : Editor {

	AIManager AIM;
	
	string[] AIStanceLabel=new string[0];
	string[] AIStanceTooltip=new string[0];
	
	int[] intVal=new int[3];
	
	void Awake () {
		AIM=(AIManager)target;
		
		InitLabel();
	}
	
	
	void InitLabel(){
		
		int enumLength = Enum.GetValues(typeof(_AIStance)).Length;
		AIStanceLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) AIStanceLabel[i]=((_AIStance)i).ToString();
		
		AIStanceTooltip=new string[enumLength];
		AIStanceTooltip[0]="AI faction/unit will stay dormant until hostile unit has come into range or it has been attacked/nthe unit will give up go inactive if it lost sight of hostile or when all hostile has move out of range";
		AIStanceTooltip[1]="AI faction/unit will stay dormant until hostile unit has come into range or it has been attacked/nonce activated, the unit will stay active until it's destroyed";
		AIStanceTooltip[2]="AI faction/unit will actively move around seeking out target";
		
		
		for(int i=0; i<intVal.Length; i++){
			intVal[i]=i;	
		}
		
	}
	
	GUIContent cont;
	GUIContent[] contL;
	
	public override void OnInspectorGUI(){
		AIM=(AIManager)target;
		
		//DrawDefaultInspector();
		
		EditorGUILayout.Space();
		
		
		int aIStance=(int)AIM.aIStance;  
		cont=new GUIContent("AI Stance:", "The behaviour of AI");
		contL=new GUIContent[AIStanceLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(AIStanceLabel[i], AIStanceTooltip[i]);
		aIStance = EditorGUILayout.IntPopup(cont, aIStance, contL, intVal);
		AIM.aIStance=(_AIStance)aIStance;
		
		
		if(GUI.changed){
			EditorUtility.SetDirty(AIM);
		}
	}
}
