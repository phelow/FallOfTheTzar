using UnityEngine;
using System.Collections;

public class UnitTBAudio : MonoBehaviour {

	//[HideInInspector] 
	public AudioClip selectSound;
	//[HideInInspector] 
	public bool loopMoveSound;
	//[HideInInspector] 
	public AudioClip moveSound;
	//[HideInInspector] 
	public AudioClip meleeAttackSound;
	//[HideInInspector] 
	public AudioClip rangeAttackSound;
	//[HideInInspector] 
	public AudioClip hitSound;
	//[HideInInspector] 
	public AudioClip missedSound;
	//[HideInInspector] 
	public AudioClip destroySound;
	
	private Transform thisT;
	
	//a unique ID indicating which audioSource is playing the move sound
	//this is used to stop the move audioSource when the unit stop moving, only applicable if the move sound is looped
	private int moveAudioID=-1;
	
	public void Awake(){
		thisT=transform;
	}
	
	public void PlaySelect(){
		if(selectSound!=null) AudioManager.PlaySound(selectSound);
	}
	
	public void PlayMove(){
		if(moveAudioID>=0) return;
		if(moveSound!=null){
			if(loopMoveSound)
				moveAudioID=AudioManager.PlaySoundLoop(moveSound, thisT);
			else
				moveAudioID=AudioManager.PlaySound(moveSound, thisT);
		}
	}
	
	public void PlayMeleeAttack(){
		if(meleeAttackSound!=null) AudioManager.PlaySound(meleeAttackSound, thisT.position);
	}
	
	public void PlayRangeAttack(){
		if(rangeAttackSound!=null) AudioManager.PlaySound(rangeAttackSound, thisT.position);
	}
	
	public void PlayHit(){
		if(hitSound!=null) AudioManager.PlaySound(hitSound, thisT.position);
	}
	
	public void PlayMissed(){	//for melee only
		if(missedSound!=null) AudioManager.PlaySound(missedSound, thisT.position);
	}
	
	public void PlayDestroy(){
		if(destroySound!=null) AudioManager.PlaySound(destroySound, thisT.position);
	}
	
	public void StopMove(){
		if(moveAudioID<0) return;
		AudioManager.StopSound(moveAudioID);
		moveAudioID=-1;
	}
}
