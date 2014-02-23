using UnityEngine;
using System.Collections;

public class LayerManager : MonoBehaviour {

	[HideInInspector] public static int ui=31;
	[HideInInspector] public static int unit=30;
	[HideInInspector] public static int unitAI=29;
	[HideInInspector] public static int unitAIInv=28;
	[HideInInspector] public static int tile=8;
	[HideInInspector] public static int obstacle=9;
	[HideInInspector] public static int terrain=10;
	
	
	void Awake() {
		LoadPrefab();
	}
	
	public static void LoadPrefab(){
		GameObject obj=Resources.Load("DamageArmorList", typeof(GameObject)) as GameObject;
		
		if(obj==null) return;
		
		LayerListPrefab prefab=obj.GetComponent<LayerListPrefab>();
		
		if(prefab!=null){
			ui=prefab.ui;
			unit=prefab.unit;
			unitAI=prefab.unitAI;
			unitAIInv=prefab.unitAIInv;
			tile=prefab.tile;
			obstacle=prefab.obstacle;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static int GetLayerUI(){
		return ui;
	}
	
	public static int GetLayerUnit(){
		return unit;
	}
	
	public static int GetLayerUnitAI(){
		return unitAI;
	}
	
	public static int GetLayerUnitAIInvisible(){
		return unitAIInv;
	}
	
	public static int GetLayerTile(){
		return tile;
	}
	
	public static int GetLayerObstacle(){
		return obstacle;
	}
	
}
