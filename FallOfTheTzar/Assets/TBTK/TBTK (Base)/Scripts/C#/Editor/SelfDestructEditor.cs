using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SelfDestruct))]
public class SelfDestructEditor : Editor {
	
	SelfDestruct sd;
	
	private static string[] modeLabel=new string[4];
	private static string[] modeTooltip=new string[4];
	int[] intVal=new int[10];
	
	void Awake(){
		InitLabel();
	}
	
	void InitLabel(){
		int enumLength = Enum.GetValues(typeof(_DelayMode)).Length;
		modeLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) modeLabel[i]=((_DelayMode)i).ToString();
		
		modeTooltip=new string[enumLength];
		modeTooltip[0]="Destroy the gameOBject according to the real time delay";
		modeTooltip[1]="Destroy the gameObject after specified number of round\n(each time a new round is commenced)";
		modeTooltip[2]="Destroy the gameObject after specified number of turns\n(each time all unit has been cycled through thier turn)";
		
		intVal=new int[10];
		for(int i=0; i<intVal.Length; i++){
			intVal[i]=i;	
		}
	}
	
	
	GUIContent cont;
	GUIContent[] contL;
	
	public override void OnInspectorGUI(){
		sd = (SelfDestruct)target;
		
		//DrawDefaultInspector();
		
		int mode=(int)sd.mode;
		cont=new GUIContent("Type:", "Delay mode used");
		contL=new GUIContent[modeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(modeLabel[i], modeTooltip[i]);
		mode = EditorGUILayout.IntPopup(cont, mode, contL, intVal);
		sd.mode=(_DelayMode)mode;
		
		if(sd.mode==_DelayMode.RealTime){
			cont=new GUIContent("Delay:", "The delay in second before the gameObject is destroyed");
			sd.delay=EditorGUILayout.FloatField(cont, sd.delay);
		}
		else if(sd.mode==_DelayMode.Round){
			cont=new GUIContent("Round:", "The number of round before the gameObject is destroyed");
			sd.round=EditorGUILayout.IntField(cont, sd.round);
		}
		else if(sd.mode==_DelayMode.Turn){
			cont=new GUIContent("Turn:", "The number of turn before the gameObject is destroyed");
			sd.turn=EditorGUILayout.IntField(cont, sd.turn);
		}
		
		if(GUI.changed){
			EditorUtility.SetDirty(sd);
		}
	}
	
	

}