#define customGui
using UnityEngine;
using System.Collections;

[RequireComponent (typeof (UIHUD))]
[RequireComponent (typeof (UIOverlay))]
[RequireComponent (typeof (UIAbilityButtons))]
[RequireComponent (typeof (UIPerkMenu))]
[RequireComponent (typeof (UIGameOver))]
[RequireComponent (typeof (UIUnitPlacement))]
public class UI : MonoBehaviour {

	//public GUISkin customSkin;
	//public static GUISkin GetUISkin(){ return instance.customSkin; }
	
	private UIHUD uiHUD;
	private UIOverlay uiOverlay;
	private UIAbilityButtons uiAbility;
	private UIPerkMenu uiPerkMenu;
	private UIGameOver uiGameOver;
	private UIUnitPlacement uiUnitPlacement;
	
	public static GUIStyle buttonStyle;
	public static GUIStyle buttonStyleAlt;
	public static Color colorH;
	public static Color colorN;
	public static Texture texBar;
	
	private bool battleStarted=false;
	private bool pause=false;
	
	public static UI instance;
	
	void Awake(){
		
		instance=this;
		
		uiHUD=gameObject.GetComponent<UIHUD>();
		uiOverlay=gameObject.GetComponent<UIOverlay>();
		uiAbility=gameObject.GetComponent<UIAbilityButtons>();
		uiPerkMenu=gameObject.GetComponent<UIPerkMenu>();
		uiGameOver=gameObject.GetComponent<UIGameOver>();
		uiUnitPlacement=gameObject.GetComponent<UIUnitPlacement>();
		
		//uiHUD.enabled=false;
		uiOverlay.enabled=false;
		//uiAbility.enabled=false;
		uiGameOver.enabled=false;
		uiUnitPlacement.enabled=false;
		
		colorH=new Color(1f, .6f, 0f, 1f);
		colorN=new Color(1f, .85f, .75f, 1f);
		texBar=Resources.Load("Textures/Bar", typeof(Texture)) as Texture;
		
		
	}
	
	// Use this for initialization
	void Start () {
		musicVol=AudioManager.GetMusicVolume();
		sfxVol=AudioManager.GetSFXVolume();
	}
	
	void OnEnable(){
		GameControlTB.onBattleStartE += OnBattleStart;
		GameControlTB.onBattleEndE += OnBattleEnd;
		
		UnitControl.onPlacementUpdateE += OnUnitPlacement;
	}
	void OnDisable(){
		GameControlTB.onBattleStartE -= OnBattleStart;
		GameControlTB.onBattleEndE -= OnBattleEnd;
		
		UnitControl.onPlacementUpdateE -= OnUnitPlacement;
	}
	
	
	void OnBattleStart(){
		battleStarted=true;
		uiOverlay.enabled=true;
	}
	
	void OnBattleEnd(int vicFactionID){ StartCoroutine(_OnBattleEnd(vicFactionID)); }
	IEnumerator _OnBattleEnd(int vicFactionID){
		yield return new WaitForSeconds(2);
		battleStarted=false;
		uiOverlay.enabled=false;
		uiGameOver.enabled=true;
		uiGameOver.Show(vicFactionID);
	}
	
	void OnUnitPlacement(){
		uiUnitPlacement.enabled=true;
	}
	
	
	void Update(){
		if(Input.GetKeyDown(KeyCode.Escape)){
			bool onFlag=false;
			if(uiAbility.IsOn()){
				onFlag=true;
				uiAbility.ExitUnitAbilityTargetSelect();
			}
			if(uiPerkMenu.isOn){
				onFlag=true;
				OnPerkMenu();
			}
			
			if(!onFlag) TogglePause();
		}
	}
	
	
	void TogglePause(){
#if pauseEnabled
		if(!pause){
			pause=true;
			Time.timeScale=0;
			
			uiHUD.enabled=false;
			uiAbility.enabled=false;
			uiPerkMenu.enabled=false;
			uiPerkMenu.isOn=false;
		}
		else{
			pause=false;
			Time.timeScale=1;
			
			uiHUD.enabled=true;
			uiAbility.enabled=true;
			//uiPerkMenu.enabled=true;
			//uiPerkMenu.isOn=true;
		}
#endif
	}
	
