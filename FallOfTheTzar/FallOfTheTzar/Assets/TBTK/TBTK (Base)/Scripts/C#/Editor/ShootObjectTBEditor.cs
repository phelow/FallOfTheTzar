using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ShootObjectTB))]
public class ShootObjectTBEditor : Editor {
	
	ShootObjectTB so;
	
	private static string[] typeLabel=new string[4];
	private static string[] typeTooltip=new string[4];
	int[] intVal=new int[10];
	
	void Awake(){
		InitLabel();
	}
	
	void InitLabel(){
		int enumLength = Enum.GetValues(typeof(_ShootObjectType)).Length;
		typeLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) typeLabel[i]=((_ShootObjectType)i).ToString();
		
		//public enum _ShootObjectType{Projectile, Missile, Beam, Effect}
		typeTooltip=new string[enumLength];
		typeTooltip[0]="ShootObject is an object which travel to shootPoint to targetPoint using a fixed trajectory";
		typeTooltip[1]="ShootObject travel from shootPoint to targetPoint with varied trajectory (with addition rotation in y-axis and varied speed over the trajectory)";
		typeTooltip[2]="ShootObject takes the form of a beam from shootPoint to targetPoint\nA LineRenderer component on the shootObject is required";
		typeTooltip[3]="ShootObject has no bearing on the mechanic, it's merely a for visual effect";
		
		intVal=new int[10];
		for(int i=0; i<intVal.Length; i++){
			intVal[i]=i;	
		}
	}
	
	
	GUIContent cont;
	GUIContent[] contL;
	
	public override void OnInspectorGUI(){
		so = (ShootObjectTB)target;
		
		//DrawDefaultInspector();
		
		int type=(int)so.type;
		cont=new GUIContent("Type:", "Type of the ShootObject");
		contL=new GUIContent[typeLabel.Length];
		for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(typeLabel[i], typeTooltip[i]);
		type = EditorGUILayout.IntPopup(cont, type, contL, intVal);
		so.type=(_ShootObjectType)type;
		
		if(so.type==_ShootObjectType.Projectile){
			cont=new GUIContent("Speed:", "The travel speed for the projectile");
			so.speed=EditorGUILayout.FloatField(cont, so.speed);
			
			cont=new GUIContent("Max Shoot Angle:", "The maximum elevation angle for the shootObject's trajectory in x-axis\nActual value is based on the range of the target");
			so.maxShootAngle=EditorGUILayout.FloatField(cont, so.maxShootAngle);
			
			cont=new GUIContent("Max Shoot Range:", "The maximum range intended for this shootObject\nHas no actual effect on game mechanic, it's just used to factor the elevation of the trajectory based on the MaxShootAngle\nIf the target range is beyond this range, maximum angle will be used\nOtherwise the elevation angle will be adjusted porportionally ");
			so.maxShootRange=EditorGUILayout.FloatField(cont, so.maxShootRange);
		}
		else if(so.type==_ShootObjectType.Missile){
			cont=new GUIContent("Speed:", "The travel speed for the missile");
			so.speed=EditorGUILayout.FloatField(cont, so.speed);
			
			cont=new GUIContent("Shoot Angle X:", "The maximum elevation angle for the shootObject's trajectory in x-axis\nActual value is randomized from zero to this value.");
			so.maxShootAngle=EditorGUILayout.FloatField(cont, so.maxShootAngle);
			
			cont=new GUIContent("Shoot Angle Y:", "The maximum deviation angle in Y-axis of the trajectory\nActual value is randomized from -ve to +ve of this value.");
			so.shootAngleY=EditorGUILayout.FloatField(cont, so.shootAngleY);
			
			cont=new GUIContent("Initial Speed Boost:", "Give the missile a speed boost during the initial phase of the trajectory");
			so.missileInitialBoost=EditorGUILayout.Toggle(cont, so.missileInitialBoost);
		}
		else if(so.type==_ShootObjectType.Beam){
			cont=new GUIContent("Beam Duration:", "The active duration of the beam");
			so.beamDuration=EditorGUILayout.FloatField(cont, so.beamDuration);
			
			cont=new GUIContent("Beam Length:", "The length of the beam\nThis will never exist the actual range from the attacking unit to the target\nSet it to infinity and the beam will always ends at target position");
			so.beamLength=EditorGUILayout.FloatField(cont, so.beamLength);
			
			cont=new GUIContent("LineRenderer:", "The LineRenderer component for this beam shootObject\nMust be within the hierarchy of this prefab");
			so.lineRenderer=(LineRenderer)EditorGUILayout.ObjectField(cont, so.lineRenderer, typeof(LineRenderer), false);
		}
		else if(so.type==_ShootObjectType.Effect){
			cont=new GUIContent("Effect Duration:", "The duration of the effects to appear on screen in seconds");
			so.effectDuration=EditorGUILayout.FloatField(cont, so.effectDuration);
		}
		
		cont=new GUIContent("Shoot Effect:", "The gameObject (optional) to be spawned at shootPoint as visual effect");
		so.shootEffect=(GameObject)EditorGUILayout.ObjectField(cont, so.shootEffect, typeof(GameObject), false);
		
		cont=new GUIContent("Hit Effect:", "The gameObject (optional) to be spawned at hitPoint as visual effect");
		so.hitEffect=(GameObject)EditorGUILayout.ObjectField(cont, so.hitEffect, typeof(GameObject), false);
		
		cont=new GUIContent("showHitEffectWhenMissed:", "Check to wpawn hit effect regardless of the attack result\nOtherwise it only spwan when the attack hits.");
		so.showHitEffectWhenMissed=EditorGUILayout.Toggle(cont, so.showHitEffectWhenMissed);
		
		
		if(GUI.changed){
			EditorUtility.SetDirty(so);
		}
	}
	
	

}