#define customSkin

using UnityEngine;
using System.Collections;

public class FallOfTheTzarMainMenu : MonoBehaviour {
#if customSkin
	public GUISkin customSkin;
#endif
	/// <summary>
	/// Constants which control the dimensions of a button
	/// </summary>
	const int HEIGHT = 200;
	const int WIDTH = 60;

	string currentLevel ="";

	/// <summary>
	/// These arrays populate the menu items
	/// </summary>
	public string[] tooltip;
	public string[] names;	//what shows up for each item on the menu
	public string[] levelLoad;	//which level to load, string "null" will indicate that the level is not currently present


	public int menuItems = 3;

	public bool useNGUIScene=false;
	private string loadPara="";
	
	// Use this for initialization
	void Start () {
		currentLevel = PlayerPrefs.GetString("CurrentLevel");
		Debug.Log("CurrentLevel:"+ currentLevel);
		loadPara=useNGUIScene ? "NGUI" : "";

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI(){
#if customSkin
		GUI.skin = customSkin;

#endif
		
		float startY=Screen.height/2-170;
		float spaceY=80;

			/*GUIContent content=new GUIContent("Mini\nCampaign", "1");
			if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 40), content)){
				Application.LoadLevel("ExPreBattleSetup"+loadPara);
			}
			content=new GUIContent("Basic\n(single level)", "2");
			if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 40), content)){
				Application.LoadLevel("ExSingleBasic"+loadPara);
			}
			content=new GUIContent("Space\n(single level)", "3");
			if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 40), content)){
				Application.LoadLevel("ExSingleSpace"+loadPara);
			}
			content=new GUIContent("Tropic\n(single level)", "4");
			if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 40), content)){
				Application.LoadLevel("ExSingleTropic"+loadPara);
			}
			content=new GUIContent("Cover System\n(single level)", "5");
			if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 40), content)){
				Application.LoadLevel("ExCoverSystem"+loadPara);
			}
			content=new GUIContent("Square Grid\n(single level)", "5");
			if(GUI.Button(new Rect(Screen.width/2-60, startY+=spaceY, 120, 40), content)){
				Application.LoadLevel("ExSquare"+loadPara);
			}*/

		for(int i=0; i< menuItems; i++){
			GUIContent content = new GUIContent (names[i], ""+i);
			if(GUI.Button(new Rect((int) (Screen.width/2- (1.58*WIDTH)), startY+=spaceY, HEIGHT, WIDTH), content)){
				if(levelLoad[i] =="Continue"){
					///if there exists a level to load, load it, otherwise start a new game
					if(currentLevel.Length > 1){
						Application.LoadLevel(currentLevel);
					}
					else{
						Application.LoadLevel("LevelOne");
					}
				}
				else if(levelLoad[i] != "null"){
					Application.LoadLevel(levelLoad[i]);
				}
			}
		}
			
			if(GUI.tooltip!=""){
				int ID=int.Parse(GUI.tooltip);

				GUIStyle style=new GUIStyle();
				style.alignment=TextAnchor.UpperCenter;
				style.fontStyle=FontStyle.Bold;
				style.normal.textColor=Color.white;
				
				GUI.Label(new Rect(0, Screen.height*0.75f+30, Screen.width, 200), tooltip[ID], style);
			}
		
	
	}
}
