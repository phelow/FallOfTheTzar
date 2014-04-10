using UnityEngine;
using System.Collections;

public enum _Axis{X, Y, Z}

public class RandomRotate : MonoBehaviour {

	public _Axis rotateAxis;
	
	public float min=-30;
	public float max=30;

	public UnitTB unit;
	
	private Quaternion targetRot;
	private float rotateSpeed;
	
	private Transform thisT;
	
	// Use this for initialization
	void Start () {
		thisT=transform;
		StartCoroutine(RotateRoutine());
	}
	
	IEnumerator RotateRoutine(){
		yield return new WaitForSeconds(Random.Range(1f, 5f));
		while(true){
			
			while(unit!=null && unit.InAction()){
				yield return new WaitForSeconds(Random.Range(1f, 3f));
			}
			
			rotateSpeed=Random.Range(3, 6);
			float val=Random.Range(min, max);
			
			if(rotateAxis==_Axis.X) targetRot=Quaternion.Euler(val, 0, 0);
			else if(rotateAxis==_Axis.Y) targetRot=Quaternion.Euler(0, val, 0);
			else if(rotateAxis==_Axis.Z) targetRot=Quaternion.Euler(0, 0, val);
			
			yield return new WaitForSeconds(Random.Range(3f, 6f));
		}
	}
	// Update is called once per frame
	void Update () {
		if(unit==null){
			thisT.localRotation=Quaternion.Slerp(thisT.localRotation, targetRot, Time.deltaTime*rotateSpeed);
		}
		else{
			if(!unit.InAction()){
				thisT.localRotation=Quaternion.Slerp(thisT.localRotation, targetRot, Time.deltaTime*rotateSpeed);
			}
			else{
				targetRot=thisT.localRotation;
			}
		}
	}
}
