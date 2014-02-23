using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (UIPerkMenu))]
[RequireComponent (typeof (UISetupUnit))]
public class UISetup : MonoBehaviour {
	
	private enum _Tab{Unit, Perk};
	private _Tab tab=_Tab.Unit;

	public _LoadMode loadMode;
	
	public int startingPlayerPoint=25;
	public static int playerPoint=0;
	
	public string mainMenu="";
	public string nextScene="";
	public bool randomNextScene=false;
	public List<string> nextScenes=new List<string>();
	
	private UIPerkMenu uiPerk;
	private UISetupUnit uiUnit;
	
	
	public static UISetup instance;
	
	void Awake(){
		instance=this;
		
		UI.colorH=new Color(1f, .6f, 0f, 1f);
		UI.colorN=new Color(1f, .85f, .75f, 1f);
		
		uiPerk=gameObject.GetComponent<UIPerkMenu>();
		uiUnit=gameObject.GetComponent<UISetupUnit>();
	}
	
	// Use this for initialization
	void Start () {
		if(loadMode==_LoadMode.UsePersistantData){
			if(!GlobalStatsTB.loaded){
				GlobalStatsTB.Init();
			}
			
			playerPoint=GlobalStatsTB.GetPlayerPoint();
			UISetupUnit.selectedUnits=GlobalStatsTB.GetPlayerUnitList();
		}
		else playerPoint=startingPlayerPoint;
	}
	
	void OnEnable(){
		GlobalStatsTB.onPlayerPointChangedE += UpdatePoints;
	}
	void OnDisable(){
		GlobalStatsTB.onPlayerPointChangedE -= UpdatePoints;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public static void UpdatePoints(int val){
		if(instance==null) return;
		playerPoint+=val;
	}
	
	void OnGUI(){
		if(UI.buttonStyle==null){
			UI.InitButtonStyle();
		}
		
		if(GUI.Button(new Rect(Screen.width-35, 5, 30, 30), "R", UI.buttonStyle)){
			GlobalStatsTB.ResetAll();
			Application.LoadLevel(Application.loadedLevelName);
		}
		
		GUIStyle style=new GUIStyle();
		style.fontSize=20;	style.fontStyle=FontStyle.Bold;	style.normal.textColor=Color.black;	style.alignment=TextAnchor.UpperRight;
		GUI.Label(new Rect(0+2, 5+2, Screen.width-45, 50), "Point: "+playerPoint, style);
		style.normal.textColor=UI.colorH;
		GUI.Label(new Rect(0, 5, Screen.width-45, 50), "Point: "+playerPoint, style);
		
		
		if(GUI.Button(new Rect(Screen.width-65, Screen.height-65, 60, 60), "Start\nBattle", UI.buttonStyle)){
			LoadNextScene();
		}
		if(GUI.Button(new Rect(5, Screen.height-65, 60, 60), "Main\nMenu", UI.buttonStyle)){
			if(mainMenu!="") Application.LoadLevel(mainMenu);
			else Debug.Log("Menu scene name not specified");
		}
		
		if(tab==_Tab.Unit){
			if(GUI.Button(new Rect(Screen.width/2-45-3, Screen.height-35-3, 86, 36), "Unit", UI.buttonStyle)){
				tab=_Tab.Unit;
			}
			if(GUI.Button(new Rect(Screen.width/2+45, Screen.height-35, 80, 30), "Perk", UI.buttonStyle)){
				tab=_Tab.Perk;
			}
			
			uiUnit.DrawUnitMenu();
		}
		else{
			if(GUI.Button(new Rect(Screen.width/2-45, Screen.height-35, 80, 30), "Unit", UI.buttonStyle)){
				tab=_Tab.Unit;
			}
			if(GUI.Button(new Rect(Screen.width/2+45-3, Screen.height-35-3, 86, 36), "Perk", UI.buttonStyle)){
				tab=_Tab.Perk;
			}
			
			uiPerk.DrawPerkMenu(true);
		}
	}
	
	
	void LoadNextScene(){
		string sceneToLoad="";
		
		if(!randomNextScene){
			sceneToLoad=nextScene;
		}
		else{
			for(int i=0; i<nextScenes.Count; i++){
				if(nextScenes[i]==""){
					nextScenes.RemoveAt(i);
					i-=1;
				}
			}
			
			if(nextScenes.Count!=0){
				int rand=Random.Range(0, nextScenes.Count);
				sceneToLoad=nextScenes[rand];
			}
		}
		
		if(sceneToLoad!="") OnStartBattle(sceneToLoad);
		else Debug.Log("Next scene is not specified");
	}
	
	void OnStartBattle(string sceneToLoad){
		if(UISetupUnit.selectedUnits.Count==0){
			//DisplayMessage("No unit is selected!");
			return;
		}
		
		//if(usePersistantData){
		if(loadMode==_LoadMode.UsePersistantData){
			GlobalStatsTB.SetPlayerPoint(playerPoint);
			GlobalStatsTB.SetPlayerUnitList(UISetupUnit.selectedUnits);
		}
		else if(loadMode==_LoadMode.UseTemporaryData){
			GlobalStatsTB.SetTempPlayerUnitList(UISetupUnit.selectedUnits);
		}
		
		Application.LoadLevel(sceneToLoad);
	}
	
	
}
