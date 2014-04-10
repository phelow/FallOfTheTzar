using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum _TileType{Hex, Square}
public enum _TileState{Default, Selected, Walkable, Hostile, AbilityRange}
public enum _ListState{Unassigned, Open, Close};


[System.Serializable]
public class Wall{
	public Transform wallObj;
	public Tile tile1;
	public Tile tile2;
	public float angle;
	
	public Wall(Transform obj, Tile t1, Tile t2, float ang){
		wallObj=obj;
		tile1=t1;
		tile2=t2;
		if(ang>360) ang-=360;
		else if (ang<0) ang+=360;
		angle=ang;
	}
	
	public bool Contains(Tile t1, Tile t2){
		if((t1==tile1 || t1==tile2) && (t2==tile1 || t2==tile2)){
			return true;
		}
		return false;
	}
}

public class Tile : MonoBehaviour {

	public delegate void OnShowUnitInfoHandler(Tile tile); 
	public static event OnShowUnitInfoHandler onShowUnitInfoE;
	
	public _TileType type;
	
	[HideInInspector] 
	public bool walkable=true;
	[HideInInspector] 
	public UnitTB unit=null;
	[HideInInspector] public _TileState state=_TileState.Default;
	
	//[HideInInspector] 
	public Obstacle obstacle=null;
	
	public Material matNormal;
	//~ public Material matHighlight;
	public Material matWalkable;
	public Material matUnwalkable;
	public Material matHostile;
	public Material matAbilityRange;
	//~ public Material matFriendly;
	
	[HideInInspector] public Transform thisT;
	[HideInInspector] public Vector3 pos;
	
	[HideInInspector] 
	[SerializeField]
	public List<Tile> neighbours=new List<Tile>();
	[HideInInspector] 
	[SerializeField]
	public List<Tile> disconnectedNeighbour=new List<Tile>();
	
	[HideInInspector] public Tile parent;
	[HideInInspector] public _ListState listState=_ListState.Unassigned;
	[HideInInspector] public float scoreG;
	[HideInInspector] public float scoreH;
	[HideInInspector] public float scoreF;
	[HideInInspector] public float tempScoreG;
	
	[HideInInspector] public CollectibleTB collectible;
	
	[HideInInspector] public bool attackableToSelected=false;
	[HideInInspector] public bool walkableToSelected=false;
	[HideInInspector] public bool openForPlacement=false;
	[HideInInspector] public int placementID=-1;
	
	
	[HideInInspector] public int HPGainModifier=0;
	[HideInInspector] public int APGainModifier=0;
	[HideInInspector] public int damageModifier=0;
	[HideInInspector] public int attRangeModifier=0;
	[HideInInspector] public float attackModifier=0;
	[HideInInspector] public float defendModifier=0;
	[HideInInspector] public float criticalModifier=0;
	[HideInInspector] public float critDefModifier=0;
	[HideInInspector] public int sightModifier=0;
	
	[HideInInspector] public List<UnitAbility> activeUnitAbilityEffectList=new List<UnitAbility>();
	
	
	[HideInInspector] public List<_CoverType> cover=new List<_CoverType>();
	[HideInInspector] public List<Vector3> overlayPos=new List<Vector3>();
	
	[HideInInspector] public List<GameObject> wall=new List<GameObject>();
	
	
	//for AI analysis
	[HideInInspector] public List<UnitTB> AIHostileList=new List<UnitTB>();
	
	[HideInInspector] public List<Wall> walls=new List<Wall>();
	
	public virtual void Awake(){
		thisT=transform;
	}
	
	
	// Use this for initialization
	public virtual void Start () {
		gameObject.layer=LayerManager.GetLayerTile();
		pos=transform.position; //this has been assign right at grid generation, this line is for runtime grid generation
		
		if(!walkable){
			renderer.material=matUnwalkable;
		}
		else{
			//cover system, wip
			if(GameControlTB.EnableCover()){
				if(Application.isPlaying) InitCover();
			}
		}
		
		if(unit!=null) unit.occupiedTile=this;
	}
	
	//called in edit more to set tile to default
	public void SetToDefault(){
		walkable=true;
		SetState(_TileState.Default);
		renderer.enabled=true;
		openForPlacement=false;
	}
	//called in edit more to set tile to openForPlacement
	public void SetToWalkable(){
		walkable=true;
		SetState(_TileState.Walkable);
		renderer.enabled=true;
		openForPlacement=true;
	}
	//called in edit more to set tile to unwalkable
	public void SetToUnwalkable(bool flag){
		walkable=false;
		renderer.material=matUnwalkable;
		if(flag) renderer.enabled=true;
		else renderer.enabled=false;
		openForPlacement=false;
	}
	

	// Update is called once per frame
	void Update () {
	
	}
	
	public void SetNeighbours(List<Tile> nn){
		neighbours=nn;
	}
	public List<Tile> GetNeighbours(){
		return neighbours;
	}
	
