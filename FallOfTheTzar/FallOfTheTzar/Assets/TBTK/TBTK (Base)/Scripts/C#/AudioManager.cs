using UnityEngine;
using System.Collections;

[AddComponentMenu("TDTK/Optional/AudioManager")]
public class AudioManager : MonoBehaviour {

	static private AudioObject[] audioObject;
	
	static public AudioManager instance;
	
	static private Transform camT;
	
	public float minFallOffRange=10;
	
	public AudioClip[] musicList;
	public bool playMusic=true;
	public bool shuffle=false;
	public float initialMusicVolume=0.5f;
	private int currentTrackID=0;
	private AudioSource musicSource;
	private Transform listenerT;
	
	public float initialSFXVolume=0.75f;
	public AudioClip gameWonSound;
	public AudioClip gameLostSound;
	public AudioClip newRoundSound;
	

	public AudioClip actionFailedSound;
	
	private GameObject thisObj;
	private Transform thisT;
	
	
	
	static public void PlayGameWonSound(){
		if(instance==null) return;
		if(instance.gameWonSound!=null) PlaySound(instance.gameWonSound);
	}
	
	static public void PlayGameLostSound(){
		if(instance==null) return;
		if(instance.gameLostSound!=null) PlaySound(instance.gameLostSound);
	}
	
	public static void PlayNewRoundSound(int round){
		if(instance==null) return;
		if(instance.newRoundSound!=null) PlaySound(instance.newRoundSound);
	}
	
	static public void PlayActionFailedSound(){
		if(instance==null) return;
		if(instance.actionFailedSound!=null) PlaySound(instance.actionFailedSound);
	}
	
	
	void Awake(){
		thisObj=gameObject;
		thisT=transform;
		
		camT=Camera.main.transform;
		
		if(playMusic && musicList!=null && musicList.Length>0){
			GameObject musicObj=new GameObject();
			musicObj.name="MusicSource";
			musicObj.transform.position=camT.position;
			musicObj.transform.parent=camT;
			musicSource=musicObj.AddComponent<AudioSource>();
			musicSource.loop=false;
			musicSource.playOnAwake=false;
			musicSource.volume=initialMusicVolume;
			
			musicSource.ignoreListenerVolume=true;
			
			if(listenerT!=null){
				musicObj.transform.parent=listenerT;
				musicObj.transform.localPosition=Vector3.zero;
			}
			
			StartCoroutine(MusicRoutine());
		}
		
		audioObject=new AudioObject[20];
		for(int i=0; i<audioObject.Length; i++){
			GameObject obj=new GameObject();
			obj.name="AudioSource"+i;
			
			AudioSource src=obj.AddComponent<AudioSource>();
			src.playOnAwake=false;
			src.loop=false;
			src.minDistance=minFallOffRange;
			
			Transform t=obj.transform;
			t.parent=thisObj.transform;
			
			audioObject[i]=new AudioObject(src, t);
		}
		
		AudioListener.volume=initialSFXVolume;
		
		if(instance==null) instance=this;
	}
	
	static public void Init(){
		if(instance==null){
			GameObject objParent=new GameObject();
			objParent.name="AudioManager";
			instance=objParent.AddComponent<AudioManager>();
		}		
	}
	
