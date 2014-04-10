using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

	public bool randomSpeed=true;
	public float rotateSpeed=5;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up*rotateSpeed);
		
		//~ float offset = Time.time * 5;
        //~ renderer.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
		
		if(randomSpeed){
			rotateSpeed+=Time.deltaTime*Random.Range(-4f, 4f);
			rotateSpeed=Mathf.Clamp(rotateSpeed, -5f, 5f);
		}
	}
}