	public void RemoveNeighbour(Tile tile){
		if(tile==null) return;
		if(neighbours.Contains(tile)){
			neighbours.Remove(tile);
			disconnectedNeighbour.Add(tile);
		}
		if(tile.neighbours.Contains(this)){
			tile.neighbours.Remove(this);
			tile.disconnectedNeighbour.Add(this);
		}
	}
	public void AddNeighbour(Tile tile){
		if(tile==null) return;
		if(!neighbours.Contains(tile)){
			neighbours.Add(tile);
			disconnectedNeighbour.Remove(tile);
		}
		if(!tile.neighbours.Contains(this)){
			tile.neighbours.Add(this);
			tile.disconnectedNeighbour.Remove(this);
		}
	}
	
	
	public Tile AddWall(float angle){ return AddWall(angle, null); }
	public Tile AddWall(float angle, Obstacle wallPrefab){
		Tile tile=null;
		for(int i=0; i<neighbours.Count; i++){
			Vector2 dir=new Vector2(neighbours[i].pos.x-pos.x, neighbours[i].pos.z-pos.z);
			
			if(type==_TileType.Hex){
				float angleN=((int)(Utility.VectorToAngle(dir)));
				if(Mathf.Abs(Mathf.DeltaAngle(angle, angleN))<30){
					tile=neighbours[i];
					break;
				}
			}
			else if(type==_TileType.Square){
				float angleN=((int)(Utility.VectorToAngle(dir)));
				if(Mathf.Abs(Mathf.DeltaAngle(angle, angleN))<45){
					tile=neighbours[i];
					break;
				}
			}
		}
		
		for(int i=0; i<walls.Count; i++){
			if(walls[i].Contains(tile, this)){
				Debug.Log("wall existed   "+tile+"    "+this);
				return null;
			}
		}
		
		if(tile==null){
			Debug.Log("no neighbour    "+this);
			return null;
		}
		
		Vector2 dirV=new Vector2(tile.pos.x-pos.x, tile.pos.z-pos.z);
		float angleW=Utility.VectorToAngle(dirV);
		Transform wallT=null;
		
		if(wallPrefab!=null){
			wallT=(Transform)Instantiate(wallPrefab.transform);
			
			Vector3 direction=tile.pos-pos;
			
			if(type==_TileType.Hex){
				float gridSize=transform.localScale.x/1.1628f;
				wallT.position=pos+(direction.normalized)*gridSize*0.5f;//+new Vector3(0, gridSize*0.3f, 0);
				wallT.localScale=new Vector3(gridSize, gridSize, gridSize)*1.2f;
			}
			else if(type==_TileType.Square){
				float gridSize=transform.localScale.x;
				wallT.position=pos+(direction.normalized)*gridSize*0.5f;//+new Vector3(0, gridSize*0.3f, 0);
				wallT.localScale=new Vector3(gridSize, gridSize, gridSize);
			}
			
			float rotOffset=90;
			wallT.rotation=Quaternion.Euler(0, -(angleW+rotOffset), 0);
			
			wallT.parent=transform;
		}
		
		walls.Add(new Wall(wallT, tile, this, angleW));
		tile.walls.Add(new Wall(wallT, tile, this, angleW+180));
		RemoveNeighbour(tile);
		
		return tile;
		
	}
	public Tile RemoveWall(float angle){
		Wall wall=null;
		for(int i=0; i<walls.Count; i++){
			if(type==_TileType.Hex){
				if(Mathf.Abs(Mathf.DeltaAngle(angle, walls[i].angle))<30){
					wall=walls[i];
					walls.RemoveAt(i);
					break;
				}
			}
			else if(type==_TileType.Square){
				if(Mathf.Abs(Mathf.DeltaAngle(angle, walls[i].angle))<45){
					wall=walls[i];
					walls.RemoveAt(i);
					break;
				}
			}
		}
		
		if(wall==null) return null;
		
		Tile newNeighbour=null;
		if(wall.tile1==this) newNeighbour=wall.tile2;
		else if(wall.tile2==this) newNeighbour=wall.tile1;
		AddNeighbour(newNeighbour);
		
		for(int i=0; i<newNeighbour.walls.Count; i++){
			if(newNeighbour.walls[i].Contains(this, newNeighbour)){
				newNeighbour.walls.RemoveAt(i);
				break;
			}
		}
		
		if(wall.wallObj!=null) DestroyImmediate(wall.wallObj.gameObject);
		
		return newNeighbour;
	}
	
	
	
	//disable in mobile so it wont interfere with touch input
	#if !UNITY_IPHONE && !UNITY_ANDROID
		//function called when mouse cursor enter the area of the tile, default MonoBehaviour method
		void OnMouseEnter(){ OnTouchMouseEnter();	}
		//function called when mouse cursor leave the area of the tile, default MonoBehaviour method
		void OnMouseExit(){ OnTouchMouseExit(); }
		//function called when mouse cursor enter the area of the tile, default MonoBehaviour method
		public void OnMouseDown(){ OnTouchMouseDown(); }
		
