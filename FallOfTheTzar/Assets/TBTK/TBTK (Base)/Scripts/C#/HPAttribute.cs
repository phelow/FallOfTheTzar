/*

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class UnitHPAttribute{
	public float fullHPDefault=10;
	public float HPGainPerTurn=0;
	[HideInInspector] public float fullHP=10;
	[HideInInspector] public float HP=10;
	
	public float fullShieldDefault=0;
	[HideInInspector] public float fullShield=0;
	[HideInInspector] public float shield=10;
	
	public float shieldRechargeRate=0.5f;
	public float shieldStagger=3;
	
	[HideInInspector] public float lastHitTime=0;
	
	public Transform overlayHP;
	public Transform overlayShield;
	public Transform overlayBase;
	public bool alwaysShowOverlay=false;
	
	private Transform cam;
	private bool overlayIsVisible=false;
	
	private Vector3 overlayScaleH;
	private Vector3 overlayScaleS;
	
	private Vector3 overlayPosH;
	private Vector3 overlayPosS;
	
	private Renderer overlayRendererH;
	private Renderer overlayRendererS;
	
	private bool neverShowBase=false;
	
	private Transform rootTransform;
	
	public void Init(Transform transform, int subClass){
		fullHPDefault=Mathf.Max(0, fullHPDefault);
		fullShieldDefault=Mathf.Max(0, fullShieldDefault);
		
		float globalModifierH=1, globalModifierS=1;
		//~ if(subClass==1){
			//~ globalModifierH=GlobalStatsModifier.CreepHP;
			//~ globalModifierS=GlobalStatsModifier.CreepShield;
		//~ }
		//~ else if(subClass==2){
			//~ globalModifierH=GlobalStatsModifier.TowerHP;
			//~ globalModifierS=GlobalStatsModifier.TowerShield;
		//~ }
		
		fullHP=fullHPDefault * globalModifierH;
		HP=fullHP;
		fullShield=fullShieldDefault * globalModifierS;
		shield=fullShield;
		
		rootTransform=transform;
		cam=Camera.main.transform;
		
		if(overlayBase==null){
			if(overlayHP){
				overlayBase=GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
				overlayBase.position=overlayHP.position;
				overlayBase.rotation=overlayHP.rotation;
			}
			else if(overlayShield){
				overlayBase=GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
				overlayBase.position=overlayShield.position;
				overlayBase.rotation=overlayShield.rotation;
			}
			else return;
			
			Utility.DestroyColliderRecursively(overlayBase);
			overlayBase.renderer.enabled=false;
			overlayBase.parent=transform; 
			
			neverShowBase=true;
		}
		
		offsetB=overlayBase.localPosition;
		
		if(overlayHP) scaleModifierH=Utility.GetWorldScale(overlayHP).x*5;
		if(overlayShield) scaleModifierS=Utility.GetWorldScale(overlayShield).x*5;
		
		if(overlayHP!=null) {
			//~ overlayHP.gameObject.layer=LayerManager.LayerOverlay();
			overlayRendererH=overlayHP.renderer;
			overlayHP.parent=overlayBase;
			overlayScaleH=overlayHP.localScale;
			offsetH=overlayHP.localPosition;
			if(alwaysShowOverlay) overlayRendererH.enabled=true;
			else overlayRendererH.enabled=false;
		}
		if(overlayShield!=null) {
			//~ overlayShield.gameObject.layer=LayerManager.LayerOverlay();
			overlayRendererS=overlayShield.renderer;
			overlayShield.parent=overlayBase;
			overlayScaleS=overlayShield.localScale;
			offsetS=overlayShield.localPosition;
			if(alwaysShowOverlay) overlayRendererS.enabled=true;
			else overlayRendererS.enabled=false;
		}
		
		if(alwaysShowOverlay){
			overlayIsVisible=true;
		}
		
	}
	
	public void Reset(){
		float globalModifierH=1, globalModifierS=1;
		
		fullHP=fullHPDefault*globalModifierH;
		HP=fullHP;
		
		fullShield=fullShieldDefault*globalModifierS;
		shield=fullShield;
		
		UpdateOverlay();
	}
	
	public void GainHP(float val){
		HP=Mathf.Min(fullHP, HP+=val);
		UpdateOverlay();
	}
	
	public void ApplyDamage(float dmg){
		lastHitTime=Time.time;
		
		if(shield>0){
			if(dmg>shield){
				dmg-=shield;
				shield=0;
			}
			else{
				shield-=dmg;
			}
		}
		
		if(shield==0) HP-=dmg;
		
		//Debug.Log(dmg+"   "+HP+"   "+shield);
		
		HP=Mathf.Clamp(HP, 0, fullHP);
		UpdateOverlay();
	}
	
	public void ApplyShieldDamage(float dmg){
		lastHitTime=Time.time;
		shield=Mathf.Max(0, shield-dmg);
		UpdateOverlay();
	}
	
	public void BreakShield(){
		lastHitTime=Time.time;
		shield=0;
		fullShield=0;
	}
	
	public IEnumerator ShieldRoutine(){
		if(fullShield<=0) yield break;
		
		while(fullShield>0){
			//staggered, stop recharging
			if(Time.time-lastHitTime<shieldStagger){
				yield return null;
			}
			//recharging
			else{
				if(shield<fullShield){
					shield=Mathf.Min(fullShield, shield+Time.deltaTime*shieldRechargeRate);
					UpdateOverlay();
					yield return null;
				}
				else{
					yield return null;
				}
			}
		}
	}
	
	private Vector3 offsetS;
	private Vector3 offsetH;
	private Vector3 offsetB;
	
	private float scaleModifierH=1;
	private float scaleModifierS=1;
	
	public IEnumerator Update(){
				
		if(!overlayHP && !overlayShield) yield break;
		
		Quaternion offset=Quaternion.Euler(-90, 0, 0);
		
		//~ float scaleModifierH=1;
		//~ float scaleModifierS=1;
		
		
		while(true){
			if(overlayIsVisible){
				if(overlayHP || overlayBase){
					Quaternion rot=cam.rotation*offset;
					
					overlayBase.rotation=rot;
					Vector3 dirRight=overlayBase.TransformDirection(-Vector3.right);
					
					if(overlayHP){
						overlayHP.localPosition=offsetH;
						float dist=((fullHP-HP)/fullHP)*scaleModifierH;
						overlayHP.Translate(dirRight*dist, Space.World);
						overlayHP.localRotation=Quaternion.Euler(0, 0, 0);
					}
					
					if(overlayShield && fullShieldDefault>0){
						overlayShield.localPosition=offsetS;
						float dist=((fullShield-shield)/fullShield)*scaleModifierS;
						overlayShield.Translate(dirRight*dist, Space.World);
						overlayShield.localRotation=Quaternion.Euler(0, 0, 0);
					}
				}
			}
			
			yield return null;
		}
	}
	
	
	public void UpdateOverlay(){
		
		if(!overlayHP && !overlayShield) return;
		
		if(!alwaysShowOverlay){
			if(fullShieldDefault>0 && overlayShield){
				if(shield>=fullShield){
					if(!overlayHP || HP>=fullHP)
						overlayShield.renderer.enabled=false;
				}
				else if(shield<=0) overlayShield.renderer.enabled=false;
				else{
					overlayShield.renderer.enabled=true;
				}
			}
			
			if(overlayHP){
				if(!overlayShield){
					if(HP>=fullHP) overlayRendererH.enabled=false;
					else if(HP<=0) overlayRendererH.enabled=false;
					else{
						overlayRendererH.enabled=true;
					}
				}
				else{
					if(fullShield>0 && shield<fullShield){
						overlayHP.renderer.enabled=true;
					}
					else{
						if(HP>=fullHP) overlayRendererH.enabled=false;
						else if(HP<=0) overlayRendererH.enabled=false;
						else{
							overlayRendererH.enabled=true;
						}
					}
				}
			}
			
			if(overlayBase && !neverShowBase){
				if((overlayHP && overlayRendererH.enabled) || (overlayShield && overlayShield.renderer.enabled)){
					overlayBase.renderer.enabled=true;
				}
				else{
					if(HP>=fullHP) overlayBase.renderer.enabled=false;
				}
			}
			
			if((overlayHP && overlayRendererH.enabled) || (overlayShield && overlayShield.renderer.enabled)){
				overlayIsVisible=true;
			}
			else{
				overlayIsVisible=false;
			}
		}
		
		if(overlayHP && overlayRendererH.enabled){
			Vector3 scale=new Vector3(HP/fullHP*overlayScaleH.x, overlayScaleH.y, overlayScaleH.z);
			overlayHP.localScale=scale;
		}
		if(overlayShield && overlayRendererS.enabled){
			Vector3 scale=new Vector3(shield/fullShield*overlayScaleS.x, overlayScaleS.y, overlayScaleS.z);
			overlayShield.localScale=scale;
		}
		
	}
	
	public void ClearParent(){
		if(overlayBase) overlayBase.parent=null;
	}
	
	public void RestoreParent(){
		if(overlayBase){
			overlayBase.parent=rootTransform;
			overlayBase.localPosition=offsetB;
		}
	}
	
}


*/