using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIOverlay : MonoBehaviour {

	//public Texture tex;
	//private bool battleStarted=false;
	
	List<EffectOverlay> effectOverlayList=new List<EffectOverlay>();
	
	void Awake(){
		//tex=Resources.Load("Textures/Bar", typeof(Texture)) as Texture;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	void OnEnable(){
		//GameControlTB.onBattleStartE += OnBattleStart;
		
		GridManager.onHoverTileEnterE += OnHoverTileEnter;
		GridManager.onHoverTileExitE += OnHoverTileExit;
		
		EffectOverlay.onEffectOverlayE += OnEffectOverlay;
	}
	void OnDisable(){
		//GameControlTB.onBattleStartE -= OnBattleStart;
		
		GridManager.onHoverTileEnterE -= OnHoverTileEnter;
		GridManager.onHoverTileExitE -= OnHoverTileExit;
		
		EffectOverlay.onEffectOverlayE += OnEffectOverlay;
	}
	
	void OnBattleStart(){
		//battleStarted=true;
	}
	
	private Tile tileHovered;
	void OnHoverTileEnter(Tile tile){
		tileHovered=tile;
		
		
	}
	void OnHoverTileExit(){
		tileHovered=null;
	}
	
	public void OnEffectOverlay(EffectOverlay eff){
		effectOverlayList.Add(eff);
		if(!eff.useColor){
			//StartCoroutine(DamageOverlayRoutine(eff, eff.pos, eff.msg, color));
			eff.color=UI.colorH;
			StartCoroutine(DamageOverlayRoutine(eff));
		}
		else{
			//StartCoroutine(DamageOverlayRoutine(eff, eff.pos, eff.msg, eff.color));
			StartCoroutine(DamageOverlayRoutine(eff));
		}
	}
	
	IEnumerator DamageOverlayRoutine(EffectOverlay eff){
		effectOverlayList.Add(eff);
		float duration=0;
		while(duration<1){
			eff.pos+=new Vector3(0, 1.5f*Time.deltaTime, 0);
			eff.color.a=1-duration;
			
			duration+=Time.deltaTime*1.5f;
			yield return null;
		}
		effectOverlayList.Remove(eff);
	}
	void DrawOverlay(){
		GUIStyle style=new GUIStyle();
		style.fontStyle=FontStyle.Bold;
		style.alignment=TextAnchor.UpperCenter;
		style.fontSize=16;
		
		for(int i=0; i<effectOverlayList.Count; i++){
			EffectOverlay effect=effectOverlayList[i];
			Camera cam=CameraControl.GetActiveCamera();
			Vector3 screenPos = cam.WorldToScreenPoint(effect.pos);
			screenPos.y=Screen.height-screenPos.y;
			
			style.normal.textColor=new Color(0, 0, 0, effect.color.a);
			GUI.Label(new Rect(screenPos.x-50+2, screenPos.y+2, 100, 40), effect.msg, style);
			style.normal.textColor=effect.color;
			GUI.Label(new Rect(screenPos.x-50, screenPos.y, 100, 40), effect.msg, style);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI(){
		//if(!battleStarted) return;
		
		DrawOverlay();
		
		List<UnitTB> unitList=UnitControl.GetAllUnit();
		
		int length=40;
		int height=3;
		
		for(int i=0; i<unitList.Count; i++){
			UnitTB unit=unitList[i];
			
			if(GameControlTB.EnableFogOfWar()){
				if(!unit.IsVisibleToPlayer()) continue;
			}
			
			Camera cam=CameraControl.GetActiveCamera();
			Vector3 screenPos = cam.WorldToScreenPoint(unitList[i].thisT.position);
			screenPos.y=Screen.height-screenPos.y;
			
			int startPosX=(int)(screenPos.x-length/2+7);
			int startPosY=(int)screenPos.y+5;
			
			float hpRatio=(float)unit.HP/(float)unit.GetFullHP() * length;
			float apRatio=(float)unit.AP/(float)unit.GetFullAP() * length;
			
			GUI.color=new Color(.5f, .5f, .5f, 1);
			GUI.DrawTexture(new Rect(startPosX-1, startPosY-1, length+2, 2*height+2), UI.texBar);
			GUI.color=Color.green;
			GUI.DrawTexture(new Rect(startPosX, startPosY, hpRatio, height), UI.texBar);
			//GUI.Label(new Rect(screenPos.x, screenPos.y, 500, 500), "5555");
			GUI.color=new Color(0f, 1f, 1f, 1);
			GUI.DrawTexture(new Rect(startPosX, startPosY+height, apRatio, height), UI.texBar);
			
			Texture fIcon=UnitControl.GetFactionIcon(unitList[i].factionID);
			if(fIcon!=null) fIcon=UI.texBar;
			
			GUI.color=Color.white;
			GUI.DrawTexture(new Rect(startPosX-15, startPosY-5, 15, 15), UI.texBar);
			GUI.color=UnitControl.GetFactionColor(unit.factionID);
			GUI.DrawTexture(new Rect(startPosX-14, startPosY-4, 13, 13), UI.texBar);
			
			GUI.color=Color.white;
		}
		
		if(tileHovered!=null) DrawHoverInfo();
		
		
	}
	
	void DrawHoverInfo(){
		UnitTB selectedUnit=UnitControl.selectedUnit;
		
		if(tileHovered.attackableToSelected){
			UnitTB unit=tileHovered.unit;
		
			int dmgMin=0, dmgMax=0;
			string hit="", crit="";
			
			int dist=GridManager.Distance(tileHovered, selectedUnit.occupiedTile);
			
			int armorType=unit.armorType;
			int damageType=selectedUnit.damageType;
			float dmgModifier=DamageTable.GetModifier(armorType, damageType);
			
			if(dist==1 && selectedUnit.attackMode!=_AttackMode.Range){
				dmgMin=(int)((float)selectedUnit.GetMeleeDamageMin()*dmgModifier);
				dmgMax=(int)((float)selectedUnit.GetMeleeDamageMax()*dmgModifier);
				
				hit=(selectedUnit.GetTotalHitChanceMelee(unit)*100).ToString("f0")+"%";
				crit=(selectedUnit.GetTotalCritChanceMelee(unit)*100).ToString("f0")+"%";
			}
			else{
				dmgMin=(int)((float)selectedUnit.GetRangeDamageMin()*dmgModifier);
				dmgMax=(int)((float)selectedUnit.GetRangeDamageMax()*dmgModifier);
				
				hit=(selectedUnit.GetTotalHitChanceRange(unit)*100).ToString("f0")+"%";
				crit=(selectedUnit.GetTotalCritChanceRange(unit)*100).ToString("f0")+"%";
			}
			
			string text="";
			text+=hit+"chance to hit\n";
			text+="Damage: "+dmgMin+"-"+dmgMax+"\n\n";
			if(crit!="0%")	text+="Critical Chance: "+crit;
			
			
			bool counter=false;
			if(GameControlTB.IsCounterAttackEnabled()){
				if(dist>=unit.GetAttackRangeMin() && dist<=unit.GetAttackRangeMax() && unit.counterAttackRemain>0){
					counter=true;
				}
			}
			
			int cost=0;
			if(GameControlTB.AttackAPCostRule()==_AttackAPCostRule.PerAttack){
				cost=selectedUnit.APCostAttack;
			}
					
			GUIStyle style=new GUIStyle();
			style.fontStyle=FontStyle.Bold;
					
			int width=500;
			int height=160;
			for(int i=0; i<3; i++) GUI.Box(new Rect(Screen.width/2-width/2, Screen.height-230, width, height), "");
			
			style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
			GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-220, width, height), "Attack", style);
			
			if(cost>0){
				style.fontSize=16;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperRight;
				GUI.Label(new Rect(Screen.width/2-width/2-5, Screen.height-220, width, height), cost+"AP", style);
			}
			
			style.fontSize=16;	style.normal.textColor=UI.colorN;	style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
			GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-190, width, height), text, style);
			if(counter){
				style.fontSize=14;	style.normal.textColor=UI.colorH;	style.wordWrap=false;
				GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-120, width, height), "Target will counter attack", style);
			}
			
			GUI.color=Color.white;
		}
		else{
			if(tileHovered.walkableToSelected && selectedUnit!=null){
				//Vector3 screenPos = cam.WorldToScreenPoint(tileHovered.pos);
				//hoverObject.transform.localPosition=screenPos+new Vector3(-40, 40, 0);
				
				List<Vector3> list=AStar.SearchWalkablePos(tileHovered, selectedUnit.occupiedTile);
				int dist=list.Count;
				string text="Move: "+(dist*selectedUnit.APCostMove)+"AP";
				//string text="Move: "+Random.Range(0, 9999)+"AP";
				GUIContent cont=new GUIContent(text);
				
				GUI.color=Color.white;
				GUIStyle style=new GUIStyle();
				style.fontStyle=FontStyle.Bold;
				style.fontSize=16;
				style.normal.textColor=UI.colorN;
				style.alignment=TextAnchor.LowerRight;
				
				Camera cam=CameraControl.GetActiveCamera();
				Vector3 screenPos = cam.WorldToScreenPoint(tileHovered.pos);
				screenPos.y=Screen.height-screenPos.y;
				
				float widthMin=0; float widthMax=0;
				style.CalcMinMaxWidth(cont, out widthMin, out widthMax);
				GUI.Box(new Rect(screenPos.x-widthMax-50, screenPos.y-50, widthMax+25, 22), "");
				GUI.Label(new Rect(screenPos.x-widthMax-50, screenPos.y-50, widthMax+25-4, 20), text, style);
			}
		}
		
	}
}