		//onMouseDown for right click
		//function called when mouse cursor enter the area of the tile, default MonoBehaviour method
		//used to detech right mouse click on the tile
		void OnMouseOver(){
			if(GameControlTB.IsUnitPlacementState()) return;
			
			if(Input.GetMouseButtonDown(1)){
				OnRightClick();
			}
		}
		public void OnRightClick(){
			if(onShowUnitInfoE!=null) onShowUnitInfoE(this);
		}
	
	#endif
	
	
	//code execution for when a mouse enter a tile or when a touch first tap on a tile
	public void OnTouchMouseEnter(){
		#if !UNITY_IPHONE && !UNITY_ANDROID
		if(GameControlTB.IsCursorOnUI(Input.mousePosition)) return;
		//if(GameControlTB.IsObjectOnUI(pos)) return;
		#endif
		
		if(!walkable && !GridManager.IsInTargetTileSelectMode()) return;
		
		GridManager.OnHoverEnter(this);
		
		if(unit!=null) UnitControl.hoveredUnit=unit;
	}
	
	
	//code execution for when a mouse exit a tile and when a touch is land on a previously un-touched tile or an empty space
	public void OnTouchMouseExit(){
		//if(GameControlTB.IsCursorOnUI(Input.mousePosition)) return;
		//if(GameControlTB.IsObjectOnUI(pos)) return;
		
		GridManager.OnHoverExit();
		
		if(!walkable && !GridManager.IsInTargetTileSelectMode()) return;
		
		
		SetToDefaultMat();
		
		if(unit!=null) UnitControl.hoveredUnit=null;
	}
	
	
	//code execution for when a left mouse click happen on a tile and when a touch is double tap on a tile
	public void OnTouchMouseDown(){
		#if !UNITY_IPHONE && !UNITY_ANDROID
		if(GameControlTB.IsCursorOnUI(Input.mousePosition)) return;
		//if(GameControlTB.IsObjectOnUI(pos)) return;
		#endif
		
		if(GameControlTB.IsUnitPlacementState()){
			PlaceUnit();
			return;
		}
		
		if(GameControlTB.GetTurnMode()!=_TurnMode.SingleUnitPerTurn){
			if(!GameControlTB.IsPlayerTurn()){
			//if(GameControlTB.turnID!=GameControlTB.GetPlayerFactionTurnID()){
				return;
			}
		}
		
		if(GameControlTB.IsActionInProgress()) return;
		
		if(!walkable && !GridManager.IsInTargetTileSelectMode()) return;
		
		UnitTB sUnit=UnitControl.selectedUnit;
		
		//if a friendly unit has been selected
		//if(sUnit!=null && sUnit.IsControllable(GameControlTB.GetPlayerFactionID())){
		if(sUnit!=null && sUnit.IsControllable()){
			//if HexFridManager is actively looking for a target for current selectedUnit
			if(GridManager.IsInTargetTileSelectMode()){
				ManualSelect();
			}
			else{
				if(!walkableToSelected && !attackableToSelected){
					ManualSelect();
				}
				else{
					if(attackableToSelected && unit!=null) sUnit.Attack(unit);
					else if(walkableToSelected){
						sUnit.Move(this);
					}
					else Debug.Log("error");
				}
			}
			
			return;
		}
		else{
			ManualSelect();
		}
	}
	
	
	//function called when tile is clicked in unit placement phase
	void PlaceUnit(){
		if(!openForPlacement){
			GameControlTB.DisplayMessage("Invalid position");
			return;
		}
		if(unit==null){
			if(placementID==UnitControl.GetPlayerUnitsBeingPlaced().factionID){
				UnitControl.PlaceUnitAt(this);
			}
		}
		else if(unit.factionID==UnitControl.GetPlayerUnitsBeingPlaced().factionID) UnitControl.RemoveUnit(unit);
	}
	
	public void SetToDefaultMat(){
		SetState(state);
	}
	
	//set the tile state and change the renderer material to match
	public void SetState(_TileState ts){
		if(!walkable){
			return;
		}
		
		state=ts;
		
		if(!walkable){
			renderer.material=matUnwalkable;
			return;
		}
		
		if(state==_TileState.Default && matNormal!=null) renderer.material=matNormal;
		else if(state==_TileState.Selected && matNormal!=null) renderer.material=matNormal;
		else if(state==_TileState.Walkable && matWalkable!=null) renderer.material=matWalkable;
		else if(state==_TileState.Hostile && matHostile!=null) renderer.material=matHostile;
		else if(state==_TileState.AbilityRange && matAbilityRange!=null) renderer.material=matAbilityRange;
		//else if(state==_TileState.Friendly) renderer.material=matFriendly;
		//else if(state==_TileState.Selected) renderer.material=matHighlight;
	}
	
