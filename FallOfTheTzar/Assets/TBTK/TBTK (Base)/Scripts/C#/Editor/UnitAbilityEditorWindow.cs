using UnityEngine;
using UnityEditor;

using System;
using System.Xml;
using System.IO;

using System.Collections;
using System.Collections.Generic;


public class UnitAbilityEditorWindow : EditorWindow {
	
	public delegate void UpdateHandler(); 
	public static event UpdateHandler onUnitAbilityUpdateE;
	
	static private UnitAbilityEditorWindow window;
	
	static string[] abilityTargetAreaLabel=new string[3];
	static string[] abilityEffectTypeLabel=new string[4];
	static string[] abilityTargetTypeLabel=new string[4];
	static string[] effectAttrTypeLabel=new string[4];
	static string[] shootModeLabel=new string[4];
	
	static string[] abilityTargetAreaTooltip=new string[3];
	static string[] abilityEffectTypeTooltip=new string[4];
	static string[] abilityTargetTypeTooltip=new string[4];
	static string[] effectAttrTypeTooltip=new string[4];
	static string[] shootModeTooltip=new string[4];
	
	
	static string[] damageList=new string[0];
	static string[] armorList=new string[0];
	static string[] damageTooltipList=new string[0];
	static string[] armorTooltipList=new string[0];
	
	
    // Add menu named "PerkEditor" to the Window menu
    //[MenuItem ("TDTK/PerkEditor")]
    public static void Init () {
        // Get existing open window or if none, make a new one:
        window = (UnitAbilityEditorWindow)EditorWindow.GetWindow(typeof (UnitAbilityEditorWindow));
		window.minSize=new Vector2(615, 600);
		window.maxSize=new Vector2(615, 801);
		
		Load();
		
		LoadDamageArmor();
		
		InitLabel();
    }
	
	static void InitLabel(){
		int enumLength = Enum.GetValues(typeof(_TargetArea)).Length;
		abilityTargetAreaLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) abilityTargetAreaLabel[i]=((_TargetArea)i).ToString();
		
