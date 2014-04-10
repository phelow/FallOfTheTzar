using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


public class CollectibleEditorWindow : EditorWindow {
	
	public delegate void UpdateHandler(); 
	public static event UpdateHandler onCollectibleUpdateE;
	
	static private CollectibleEditorWindow window;
	
	private static CollectibleListPrefab prefab;
	private static List<CollectibleTB> collectibleList=new List<CollectibleTB>();
	private static string[] nameList=new string[0];
	
	private int index=0;
	
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (CollectibleEditorWindow)EditorWindow.GetWindow(typeof (CollectibleEditorWindow));
		window.minSize=new Vector2(615, 250);
		window.maxSize=new Vector2(615, 251);
		
		EditorCollectibleList eCollectibleList=CollectibleManagerWindow.LoadCollectible();
		prefab=eCollectibleList.prefab;
		collectibleList=prefab.collectibleList;
		nameList=eCollectibleList.nameList;
		
		int enumLength = Enum.GetValues(typeof(_EffectAttrType)).Length;
		effectTypeLabel=new string[enumLength];
		effectTypeTooltip=new string[enumLength];
		for(int i=0; i<enumLength; i++) effectTypeLabel[i]=((_EffectAttrType)i).ToString();
		for(int i=0; i<enumLength; i++) effectTypeTooltip[i]="";
		