	//buffer function for select when the selection is initiated by player, check turn mode condition before proceed
	public void ManualSelect(){
		//if not in target select phase for unitAbility
		if(!GridManager.IsInTargetTileSelectMode()){
			if(!GameControlTB.AllowUnitSwitching()){
				//Debug.Log("unit swtiching is lock");
				return;
			}
			
			_MoveOrder moveOrder=GameControlTB.GetMoveOrder();
			
			//if turn mode and turnOrder doesnt not support unit switching, return
			if(moveOrder!=_MoveOrder.Free) return;
			
			_TurnMode turnMode=GameControlTB.GetTurnMode();
			if(turnMode==_TurnMode.FactionSingleUnitPerTurnAll){
				if(unit.MovedForTheRound()){
					Debug.Log("unit has been moved");
					return;
				}
			}
			else if(turnMode==_TurnMode.SingleUnitPerTurn){
				//Debug.Log("turn mode not allow switching");
				return;
			}
		}
		Select();
	}
	public void Select(){
		if(!walkable) return;
		GridManager.Select(this);
	}
	
	
	
	public void SetWalkableToSelectedFlag(){
		walkableToSelected=true;
	}


	
	public void ClearUnit(){
		unit=null;
		SetState(_TileState.Default);
		if(attackableToSelected){
			GridManager.ClearIndicatorH(this);
			attackableToSelected=false;
		}
		
	}
	
	public void SetUnit(UnitTB newUnit){
		unit=newUnit;
		
		if(collectible!=null){
			collectible.Trigger(newUnit);
		}
	}
	
	
	private int countTillNextTurn=0;
	void OnNextTurn(){
		for(int i=0; i<activeUnitAbilityEffectList.Count; i++){
			UnitAbility uAB=activeUnitAbilityEffectList[i];
			uAB.countTillNextTurn-=1;
			if(uAB.countTillNextTurn==0){
				if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
					uAB.countTillNextTurn=UnitControl.activeFactionCount;
				}
				else{
					uAB.countTillNextTurn=UnitControl.GetAllUnitCount();
				}
				
				bool flag=CalulateEffect(uAB);
				
				//false means no more effect active for the ability, remove the ability
				if(!flag){
					activeUnitAbilityEffectList.RemoveAt(i);
					i-=1;
					
					//if not more ability, stop listening to any event
					if(activeUnitAbilityEffectList.Count==0) UnsubscribeEvent();
				}
			}
		}
		