	public IEnumerator MusicRoutine(){
		while(true){
			if(shuffle) musicSource.clip=musicList[Random.Range(0, musicList.Length)];
			else{
				musicSource.clip=musicList[currentTrackID];
				currentTrackID+=1;
				if(currentTrackID==musicList.Length) currentTrackID=0;
			}
			
			musicSource.Play();
			
			yield return new WaitForSeconds(musicSource.clip.length);
		}
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	void OnEnable(){
		GameControlTB.onBattleEndE += OnGameOver;
		GameControlTB.onNewRoundE += PlayNewRoundSound;
	}
	
	void OnDisable(){
		GameControlTB.onBattleEndE -= OnGameOver;
		GameControlTB.onNewRoundE -= PlayNewRoundSound;
	}
	
	void OnGameOver(int vicFactionID){
		//if(victory) PlayGameWonSound();
		//else PlayGameLostSound();
		
		if(GameControlTB.playerFactionExisted){
			if(GameControlTB.IsPlayerFaction(vicFactionID)){
				PlayGameWonSound();
			}
			else{
				PlayGameLostSound();
			}
		}
		else{
			PlayGameWonSound();
		}
	}
	
	
	// Update is called once per frame
	void Update () {
	
	}
	
	//check for the next free, unused audioObject
	static private int GetUnusedAudioObject(){
		for(int i=0; i<audioObject.Length; i++){
			if(!audioObject[i].inUse){
				return i;
			}
		}
		
		//if everything is used up, use item number zero
		return 0;
	}
	
	
	//this is a 3D sound that has to be played at a particular position following a particular event
	static public int PlaySound(AudioClip clip, Vector3 pos){
		if(instance==null) Init();
		
		
		int ID=GetUnusedAudioObject();
		
		
		audioObject[ID].inUse=true;
		
		audioObject[ID].thisT.position=pos;
		audioObject[ID].source.clip=clip;
		audioObject[ID].source.loop=false;
		audioObject[ID].source.Play();
		
		float duration=audioObject[ID].source.clip.length;
		
		instance.StartCoroutine(instance.ClearAudioObject(ID, duration));
		
		return ID;
	}
	
	//this is a 3D sound that has to be played at a particular position following a particular event
	static public int PlaySound(AudioClip clip, Transform srcT){
		if(instance==null) Init();
		
		int ID=GetUnusedAudioObject();
		
		audioObject[ID].inUse=true;
		
		//audioObject[ID].thisT.position=pos;
		audioObject[ID].thisT.parent=srcT;
		audioObject[ID].thisT.localPosition=Vector3.zero;
		audioObject[ID].source.clip=clip;
		audioObject[ID].source.loop=false;
		audioObject[ID].source.Play();
		
		float duration=audioObject[ID].source.clip.length;
		
		instance.StartCoroutine(instance.ClearAudioObject(ID, duration));
		
		return ID;
	}
	
	static public int PlaySoundLoop(AudioClip clip, Transform srcT){
		if(instance==null) Init();
		
		int ID=GetUnusedAudioObject();
		
		audioObject[ID].inUse=true;
		
		audioObject[ID].thisT.parent=srcT;
		audioObject[ID].thisT.localPosition=Vector3.zero;
		audioObject[ID].source.clip=clip;
		audioObject[ID].source.loop=true;
		audioObject[ID].source.Play();
		
		return ID;
	}
	
	
	
	//this no position has been given, assume this is a 2D sound
	static public int PlaySound(AudioClip clip){
		if(instance==null) Init();
		
		int ID=GetUnusedAudioObject();
		
		audioObject[ID].inUse=true;
		
		audioObject[ID].source.clip=clip;
		audioObject[ID].source.loop=false;
		audioObject[ID].source.Play();
		
		audioObject[ID].thisT.position=camT.position;
		
		float duration=audioObject[ID].source.clip.length;
		
		instance.StartCoroutine(instance.ClearAudioObject(ID, duration));
		
		return ID;
	}
	
	static public int PlaySoundLoop(AudioClip clip){
		if(instance==null) Init();
		
		int ID=GetUnusedAudioObject();
		
		audioObject[ID].inUse=true;
		
		audioObject[ID].source.clip=clip;
		audioObject[ID].source.loop=true;
		audioObject[ID].source.Play();
		
		return ID;
	}
	
	
	
	public static void StopSound(int ID){
		audioObject[ID].inUse=false;
		audioObject[ID].source.Stop();
		audioObject[ID].source.clip=null;
		audioObject[ID].thisT.parent=instance.thisT;
		//~ Debug.Log(audioObject[ID].source.gameObject);
	}
	
	//a sound routine for 2D sound, make sure they follow the listener, which is assumed to be the main camera
	static IEnumerator SoundRoutine2D(int ID, float duration){
		while(duration>0){
			audioObject[ID].thisT.position=camT.position;
			yield return null;
		}
		
		//finish playing, clear the audioObject
		instance.StartCoroutine(instance.ClearAudioObject(ID, 0));
	}
	
	//function call to clear flag of an audioObject, indicate it's is free to be used again
	private IEnumerator ClearAudioObject(int ID, float duration){
		yield return new WaitForSeconds(duration);
		audioObject[ID].inUse=false;
		audioObject[ID].thisT.parent=thisT;
	}
	
	public static void SetSFXVolume(float val){
		AudioListener.volume=val;
	}
	
	public static void SetMusicVolume(float val){
		if(instance  && instance.musicSource){
			instance.musicSource.volume=val;
		}
	}
	
	public static float GetSFXVolume(){
		return AudioListener.volume;
	}
	
	public static float GetMusicVolume(){
		if(instance && instance.musicSource){
			return instance.musicSource.volume;
		}
		return instance.initialMusicVolume;
	}
	
	public static void SetMusicSourceToListener(Transform lisT){
		instance.listenerT=lisT;
		if(instance.musicSource!=null){
			instance.musicSource.transform.parent=instance.listenerT;
			instance.musicSource.transform.localPosition=Vector3.zero;
		}
	}
	
}


[System.Serializable]
public class AudioObject{
	public AudioSource source;
	public bool inUse=false;
	public Transform thisT;
	
	public AudioObject(AudioSource src, Transform t){
		source=src;
		thisT=t;
	}
}