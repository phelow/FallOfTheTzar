using UnityEngine;
using System.Collections;

public class UnitTBAnimation : MonoBehaviour {

	[HideInInspector] public GameObject meleeAttackAniBody;
	[HideInInspector] public Animation meleeAttackAnimation;
	[HideInInspector] public AnimationClip[] meleeAttackAniClip;
	[HideInInspector] public float[] meleeAttackAniDelay;
	
	[HideInInspector] public GameObject rangeAttackAniBody;
	[HideInInspector] public Animation rangeAttackAnimation;
	[HideInInspector] public AnimationClip[] rangeAttackAniClip;
	[HideInInspector] public float[] rangeAttackAniDelay;
	
	[HideInInspector] public GameObject idleAniBody;
	[HideInInspector] public Animation idleAnimation;
	[HideInInspector] public AnimationClip[] idleAniClip;
	
	[HideInInspector] public GameObject moveAniBody;
	[HideInInspector] public Animation moveAnimation;
	[HideInInspector] public AnimationClip[] moveAniClip;
	
	//~ [HideInInspector] public GameObject rotateLAniBody;
	//~ [HideInInspector] public Animation rotateLAnimation;
	//~ [HideInInspector] public AnimationClip[] rotateLAniClip;
	
	//~ [HideInInspector] public GameObject rotateRAniBody;
	//~ [HideInInspector] public Animation rotateRAnimation;
	//~ [HideInInspector] public AnimationClip[] rotateRAniClip;
	
	[HideInInspector] public GameObject hitAniBody;
	[HideInInspector] public Animation hitAnimation;
	[HideInInspector] public AnimationClip[] hitAniClip;
	
	[HideInInspector] public GameObject destroyAniBody;
	[HideInInspector] public Animation destroyAnimation;
	[HideInInspector] public AnimationClip[] destroyAniClip;
	[HideInInspector] public float[] destroyAniDelay;
	
	public void Start(){
		InitAnimation();
	}
	
	public void InitAnimation(){
		if(meleeAttackAniBody!=null){
			meleeAttackAnimation=meleeAttackAniBody.GetComponent<Animation>();
			if(meleeAttackAnimation==null) meleeAttackAnimation=meleeAttackAniBody.AddComponent<Animation>();
			if(meleeAttackAniClip!=null && meleeAttackAniClip.Length>0){
				foreach(AnimationClip clip in meleeAttackAniClip){
					if(clip!=null){
						meleeAttackAnimation.AddClip(clip, clip.name);
						meleeAttackAnimation.animation[clip.name].layer=1;
						meleeAttackAnimation.animation[clip.name].wrapMode=WrapMode.Once;
						meleeAttackAnimation.animation[clip.name].speed=1;
					}
				}
				
				if(meleeAttackAniDelay.Length!=meleeAttackAniClip.Length){
					float[] temp=new float[meleeAttackAniClip.Length];
					for(int i=0; i<meleeAttackAniClip.Length; i++){
						if(i<meleeAttackAniDelay.Length) temp[i]=meleeAttackAniDelay[i];
						else temp[i]=meleeAttackAniClip[i].length;
					}
					meleeAttackAniDelay=temp;
				}
			}
		}
		
		if(rangeAttackAniBody!=null){
			rangeAttackAnimation=rangeAttackAniBody.GetComponent<Animation>();
			if(rangeAttackAnimation==null) rangeAttackAnimation=rangeAttackAniBody.AddComponent<Animation>();
			if(rangeAttackAniClip!=null && rangeAttackAniClip.Length>0){
				foreach(AnimationClip clip in rangeAttackAniClip){
					if(clip!=null){
						rangeAttackAnimation.AddClip(clip, clip.name);
						rangeAttackAnimation.animation[clip.name].layer=1;
						rangeAttackAnimation.animation[clip.name].wrapMode=WrapMode.Once;
						rangeAttackAnimation.animation[clip.name].speed=1;
					}
				}
				
				if(rangeAttackAniDelay.Length!=rangeAttackAniClip.Length){
					float[] temp=new float[rangeAttackAniClip.Length];
					for(int i=0; i<rangeAttackAniClip.Length; i++){
						if(i<rangeAttackAniDelay.Length) temp[i]=rangeAttackAniDelay[i];
						else temp[i]=rangeAttackAniClip[i].length;
					}
					rangeAttackAniDelay=temp;
				}
			}
		}
		
		
		if(idleAniBody!=null){
			idleAnimation=idleAniBody.GetComponent<Animation>();
			if(idleAnimation==null) idleAnimation=idleAniBody.AddComponent<Animation>();
			if(idleAniClip!=null && idleAniClip.Length>0){
				foreach(AnimationClip clip in idleAniClip){
					if(clip!=null){
						idleAnimation.AddClip(clip, clip.name);
						idleAnimation.animation[clip.name].layer=0;
						idleAnimation.animation[clip.name].wrapMode=WrapMode.Loop;
						idleAnimation.animation[clip.name].speed=1;
					}
				}
			}
		}
		
		
		if(moveAniBody!=null){
			moveAnimation=moveAniBody.GetComponent<Animation>();
			if(moveAnimation==null) moveAnimation=moveAniBody.AddComponent<Animation>();
			if(moveAniClip!=null && moveAniClip.Length>0){
				foreach(AnimationClip clip in moveAniClip){
					if(clip!=null){
						moveAnimation.AddClip(clip, clip.name);
						moveAnimation.animation[clip.name].layer=1;
						moveAnimation.animation[clip.name].wrapMode=WrapMode.Loop;
						moveAnimation.animation[clip.name].speed=1;
					}
				}
			}
		}
		
		
		if(hitAniBody!=null){
			hitAnimation=hitAniBody.GetComponent<Animation>();
			if(hitAnimation==null) hitAnimation=hitAniBody.AddComponent<Animation>();
			if(hitAniClip!=null && hitAniClip.Length>0){
				foreach(AnimationClip clip in hitAniClip){
					if(clip!=null){
						hitAnimation.AddClip(clip, clip.name);
						hitAnimation.animation[clip.name].layer=1;
						hitAnimation.animation[clip.name].wrapMode=WrapMode.Once;
						hitAnimation.animation[clip.name].speed=1;
					}
				}
			}
		}
		
		if(destroyAniBody!=null){
			destroyAnimation=destroyAniBody.GetComponent<Animation>();
			if(destroyAnimation==null) destroyAnimation=destroyAniBody.AddComponent<Animation>();
			if(destroyAniClip!=null && destroyAniClip.Length>0){
				foreach(AnimationClip clip in destroyAniClip){
					if(clip!=null){
						destroyAnimation.AddClip(clip, clip.name);
						destroyAnimation.animation[clip.name].layer=1;
						destroyAnimation.animation[clip.name].wrapMode=WrapMode.Clamp;
						destroyAnimation.animation[clip.name].speed=1;
					}
				}
				
				if(destroyAniDelay.Length!=destroyAniClip.Length){
					float[] temp=new float[destroyAniClip.Length];
					for(int i=0; i<destroyAniClip.Length; i++){
						if(i<destroyAniDelay.Length) temp[i]=destroyAniDelay[i];
						else temp[i]=destroyAniClip[i].length;
					}
					destroyAniDelay=temp;
				}
			}
		}
		
		
		StartCoroutine(DelayIdleAnimation());
	}
	