		//apply the effect, this is placed here so it only runs when there are active effect
		countTillNextTurn-=1;
		if(countTillNextTurn==0){
			//apply the effect on unit
			if(unit!=null){
				if(HPGainModifier>0) unit.ApplyHeal(HPGainModifier);
				else if(HPGainModifier<0) unit.ApplyDamage(HPGainModifier);
				if(APGainModifier!=0) unit.GainAP(APGainModifier);
			}
			
			if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
				countTillNextTurn=UnitControl.activeFactionCount;
			}
			else{
				countTillNextTurn=UnitControl.GetAllUnitCount();
			}
		}
	}
	
	void OnUnitDestroyed(UnitTB unit){
		for(int i=0; i<activeUnitAbilityEffectList.Count; i++){
			activeUnitAbilityEffectList[i].countTillNextTurn-=1;
		}
	}
	
	//calculate the active effect on the tile, called in every new round event
	bool CalulateEffect(UnitAbility ability){
		//loop through all the active effect list
		
		for(int j=0; j<ability.effectAttrs.Count; j++){
			EffectAttr effectAttr=ability.effectAttrs[j];
			
			effectAttr.duration-=1;
			
			//if the effect duration has reach zero, cancel it by changing the modifier
			if(effectAttr.duration==0){
				//commented effect are disabled for tile
				if(effectAttr.type==_EffectAttrType.HPDamage){
					HPGainModifier+=(int)effectAttr.value;
				}
				else if(effectAttr.type==_EffectAttrType.HPGain){
					HPGainModifier-=(int)effectAttr.value;
				}
				else if(effectAttr.type==_EffectAttrType.APDamage){
					APGainModifier+=(int)effectAttr.value;
				}
				else if(effectAttr.type==_EffectAttrType.APGain){
					APGainModifier-=(int)effectAttr.value;
				}
				else if(effectAttr.type==_EffectAttrType.Damage){
					damageModifier-=(int)effectAttr.value;
				}
				//~ else if(effectAttr.type==_EffectAttrType.MovementRange){
					//~ movementModifier-=(int)effectAttr.value;
				//~ }
				else if(effectAttr.type==_EffectAttrType.AttackRange){
					attRangeModifier-=(int)effectAttr.value;
				}
				//~ else if(effectAttr.type==_EffectAttrType.Speed){
					//~ speedModifier-=(int)effectAttr.value;
				//~ }
				else if(effectAttr.type==_EffectAttrType.HitChance){
					attackModifier-=effectAttr.value;
				}
				else if(effectAttr.type==_EffectAttrType.DodgeChance){
					defendModifier-=effectAttr.value;
				}
				else if(effectAttr.type==_EffectAttrType.CriticalChance){
					criticalModifier-=effectAttr.value;
				}
				else if(effectAttr.type==_EffectAttrType.CriticalImmunity){
					critDefModifier-=effectAttr.value;
				}
				/*
				//~ else if(effectAttr.type==_EffectAttrType.ExtraAttack){
					//~ extraAttackModifier-=1;
				//~ }
				//~ else if(effectAttr.type==_EffectAttrType.ExtraCounterAttack){
					//~ counterAttackModifier-=1;
				//~ }
				//~ else if(effectAttr.type==_EffectAttrType.Stun){
					//~ stun-=1;
				//~ }
				//~ else if(effectAttr.type==_EffectAttrType.DisableAttack){
					//~ attackDisabled-=1;
				//~ }
				//~ else if(effectAttr.type==_EffectAttrType.DisableMovement){
					//~ movementDisabled-=1;
				//~ }
				//~ else if(effectAttr.type==_EffectAttrType.DisableAbility){
					//~ abilityDisabled-=1;
				//~ }
				*/
				
				//remove the effect from the ability
				ability.effectAttrs.RemoveAt(j);
				j-=1;
			}
		}
			
		//if there's no more effect active for the ability, return false, the ability will be removed
		if(ability.effectAttrs.Count==0){
			return false;
		}
		
		return true;
	}
	
	//apply a unit ability to the tile
	public void ApplyAbility(UnitAbility ability){
		//spawn effect if any
		if(ability.effectTarget!=null){
			StartCoroutine(SpawnAbilityEffect(ability.effectTarget, ability.effectTargetDelay, pos));
		}
		
		//loop through the effect list and change the modifier
		//commented effect are disabled for tile
		foreach(EffectAttr effectAttr in ability.effectAttrs){
			if(effectAttr.type==_EffectAttrType.HPDamage){
				effectAttr.value=Random.Range(effectAttr.value, effectAttr.valueAlt);
				HPGainModifier-=(int)effectAttr.value;
				if(unit!=null) unit.ApplyDamage((int)effectAttr.value);
			}
			else if(effectAttr.type==_EffectAttrType.HPGain){
				effectAttr.value=Random.Range(effectAttr.value, effectAttr.valueAlt);
				HPGainModifier+=(int)effectAttr.value;
				if(unit!=null) unit.ApplyHeal((int)effectAttr.value);
			}
			else if(effectAttr.type==_EffectAttrType.APDamage){
				effectAttr.value=Random.Range(effectAttr.value, effectAttr.valueAlt);
			APGainModifier+=(int)effectAttr.value;
				if(unit!=null) unit.GainAP(-(int)effectAttr.value);
			}
			else if(effectAttr.type==_EffectAttrType.APGain){
				effectAttr.value=Random.Range(effectAttr.value, effectAttr.valueAlt);
				APGainModifier-=(int)effectAttr.value;
				if(unit!=null) unit.GainAP((int)effectAttr.value);
			}
			else if(effectAttr.type==_EffectAttrType.Damage){
				damageModifier+=(int)effectAttr.value;
			}
			//~ else if(effectAttr.type==_EffectAttrType.MovementRange){
				//~ movementModifier+=(int)effectAttr.value;
			//~ }
			else if(effectAttr.type==_EffectAttrType.AttackRange){
				attRangeModifier+=(int)effectAttr.value;
			}
			//~ else if(effectAttr.type==_EffectAttrType.Speed){
				//~ speedModifier+=(int)effectAttr.value;
			//~ }
			else if(effectAttr.type==_EffectAttrType.HitChance){
				attackModifier+=effectAttr.value;
			}
			else if(effectAttr.type==_EffectAttrType.DodgeChance){
				defendModifier+=effectAttr.value;
			}
			else if(effectAttr.type==_EffectAttrType.CriticalChance){
				criticalModifier+=effectAttr.value;
			}
			else if(effectAttr.type==_EffectAttrType.CriticalImmunity){
				critDefModifier+=effectAttr.value;
			}
			//~ else if(effectAttr.type==_EffectAttrType.ExtraAttack){
			
			//~ }
			//~ else if(effectAttr.type==_EffectAttrType.ExtraCounterAttack){
			
			//~ }
			//~ else if(effectAttr.type==_EffectAttrType.Stun){
			
			//~ }
			//~ else if(effectAttr.type==_EffectAttrType.DisableAttack){
			
			//~ }
			//~ else if(effectAttr.type==_EffectAttrType.DisableMovement){
			
			//~ }
			//~ else if(effectAttr.type==_EffectAttrType.DisableAbility){
			
			//~ }
			//~ else if(effectAttr.type==_EffectAttrType.Teleport){
			
			//~ }
		}
		
		if(activeUnitAbilityEffectList.Count==0){
			SubscribeEvent();
			if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
				countTillNextTurn=UnitControl.activeFactionCount;
			}
			else{
				countTillNextTurn=UnitControl.GetAllUnitCount();
			}
		}
		
		if(GameControlTB.GetTurnMode()==_TurnMode.FactionAllUnitPerTurn){
			ability.countTillNextTurn=UnitControl.activeFactionCount;
		}
		else{
			ability.countTillNextTurn=UnitControl.GetAllUnitCount();
		}
		
		//add the ability to the list so it can be keep tracked
		activeUnitAbilityEffectList.Add(ability);
	}
	
	IEnumerator SpawnAbilityEffect(GameObject effect, float delay, Vector3 pos){
		yield return new WaitForSeconds(delay);
		Instantiate(effect, pos, effect.transform.rotation);
	}
	
	void SubscribeEvent(){
		//GameControlTB.onNewRoundE += OnNewRound;
		GameControlTB.onNextTurnE += OnNextTurn;
		UnitTB.onUnitDestroyedE += OnUnitDestroyed;
	}
	void UnsubscribeEvent(){
		//GameControlTB.onNewRoundE -= OnNewRound;
		GameControlTB.onNextTurnE -= OnNextTurn;
		UnitTB.onUnitDestroyedE -= OnUnitDestroyed;
	}
	
	
	
