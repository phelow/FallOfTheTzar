using UnityEngine;
using System.Collections;

public class Hover : MonoBehaviour {

	public float offset;
	public float speed=3.5f;
	public float magnitude=1;
	
	// Use this for initialization
	void Start () {
		offset=Random.Range(-5f, 5f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate(Vector3.up*magnitude*Mathf.Sin(speed*Time.time+offset)*Time.deltaTime);
	}
}