		effectTypeTooltip[0]="Reduce target's HP";
		effectTypeTooltip[1]="Restore target's HP";
		effectTypeTooltip[2]="Reduce target's AP";
		effectTypeTooltip[3]="Restore target's AP";
		effectTypeTooltip[4]="Increase/decrease target's damage";
		effectTypeTooltip[5]="Increase/decrease target's movement range";
		effectTypeTooltip[6]="Increase/decrease target's attack range";
		effectTypeTooltip[7]="Increase/decrease target's speed";
		effectTypeTooltip[8]="Increase/decrease target's attack";
		effectTypeTooltip[9]="Increase/decrease target's defend";
		effectTypeTooltip[10]="Increase/decrease target's critical chance";
		effectTypeTooltip[11]="Increase/decrease target's critical immunity";
		effectTypeTooltip[12]="Increase/decrease target's attack per turn";
		effectTypeTooltip[13]="Increase/decrease target's counter attack limit ";
		effectTypeTooltip[14]="Stun target, stop target from doing anything";
		effectTypeTooltip[15]="Prevent target from attack";
		effectTypeTooltip[16]="Prevent target from moving";
		effectTypeTooltip[17]="Prevent target from using ability";
		effectTypeTooltip[18]="Faction gain points";
    }
	
	
	float startX, startY, height, spaceY;//, lW;
	float contentHeight;
	
	private Vector2 scrollPos;
	
	private GUIContent cont;
	private GUIContent[] contList;
	
	
	void OnGUI () {
		if(window==null) Init();
		
		Rect visibleRect=new Rect(0, 0, window.position.width, window.position.height);
		Rect contentRect=new Rect(0, 0, window.position.width-15, contentHeight);
		scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
		
		GUI.changed = false;
		
		if(GUI.Button(new Rect(window.position.width-130, 5, 120, 25), "CollectibleManager")){
			this.Close();
			CollectibleManagerWindow.Init();
		}
		
		startX=3;
		startY=3;
		height=15;
		spaceY=17;
			
		if(collectibleList.Count>0) {
			GUI.SetNextControlName ("CollectibleSelect");
			index = EditorGUI.Popup(new Rect(startX, startY, 300, height), "Collectible:", index, nameList);
			if(GUI.changed){
				GUI.FocusControl ("CollectibleSelect");
			}
			
			CollectibleTB collectible=collectibleList[index];
			Effect effect=collectible.effect;
			
			EditorGUI.LabelField(new Rect(340, startY, 70, height), "Icon: ");
			collectible.icon=(Texture)EditorGUI.ObjectField(new Rect(380, startY, 70, 70), collectible.icon, typeof(Texture), false);
			effect.icon=collectible.icon;
			
			
			//~ cont=new GUIContent("PointCost:", "The cost of unit");
			//~ unit.pointCost = EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, unit.pointCost);
			
			cont=new GUIContent("Name:", "The name for the unit ability");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Name: ");
			collectible.collectibleName=EditorGUI.TextField(new Rect(startX, startY+=spaceY, 300, height), cont, collectible.collectibleName);
			effect.name=collectible.collectibleName;
			
			startY+=7;
			
			//~ cont=new GUIContent("Is Buff:", "Check if the collectible gives buff effect, this is merely for GUI purpose");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			//~ effect.isBuff=EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, height), cont, effect.isBuff);
			
			cont=new GUIContent("Enable AOE:", "Check if the collectible has an area of effect (affects more than single tile)");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			collectible.enableAOE=EditorGUI.Toggle(new Rect(startX, startY+=spaceY, 300, height), cont, collectible.enableAOE);
			
			if(collectible.enableAOE){
				cont=new GUIContent("AOE Range:", "The range of aoe in term of tile");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				collectible.aoeRange=EditorGUI.IntField(new Rect(startX, startY+=spaceY, 300, height), cont, collectible.aoeRange);
			}
			else startY+=spaceY;
			
			startY+=3;
			
			if(GUI.Button(new Rect(startX, startY+=spaceY, 100, height), "add effect")){
				if(effect.effectAttrs.Count<4)
					effect.effectAttrs.Add(new EffectAttr());
				else
					Debug.Log("Cannot have more than 4 effects");
			}
			
			for(int i=0; i<effect.effectAttrs.Count; i++){
				EffectAttrConfigurator(effect, i, startX, startY);
				startX+=135;
			}
			
			startX=3;
			
			//~ startY+=150;
			//~ EditorGUI.LabelField(new Rect(startX, startY+=5, 300, 25), "Collectible Description (for runtime UI): ");
			//~ collectible.effect.desp=EditorGUI.TextArea(new Rect(startX, startY+=17, window.position.width-20, 50), collectible.effect.desp);
			//~ startY+=60;
			contentHeight=startY;
			
			
			if (GUI.changed){
				if(GUI.changed) EditorUtility.SetDirty(collectible);
				if(onCollectibleUpdateE!=null) onCollectibleUpdateE();
			}
			
		}
		else{
			EditorGUI.LabelField(new Rect(startX, startY, 450, height), "No Collectible has been assigned, please do so in the CollectibleManager");
		}

		GUI.EndScrollView();
	}
	
	static string[] effectTypeLabel=new string[0];
	static string[] effectTypeTooltip=new string[0];
	void EffectAttrConfigurator(Effect effect, int ID, float startX, float startY){
		EffectAttr effectAttr=effect.effectAttrs[ID];
		
		if(GUI.Button(new Rect(startX, startY+=18, 70, 14), "Remove")){
			effect.effectAttrs.Remove(effectAttr);
			return;
		}
		
		int type=(int)effectAttr.type;
		cont=new GUIContent("Type:", "Type of the effect");
		contList=new GUIContent[effectTypeLabel.Length];
		for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(effectTypeLabel[i], effectTypeTooltip[i]);
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		type = EditorGUI.Popup(new Rect(startX+40, startY, 80, 16), type, contList);
		effectAttr.type=(_EffectAttrType)type;
		
		
		if(effectAttr.type==_EffectAttrType.HPDamage || effectAttr.type==_EffectAttrType.HPGain || effectAttr.type==_EffectAttrType.APGain || effectAttr.type==_EffectAttrType.APDamage){
			cont=new GUIContent("ValueMin:", "Minimum value for the effect");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
			
			cont=new GUIContent("ValueMax:", "Maximum value for the effect");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			effectAttr.valueAlt=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.valueAlt);
		}
		else{
			cont=new GUIContent("ValueMax:", "Value for the effect");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
		}
		
		
		//~ if(type==1){
			//~ //cont=new GUIContent("Range:", "Effective range of the ability in term of tile");
			//~ //EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), "use Default Damage:");
			//~ //effect.useDefaultDamageValue=EditorGUI.Toggle(new Rect(startX+120, startY-1, 50, 16), effect.useDefaultDamageValue);
		//~ }
		
		//~ if(type!=2 || !effectAttr.useDefaultDamageValue){
			//~ //cont=new GUIContent("Range:", "Effective range of the ability in term of tile");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), "value:");
			//~ effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
		//~ }
		
		cont=new GUIContent("Duration:", "Effective duration of the effect in term of round");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		effectAttr.duration=EditorGUI.IntField(new Rect(startX+70, startY-1, 50, 16), effectAttr.duration);
	}
	
	
	/*
	void UAbConfigurator(){
		int startY=315;
		int startX=5;
		
		GUIStyle style=new GUIStyle();
		style.wordWrap=true;
		
		UnitAbility uAB=allUAbList[selectedUAbID];
		
		cont=new GUIContent("Default Icon:", "The icon for the unit ability");
		EditorGUI.LabelField(new Rect(startX, startY, 80, 20), cont);
		uAB.icon=(Texture)EditorGUI.ObjectField(new Rect(startX+10, startY+17, 60, 60), uAB.icon, typeof(Texture), false);
		startX+=100;
		
		cont=new GUIContent("Unavailable:", "The icon for the unit ability when it's unavailable (on cooldown and etc.)");
		EditorGUI.LabelField(new Rect(startX, startY, 80, 34), cont);
		uAB.iconUnavailable=(Texture)EditorGUI.ObjectField(new Rect(startX+10, startY+17, 60, 60), uAB.iconUnavailable, typeof(Texture), false);
		startX+=80;
		
		
		//startX+=100;
		//cont=new GUIContent("Effect:", "The prefab intend as visual effect to spawn everytime the ability is used");
		//EditorGUI.LabelField(new Rect(startX, startY, 200, 20), cont);
		//uAB.effect=(GameObject)EditorGUI.ObjectField(new Rect(startX+50, startY-1, 120, 17), uAB.effect, typeof(GameObject), false);
		
		

		startX=5;
		startY=390;
		
		cont=new GUIContent("Name:", "The name for the unit ability");
		EditorGUI.LabelField(new Rect(startX, startY+=20, 200, 20), "Name: ");
		uAB.name=EditorGUI.TextField(new Rect(startX+50, startY-1, 120, 17), uAB.name);
		
		startY+=8;
		
		int type=(int)uAB.type;
		cont=new GUIContent("Type:", "Type of the ability");
		contList=new GUIContent[abilityTypeLabel.Length];
		for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(abilityTypeLabel[i], abilityTypeTooltip[i]);
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		type = EditorGUI.Popup(new Rect(startX+70, startY, 100, 16), type, contList);
		uAB.type=(_UnitAbilityType)type;
		
		if(uAB.type!=_UnitAbilityType.SelfBuff && uAB.type!=_UnitAbilityType.SelfTile){
			cont=new GUIContent("RequireTargetSelect:", "Check if a target is required for the ability, else the ability will be casted on the unit tile");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			uAB.requireTargetSelection=EditorGUI.Toggle(new Rect(startX+155, startY-1, 20, 17), uAB.requireTargetSelection);
			
			cont=new GUIContent("enableAOE:", "");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			uAB.enableAOE=EditorGUI.Toggle(new Rect(startX+155, startY-1, 20, 17), uAB.enableAOE);
			
			if(uAB.enableAOE){
				cont=new GUIContent("aoeRange:", "Effective aoe range of the ability in term of tile, ");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				uAB.aoeRange=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.aoeRange);
				if(uAB.requireTargetSelection) uAB.aoeRange=Math.Max(0, uAB.aoeRange);
				else uAB.aoeRange=Math.Max(1, uAB.aoeRange);
			}
			
			if(uAB.requireTargetSelection){
				cont=new GUIContent("Range:", "Effective range of the ability in term of tile");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				uAB.range=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.range);
				uAB.range=Math.Max(1, uAB.range);
			}
		}
		
		cont=new GUIContent("AP Cost:", "The AP cost to use this unit ability");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		uAB.cost=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.cost);
		
		cont=new GUIContent("Cooldown:", "The cooldown (in round) required before the unit ability become available again after each use");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		uAB.cdDuration=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.cdDuration);
		
		cont=new GUIContent("Limit:", "The maximum amount of time the ability can be use in the game (set to -1 for infinite use)");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		uAB.useLimit=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.useLimit);
		
		
		startY=330;
		startX=205;
		//EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "AbilityEffect:");
		if(GUI.Button(new Rect(startX, startY, 100, 20), "Add Effect")){
			if(uAB.effectAttrs.Count<3){
				uAB.effectAttrs.Add(new EffectAttr());
			}
		}
		cont=new GUIContent("Delay:", "The delay in second before the effects take place (only applicable when the ability doesnt use a shoot mechanism)");
		EditorGUI.LabelField(new Rect(startX+135, startY, 200, 20), cont);
		uAB.effectUseDelay=EditorGUI.FloatField(new Rect(startX+180, startY-1, 50, 16), uAB.effectUseDelay);
		
		startY+=7;
		for(int i=0; i<uAB.effectAttrs.Count; i++){
			EffectConfigurator(uAB, i, startX, startY);
			startX+=135;
		}
		
		
		startY+=150;
		startX=205;
		
		cont=new GUIContent("Effect Self:", "The prefab intend as visual effect to spawn on the unit using the ability everytime the ability is used");
		EditorGUI.LabelField(new Rect(startX, startY, 200, 20), cont);
		uAB.effectUse=(GameObject)EditorGUI.ObjectField(new Rect(startX+80, startY-1, 90, 17), uAB.effectUse, typeof(GameObject), false);
		
		cont=new GUIContent("  -  Delay:", "The delay in second before the visual effect is spawned");
		EditorGUI.LabelField(new Rect(startX+170, startY, 200, 20), cont);
		uAB.effectUseDelay=EditorGUI.FloatField(new Rect(startX+235, startY-1, 50, 16), uAB.effectUseDelay);
		
		cont=new GUIContent("Effect Target:", "The prefab intend as visual effect to spawn on the unit using the ability everytime the ability is used");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		uAB.effectTarget=(GameObject)EditorGUI.ObjectField(new Rect(startX+80, startY-1, 90, 17), uAB.effectTarget, typeof(GameObject), false);
		
		cont=new GUIContent("  -  Delay:", "The delay in second before the target visual effect is spawned");
		EditorGUI.LabelField(new Rect(startX+170, startY, 200, 20), cont);
		uAB.effectTargetDelay=EditorGUI.FloatField(new Rect(startX+235, startY-1, 50, 16), uAB.effectTargetDelay);
		
		startY+=20;
		if(type!=0){
			int shootMode=(int)uAB.shootMode;
			cont=new GUIContent("ShootMode:", "Shoot object setting for the ability if applicable");
			contList=new GUIContent[shootModeLabel.Length];
			for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(shootModeLabel[i], shootModeTooltip[i]);
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			shootMode = EditorGUI.Popup(new Rect(startX+80, startY, 100, 16), shootMode, contList);
			uAB.shootMode=(_AbilityShootMode)shootMode;
			
			if(shootMode!=0){
				cont=new GUIContent("ShootObject:", "The shootObject prefab to be used");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				uAB.shootObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+80, startY-1, 100, 17), uAB.shootObject, typeof(GameObject), false);
			}
			else startY+=18;
		}
		
		startY+=10;
		cont=new GUIContent("Sound Use:", "The sound to play when the ability is used");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		uAB.soundUse=(AudioClip)EditorGUI.ObjectField(new Rect(startX+80, startY-1, 90, 17), uAB.soundUse, typeof(AudioClip), false);
		
		cont=new GUIContent("Sound Hit:", "The sound to play on the target when the ability hit it's target");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		uAB.soundHit=(AudioClip)EditorGUI.ObjectField(new Rect(startX+80, startY-1, 90, 17), uAB.soundHit, typeof(AudioClip), false);
		
		
		startX=5;
		startY=(int)window.position.height-75;
		EditorGUI.LabelField(new Rect(startX, startY, 300, 25), "Ability Description (for runtime UI): ");
		uAB.desp=EditorGUI.TextArea(new Rect(startX, startY+17, window.position.width-10, 50), uAB.desp);
	}
	
	
	void EffectConfigurator(UnitAbility uAB, int ID, int startX, int startY){
		EffectAttr effectAttr=uAB.effectAttrs[ID];
		
		if(GUI.Button(new Rect(startX, startY+=18, 70, 14), "Remove")){
			uAB.effectAttrs.Remove(effectAttr);
			return;
		}
		
		int type=(int)effectAttr.type;
		cont=new GUIContent("Type:", "Type of the effect");
		contList=new GUIContent[effectTypeLabel.Length];
		for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(effectTypeLabel[i], effectTypeTooltip[i]);
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		type = EditorGUI.Popup(new Rect(startX+40, startY, 80, 16), type, contList);
		effectAttr.type=(_AbilityEffectType)type;
		
		if(type==1){
			//cont=new GUIContent("Range:", "Effective range of the ability in term of tile");
			//EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), "use Default Damage:");
			//effect.useDefaultDamageValue=EditorGUI.Toggle(new Rect(startX+120, startY-1, 50, 16), effect.useDefaultDamageValue);
		}
		
		if(type!=2 || !effectAttr.useDefaultDamageValue){
			//cont=new GUIContent("Range:", "Effective range of the ability in term of tile");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), "value:");
			effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
		}
		
		cont=new GUIContent("Duration:", "Effective duration of the effect in term of round");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		effectAttr.duration=EditorGUI.IntField(new Rect(startX+70, startY-1, 50, 16), effectAttr.duration);
	}
	*/
	

	
	
}




