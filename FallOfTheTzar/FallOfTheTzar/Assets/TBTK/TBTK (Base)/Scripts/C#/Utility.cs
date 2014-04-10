using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utility : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static bool IsActive(Transform obj){
		return IsActive(obj.gameObject);
	}
	public static bool IsActive(GameObject obj){
		#if UNITY_3_5 || UNITY_3_4
			if(obj.active) return true;
		#else
			if(obj.activeInHierarchy) return true;
		#endif
		
		return false;
	}
	
	public static void SetActive(Transform obj, bool flag){
		SetActive(obj.gameObject, flag);
	}
	public static void SetActive(GameObject obj, bool flag){
		#if UNITY_3_5 || UNITY_3_4
			if(IsActive(obj)!=flag) obj.SetActiveRecursively(flag);
		#else
			if(IsActive(obj)!=flag) obj.SetActive(flag);
		#endif
	}
	
	static public void SetLayerRecursively(Transform root, int layer){
		foreach(Transform child in root) {
			child.gameObject.layer=layer;
			SetLayerRecursively(child, layer);
		}
	}
	
	
	
	//converting vector to angle
	public static float VectorToAngle(Vector3 dir){
		return VectorToAngle(new Vector3(dir.x, dir.z));
	}
	public static float VectorToAngle(Vector2 dir){
		if(dir.x==0){
			if(dir.y>0) return 90;
			else if(dir.y<0) return 270;
			else return 0;
		}
		else if(dir.y==0){
			if(dir.x>0) return 0;
			else if(dir.x<0) return 180;
			else return 0;
		}
		
		float h=Mathf.Sqrt(dir.x*dir.x+dir.y*dir.y);
		float angle=Mathf.Asin(dir.y/h)*Mathf.Rad2Deg;
		
		if(dir.y>0){
			if(dir.x<0)  angle=180-angle;
		}
		else{
			if(dir.x>0)  angle=360+angle;
			if(dir.x<0)  angle=180-angle;
		}
		
		return angle;
	}
	
	
	
	//from unit utility
	public static Vector3 GetWorldScale(Transform transform){
		Vector3 worldScale = transform.localScale;
		Transform parent = transform.parent;
		
		while (parent != null){
			worldScale = Vector3.Scale(worldScale,parent.localScale);
			parent = parent.parent;
		}
		
		return worldScale;
	}
	
	static public void DestroyColliderRecursively(Transform root){
		foreach(Transform child in root) {
			if(child.collider!=null) {
				Destroy(child.collider);
			}
			DestroyColliderRecursively(child);
		}
	}
	
	static public void DisableColliderRecursively(Transform root){
		foreach(Transform child in root) {
			if(child.gameObject.collider!=null)  child.gameObject.collider.enabled=false;
			DisableColliderRecursively(child);
		}
	}
	
	static public void SetMat2DiffuseRecursively(Transform root){
		foreach(Transform child in root) {
			if(child.renderer!=null){
				foreach(Material mat in child.renderer.materials) {
					mat.shader=Shader.Find( "Diffuse" );
				}
			}
			//recurse.
			SetMat2DiffuseRecursively(child);
		}
	}
	
	
	
	static public void SetMat2AdditiveRecursively2(Transform root){
		foreach(Transform child in root) {
			if(child.renderer!=null){
				foreach(Material mat in child.renderer.materials)
					mat.shader=Shader.Find("Particles/Additive");
			}
			//recurse.
			SetMat2AdditiveRecursively(child);
		}
	}
	
	static public void DebugLog(MatShaderList list){
		for(int i=0; i<list.shaders.Count; i++){
			Debug.Log(list.mats[i]+"   "+list.shaders[i]);
		}
	}
	
	static public MatShaderList SetMat2AdditiveRecursively(Transform root){
		return SetMat2AdditiveRecursively(root, null);
	}
	static public MatShaderList SetMat2AdditiveRecursively(Transform root, MatShaderList list){
		if(list==null){
			list=new MatShaderList();
		}
		foreach(Transform child in root) {
			if(child.renderer!=null){
				foreach(Material mat in child.renderer.materials){
					
					list.Add(mat, mat.shader);
					mat.shader=Shader.Find("Particles/Additive");
					
				}
			}
			//recurse.
			list=SetMat2AdditiveRecursively(child, list);
		}
		return list;
	}
	
	static public void ResetMatRecursively(Transform root, MatShaderList list, int num=0){
		foreach(Transform child in root) {
			if(child.renderer!=null){
				foreach(Material mat in child.renderer.materials){
					//mat.SetColor("_TintColor", color);
					for(int i=0; i<list.mats.Count; i++){
						if(mat==list.mats[i]){
							mat.shader=list.shaders[i];
							list.RemoveAt(i);
							break;
						}
					}
					num+=1;
				}
			}
			//recurse.
			ResetMatRecursively(child, list, num);
		}
	}
	
	static public void SetAdditiveMatColorRecursively(Transform root, Color color){
		foreach(Transform child in root) {
			if(child.renderer!=null){
				foreach(Material mat in child.renderer.materials)  
					mat.SetColor("_TintColor", color);
			}
			//recurse.
			SetAdditiveMatColorRecursively(child, color);
		}
	}

}



public class MatShaderList{
	public int count=0;
	public List<Material> mats=new List<Material>();
	public List<Shader> shaders=new List<Shader>();
	
	public void Add(Material mat, Shader sha){
		count+=1;
		mats.Add(mat);
		shaders.Add(sha);
	}
	
	public void RemoveAt(int num){
		count-=1;
		if(num<mats.Count && num<shaders.Count){
			mats.RemoveAt(num);
			shaders.RemoveAt(num);
		}
	}
}