		abilityTargetAreaTooltip=new string[enumLength];
		abilityTargetAreaTooltip[0]="Any tile within range, when AOE is enabled, surrounding tile  of the selected tile is included";
		abilityTargetAreaTooltip[1]="A a striaight line of tiles extend from the tile occupied by source unit";
		abilityTargetAreaTooltip[2]="A conical area extend from the tile occupied by source unit\nNot applicable for SquareTile, Line will be used instead";
		
		
		enumLength = Enum.GetValues(typeof(_EffectType)).Length;
		abilityEffectTypeLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) abilityEffectTypeLabel[i]=((_EffectType)i).ToString();
		
		abilityEffectTypeTooltip=new string[enumLength];
		abilityEffectTypeTooltip[0]="for instant effect (doesnt persist through round) that target friendly units";
		abilityEffectTypeTooltip[1]="for instant effect (doesnt persist through round) that target hostile units";
		abilityEffectTypeTooltip[2]="for buff effect (persist through round) that target friendly units";
		abilityEffectTypeTooltip[3]="for debuff effect (persist through round) that target hostile units";
		
		
		enumLength = Enum.GetValues(typeof(_AbilityTargetType)).Length;
		abilityTargetTypeLabel=new string[enumLength];
		for(int i=0; i<enumLength; i++) abilityTargetTypeLabel[i]=((_AbilityTargetType)i).ToString();
		
		abilityTargetTypeTooltip=new string[enumLength];
		abilityTargetTypeTooltip[0]="Target all unit, both friendly or hostile";
		abilityTargetTypeTooltip[1]="Target friendly unit";
		abilityTargetTypeTooltip[2]="Target hostile unit";
		abilityTargetTypeTooltip[3]="Target tiles only";
		
		//AllUnits, Friendly, Hostile, Tile
		//~ abilityTargetTypeTooltip=new string[enumLength];
		//~ abilityTargetTypeTooltip[0]="Target effect to self";
		//~ abilityTargetTypeTooltip[1]="Target effect to the occupied tile";
		//~ abilityTargetTypeTooltip[2]="Target effect to friendly unit";
		//~ abilityTargetTypeTooltip[3]="Target effect to hostile unit";
		//~ abilityTargetTypeTooltip[4]="Target effect to any tile";
		
		
		
		enumLength = Enum.GetValues(typeof(_EffectAttrType)).Length;
		effectAttrTypeLabel=new string[enumLength];
		effectAttrTypeTooltip=new string[enumLength];
		for(int i=0; i<enumLength; i++) effectAttrTypeLabel[i]=((_EffectAttrType)i).ToString();
		//~ for(int i=0; i<enumLength; i++) effectAttrTypeTooltip[i]="";
		
		effectAttrTypeTooltip[0]="Reduce target's HP";
		effectAttrTypeTooltip[1]="Restore target's HP";
		effectAttrTypeTooltip[2]="Reduce target's AP";
		effectAttrTypeTooltip[3]="Restore target's AP";
		effectAttrTypeTooltip[4]="Increase/decrease target's damage";
		effectAttrTypeTooltip[5]="Increase/decrease target's movement range";
		effectAttrTypeTooltip[6]="Increase/decrease target's attack range";
		effectAttrTypeTooltip[7]="Increase/decrease target's speed";
		effectAttrTypeTooltip[8]="Increase/decrease target's hit chance";
		effectAttrTypeTooltip[9]="Increase/decrease target's dodge chance";
		effectAttrTypeTooltip[10]="Increase/decrease target's critical chance";
		effectAttrTypeTooltip[11]="Increase/decrease target's critical immunity";
		effectAttrTypeTooltip[12]="Increase/decrease target's attack per turn";
		effectAttrTypeTooltip[13]="Increase/decrease target's counter attack limit ";
		effectAttrTypeTooltip[14]="Stun target, stop target from doing anything";
		effectAttrTypeTooltip[15]="Prevent target from attack\nTakes binary value: 0-no effect. >0-cannot attack";
		effectAttrTypeTooltip[16]="Prevent target from moving\nTakes binary value: 0-no effect. >0-cannot move";
		effectAttrTypeTooltip[17]="Prevent target from using ability\nTakes binary value: 0-no effect. >0-cannot use ability";
		effectAttrTypeTooltip[18]="Faction gain points\nFor data currency used to purchase unit";
		effectAttrTypeTooltip[19]="Teleport to a specified tile";
		effectAttrTypeTooltip[20]="Spawn a unit";
		effectAttrTypeTooltip[21]="Convert the target unit to the faction of the casting unit";
		effectAttrTypeTooltip[22]="Spawn a collectible";

		
		//~ enumLength = Enum.GetValues(typeof(_AbilityEffectType)).Length;
		//~ effectTypeLabel=new string[enumLength];
		//~ effectTypeLabel[0]="HPGain";
		//~ effectTypeLabel[1]="HPDamage";
		//~ effectTypeLabel[2]="Damage";
		//~ effectTypeLabel[3]="Attack";
		//~ effectTypeLabel[4]="Defend";
		//~ effectTypeLabel[5]="Critical";
		//~ effectTypeLabel[6]="CritDef";
		//~ effectTypeLabel[7]="Movement";
		//~ effectTypeLabel[8]="ExtraAttack";
		//~ effectTypeLabel[9]="Stun";
		
		//~ effectTypeTooltip=new string[enumLength];
		//~ effectTypeTooltip[0]="HPGain";
		//~ effectTypeTooltip[1]="HPDamage";
		//~ effectTypeTooltip[2]="modify damage";
		//~ effectTypeTooltip[3]="modify attack";
		//~ effectTypeTooltip[4]="modify defend";
		//~ effectTypeTooltip[5]="modify critical";
		//~ effectTypeTooltip[6]="modify critical immunity";
		//~ effectTypeTooltip[7]="modify movement";
		//~ effectTypeTooltip[8]="modify attack per turn";
		//~ effectTypeTooltip[9]="modify stun";
		
		
		enumLength = Enum.GetValues(typeof(_AbilityShootMode)).Length;
		shootModeLabel=new string[enumLength];
		shootModeLabel[0]="None";
		shootModeLabel[1]="ShootToCenter";
		shootModeLabel[2]="ShootToAll";
		
		shootModeTooltip=new string[enumLength];
		shootModeTooltip[0]="No ShootObject will be fire at target tile";
		shootModeTooltip[1]="A single shootObject will be fired at the center of all target tile(s)";
		shootModeTooltip[2]="shootObjects will be fired to all target tile(s)";
	}
	
	
	public static void Load(){
		GameObject obj=Resources.Load("PrefabList/UnitAbilityListPrefab", typeof(GameObject)) as GameObject;
		if(obj==null) obj=CreatePrefab();
		
		prefab=obj.GetComponent<UnitAbilityListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<UnitAbilityListPrefab>();
		
		allUAbList=prefab.unitAbilityList;
		foreach(UnitAbility uAB in allUAbList){
			UAbIDList.Add(uAB.ID);
		}
	}
	
	public static GameObject CreatePrefab(){
		GameObject obj=new GameObject();
		obj.AddComponent<UnitAbilityListPrefab>();
		GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/PrefabList/UnitAbilityListPrefab.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
		DestroyImmediate(obj);
		AssetDatabase.Refresh ();
		return prefab;
	}
	
	private static void LoadDamageArmor(){
		GameObject obj=Resources.Load("PrefabList/DamageArmorList", typeof(GameObject)) as GameObject;
		
		if(obj==null) return;
		
		DamageArmorListPrefab prefab=obj.GetComponent<DamageArmorListPrefab>();
		if(prefab==null) prefab=obj.AddComponent<DamageArmorListPrefab>();
		
		armorList=new string[prefab.armorList.Count];
		armorTooltipList=new string[prefab.armorList.Count];
		damageList=new string[prefab.damageList.Count];
		damageTooltipList=new string[prefab.damageList.Count];
		
		for(int i=0; i<prefab.armorList.Count; i++){
			armorList[i]=(prefab.armorList[i].name);
			armorTooltipList[i]=prefab.armorList[i].desp;
		}
		for(int i=0; i<prefab.damageList.Count; i++){
			damageList[i]=prefab.damageList[i].name;
			damageTooltipList[i]=prefab.damageList[i].desp;
		}
	}
	
	
	private Vector2 mainScrollPos;
	
	private int deleteID=-1;
	private int selectedUAbID=-1;
	private int swapID=-1;
	private Vector2 scrollPos;
	
	private static UnitAbilityListPrefab prefab;
	private static List<UnitAbility> allUAbList=new List<UnitAbility>();
	private static List<int> UAbIDList=new List<int>();
	
	
	//~ GUIContent cont;
	private GUIContent cont;
	private GUIContent[] contList;
	
	void NewUnitAbilityRoutine(UnitAbility UAb){
		UAb.ID=GenerateNewID();
		UAbIDList.Add(UAb.ID);
		UAb.name="UnitAbility "+allUAbList.Count;
		allUAbList.Add(UAb);
		
		selectedUAbID=allUAbList.Count-1;
		deleteID=-1;
	}
	
	void OnGUI () {
		if(window==null) Init();
		
		//if(GUI.Button(new Rect(window.position.width-110, 10, 100, 30), "Save")){
		//	SaveToXML();
		//}
		
		int currentAbilityCount=allUAbList.Count;
		
		cont=new GUIContent("New UnitAbility", "Create a new unit ability");
		if(GUI.Button(new Rect(5, 10, 100, 30), cont)){
			UnitAbility UAb=new UnitAbility();
			NewUnitAbilityRoutine(UAb);
			if(onUnitAbilityUpdateE!=null) onUnitAbilityUpdateE();
		}
		cont=new GUIContent("Clone UnitAbility", "Create a new ability by cloning the current selected unit ability");
		if(selectedUAbID>=0 && selectedUAbID<allUAbList.Count){
			if(GUI.Button(new Rect(115, 10, 100, 30), cont)){
				UnitAbility UAb=allUAbList[selectedUAbID].Clone();
				NewUnitAbilityRoutine(UAb);
				if(onUnitAbilityUpdateE!=null) onUnitAbilityUpdateE();
			}
		}
		
		
		//~ Rect visibleRect=;
		//~ Rect contentRect=
		GUI.Box(new Rect(5, 50, window.position.width-10, 260), "");
		scrollPos = GUI.BeginScrollView(new Rect(5, 55, window.position.width-12, 250), scrollPos, new Rect(5, 50, window.position.width-40, 10+((allUAbList.Count-1)/3)*35));
		
		int row=0;
		int column=0;
		for(int i=0; i<allUAbList.Count; i++){
			GUIStyle style=GUI.skin.label;
			style.alignment=TextAnchor.MiddleCenter;
			if(swapID==i) GUI.color=new Color(.9f, .9f, .0f, 1);
			else GUI.color=new Color(.8f, .8f, .8f, 1);
			GUI.Box(new Rect(10+column*210, 50+row*30, 25, 25), "");
			if(GUI.Button(new Rect(10+column*210, 50+row*30, 25, 25), "")){
				//~ deleteID=-1;
				//~ if(swapID==i) swapID=-1;
				//~ else if(swapID==-1) swapID=i;
				//~ else{
					//~ SwapUAbInList(swapID, i);
					//~ swapID=-1;
				//~ }
			}
			GUI.Label(new Rect(8+column*210, 50+row*30, 25, 25), allUAbList[i].ID.ToString(), style);
			GUI.color=Color.white;
			style.alignment=TextAnchor.MiddleLeft;
			
			int ID=allUAbList[i].ID;
			if(selectedUAbID>=0 && allUAbList[selectedUAbID].chainedAbilityIDList.Contains(ID)) GUI.color=new Color(0, 1f, 1f, 1f);
			else if(selectedUAbID==i) GUI.color = Color.green;
			style=GUI.skin.button;
			style.fontStyle=FontStyle.Bold;
			
			GUI.SetNextControlName ("AbilityButton");
			if(GUI.Button(new Rect(10+27+column*210, 50+row*30, 100, 25), allUAbList[i].name, style)){
				GUI.FocusControl ("AbilityButton");
				selectedUAbID=i;
				deleteID=-1;
			}
			GUI.color = Color.white;
			style.fontStyle=FontStyle.Normal;
			
			if(deleteID!=i){
				if(GUI.Button(new Rect(10+27+102+column*210, 50+row*30, 25, 25), "X")){
					deleteID=i;
				}
			}
			else{
				GUI.color = Color.red;
				if(GUI.Button(new Rect(10+27+102+column*210, 50+row*30, 55, 25), "Delete")){
					UAbIDList.Remove(allUAbList[i].ID);
					allUAbList.RemoveAt(i);
					if(i<=selectedUAbID) selectedUAbID-=1;
					deleteID=-1;
					if(onUnitAbilityUpdateE!=null) onUnitAbilityUpdateE();
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
		
		if(selectedUAbID>-1 && selectedUAbID<allUAbList.Count){
			UAbConfigurator();
		}

		if (GUI.changed || currentAbilityCount!=allUAbList.Count){
			prefab.unitAbilityList=allUAbList;
			EditorUtility.SetDirty(prefab);
			if(onUnitAbilityUpdateE!=null) onUnitAbilityUpdateE();
		}
	}
	
	
	void UAbConfigurator(){
		Rect displayRect=new Rect(5, 315, window.position.width-5, window.position.height-315);
		Rect contentRect=new Rect(5, 315, window.position.width-40, 485);
		mainScrollPos = GUI.BeginScrollView(displayRect, mainScrollPos, contentRect);
		
		int startY=315;
		int startX=5;
		
		GUIStyle style=new GUIStyle();
		style.wordWrap=true;
		
		UnitAbility uAB=allUAbList[selectedUAbID];
		
		//~ GUI.Box(new Rect(startX, startY, 80, 100), "");
		
		cont=new GUIContent("Default Icon:", "The icon for the unit ability");
		EditorGUI.LabelField(new Rect(startX, startY, 80, 20), cont);
		uAB.icon=(Texture)EditorGUI.ObjectField(new Rect(startX+10, startY+17, 60, 60), uAB.icon, typeof(Texture), false);
		startX+=100;
		
		cont=new GUIContent("Unavailable:", "The icon for the unit ability when it's unavailable (on cooldown and etc.)");
		EditorGUI.LabelField(new Rect(startX, startY, 80, 34), cont);
		uAB.iconUnavailable=(Texture)EditorGUI.ObjectField(new Rect(startX+10, startY+17, 60, 60), uAB.iconUnavailable, typeof(Texture), false);
		startX+=80;
		
		
		if(uAB.icon!=null && uAB.icon.name!=uAB.iconName){
			uAB.iconName=uAB.icon.name;
			GUI.changed=true;
		}
		if(uAB.iconUnavailable!=null && uAB.iconUnavailable.name!=uAB.iconUnavailableName){
			uAB.iconUnavailableName=uAB.iconUnavailable.name;
			GUI.changed=true;
		}
		
		

		startX=5;
		startY=390;
		
		cont=new GUIContent("Name:", "The name for the unit ability");
		EditorGUI.LabelField(new Rect(startX, startY+=20, 80, 20), "Name: ");
		uAB.name=EditorGUI.TextField(new Rect(startX+50, startY-1, 120, 17), uAB.name);
		
		startY+=8;
		
		
		int type=(int)uAB.effectType;
		cont=new GUIContent("EffectType:", "Effect type of the ability. Set to Buff/Debuff type will enable duration setting on the effect. It will also enable buff/debuff indicator on the unit-overlay");
		contList=new GUIContent[abilityEffectTypeLabel.Length];
		for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(abilityEffectTypeLabel[i], abilityEffectTypeTooltip[i]);
		EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
		type = EditorGUI.Popup(new Rect(startX+80, startY, 90, 16), type, contList);
		uAB.effectType=(_EffectType)type;
		
		type=(int)uAB.targetType;
		cont=new GUIContent("TargetType:", "Target type of the ability");
		contList=new GUIContent[abilityTargetTypeLabel.Length];
		for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(abilityTargetTypeLabel[i], abilityTargetTypeTooltip[i]);
		EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
		type = EditorGUI.Popup(new Rect(startX+80, startY, 90, 16), type, contList);
		uAB.targetType=(_AbilityTargetType)type;
		
		startY+=8;
		
		//~ if(uAB.targetType!=_AbilityTargetType.Self && uAB.targetType!=_AbilityTargetType.SelfTile){
			cont=new GUIContent("RequireTargetSelect:", "Check if a target is required for the ability, else the ability will be casted on the unit tile");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			uAB.requireTargetSelection=EditorGUI.Toggle(new Rect(startX+155, startY-1, 20, 17), uAB.requireTargetSelection);
			
			if(uAB.requireTargetSelection){
				int areaType=(int)uAB.targetArea;
				cont=new GUIContent("TargetArea:", "Target area of the ability");
				contList=new GUIContent[abilityTargetAreaLabel.Length];
				for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(abilityTargetAreaLabel[i], abilityTargetAreaTooltip[i]);
				EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
				areaType = EditorGUI.Popup(new Rect(startX+80, startY, 90, 16), areaType, contList);
				uAB.targetArea=(_TargetArea)areaType;
				
				
				cont=new GUIContent("Range:", "Effective range of the ability in term of tile");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
				uAB.range=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.range);
				uAB.range=Math.Max(1, uAB.range);
			}
			
			//cont=new GUIContent("enableAOE:", "");
			//EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			//uAB.enableAOE=EditorGUI.Toggle(new Rect(startX+155, startY-1, 20, 17), uAB.enableAOE);
			
			//if target hostile and no target selection, ability must have aoe capability otherwise it doesnt do anything
			//if(uAB.targetType==_AbilityTargetType.Hostile && !uAB.requireTargetSelection) uAB.enableAOE=true;
			
			//if(uAB.enableAOE){
			if(!uAB.requireTargetSelection || uAB.targetArea==_TargetArea.Default){
				cont=new GUIContent("aoeRange:", "Effective aoe range of the ability in term of tile");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
				uAB.aoeRange=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.aoeRange);
				if(uAB.requireTargetSelection) uAB.aoeRange=Math.Max(0, uAB.aoeRange);
				else uAB.aoeRange=Math.Max(0, uAB.aoeRange);
			}
			
			//~ int aoeType=(int)uAB.aoe;
			//~ cont=new GUIContent("AOE Mode:", "Switch between different AOE options");
			//~ contList=new GUIContent[aoeTypeLabel.Length];
			//~ for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(aoeTypeLabel[i], aoeTypeTooltip[i]);
			//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			//~ aoeType = EditorGUI.Popup(new Rect(startX+70, startY, 100, 16), aoeType, contList);
			//~ uAB.aoe=(_AOETypeHex)aoeType;
			
			
		//~ }
		
		//~ cont=new GUIContent("Duration:", "Effective duration of the ability in term of round");
		//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		//~ uAB.duration=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.duration);
		
		startY+=8;
		
		uAB.totalCost=uAB.cost;
		for(int i=0; i<uAB.chainedAbilityIDList.Count; i++){
			for(int n=0; n<allUAbList.Count; n++){
				if(allUAbList[n].ID==uAB.chainedAbilityIDList[i]){
					uAB.totalCost+=allUAbList[n].cost;
				}
			}
		}
		
		cont=new GUIContent("AP Cost:", "The AP cost to use this unit ability\nvalue in bracket being the total cost required (taking acount of the chained abilities)");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
		EditorGUI.LabelField(new Rect(startX+90, startY, 50, 20), "("+uAB.totalCost+")");
		uAB.cost=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.cost);
		
		cont=new GUIContent("Cooldown:", "The cooldown (in round) required before the unit ability become available again after each use");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
		uAB.cdDuration=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.cdDuration);
		
		cont=new GUIContent("Limit:", "The maximum amount of time the ability can be use in the game (set to -1 for infinite use)");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
		uAB.useLimit=EditorGUI.IntField(new Rect(startX+120, startY-1, 50, 16), uAB.useLimit);
		
		
		cont=new GUIContent("enableMovementAfter:", "check to allow unit to move after using the ability (only if unit hasnt moved already)");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 140, 20), cont);
		uAB.enableMovementAfter=EditorGUI.Toggle(new Rect(startX+155, startY-1, 20, 17), uAB.enableMovementAfter);
		
		cont=new GUIContent("enableAttackAfter:", "check to allow unit to attack after using the ability (only if unit hasnt attacked already)");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 140, 20), cont);
		uAB.enableAttackAfter=EditorGUI.Toggle(new Rect(startX+155, startY-1, 20, 17), uAB.enableAttackAfter);
		
		startY+=8;
		
		
		cont=new GUIContent("Can Fail:", "Check if the ability can fail to activate\nwhen enabled, takes value from 0-1");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
		uAB.canFail=EditorGUI.Toggle(new Rect(startX+100, startY-1, 15, 16), uAB.canFail);
		if(uAB.canFail) uAB.failChance=EditorGUI.FloatField(new Rect(startX+120, startY-1, 50, 16), uAB.failChance);
		
		cont=new GUIContent("Can Miss:", "Check if the ability can miss it's target\nwhen enabled, takes value from 0-1");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 80, 20), cont);
		uAB.canMiss=EditorGUI.Toggle(new Rect(startX+100, startY-1, 15, 16), uAB.canMiss);
		if(uAB.canMiss) uAB.missChance=EditorGUI.FloatField(new Rect(startX+120, startY-1, 50, 16), uAB.missChance);
		
		if(uAB.canMiss){
			cont=new GUIContent("  - Stack with dodge :", "Check to stack the ability miss chance with target's dodge chance");
			EditorGUI.LabelField(new Rect(startX, startY+=16, 156, 20), cont);
			uAB.stackMissWithDodge=EditorGUI.Toggle(new Rect(startX+157, startY-1, 50, 16), uAB.stackMissWithDodge);
		}
		
		
		startY=320;
		startX=205;
		//~ EditorGUI.LabelField(new Rect(startX, startY, 200, 20), "AbilityEffect:");
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
			EffectAttrConfigurator(uAB, i, startX, startY);
			startX+=135;
		}
		
		
		startY+=155;
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
		
		startY+=15;
		if(uAB.requireTargetSelection){
			int shootMode=(int)uAB.shootMode;
			cont=new GUIContent("ShootMode:", "Shoot object setting for the ability if applicable");
			contList=new GUIContent[shootModeLabel.Length];
			for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(shootModeLabel[i], shootModeTooltip[i]);
			EditorGUI.LabelField(new Rect(startX, startY+=18, 180, 20), cont);
			shootMode = EditorGUI.Popup(new Rect(startX+80, startY, 100, 16), shootMode, contList);
			uAB.shootMode=(_AbilityShootMode)shootMode;
			
			if(shootMode!=0){
				cont=new GUIContent("ShootObject:", "The shootObject prefab to be used");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 180, 20), cont);
				uAB.shootObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+80, startY-1, 100, 17), uAB.shootObject, typeof(GameObject), false);
			}
			else startY+=18;
		}
		else{
			cont=new GUIContent("ShootMode: None", "Shoot object setting for the ability if applicable");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 180, 20), cont);
			startY+=18;
		}
		
		//startY+=10;
		
		startX+=210;
		startY-=36;
		cont=new GUIContent("Sound Use:", "The sound to play when the ability is used");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 180, 20), cont);
		uAB.soundUse=(AudioClip)EditorGUI.ObjectField(new Rect(startX+80, startY-1, 100, 17), uAB.soundUse, typeof(AudioClip), false);
		
		cont=new GUIContent("Sound Hit:", "The sound to play on the target when the ability hit it's target");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 180, 20), cont);
		uAB.soundHit=(AudioClip)EditorGUI.ObjectField(new Rect(startX+80, startY-1, 100, 17), uAB.soundHit, typeof(AudioClip), false);
		
		
		startX-=210;
		startY+=10;
		cont=new GUIContent("Chained-Ability:", "The ability that will be used along with the current one");
		EditorGUI.LabelField(new Rect(startX, startY+=18, 150, 20), cont);
		//~ EditorGUI.LabelField(new Rect(startX, startY, 200, 20), cont);
		
		string label="Expand";
		if(expandChainedAbilityPanel) label="Minimise";
		if(GUI.Button(new Rect(startX+98, startY, 70, 17), label)){
			expandChainedAbilityPanel=!expandChainedAbilityPanel;
		}
		startY+=20;
		
		int count=0;
		if(!expandChainedAbilityPanel){
			foreach(int abID in uAB.chainedAbilityIDList){
				foreach(UnitAbility unitAB in allUAbList){
					if(unitAB.ID==abID){
						GUI.Box(new Rect(startX+count*60, startY, 50, 50), "");
						cont=new GUIContent(unitAB.icon, unitAB.name+" - "+unitAB.desp);
						EditorGUI.LabelField(new Rect(startX+count*60+3, startY+4, 44, 44), cont);
						count+=1;
					}
				}
			}
			
			startY+=65;
		}
		else{
			Rect abilityBoxRect=new Rect(startX, startY, 390, 130);
			Rect abilityBoxcontentRect=new Rect(startX-2, startY-2, 390-20, Mathf.Ceil((allUAbList.Count/6)+1)*60);
			GUI.Box(abilityBoxRect, "");
			
			scrollPosAbilBox = GUI.BeginScrollView(abilityBoxRect, scrollPosAbilBox, abilityBoxcontentRect);
			
			float startXX=startX+8; 
			float startYY=startY+8;
			
			foreach(UnitAbility unitAB in allUAbList){
				if(uAB.chainedAbilityIDList.Contains(unitAB.ID)) GUI.color=Color.green;
				GUI.Box(new Rect(startXX+count*60, startYY, 50, 50), "");
				GUI.color=Color.white;
				
				if(uAB.ID==unitAB.ID){
					cont=new GUIContent(unitAB.iconUnavailable, unitAB.name+" - "+unitAB.desp);
					GUI.Label(new Rect(startXX+count*60+4, startYY+4, 42, 42), cont);
				}
				else{
					cont=new GUIContent(unitAB.icon, unitAB.name+" - "+unitAB.desp);
					if(GUI.Button(new Rect(startXX+count*60+4, startYY+4, 42, 42), cont)){
						if(!uAB.chainedAbilityIDList.Contains(unitAB.ID)){
							if(uAB.chainedAbilityIDList.Count<6){
								uAB.chainedAbilityIDList.Add(unitAB.ID);
							}
							else Debug.Log("cannot have more than 6 chained-abilities");
						}
						else uAB.chainedAbilityIDList.Remove(unitAB.ID);
					}
				}
				
				count+=1;
				if(count%6==0){
					startXX-=6*60;
					startYY+=60;
				}
			}
			
			GUI.EndScrollView();
			
			startY+=220;
		}
		
		
		
		startX=5;
		//startY=Mathf.Max((int)window.position.height-75, startY);
		startY=725;
		EditorGUI.LabelField(new Rect(startX, startY, 300, 25), "Description for runtime UI: ");
		uAB.desp=EditorGUI.TextArea(new Rect(startX, startY+17, window.position.width-10, 50), uAB.desp);
		
		GUI.EndScrollView();
	}
	
	private bool expandChainedAbilityPanel=false;
	private Vector2 scrollPosAbilBox;
	
	
	void EffectAttrConfigurator(UnitAbility uAB, int ID, int startX, int startY){
		EffectAttr effectAttr=uAB.effectAttrs[ID];
		
		if(GUI.Button(new Rect(startX, startY+=18, 70, 14), "Remove")){
			uAB.effectAttrs.Remove(effectAttr);
			return;
		}
		
		int type=(int)effectAttr.type;
		cont=new GUIContent("Type:", "Type of the effect.");
		contList=new GUIContent[effectAttrTypeLabel.Length];
		for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(effectAttrTypeLabel[i], effectAttrTypeTooltip[i]);
		EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		type = EditorGUI.Popup(new Rect(startX+40, startY, 80, 16), type, contList);
		effectAttr.type=(_EffectAttrType)type;
		
		
		
		if(effectAttr.type==_EffectAttrType.Teleport){
			uAB.targetType=_AbilityTargetType.EmptyTile;
			uAB.requireTargetSelection=true;
			uAB.targetArea=_TargetArea.Default;
			uAB.aoeRange=0;
		}
		else if(effectAttr.type==_EffectAttrType.SpawnUnit || effectAttr.type==_EffectAttrType.SpawnCollectible){
			if(effectAttr.type==_EffectAttrType.SpawnUnit){
				cont=new GUIContent("Unit To Spawn:", "The unit prefab to be used spawned");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 180, 20), cont);
				effectAttr.unit=(UnitTB)EditorGUI.ObjectField(new Rect(startX, startY+=18, 125, 17), effectAttr.unit, typeof(UnitTB), false);
				
				cont=new GUIContent("Duration:", "The effective duration in which the spawned unit will last\nSet to -ve value for the spawn to be permenant" );
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				effectAttr.duration=EditorGUI.IntField(new Rect(startX+70, startY-1, 50, 16), effectAttr.duration);
				
				if(effectAttr.duration==0) effectAttr.duration=-1;
			}
			else if(effectAttr.type==_EffectAttrType.SpawnCollectible){
				cont=new GUIContent("Collectible To Spawn:", "The collectible prefab to be used spawned");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 180, 20), cont);
				effectAttr.collectible=(CollectibleTB)EditorGUI.ObjectField(new Rect(startX, startY+=18, 125, 17), effectAttr.collectible, typeof(CollectibleTB), false);
			}
			
			
			uAB.targetType=_AbilityTargetType.EmptyTile;
			uAB.requireTargetSelection=true;
			uAB.targetArea=_TargetArea.Default;
			uAB.aoeRange=0;
		}
		else if(effectAttr.type==_EffectAttrType.ChangeTargetFaction){
			cont=new GUIContent("Chance:", "The chance of success. Take value from 0 to 1");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
			
			//~ cont=new GUIContent("Duration:", "The effective duration of the faction change");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			//~ effectAttr.duration=EditorGUI.IntField(new Rect(startX+70, startY-1, 50, 16), effectAttr.duration);
			
			uAB.targetType=_AbilityTargetType.Hostile;
			uAB.requireTargetSelection=true;
			uAB.targetArea=_TargetArea.Default;
			uAB.aoeRange=0;
		}
		else if(effectAttr.type==_EffectAttrType.HPDamage){
			cont=new GUIContent("UseDefaultValue:", "Check to use the unit default attack damage value");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			effectAttr.useDefaultDamageValue=EditorGUI.Toggle(new Rect(startX+107, startY-1, 50, 16), effectAttr.useDefaultDamageValue);
			if(!effectAttr.useDefaultDamageValue){
				cont=new GUIContent("ValueMin:", "Minimum value for the effect");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
				
				cont=new GUIContent("ValueMax:", "Maximum value for the effect");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				effectAttr.valueAlt=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.valueAlt);
			}
		}
		else if(effectAttr.type==_EffectAttrType.HPGain || effectAttr.type==_EffectAttrType.APGain || effectAttr.type==_EffectAttrType.APDamage){
			cont=new GUIContent("ValueMin:", "Minimum value for the effect");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
			
			effectAttr.value=Mathf.Min(effectAttr.value, effectAttr.valueAlt);
			
			cont=new GUIContent("ValueMax:", "Maximum value for the effect");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			effectAttr.valueAlt=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.valueAlt);
			
			effectAttr.valueAlt=Mathf.Max(effectAttr.value, effectAttr.valueAlt);
		}
		else{
			cont=new GUIContent("Value:", "Value for the effect");
			EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
			effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
		}
		
		//~ if(type==1){
			//~ cont=new GUIContent("Range:", "Effective range of the ability in term of tile");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), "use Default Damage:");
			//~ effect.useDefaultDamageValue=EditorGUI.Toggle(new Rect(startX+120, startY-1, 50, 16), effect.useDefaultDamageValue);
		//~ }
		
		//~ if(type!=2 || !effectAttr.useDefaultDamageValue){
			//~ cont=new GUIContent("Range:", "Effective range of the ability in term of tile");
			//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), "value:");
			//~ effectAttr.value=EditorGUI.FloatField(new Rect(startX+70, startY-1, 50, 16), effectAttr.value);
		//~ }
		
		
		if(uAB.effectType==_EffectType.Debuff || uAB.effectType==_EffectType.Buff){
			if(effectAttr.type!=_EffectAttrType.SpawnUnit){
				cont=new GUIContent("Duration:", "Effective duration of the effect in term of round");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				effectAttr.duration=EditorGUI.IntField(new Rect(startX+70, startY-1, 50, 16), effectAttr.duration);
			}
		}
		
		
		//~ int type=(int)effectAttr.type;
		//~ cont=new GUIContent("Type:", "Type of the effect");
		//~ contList=new GUIContent[effectAttrTypeLabel.Length];
		//~ for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(effectAttrTypeLabel[i], effectAttrTypeTooltip[i]);
		//~ EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
		//~ type = EditorGUI.Popup(new Rect(startX+40, startY, 80, 16), type, contList);
		//~ effectAttr.type=(_EffectAttrType)type;
		
		if(effectAttr.type==_EffectAttrType.HPDamage){
			if(damageList.Length>0){
				contList=new GUIContent[damageTooltipList.Length];
				for(int i=0; i<contList.Length; i++) contList[i]=new GUIContent(damageList[i], damageTooltipList[i]);
				cont=new GUIContent("Type:", "Damage type to be inflicted on target");
				
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				effectAttr.damageType = EditorGUI.Popup(new Rect(startX+40, startY, 80, 16), effectAttr.damageType, contList);
			}
			else{
				cont=new GUIContent("Type:", "No damage type has been created, use DamageArmorTableEditor to create one");
				EditorGUI.LabelField(new Rect(startX, startY+=18, 200, 20), cont);
				
				if(GUI.Button(new Rect(startX+40, startY, 80, 15), "OpenEditor")){
					DamageArmorTableEditor.Init();
				}
			}
		}
	}
	
	
	int GenerateNewID(){
		int newID=0;
		while(UAbIDList.Contains(newID)) newID+=1;
		return newID;
	}
	
	static void SwapUAbInList(int ID1, int ID2){
		UnitAbility temp=allUAbList[ID1].Clone();
		allUAbList[ID1]=allUAbList[ID2].Clone();
		allUAbList[ID2]=temp.Clone();
	}
	
	
}




