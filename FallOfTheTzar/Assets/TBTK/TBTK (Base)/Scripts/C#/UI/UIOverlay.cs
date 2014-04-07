#define ibox
//#define mo
//mo stands for mouseover and is a tool which shows AP and HP when a unit is moused over
#define window
#define customGui
#define mousePos
#define hoverHP
//#define hoverInfo
//ibox is short for infobox, it is a suite of interface tools to make the interface easier to read
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIOverlay : MonoBehaviour {
	private float timeSinceCheck = 0f;

	
	private float y_offset  = -200;
	private float x_offset = 30;
	private float mouseRange = 10; //how ffar from an object the mouse must be to allow for overlap


	public bool messageSent = false;
	private float posY = 0f;
	private float posX =0f;

	public static int w_width = 500;
	public static int w_height = 160;
#if window
	//public Rect windowRect0 = new Rect(20, 20, w_width, w_height);
#endif
	#if customGui
	public GUISkin customSkin;
	public GUIStyle styleA;
	#endif

	//public Texture tex;
	//private bool battleStarted=false;
#if ibox
	
	public float idleUnits = 0;	//number of idle units for the person currently playing
	public List<UnitTB> p_units;	//list of player units
	private float totalUnits;
	private int p_factionId;	//player faction id
	public float waitTime = 3.0f;		//TODO: change to private once a reasonable time is found
	public float lastCheck;
#endif
	List<EffectOverlay> effectOverlayList=new List<EffectOverlay>();
	
	void Awake(){
		//tex=Resources.Load("Textures/Bar", typeof(Texture)) as Texture;

		 styleA=new GUIStyle();
		styleA.fontStyle=FontStyle.Bold;
	}
	
	// Use this for initialization
	void Start () {
#if ibox
		//get the faction id
		GameControlTB gctb = GameObject.Find("GameControl").GetComponent<GameControlTB>();
		List<int> factionIds = gctb.playerFactionID;
		p_factionId = factionIds[0];
		p_units = UnitControl.GetAllUnitsOfFaction(p_factionId);
		totalUnits = p_units.Count;
		idleUnits = totalUnits;

#endif
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

	void resetBool(){
		messageSent = false;
	}

	void OnGUI(){
		//if(!battleStarted) return;
		#if customGui
		GUI.skin= Resources.Load("Skins/Fantasy-Colorable") as GUISkin;
		#endif

		DrawOverlay();
		
		List<UnitTB> unitList=UnitControl.GetAllUnit();
		
		int length=40;
		int height=3;
		
		for(int i=0; i<unitList.Count; i++){
			UnitTB unit=unitList[i];
			
			if(GameControlTB.EnableFogOfWar()){
				if(!unit.IsVisibleToPlayer()) continue;
			}
			

			


#if hoverInfo
			Camera cam=CameraControl.GetActiveCamera();
			Vector3 screenPos = cam.WorldToScreenPoint(unitList[i].thisT.position);
			screenPos.y=Screen.height-screenPos.y;
			
			int startPosX=(int)(screenPos.x-length/2+7);
			int startPosY=(int)screenPos.y+5;
			
			float hpRatio=(float)unit.HP/(float)unit.GetFullHP() * length;
			float apRatio=(float)unit.AP/(float)unit.GetFullAP() * length;
			
		
			GUIStyle style=new GUIStyle();
			style.fontStyle=FontStyle.Bold;

			style.fontSize = 20;




			style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
			GUI.Label(new Rect(startPosX-x_offset,startPosY+y_offset,90,90),"\n HP:"+unit.HP+"/" +unit.GetFullHP()+"\n AP:" + unit.AP + "/"+unit.GetFullAP(),style);
		

			GUI.color=Color.white;
			//draw the hp and AP
			//GUI.Box(new Rect(startPosX, startPosY-20, 200, 200), UI.texBar);
#else
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
			//TODO: Add the numbers for AP and HP here, we have startpos X and Y, and other values
			//are calculated elsewhere
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
#endif

		}
		
		if(tileHovered!=null) DrawHoverInfo();
		///Draw text box with selected unit health and AP
		#if ibox

		/*
		 * 
	void ShowTooltip(UnitAbility ability){
		GUIStyle style=new GUIStyle();
		style.fontStyle=FontStyle.Bold;
		
		int width=500;
		int height=160;
		
		
		for(int i=0; i<3; i++) GUI.Box(new Rect(Screen.width/2-width/2, Screen.height-230, width, height), "");
		
		style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
		GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-220, width, height), ability.name, style);
		
		style.fontSize=16;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperRight;
		GUI.Label(new Rect(Screen.width/2-width/2-5, Screen.height-220, width, height), ability.cost+"AP", style);
		
		style.fontSize=16;	style.normal.textColor=UI.colorN;	style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
		GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-190, width, height), ability.desp, style);
		
		GUI.color=Color.white;
	}
		 * */

		if(UnitControl.selectedUnit != null){ //only perform this calculation once every second
			//get the selected unit
			UnitTB selectedUnit=UnitControl.selectedUnit;
			string name =selectedUnit.unitName;


			Camera cam=CameraControl.GetActiveCamera();
			Vector3 screenPos = cam.WorldToScreenPoint(UnitControl.selectedUnit.thisT.position);
			screenPos.y=Screen.height-screenPos.y;
			
			int startPosX=(int)(screenPos.x-length/2+7);
			int startPosY=(int)screenPos.y+5;


			
			styleA.fontSize = 20;
			
			

			styleA.fontSize=20;	styleA.normal.textColor=UI.colorH;	styleA.alignment=TextAnchor.UpperCenter;


			GUI.Box(new Rect(startPosX-x_offset-25,startPosY+y_offset,140,140),"");
			
			GUI.Label(new Rect(startPosX-x_offset-25,startPosY+y_offset,140,140),"\n HP:"+UnitControl.selectedUnit.HP+"/" +UnitControl.selectedUnit.GetFullHP()+"\n AP:" + UnitControl.selectedUnit.AP + "/"+UnitControl.selectedUnit.GetFullAP() + "\n Moves:"+UnitControl.selectedUnit.moveRemain + "\n Attacks:"+UnitControl.selectedUnit.attackRemain,styleA);


			GUI.color=Color.white;


			GUIStyle style=new GUIStyle();
			style.fontStyle=FontStyle.Bold;

			int w=w_width;
			int h=w_height;

			style.fontSize = 20;

			style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
#if window
			//Color c = GUI.color;
			//GUI.color = Color.red;
			//windowRect0 = GUI.Window(0,windowRect0,DoMyWindow, "Green Window");
			//GUI.color = c;

			#if devIbox
			GUI.Box(new Rect(Screen.width/2-750/2, -80, 750, h), "");
			GUI.Label(new Rect(Screen.width/2-w/2, 0, w, h), name+" HP:"+selectedUnit.HP+" AP:"+selectedUnit.AP+" Remaining Moves:"+selectedUnit.moveRemain +" Remaining Attacks: " +selectedUnit.attackRemain, style);
#endif
#if mo
			if(timeSinceCheck > .25){
				//TODO: This method is not efficient, change it later
				//get the unit position
					//get the gameObject first
				GameObject g = selectedUnit.thisObj;
				//get the camera
				Camera c = GameObject.Find("MainCamera").GetComponent<Camera>();
				Vector3 screenPos = camera.WorldToScreenPoint(g.transform.position);

			Debug.Log (screenPos.x + " : " + screenPos.y);
				//put a simplified version of the ibox at the unit position
				GUI.Box(new Rect(screenPos.x, screenPos.y, 100, 30), "");
				GUI.Label(new Rect(screenPos.x, screenPos.y, 100, 30), " HP:"+selectedUnit.HP+" AP:"+selectedUnit.AP, style);
			}
#endif

			//print unit information
			//GUI.Label(new Rect(Screen.width/2-250, 0, 500, 20), name+" HP:"+selectedUnit.HP+" AP:"+selectedUnit.AP+" Remaining Moves:"+selectedUnit.moveRemain +" Remaining Attacks: " +selectedUnit.attackRemain);
			if(lastCheck >= waitTime){
				lastCheck = 0;
				idleUnits = totalUnits;
				//get idle units
				//get an array of their units
				p_units = UnitControl.GetAllUnitsOfFaction(p_factionId);
				//loop through the array
				foreach(UnitTB unit in p_units){
					if(unit.AreAllActionsCompleted()){
						/*GameObject g = unit.thisObj;
						Renderer r = g.renderer;
						r.material.color = Color.grey;*/
						idleUnits--;
					}
					else if(false){ //TODO: if the unit has no more moves, cannot use an ability, and cannot attack anyone
						
					}
					//if a unit has moves left, or attacks left, increment idle units
				}
				
			}
			else{
				lastCheck+=Time.deltaTime;
			}
			//print idle information
			if(idleUnits ==0){
				//TODO: change coords to put it right above next turn button
				//GUI.Label(new Rect(Screen.width/2-w/2, 25, w, h), "All units have exhausted their moves, hit the next turn.", style);
				//only toggle glow once per turn
				if(!messageSent){
					BroadcastMessage("toggleGlow");
					messageSent = true;
				}
				//GUI.Label (new Rect(Screen.width/2-250, 15, 500, 20), "All units have exhausted their moves, hit the next turn.");
			}
			else{
				BroadcastMessage("turnoffGlow");
				GUI.Label(new Rect(Screen.width-100, Screen.height-65-20, 60, 60), idleUnits + "/"+ totalUnits + " idle units", style);
				
				//GUI.Label (new Rect(Screen.width/2-250, 15, 500, 20),idleUnits + "/"+ totalUnits + " units still have available actions.");
			}

#else
			GUI.Box(new Rect(Screen.width/2-750/2, -80, 750, h), "");
			GUI.Label(new Rect(Screen.width/2-w/2, 0, w, h), name+" HP:"+selectedUnit.HP+" AP:"+selectedUnit.AP+" Remaining Moves:"+selectedUnit.moveRemain +" Remaining Attacks: " +selectedUnit.attackRemain, style);

			//print unit information
			//GUI.Label(new Rect(Screen.width/2-250, 0, 500, 20), name+" HP:"+selectedUnit.HP+" AP:"+selectedUnit.AP+" Remaining Moves:"+selectedUnit.moveRemain +" Remaining Attacks: " +selectedUnit.attackRemain);
				if(lastCheck >= waitTime){
				lastCheck = 0;
				idleUnits = totalUnits;
				//get idle units
					//get an array of their units
					p_units = UnitControl.GetAllUnitsOfFaction(p_factionId);
					//loop through the array
				foreach(UnitTB unit in p_units){
					if(unit.AreAllActionsCompleted()){
						idleUnits--;
					}
					else if(false){ //TODO: if the unit has no more moves, cannot use an ability, and cannot attack anyone

					}
					//if a unit has moves left, or attacks left, increment idle units
				}

			}
			else{
				lastCheck+=Time.deltaTime;
			}
			//print idle information
			if(idleUnits ==0){
				GUI.Label(new Rect(Screen.width/2-w/2, 25, w, h), "All units have exhausted their moves, hit the next turn.", style);
				//GUI.Label (new Rect(Screen.width/2-250, 15, 500, 20), "All units have exhausted their moves, hit the next turn.");
			}
			else{
				GUI.Label(new Rect(Screen.width/2-w/2, 25, w, h), idleUnits + "/"+ totalUnits + " units still have available actions.", style);

				//GUI.Label (new Rect(Screen.width/2-250, 15, 500, 20),idleUnits + "/"+ totalUnits + " units still have available actions.");
			}
#endif

		}
		#endif
		
	}

	void getMousePos(){
		timeSinceCheck += Time.deltaTime;
		if(timeSinceCheck > 0.25){
			timeSinceCheck = 0;
			posY = Input.mousePosition.y;
			posX = Input.mousePosition.x;
		}
	}
	
	void DrawHoverInfo(){
		UnitTB selectedUnit=UnitControl.selectedUnit;

#if hoverHP
		if(tileHovered.unit != null){
			//draw hp and AP for unit
			UnitTB hUnit=tileHovered.unit;

			Camera camA=CameraControl.GetActiveCamera();
			Vector3 screenPosA = camA.WorldToScreenPoint(hUnit.thisT.position);
			screenPosA.y=Screen.height-screenPosA.y;

			int startPosX=(int)(screenPosA.x-40/2+7);
			int startPosY=(int)screenPosA.y+5;

			styleA.fontSize=20;	styleA.normal.textColor=UI.colorH;	styleA.alignment=TextAnchor.UpperCenter;
			GUI.Box(new Rect(startPosX-x_offset-25,startPosY+y_offset,140,140),"");

			GUI.Label(new Rect(startPosX-x_offset-25,startPosY+y_offset,140,140),"\n HP:"+hUnit.HP+"/" +hUnit.GetFullHP()+"\n AP:" + hUnit.AP + "/"+hUnit.GetFullAP() + "\n Moves:"+hUnit.moveRemain + "\n Attacks:"+hUnit.attackRemain,styleA);
		}
#endif
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
					
#if mousePos
			
			int width=500;
			int w_offset =50;
			int height=160;
			int h_offset = 20;
			

			//get pos X and Y once every second to prevent flicker
			getMousePos();
			
			/*for(int i=0; i<3; i++) GUI.Box(new Rect(posX-(width+w_offset)/2, posY-230, width+w_offset, height), "");
			
			style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
			GUI.Label(new Rect(posX-width/2, posY-240, width, height), ability.name, style);
			
			style.fontSize=16;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperRight;
			GUI.Label(new Rect(posX-width/2-5, posY-240, width, height), ability.cost+"AP", style);
			
			style.fontSize=16;	style.normal.textColor=UI.colorN;	style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
			GUI.Label(new Rect(posX-width/2, posY-190, width, height), ability.desp, style);
			
			GUI.color=Color.white;*/

			//for(int i=0; i<3; i++) GUI.Box(new Rect(posX-(width +w_offset)/2, posY, width+w_offset, height), "");


				GUI.Box(new Rect(posX-(width/2)/2, Screen.height-posY+40, width/2, height-20), ""); // to remove flicker comment out this line

				style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
				GUI.Label(new Rect(posX-(width)/2, Screen.height-posY+40+h_offset, width, height), "Attack", style);
				

				if(cost>0){
					style.fontSize=16;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperRight;
					GUI.Label(new Rect(posX-width/2-5, Screen.height-posY+50+h_offset, width, height), cost+"AP", style);
				}
				
				//reposition to be at location of mouse
				style.fontSize=16;	style.normal.textColor=UI.colorN;	style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
			GUI.Label(new Rect(posX-width/2, Screen.height-posY+60+h_offset, width, height), text, style);	
				if(counter){
					style.fontSize=14;	style.normal.textColor=UI.colorH;	style.wordWrap=false;
					GUI.Label(new Rect(posX-width/2, Screen.height-posY+40+h_offset, width, height), "Target will counter attack", style);
				}

#else

			int width=500;
			int height=160;
			for(int i=0; i<3; i++) GUI.Box(new Rect(Screen.width/2-width/2, Screen.height-230, width, height), "");
			
			style.fontSize=20;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperCenter;
			GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-240, width, height), "Attack", style);
			
			if(cost>0){
				style.fontSize=16;	style.normal.textColor=UI.colorH;	style.alignment=TextAnchor.UpperRight;
				GUI.Label(new Rect(Screen.width/2-width/2-5, Screen.height-220, width, height), cost+"AP", style);
			}

			//reposition to be at location of mouse
			style.fontSize=16;	style.normal.textColor=UI.colorN;	style.alignment=TextAnchor.UpperCenter;	style.wordWrap=true;
			GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-190, width, height), text, style);	
			if(counter){
				style.fontSize=14;	style.normal.textColor=UI.colorH;	style.wordWrap=false;
				GUI.Label(new Rect(Screen.width/2-width/2, Screen.height-120, width, height), "Target will counter attack", style);
			}
#endif
			
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
#if ibox
#else
				GUI.Box(new Rect(screenPos.x-widthMax-50, screenPos.y-50, widthMax+25, 22), "");
				GUI.Label(new Rect(screenPos.x-widthMax-50, screenPos.y-50, widthMax+25-4, 20), text, style);
#endif
			}
		}
		
	}

	// Make the contents of the window
	/*void DoMyWindow(int windowID) {
		if (GUI.Button(new Rect(10, 20, 100, 20), "Hello World"))
			print("Got a click in window with color " + GUI.color);
		
		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}*/

}
