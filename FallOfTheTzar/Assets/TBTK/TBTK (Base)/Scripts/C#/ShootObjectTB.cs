using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum _ShootObjectType{Projectile, Missile, Beam, Effect}

public class ShootObjectTB : MonoBehaviour {

	public _ShootObjectType type;
	
	public float speed=5;
	
	private Tile abilityTargetTile;
	private List<Tile> targetTileList;
	private UnitTB targetUnit;
	private Tile targetTile;
	private Vector3 targetPos;
	private Vector3 offsetPos;
	
	private AttackInstance attInstance;
	private bool attMissed=false;
	
	public float maxShootAngle=25;
	public float maxShootRange=1;
	private bool hit=false;
	private Transform thisT;
	
	//for effect type only
	public float effectDuration=1f;
	
	public bool showHitEffectWhenMissed=true;
	public GameObject hitEffect;
	
	// Use this for initialization
	void Start () {
		thisT=transform;
	}
	
	
	public void SetAbilityTargetTile(Tile tile){
		abilityTargetTile=tile;
	}
	

	//shoot at unit
	public void Shoot(UnitTB tgt, AttackInstance aInstance){
		Shoot(tgt, aInstance, false);
	}
	public void Shoot(UnitTB tgt, AttackInstance aInstance, bool missed){
		attMissed=missed;
		
		float val=GridManager.GetTileSize()/(missed ? 2f : 8);
		offsetPos=new Vector3(Random.Range(-val, val), Random.Range(0, val/2), Random.Range(-val, val));
		
		targetUnit=tgt;
		if(tgt!=null) targetPos=targetUnit.GetTargetT().position+offsetPos;
		if(tgt!=null) targetTile=targetUnit.occupiedTile;
		
		Shoot(aInstance);
	}
	//shoot at tile
	public void Shoot(Tile tgt, AttackInstance aInstance){
		float val=GridManager.GetTileSize()/4;
		offsetPos=new Vector3(Random.Range(-val, val), Random.Range(0, val/2), Random.Range(-val, val));
		
		if(tgt.unit!=null) targetUnit=tgt.unit;
		targetTile=tgt;
		targetPos=targetTile.pos+offsetPos;
		
		Shoot(aInstance);
	}
	//for ability shootToCenter
	public void Shoot(Vector3 pos, List<Tile> allTgt, AttackInstance aInstance){
		float val=GridManager.GetTileSize()/4;
		offsetPos=new Vector3(Random.Range(-val, val), Random.Range(0, val/2), Random.Range(-val, val));
		
		targetUnit=null;	targetTile=null;
		targetPos=pos+offsetPos;
		targetTileList=allTgt;
		
		Shoot(aInstance);
	}
	public void Shoot(AttackInstance aInstance){
		thisT=transform;
		
		attInstance=aInstance;
		
		if(type==_ShootObjectType.Projectile) StartCoroutine(ProjectileRoutine());
		else if(type==_ShootObjectType.Missile) StartCoroutine(MissileRoutine());
		else if(type==_ShootObjectType.Beam) StartCoroutine(BeamRoutine());
		else if(type==_ShootObjectType.Effect) StartCoroutine(EffectRoutine());
	}
	
