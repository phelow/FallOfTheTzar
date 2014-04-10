using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum _CoverType{None, BlockHalf, BlockFull}//, WallFull, WalkFull}
public enum _ObsType{Obstacle, Wall}//, WallFull, WalkFull}
public enum _ObsTileType{Hex, Square, Universal}

//for cover system, wip
[RequireComponent (typeof (Collider))]
public class Obstacle : MonoBehaviour {
	public string obsName="obstacle";
	[HideInInspector] public int prefabID=-1;
	public _CoverType coverType=_CoverType.BlockHalf;
	public _ObsType obsType=_ObsType.Obstacle;
	public _ObsTileType tileType=_ObsTileType.Universal;
	
	//for obstacle only
	[HideInInspector] public Tile occupiedTile;
	
	[HideInInspector] public List<Obstacle> adjacentObs=new List<Obstacle>();
	//[HideInInspector] public List<GameObject> adjacentCol=new List<GameObject>();
	
	//[HideInInspector] public List<AdjacentObstacle> adjacentCol=new List<AdjacentObstacle>();
	
	void Start(){
		//~ Collider collider=gameObject.GetComponent<Collider>();
		//~ if(collider!=null) collider.enabled=true;
		//~ else Debug.Log(gameObject);
		collider.enabled=true;
		int obsLayer=LayerManager.GetLayerObstacle();
		gameObject.layer=obsLayer;
		
		//create collider with obstacle in between each obstacle
		//so adjacent obstacle are "fully joined" (no raycast can pass through middle
		if(occupiedTile!=null){
			foreach(Tile tile in occupiedTile.GetNeighbours()){
				if(tile.obstacle!=null){
					if(!adjacentObs.Contains(tile.obstacle)){
						GameObject colObj=new GameObject();
						colObj.layer=obsLayer;
						colObj.name="collider";
						colObj.transform.parent=tile.transform;
						colObj.transform.position=(occupiedTile.pos+tile.pos)/2;
						SphereCollider col=colObj.AddComponent<SphereCollider>();
						col.radius=GridManager.GetTileSize()*0.25f;
						
						adjacentObs.Add(tile.obstacle);
						tile.obstacle.adjacentObs.Add(this);
						
						Obstacle obs=colObj.AddComponent<Obstacle>();
						if(coverType==_CoverType.BlockFull || tile.obstacle.coverType==_CoverType.BlockFull){
							obs.coverType=_CoverType.BlockFull;
						}
						else{
							obs.coverType=_CoverType.BlockHalf;
						}
					}
				}
			}
		}
	}
}


/*
[System.Serializable]
public class AdjacentObstacle{
	public HexTile tile1;
	public HexTile tile2;
	public HexTile pos
	public Obstacle obs;
	public GameObject colObj;
}	
*/