using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AStar : MonoBehaviour {

	
	//search for a path, through walkable tile only
	//for normal movement, return pos only
	public static List<Vector3> SearchWalkablePos(Tile originTile, Tile destTile){
		
		List<Tile> closeList=new List<Tile>();
		List<Tile> openList=new List<Tile>();
		
		Tile currentTile=originTile;
		
		float currentLowestF=Mathf.Infinity;
		int id=0;
		int i=0;
		
		while(true){
		
			//if we have reach the destination
			if(currentTile==destTile) break;
			
			//move currentNode to closeList;
			closeList.Add(currentTile);
			currentTile.listState=_ListState.Close;
			
			//loop through the neighbour of current loop, calculate  score and stuff
			currentTile.ProcessWalkableNeighbour(destTile);
			
			//put all neighbour in openlist
			foreach(Tile neighbour in currentTile.GetNeighbours()){
				if((neighbour.listState==_ListState.Unassigned && neighbour.walkable && neighbour.unit==null) || neighbour==destTile){
					//set the node state to open
					neighbour.listState=_ListState.Open;
					openList.Add(neighbour);
				}
			}
			
			//clear the current node, before getting a new one, so we know if there isnt any suitable next node
			currentTile=null;
			
			currentLowestF=Mathf.Infinity;
			id=0;
			for(i=0; i<openList.Count; i++){
				//~ if(openList[i]!=null){
					if(openList[i].scoreF<currentLowestF){
						currentLowestF=openList[i].scoreF;
						currentTile=openList[i];
						id=i;
					}
				//~ }
			}
			
			//if there's no node left in openlist, path doesnt exist
			if(currentTile==null) {
				//~ pathFound=false;
				break;
			}

			openList.RemoveAt(id);
		}
		
		List<Vector3> path=new List<Vector3>();
		while(currentTile!=null){
			path.Add(currentTile.pos);
			currentTile=currentTile.parent;
		}
		
		path=InvertArray(path);
		path.RemoveAt(0);
		
		ResetGraph(destTile, openList, closeList);
		
		return path;
	}
	
	//for normal movement, return the list of hexTile
	public static List<Tile> SearchWalkableTile(Tile originTile, Tile destTile){
		List<Tile> closeList=new List<Tile>();
		List<Tile> openList=new List<Tile>();
		
		Tile currentTile=originTile;
		
		float currentLowestF=Mathf.Infinity;
		int id=0;
		int i=0;
		
		while(true){
		
			//if we have reach the destination
			if(currentTile==destTile) break;
			
			//move currentNode to closeList;
			closeList.Add(currentTile);
			currentTile.listState=_ListState.Close;
			
			//loop through the neighbour of current loop, calculate  score and stuff
			currentTile.ProcessWalkableNeighbour(destTile);
			
			//put all neighbour in openlist
			foreach(Tile neighbour in currentTile.GetNeighbours()){
				if((neighbour.listState==_ListState.Unassigned && neighbour.walkable && neighbour.unit==null) || neighbour==destTile){
					//set the node state to open
					neighbour.listState=_ListState.Open;
					openList.Add(neighbour);
				}
			}
			
			//clear the current node, before getting a new one, so we know if there isnt any suitable next node
			currentTile=null;
			
			currentLowestF=Mathf.Infinity;
			id=0;
			for(i=0; i<openList.Count; i++){
				//~ if(openList[i]!=null){
					if(openList[i].scoreF<currentLowestF){
						currentLowestF=openList[i].scoreF;
						currentTile=openList[i];
						id=i;
					}
				//~ }
			}
			
			//if there's no node left in openlist, path doesnt exist
			if(currentTile==null) {
				break;
			}

			openList.RemoveAt(id);
		}
		
		
		List<Tile> path=new List<Tile>();
		while(currentTile!=null){
			path.Add(currentTile);
			currentTile=currentTile.parent;
			
		}
		
		path=InvertTileArray(path);
		
		ResetGraph(destTile, openList, closeList);
		
		return path;
	}
	
	
	
	
	
	
	
	
	
	
	
	//search for a path, through walkable tile only
	//for attack move, return series of tile as path, destTile is often occupied by hostile, unit will stop to attack before moving into destination
	public static List<Tile> SearchToOccupiedTile(Tile originTile, Tile destTile){
		
		List<Tile> closeList=new List<Tile>();
		List<Tile> openList=new List<Tile>();
		
		Tile currentTile=originTile;
		
		float currentLowestF=Mathf.Infinity;
		int id=0;
		int i=0;
		
		while(true){
		
			//if we have reach the destination
			if(currentTile==destTile) break;
			
			//move currentNode to closeList;
			closeList.Add(currentTile);
			currentTile.listState=_ListState.Close;
			
			//loop through the neighbour of current loop, calculate  score and stuff
			currentTile.ProcessWalkableNeighbour(destTile);
			
			//put all neighbour in openlist
			foreach(Tile neighbour in currentTile.GetNeighbours()){
				if((neighbour.listState==_ListState.Unassigned && neighbour.walkable && neighbour.unit==null) || neighbour==destTile) {
					//set the node state to open
					neighbour.listState=_ListState.Open;
					openList.Add(neighbour);
				}
			}
			
			//clear the current node, before getting a new one, so we know if there isnt any suitable next node
			currentTile=null;
			
			//get the next point from openlist, set it as current point
			//just loop through the openlist until we reach the maximum occupication
			//while that, get the node with the lowest score
			currentLowestF=Mathf.Infinity;
			id=0;
			for(i=0; i<openList.Count; i++){
				//~ if(openList[i]!=null){
					if(openList[i].scoreF<currentLowestF){
						currentLowestF=openList[i].scoreF;
						currentTile=openList[i];
						id=i;
					}
				//~ }
			}
			
			//if there's no node left in openlist, path doesnt exist
			if(currentTile==null) {
				break;
			}

			openList.RemoveAt(id);

		}
		
		
		List<Tile> path=new List<Tile>();
		while(currentTile!=null){
			path.Add(currentTile);
			currentTile=currentTile.parent;
		}
		
		path=InvertTileArray(path);
		
		
		
		ResetGraph(destTile, openList, closeList);
		
		return path;
	}
	
	
	
	
	
	
	//search the shortest path through all tile reagardless of status
	//this is used to accurately calculate the distance between 2 tiles in term of tile
	public static int Distance(Tile srcTile, Tile targetTile){
		List<Tile> closeList=new List<Tile>();
		List<Tile> openList=new List<Tile>();
		
		Tile currentTile=srcTile;
		if(srcTile==null) Debug.Log("src tile is null!!!");
		
		float currentLowestF=Mathf.Infinity;
		int id=0;
		int i=0;
		
		while(true){
		
			//~ Debug.DrawLine(currentTile.thisT.position, currentTile.thisT.position+new Vector3(0, 2, 0), Color.red, 5);
			
			//if we have reach the destination
			if(currentTile==targetTile) break;
			
			//move currentNode to closeList;
			closeList.Add(currentTile);
			currentTile.listState=_ListState.Close;
			
			//loop through all neighbours, regardless of status 
			currentTile.ProcessAllNeighbours(targetTile);
			
			//put all neighbour in openlist
			foreach(Tile neighbour in currentTile.GetNeighbours()){
				if(neighbour.listState==_ListState.Unassigned) {
					//set the node state to open
					neighbour.listState=_ListState.Open;
					openList.Add(neighbour);
				}
			}
			
			currentTile=null;
			
			currentLowestF=Mathf.Infinity;
			id=0;
			for(i=0; i<openList.Count; i++){
				if(openList[i].scoreF<currentLowestF){
					currentLowestF=openList[i].scoreF;
					currentTile=openList[i];
					id=i;
				}
			}
			
			if(currentTile==null) {
				break;
			}

			openList.RemoveAt(id);
		}
		
		/*
		//for debug, draw the path
		for(int j=0; j<closeList.Count; j++){
			if(closeList[j].parent!=null){
				Debug.DrawLine(closeList[j].parent.thisT.position, closeList[j].thisT.position, Color.white, 10);
			}
			else {
				Debug.DrawLine(closeList[j].thisT.position, closeList[j].thisT.position+new Vector3(0, 5, 0), Color.white, 10);
			}
		}
		*/
		
		int counter=0;
		while(currentTile!=null){
			//~ Debug.DrawLine(currentTile.thisT.position, currentTile.thisT.position+new Vector3(0, 2, 0), Color.red, 5);
			counter+=1;
			currentTile=currentTile.parent;
		}
		
		ResetGraph(targetTile, openList, closeList);
		
		return counter-1;
	}
	
	
	
	private static List<Vector3> InvertArray(List<Vector3> p){
		List<Vector3> pInverted=new List<Vector3>();
		for(int i=0; i<p.Count; i++){
			pInverted.Add(p[p.Count-(i+1)]);
		}
		return pInverted;
	}
	
	private static List<Tile> InvertTileArray(List<Tile> p){
		List<Tile> pInverted=new List<Tile>();
		for(int i=0; i<p.Count; i++){
			pInverted.Add(p[p.Count-(i+1)]);
		}
		return pInverted;
	}
	
	
	//reset all the tile, called after a search is complete
	private static void ResetGraph(Tile hTile, List<Tile> oList, List<Tile> cList){
		hTile.listState=_ListState.Unassigned;
		hTile.parent=null;
		
		foreach(Tile tile in oList){
			tile.listState=_ListState.Unassigned;
			tile.parent=null;
		}
		foreach(Tile tile in cList){
			tile.listState=_ListState.Unassigned;
			tile.parent=null;
		}
	}
}