	IEnumerator DelayIdleAnimation(){
		yield return new WaitForSeconds(Random.Range(0f, 1.5f));
		if(idleAnimation!=null && idleAniClip.Length>0){
			int rand=Random.Range(0, idleAniClip.Length-1);
			idleAnimation.Play(idleAniClip[rand].name);
		}
	}
	
	public float PlayMeleeAttack(){
		if(meleeAttackAniBody!=null && meleeAttackAniClip!=null && meleeAttackAniClip.Length>0){
			int rand=Random.Range(0, meleeAttackAniClip.Length-1);
			meleeAttackAnimation.CrossFade(meleeAttackAniClip[rand].name);
			return meleeAttackAniDelay[rand];
		}
		return 0;
	}
	
	public float PlayRangeAttack(){
		if(rangeAttackAniBody!=null && rangeAttackAniClip!=null && rangeAttackAniClip.Length>0){
			int rand=Random.Range(0, rangeAttackAniClip.Length-1);
			rangeAttackAnimation.CrossFade(rangeAttackAniClip[rand].name);
			return rangeAttackAniDelay[rand];
		}
		return 0;
	}
	
	public bool PlayMove(){
		if(moveAniBody!=null && moveAniClip!=null && moveAniClip.Length>0){
			moveAnimation.CrossFade(moveAniClip[Random.Range(0, moveAniClip.Length-1)].name);
			return true;
		}
		return false;
	}
	
	public bool PlayHit(){
		if(hitAniBody!=null && hitAniClip!=null && hitAniClip.Length>0){
			hitAnimation.CrossFade(hitAniClip[Random.Range(0, hitAniClip.Length-1)].name);
			return true;
		}
		return false;
	}
	
	public float PlayDestroyed(){
		if(idleAnimation!=null) idleAnimation.Stop();
		
		if(destroyAniBody!=null && destroyAniClip!=null && destroyAniClip.Length>0){
			int rand=Random.Range(0, destroyAniClip.Length-1);
			destroyAnimation.Play(destroyAniClip[rand].name);
			return destroyAniDelay[rand];
		}
		return 0;
	}
	
	public void PlayIdle(){
		if(idleAnimation!=null && destroyAniClip!=null && idleAniClip.Length>0){
			int rand=Random.Range(0, idleAniClip.Length-1);
			idleAnimation.Play(idleAniClip[rand].name);
		}
	}
	
	public void StopMove(){
		if(moveAnimation!=null) moveAnimation.Stop();
		
		PlayIdle();
	}
}