	IEnumerator EffectRoutine(){
		if(shootEffect!=null) Instantiate(shootEffect, thisT.position, thisT.rotation);
		yield return new WaitForSeconds(effectDuration);
		Hit();
	}

	
	private float shootAngleDev=90;
	IEnumerator ProjectileRoutine(){
		if(shootEffect!=null) Instantiate(shootEffect, thisT.position, thisT.rotation);
		
		float timeShot=Time.time;
		
		//make sure the shootObject is facing the target and adjust the projectile angle
		thisT.LookAt(targetPos);
		float angle=Mathf.Min(1, Vector3.Distance(thisT.position, targetPos)/maxShootRange)*maxShootAngle;
		//clamp the angle magnitude to be less than 45 or less the dist ratio will be off
		thisT.rotation=thisT.rotation*Quaternion.Euler(Mathf.Clamp(-angle, -shootAngleDev, shootAngleDev), 0, 0);
		
		Vector3 startPos=thisT.position;
		float iniRotX=thisT.rotation.eulerAngles.x;
		
		float y=targetPos.y-startPos.y;
		
		//~ if(shootEffect!=null) ObjectPoolManager.Spawn(shootEffect, thisT.position, thisT.rotation);
		
		//while the shootObject havent hit the target
		while(!hit){
			//calculating distance to targetPos
			Vector3 curPos=thisT.position;
			curPos.y=y;
			float currentDist=Vector3.Distance(curPos, targetPos);
			float curDist=Vector3.Distance(thisT.position, targetPos);
			//if the target is close enough, trigger a hit
			if(curDist<0.15f && !hit) Hit();
			
			if(Time.time-timeShot<3.5f){
				//calculate ratio of distance covered to total distance
				float totalDist=Vector3.Distance(startPos, targetPos);
				float invR=1-Mathf.Min(1, currentDist/totalDist);
				
				//use the distance information to set the rotation, 
				//as the projectile approach target, it will aim straight at the target
				Vector3 wantedDir=targetPos-thisT.position;
				if(wantedDir!=Vector3.zero){
					Quaternion wantedRotation=Quaternion.LookRotation(wantedDir);
					float rotX=Mathf.LerpAngle(iniRotX, wantedRotation.eulerAngles.x, invR);
					
					//make y-rotation always face target
					thisT.rotation=Quaternion.Euler(rotX, wantedRotation.eulerAngles.y, wantedRotation.eulerAngles.z);
				}
			}
			else{
				//this shoot time exceed 3.5sec, abort the trajectory and just head to the target
				thisT.LookAt(targetPos);
			}
				
			//move forward
			thisT.Translate(Vector3.forward*Mathf.Min(speed*Time.deltaTime, curDist));
			
			yield return null;
		}
	}
	
	
	
	public float shootAngleY=30;
	public bool missileInitialBoost=true;
	private float missileSpeedModifier=1f;
	IEnumerator MissileSpeedRoutine(){
		missileSpeedModifier=2.5f;
		float duration=0;
		while(duration<1){
			missileSpeedModifier=Mathf.Lerp(2.5f, 1, duration);
			duration+=Time.deltaTime*6f;
			yield return null;
		}
		missileSpeedModifier=1;
	}
	
	IEnumerator MissileRoutine() {
		
		if(missileInitialBoost) StartCoroutine(MissileSpeedRoutine());
		
		if(shootEffect!=null) Instantiate(shootEffect, thisT.position, thisT.rotation);
		
		float timeShot=Time.time;
		
		float randX=Random.Range(-maxShootAngle, -5); //randX=30;
		float randY=Random.Range(-shootAngleY, shootAngleY);
		float randZ=Random.Range(-10f, 10f);
		
		//make sure the shootObject is facing the target and adjust the projectile angle
		thisT.LookAt(targetPos);
		float angle=Mathf.Min(1, Vector3.Distance(thisT.position, targetPos)/maxShootRange)*maxShootAngle;
		thisT.rotation=thisT.rotation*Quaternion.Euler(Mathf.Clamp(-angle+randX, -40, 40), randY, randZ);
		
		Vector3 startPos=thisT.position;
		Quaternion iniRot=thisT.rotation;
		
		float y=targetPos.y-startPos.y;
		
		//~ if(shootEffect!=null) ObjectPoolManager.Spawn(shootEffect, thisT.position, thisT.rotation);
		
		//while the shootObject havent hit the target
		while(!hit){
			//calculating distance to targetPos
			Vector3 curPos=thisT.position;
			curPos.y=y;
			float currentDist=Vector3.Distance(curPos, targetPos);
			float curDist=Vector3.Distance(thisT.position, targetPos);
			//if the target is close enough, trigger a hit
			if(curDist<0.15f && !hit) Hit();
			
			if(Time.time-timeShot<3.5f){
				//calculate ratio of distance covered to total distance
				float totalDist=Vector3.Distance(startPos, targetPos);
				float invR=1-Mathf.Min(1, currentDist/totalDist);
				
				//use the distance information to set the rotation, 
				//as the projectile approach target, it will aim straight at the target
				Vector3 wantedDir=targetPos-thisT.position;
				if(wantedDir!=Vector3.zero){
					Quaternion wantedRotation=Quaternion.LookRotation(wantedDir);
					thisT.rotation=Quaternion.Lerp(iniRot, wantedRotation, invR);
				}
			}
			else{
				//this shoot time exceed 3.5sec, abort the trajectory and just head to the target
				thisT.LookAt(targetPos);
			}
			
			//move forward
			thisT.Translate(Vector3.forward*Mathf.Min(speed*Time.deltaTime*missileSpeedModifier, curDist));
			
			yield return null;
		}
	}
	
	
	public float beamDuration=0.5f;
	public float beamLength=Mathf.Infinity;
	public LineRenderer lineRenderer;
	public GameObject shootEffect;
	IEnumerator BeamRoutine(){
		float dist=Vector3.Distance(thisT.position, targetPos);
		if(beamLength<dist){
			Ray ray=new Ray(thisT.position, (targetPos-thisT.position));
			targetPos=ray.GetPoint(beamLength);
		}
		
		if(shootEffect!=null) Instantiate(shootEffect, thisT.position, thisT.rotation);
		//if(hitEffect!=null) Instantiate(hitEffect, targetPos, Quaternion.identity);
		
		if(lineRenderer!=null){
			lineRenderer.SetPosition(0, thisT.position);
			lineRenderer.SetPosition(1, targetPos);
		}
		
		yield return new WaitForSeconds(beamDuration);

		Hit();
	}
	
	
	