//******************************************************************************************************************************//
//A* related code section	
	
	//call during a serach to scan through neighbour, check their score against the position passed
	//process walkable neighbours only, used to search for a walkable path via A*
	public void ProcessWalkableNeighbour(Tile tile){
		for(int i=0; i<neighbours.Count; i++){
			if((neighbours[i].walkable && neighbours[i].unit==null) || neighbours[i]==tile){
				//if the neightbour state is clean (never evaluated so far in the search)
				if(neighbours[i].listState==_ListState.Unassigned){
					//check the score of G and H and update F, also assign the parent to currentNode
					neighbours[i].scoreG=scoreG+1;
					neighbours[i].scoreH=Vector3.Distance(neighbours[i].thisT.position, tile.thisT.position);
					neighbours[i].UpdateScoreF();
					neighbours[i].parent=this;
				}
				//if the neighbour state is open (it has been evaluated and added to the open list)
				else if(neighbours[i].listState==_ListState.Open){
					//calculate if the path if using this neighbour node through current node would be shorter compare to previous assigned parent node
					tempScoreG=scoreG+1;
					if(neighbours[i].scoreG>tempScoreG){
						//if so, update the corresponding score and and reassigned parent
						neighbours[i].parent=this;
						neighbours[i].scoreG=tempScoreG;
						neighbours[i].UpdateScoreF();
					}
				}
			}
		}
	}
	
	
	//call during a serach to scan through neighbour tile, check their score against the position passed
	//process all neighbours regardless of status, used to calculate distance via A*
	public void ProcessAllNeighbours(Tile tile){ 
		Vector3 pos=tile.pos;
		for(int i=0; i<neighbours.Count; i++){
			//if the neightbour state is clean (never evaluated so far in the search)
			if(neighbours[i].listState==_ListState.Unassigned){
				//check the score of G and H and update F, also assign the parent to currentNode
				neighbours[i].scoreG=scoreG+1;
				neighbours[i].scoreH=Vector3.Distance(neighbours[i].thisT.position, pos);
				neighbours[i].UpdateScoreF();
				neighbours[i].parent=this;
			}
			//if the neighbour state is open (it has been evaluated and added to the open list)
			else if(neighbours[i].listState==_ListState.Open){
				//calculate if the path if using this neighbour node through current node would be shorter compare to previous assigned parent node
				tempScoreG=scoreG+1;
				if(neighbours[i].scoreG>tempScoreG){
					//if so, update the corresponding score and and reassigned parent
					neighbours[i].parent=this;
					neighbours[i].scoreG=tempScoreG;
					neighbours[i].UpdateScoreF();
				}
			}
		}
	}
	
	void UpdateScoreF(){
		scoreF=scoreG+scoreH;
	}
	
	
	
	