	public static void OnPerkMenu(){
		instance.uiPerkMenu.OnPerkmenu();
	}
	
	public static void InitButtonStyle(){
		buttonStyle=new GUIStyle("Button");
		buttonStyle.fontSize=14;	buttonStyle.fontStyle=FontStyle.Bold;	buttonStyle.normal.textColor=colorH;
		buttonStyle.padding=new RectOffset(0, 0, 0, 0);
		buttonStyle.hover.textColor=UI.colorN;
		
		buttonStyleAlt=new GUIStyle("Button");
		buttonStyleAlt.fontSize=14;	buttonStyleAlt.fontStyle=FontStyle.Bold;	buttonStyleAlt.normal.textColor=colorN;
		buttonStyleAlt.padding=new RectOffset(0, 0, 0, 0);
	}
	
	void OnGUI(){///assets/NecromancerGUI
		#if customGui 
		GUI.skin= Resources.Load("Skins/Fantasy-Colorable") as GUISkin;
		#endif
		if(buttonStyle==null){
			InitButtonStyle();
		}
		
		if(!battleStarted) return;
#if pause
		if(GUI.Button(new Rect(Screen.width-100, 5, 60, 60), "Pause\nMenu", buttonStyle)){
			TogglePause();
		}
#endif
		
		if(pause) PauseMenu();
		else{
			if(uiPerkMenu.isOn) uiPerkMenu.DrawPerkMenu();
			else{
				if(uiHUD.enabled) uiHUD.Draw();
				if(uiAbility.enabled) uiAbility.Draw();
			}
		}
	}
	
	private float musicVol=.7f;
	private float sfxVol=.7f;
	
	private int boxHeight=400;
	void PauseMenu(){
		GUIStyle style=new GUIStyle();
		style.alignment=TextAnchor.UpperCenter;
		
		int startX=Screen.width/2-70;
		int startY=Screen.height/2-200;
		int width=120; int height=25; int spaceY=30;
		
		for(int i=0; i<3; i++) GUI.Box(new Rect(startX, startY, 140, boxHeight), "");
		
		style.fontSize=20;	style.fontStyle=FontStyle.Bold;	style.normal.textColor=UI.colorH;
		GUI.Label(new Rect(startX+5, startY+=5, width, height), "Paused", style);
		
		startX+=10;	startY+=20;
		
		GUI.changed=false;
		
		GUIStyle style2=new GUIStyle("Label");
		style2.alignment=TextAnchor.UpperCenter;
		
		//~ float musicVol=AudioManager.GetMusicVolume();
		//~ float sfxVol=AudioManager.GetSFXVolume();
		
		GUI.Label(new Rect(startX, startY+=35, width, 30), "Music Volume: "+((musicVol*100).ToString("f0"))+"%", style2);
		musicVol = GUI.HorizontalSlider(new Rect(startX, startY+=20, width, height), musicVol, 0.0F, 1.0F);
		GUI.Label(new Rect(startX, startY+=35, width, 30), "SFX Volume: "+((sfxVol*100).ToString("f0"))+"%", style2);
		sfxVol = GUI.HorizontalSlider(new Rect(startX, startY+=20, width, height), sfxVol, 0.0F, 1.0F);
		
		if(GUI.changed){
			AudioManager.SetSFXVolume(sfxVol);
			AudioManager.SetMusicVolume(musicVol);
		}
		
		if(GUI.Button(new Rect(startX, startY+=spaceY, width, height), "Resume", buttonStyleAlt)) TogglePause();
		if(GUI.Button(new Rect(startX, startY+=spaceY, width, height), "Restart", buttonStyleAlt)){
			Time.timeScale=1;
			Application.LoadLevel(Application.loadedLevelName);
		}
		if(GUI.Button(new Rect(startX, startY+=spaceY, width, height), "MainMenu", buttonStyleAlt)){
			Time.timeScale=1;
			GameControlTB.LoadMainMenu();
		}
		
		
		startY+=spaceY+5;
		boxHeight=startY-(Screen.height/2-200);
	}
}
