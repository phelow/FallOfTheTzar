using UnityEngine;
using System.Collections;

public class DemoMainMenu : MonoBehaviour {
	
	private string[] tooltip;
	
	public bool useNGUIScene=false;
	private string loadPara="";
	
	// Use this for initialization
	void Start () {
		tooltip=new string[8];
		tooltip[0]="Series of continous levels in random order that carry allow unit selection before each battle\nThe unit and point earns in each level are carried forth to next level.";
		
		tooltip[1]="A simple level, each unit takes turn to move based on their stats in each round\nUnits gain AP over each round which is then use to perform special ability\nThe level is procedurally generated in every loading";
		
		tooltip[2]="A space theme level, each faction takes turn to move all units in each round. Unit switching is not allowed.\nUnits gain AP over each round which is then use to perform special ability";
		
		tooltip[3]="A tropic theme level, each faction take turn to move a single unit, a round is completed when all unit is moved\nUnits is given full AP at each turn but every movement, attack, and ability usage will cost AP.";
		
		tooltip[4]="A test level showcase the XCom style cover system with fog-of-war system\nEach faction takes turn to move all the units.\nUnits gain AP over each round which is then use to perform special ability";
		
		tooltip[5]="A test level with square grid\nEach faction takes turn to move all the units.\nUnits gain AP over each round which is then use to perform special ability";
		
		loadPara=useNGUIScene ? "NGUI" : "";
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI(){
		
		float startY=Screen.height/2-170;
		float spaceY=50;
		
		GUIContent content=new GUIContent("Mini\nCampaign", "1");
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
		}
		
		
		if(GUI.tooltip!=""){
			int ID=int.Parse(GUI.tooltip)-1;
			
			GUIStyle style=new GUIStyle();
			style.alignment=TextAnchor.UpperCenter;
			style.fontStyle=FontStyle.Bold;
			style.normal.textColor=Color.white;
			
			GUI.Label(new Rect(0, Screen.height*0.75f+30, Screen.width, 200), tooltip[ID], style);
		}
	}
}