//******************************************************************************************************************************//
//cover system related code section	
	
	
	void InitCover(){
		if(type==_TileType.Square) InitCoverSquare();
		else if(type==_TileType.Hex) InitCoverHex();
	}
	void InitCoverHex(){
		if(cover.Count!=6){
			cover=new List<_CoverType>();
			for(int i=0; i<6; i++) cover.Add(_CoverType.None);
			
			overlayPos=new List<Vector3>();
			for(int i=0; i<6; i++) overlayPos.Add(new Vector3(50000, 50000, 50000));
		}
		
		if(gameObject.name=="HexTile60" || gameObject.name=="HexTile61"){
			Debug.Log(name+"  "+neighbours.Count);
		}
		
		List<Tile> adjacentTile=new List<Tile>();
		foreach(Tile tile in neighbours) adjacentTile.Add(tile);
		foreach(Tile tile in disconnectedNeighbour) adjacentTile.Add(tile);
		
		LayerMask mask=1<<LayerManager.GetLayerObstacle();
		foreach(Tile tile in adjacentTile){
			int neighbourID=0;
			Vector2 dir=new Vector2(tile.transform.position.x-pos.x, tile.transform.position.z-pos.z);
			float angle=Utility.VectorToAngle(dir);
			if(angle>0 && angle<60) neighbourID=0;
			else if(angle>60 && angle<120) neighbourID=1;
			else if(angle>120 && angle<180) neighbourID=2;
			else if(angle>180 && angle<240) neighbourID=3;
			else if(angle>240 && angle<300) neighbourID=4;
			else if(angle>300 && angle<360) neighbourID=5;
			
			float dist=Vector3.Distance(pos, tile.pos);
			Vector3 direction=tile.pos-pos;
			
			float gridSize=GridManager.GetTileSize();
			overlayPos[neighbourID]=pos+(direction.normalized)*gridSize*0.475f+new Vector3(0, gridSize*0.3f, 0);
			
			RaycastHit hit;
			if(Physics.Raycast(pos, direction.normalized, out hit, dist, mask)){
				Obstacle obs=hit.collider.gameObject.GetComponent<Obstacle>();
				if(obs!=null){
					cover[neighbourID]=obs.coverType;
				}
			}
		}
	}
	void InitCoverSquare(){
		if(cover.Count!=4){
			cover=new List<_CoverType>();
			for(int i=0; i<4; i++) cover.Add(_CoverType.None);
			
			overlayPos=new List<Vector3>();
			for(int i=0; i<4; i++) overlayPos.Add(new Vector3(50000, 50000, 50000));
		}
		
		List<Tile> adjacentTile=new List<Tile>();
		foreach(Tile tile in neighbours) adjacentTile.Add(tile);
		foreach(Tile tile in disconnectedNeighbour) adjacentTile.Add(tile);
		
		LayerMask mask=1<<LayerManager.GetLayerObstacle();
		foreach(Tile tile in adjacentTile){
			int neighbourID=0;
			Vector2 dir=new Vector2(tile.transform.position.x-pos.x, tile.transform.position.z-pos.z);
			float angle=Utility.VectorToAngle(dir);
			if(angle>45 && angle<135) neighbourID=1;
			else if(angle>135 && angle<225) neighbourID=2;
			else if(angle>225 && angle<315) neighbourID=3;
			else neighbourID=0;
			
			float dist=Vector3.Distance(pos, tile.pos);
			Vector3 direction=tile.pos-pos;
			
			float gridSize=GridManager.GetTileSize();
			overlayPos[neighbourID]=pos+(direction.normalized)*gridSize*0.475f+new Vector3(0, gridSize*0.3f, 0);
			
			RaycastHit hit;
			if(Physics.Raycast(pos, direction.normalized, out hit, dist, mask)){
				Obstacle obs=hit.collider.gameObject.GetComponent<Obstacle>();
				if(obs!=null){
					cover[neighbourID]=obs.coverType;
				}
			}
		}
	}
	
	
	public float GetCoverDefendBonus(Vector3 posS){
		if(type==_TileType.Square) return GetCoverDefendBonusSquare(posS);
		else if(type==_TileType.Hex) return GetCoverDefendBonusHex(posS);
		return 0;
	}
	public float GetCoverDefendBonusHex(Vector3 posS){
		if(cover.Count<6){
			Debug.Log("error getting cover, cover value not setup properly");
			return 0;
		}
		
		Vector2 dir=new Vector2(posS.x-pos.x, posS.z-pos.z);
		float angle=Utility.VectorToAngle(dir);
		int coverType=0;
		if(angle==0) coverType=Mathf.Max((int)cover[0], (int)cover[5]);
		else if(angle==60) coverType=Mathf.Max((int)cover[0], (int)cover[1]);
		else if(angle==120) coverType=Mathf.Max((int)cover[1], (int)cover[2]);
		else if(angle==180) coverType=Mathf.Max((int)cover[2], (int)cover[3]);
		else if(angle==240) coverType=Mathf.Max((int)cover[3], (int)cover[4]);
		else if(angle==300) coverType=Mathf.Max((int)cover[4], (int)cover[5]);
		else if(angle>0 && angle<60) coverType=(int)cover[0];
		else if(angle>60 && angle<120) coverType=(int)cover[1];
		else if(angle>120 && angle<180) coverType=(int)cover[2];
		else if(angle>180 && angle<240) coverType=(int)cover[3];
		else if(angle>240 && angle<300) coverType=(int)cover[4];
		else if(angle>300 && angle<360) coverType=(int)cover[5];
		else Debug.Log("error getting cover, angle exceed 360");
		
		if((_CoverType)coverType==_CoverType.BlockHalf) return GameControlTB.GetCoverHalf();
		else if((_CoverType)coverType==_CoverType.BlockFull) return GameControlTB.GetCoverFull();
		
		return 0;
	}
	public float GetCoverDefendBonusSquare(Vector3 posS){
		if(cover.Count<4){
			Debug.Log("error getting cover, cover value not setup properly");
			return 0;
		}
		
		Vector2 dir=new Vector2(posS.x-pos.x, posS.z-pos.z);
		float angle=Utility.VectorToAngle(dir);
		int coverType=0;
		if(angle==45) coverType=Mathf.Max((int)cover[0], (int)cover[1]);
		else if(angle==135) coverType=Mathf.Max((int)cover[1], (int)cover[2]);
		else if(angle==225) coverType=Mathf.Max((int)cover[2], (int)cover[3]);
		else if(angle==315) coverType=Mathf.Max((int)cover[3], (int)cover[0]);
		else if(angle>45 && angle<135) coverType=(int)cover[1];
		else if(angle>135 && angle<225) coverType=(int)cover[2];
		else if(angle>225 && angle<315) coverType=(int)cover[3];
		else if(angle>=0 && angle<45 || angle>315 && angle<360) coverType=(int)cover[0];
		else Debug.Log("error getting cover, angle exceed 360");
		
		if((_CoverType)coverType==_CoverType.BlockHalf) return GameControlTB.GetCoverHalf();
		else if((_CoverType)coverType==_CoverType.BlockFull) return GameControlTB.GetCoverFull();
		
		return 0;
	}
	
	public _CoverType GetCoverType(Vector3 posS){
		if(type==_TileType.Square) return GetCoverTypeSquare(posS);
		else if(type==_TileType.Hex) return GetCoverTypeHex(posS);
		return _CoverType.None;
	}
	public _CoverType GetCoverTypeHex(Vector3 posS){
		if(cover.Count<6){
			Debug.Log("error getting cover, cover value not setup properly");
			return 0;
		}
		
		Vector2 dir=new Vector2(posS.x-pos.x, posS.z-pos.z);
		float angle=Utility.VectorToAngle(dir);
		int coverType=0;
		if(angle==0) coverType=Mathf.Max((int)cover[0], (int)cover[5]);
		else if(angle==60) coverType=Mathf.Max((int)cover[0], (int)cover[1]);
		else if(angle==120) coverType=Mathf.Max((int)cover[1], (int)cover[2]);
		else if(angle==180) coverType=Mathf.Max((int)cover[2], (int)cover[3]);
		else if(angle==240) coverType=Mathf.Max((int)cover[3], (int)cover[4]);
		else if(angle==300) coverType=Mathf.Max((int)cover[4], (int)cover[5]);
		else if(angle>0 && angle<60) coverType=(int)cover[0];
		else if(angle>60 && angle<120) coverType=(int)cover[1];
		else if(angle>120 && angle<180) coverType=(int)cover[2];
		else if(angle>180 && angle<240) coverType=(int)cover[3];
		else if(angle>240 && angle<300) coverType=(int)cover[4];
		else if(angle>300 && angle<360) coverType=(int)cover[5];
		else Debug.Log("error getting cover, angle exceed 360");
		
		return (_CoverType)coverType;
	}
	public _CoverType GetCoverTypeSquare(Vector3 posS){
		if(cover.Count<4){
			Debug.Log("error getting cover, cover value not setup properly");
			return 0;
		}
		
		Vector2 dir=new Vector2(posS.x-pos.x, posS.z-pos.z);
		float angle=Utility.VectorToAngle(dir);
		int coverType=0;
		if(angle==45) coverType=Mathf.Max((int)cover[0], (int)cover[1]);
		else if(angle==135) coverType=Mathf.Max((int)cover[1], (int)cover[2]);
		else if(angle==225) coverType=Mathf.Max((int)cover[2], (int)cover[3]);
		else if(angle==315) coverType=Mathf.Max((int)cover[3], (int)cover[0]);
		else if(angle>45 && angle<135) coverType=(int)cover[1];
		else if(angle>135 && angle<225) coverType=(int)cover[2];
		else if(angle>225 && angle<315) coverType=(int)cover[3];
		else if(angle>=0 && angle<45 || angle>315 && angle<360) coverType=(int)cover[0];
		else Debug.Log("error getting cover, angle exceed 360");
		
		return (_CoverType)coverType;
	}
	
	public bool GotCover(){
		bool covered=false;
		for(int i=0; i<cover.Count; i++){
			if(cover[i]!=_CoverType.None){
				covered=true;
				break;
			}
		}
		return covered;
	}
	
	
	
	
}