	void Hit(){
		if(type==_ShootObjectType.Effect){
			if(hitEffect!=null){
				if(!attMissed) Instantiate(hitEffect, thisT.position, hitEffect.transform.rotation);
				else if(showHitEffectWhenMissed) Instantiate(hitEffect, thisT.position, hitEffect.transform.rotation);
			}
		}
		else{
			if(hitEffect!=null){
				if(!attMissed) Instantiate(hitEffect, thisT.position, hitEffect.transform.rotation);
				else if(showHitEffectWhenMissed) Instantiate(hitEffect, thisT.position, hitEffect.transform.rotation);
			}
		}
		
		if(attInstance!=null){
			if(attInstance.unitAbility!=null){
				UnitAbility uAB=attInstance.unitAbility;
				
				
				if(attInstance.unitAbility.shootMode==_AbilityShootMode.ShootToAll){
					if(uAB.targetType==_AbilityTargetType.AllUnits){
						if(targetTile.unit!=null){
							StartCoroutine(targetTile.unit.ApplyAbilityDelay(uAB.Clone()));
						}
					}
					if(uAB.targetType==_AbilityTargetType.Friendly){
						if(targetTile.unit!=null && targetTile.unit.factionID==uAB.factionID){
							StartCoroutine(targetTile.unit.ApplyAbilityDelay(uAB.Clone()));
						}
					}
					else if(uAB.targetType==_AbilityTargetType.Hostile){
						if(targetTile.unit!=null && targetTile.unit.factionID!=uAB.factionID){
							StartCoroutine(targetTile.unit.ApplyAbilityDelay(uAB.Clone()));
						}
					}
					else if(uAB.targetType==_AbilityTargetType.AllTile){
						targetTile.ApplyAbility(uAB.Clone());
					}
				}
				else if(uAB.shootMode==_AbilityShootMode.ShootToCenter){
					foreach(Tile tile in targetTileList){
						if(uAB.targetType==_AbilityTargetType.AllUnits){
							if(tile.unit!=null){
								StartCoroutine(tile.unit.ApplyAbilityDelay(uAB.Clone()));
							}
						}
						if(uAB.targetType==_AbilityTargetType.Friendly){
							if(tile.unit!=null && tile.unit.factionID==uAB.factionID){
								StartCoroutine(tile.unit.ApplyAbilityDelay(uAB.Clone()));
							}
						}
						else if(uAB.targetType==_AbilityTargetType.Hostile){
							if(tile.unit!=null && tile.unit.factionID!=uAB.factionID){
								StartCoroutine(tile.unit.ApplyAbilityDelay(uAB.Clone()));
							}
						}
						else if(uAB.targetType==_AbilityTargetType.AllTile){
							tile.ApplyAbility(uAB.Clone());
						}
					}
				}
				
				
				for(int i=0; i<uAB.chainedAbilityIDList.Count; i++){
					UnitAbility unitAB=AbilityManagerTB.GetUnitAbility(uAB.chainedAbilityIDList[i]);
					if(unitAB.requireTargetSelection){
						unitAB.shootMode=_AbilityShootMode.None;
						attInstance.srcUnit.SetActiveAbilityPendingTarget(unitAB);
						GridManager.SetTargetTileSelectMode(abilityTargetTile, unitAB);
						GridManager.OnHoverEnter(abilityTargetTile);
						GridManager.Select(abilityTargetTile);
					}
					else{
						attInstance.srcUnit._ActivateAbility(unitAB);
					}
				}
				
				attInstance.srcUnit.HitTarget(null, null);
			}
			else{
				attInstance.srcUnit.HitTarget(targetUnit, attInstance);
			}
		}
		
		Destroy(gameObject);
	}
	
	/*
	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawLine(transform.position, targetPos);
	}
	*/
}
