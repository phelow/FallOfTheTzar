using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	
	public float panSpeed=8;
	public float zoomSpeed=5;
	public float rotateSpeed=5;
	
	public bool enableKeyRotate=true;
	public bool enableKeyPanning=true;
	public bool enableMouseWheelZoom=true;
	public bool enableMousePanning=false;
	
	public int mousePanningZoneWidth=10;
	
	#if UNITY_IPHONE || UNITY_ANDROID
	public bool enableTouchRotate=true;
	public bool enableTouchPan=true;
	public bool enableTouchZoom=true;
	
	private Vector3 lastTouchPos=new Vector3(9999, 9999, 9999);
	private Vector3 moveDir=Vector3.zero;
	private float touchZoomSpeed;
	public float rotationSpeed=1;
	
	public float minRotateAngle=10;
	public float maxRotateAngle=89;
	#endif
	
	public float minPosX=-4;
	public float maxPosX=4;
	public float minPosZ=-4;
	public float maxPosZ=4;
	
	public float minRadius=8;
	public float maxRadius=30;
	
	private float deltaT=1;
	
	private Transform camT;
	private Transform thisT;
	
	private Transform actionCamT;
	[HideInInspector] public Transform actionCamDummyT;
	
	public Camera actionCam;
	public Camera mainCam;
	
	public float actionCamDistance=5.5f;
	public float actionCamDelay=0.5f;
	
	
	[HideInInspector] public bool actionCamActivated=false;
	[HideInInspector] public bool killCamActivated=false;
	
	public static CameraControl instance;
	
	void Awake(){
		instance=this;
	}
	
	// Use this for initialization
	void Start () {
		thisT=transform;
		
		camT=mainCam.transform;
		actionCamT=actionCam.transform;
		
		if(actionCamDummyT==null){
			actionCamDummyT=new GameObject().transform;
			actionCamDummyT.parent=thisT;
			actionCamDummyT.gameObject.name="ActionCamDummy";
		}
		actionCam.enabled=false;
		
		AudioManager.SetMusicSourceToListener(actionCamT);
		
		int layerUI=LayerManager.GetLayerUI();
		int layerUnitInvisible=LayerManager.GetLayerUnitAIInvisible();
		LayerMask mask=1<<layerUI | 1<<layerUnitInvisible;
		mainCam.cullingMask=~mask;
		actionCam.cullingMask=~mask;
		
		actionCam.depth=99;
	}
	
	public static void ActionCam(Tile attacker, Tile target){
		if(instance!=null) {
			instance.StartCoroutine(instance.ActionCamRoutine(attacker, target));
		}
	}
	
	IEnumerator ActionCamRoutine(Tile attacker, Tile target){
		actionCamActivated=true;
		
		UnitTB.onActionCompletedE += OnActionCompleted;
		actionInProgress=true;
		
		actionCamDummyT.position=attacker.pos;
		actionCamDummyT.LookAt(target.thisT);
		float x=Random.Range(-2f, 2f);
		if(x>0) x=1.5f;
		else x=-1.5f;
		Vector3 dir=new Vector3(x, 2, -5).normalized;
		Vector3 wantedPos=actionCamDummyT.TransformPoint(dir*actionCamDistance);
		
		actionCamT.position=wantedPos;
		actionCamT.LookAt(target.thisT);
		
		actionCam.enabled=true;
		mainCam.enabled=false;
		
		//yield return new WaitForSeconds(5);
		while(actionInProgress && !stopActionCam) yield return null;
		
		if(!stopActionCam) yield return new WaitForSeconds(actionCamDelay);
		
		actionCam.enabled=false;
		mainCam.enabled=true;
		
		actionCamActivated=false;
		
		stopActionCam=false;
	}
	
	private bool stopActionCam=false;
	public static void StopActionCam(){
		if(instance.actionCamActivated) instance.stopActionCam=true;
	}
	
	
	public static bool ActionCamInAction(){
		if(instance.actionCamActivated || instance.killCamActivated) return true;
		return false;
	}
	
	
	private bool actionInProgress=false;
	void OnActionCompleted(UnitTB unit){
		UnitTB.onActionCompletedE -= OnActionCompleted;
		StartCoroutine(_OnActionCompleted());
	}
	
	IEnumerator _OnActionCompleted(){
		yield return new WaitForSeconds(0.5f);
		actionInProgress=false;
	}
	
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.C)){
			StopActionCam();
		}
		
		if(Time.timeScale==1) deltaT=Time.deltaTime;
		else if(Time.timeScale>1) deltaT=Time.deltaTime/Time.timeScale;
		else deltaT=0.015f;
		
		#if UNITY_IPHONE || UNITY_ANDROID
		
		if(enableTouchPan){
			Quaternion camDir=Quaternion.Euler(0, transform.eulerAngles.y, 0);
			if(Input.touchCount==1){
				Touch touch=Input.touches[0];
				if(touch.phase == TouchPhase.Moved){
					Vector3 deltaPos = touch.position;
					
					if(lastTouchPos!=new Vector3(9999, 9999, 9999)){
						deltaPos=deltaPos-lastTouchPos;
						moveDir=new Vector3(deltaPos.x, 0, deltaPos.y).normalized*-1;
					}
	
					lastTouchPos=touch.position;
				}
			}
			else lastTouchPos=new Vector3(9999, 9999, 9999);
			
			Vector3 dir=thisT.InverseTransformDirection(camDir*moveDir)*1.5f;
			thisT.Translate (dir * panSpeed * deltaT);
			
			moveDir=moveDir*(1-deltaT*5);
		}
		
		
		if(enableTouchZoom){
			if(Input.touchCount==2){
				Touch touch1 = Input.touches[0];
				Touch touch2 = Input.touches[1];
				
				if(touch1.phase==TouchPhase.Moved && touch1.phase==TouchPhase.Moved){
					//float dot=Vector2.Dot(touch1.deltaPosition, touch1.deltaPosition);
					Vector3 dirDelta=(touch1.position-touch1.deltaPosition)-(touch2.position-touch2.deltaPosition);
					Vector3 dir=touch1.position-touch2.position;
					float dot=Vector3.Dot(dirDelta.normalized, dir.normalized);
					
					if(Mathf.Abs(dot)>0.7f){	
						touchZoomSpeed=dir.magnitude-dirDelta.magnitude;
					}	
				}
			}
			
			camT.Translate(Vector3.forward*Time.deltaTime*zoomSpeed*touchZoomSpeed);
			
			touchZoomSpeed=touchZoomSpeed*(1-Time.deltaTime*5);
		}
		
		
		if(enableTouchRotate){
			if(Input.touchCount==2){
				Touch touch1 = Input.touches[0];
				Touch touch2 = Input.touches[1];
				
				Vector2 delta1=touch1.deltaPosition.normalized;
				Vector2 delta2=touch2.deltaPosition.normalized;
				Vector2 delta=(delta1+delta2)/2;
				
				float rotX=thisT.rotation.eulerAngles.x-delta.y*rotationSpeed;
				float rotY=thisT.rotation.eulerAngles.y+delta.x*rotationSpeed;
				rotX=Mathf.Clamp(rotX, minRotateAngle, maxRotateAngle);
				
				//Quaternion rot=Quaternion.Euler(delta.y, delta.x, 0);
				//Debug.Log(rotX+"   "+rotY);
				thisT.rotation=Quaternion.Euler(rotX, rotY, 0);
				//thisT.rotation*=rot;
			}
		}
		
		#endif
		
		Quaternion direction=Quaternion.Euler(0, thisT.eulerAngles.y, 0);
		if(enableKeyPanning){
			if(Input.GetButton("Horizontal")) {
				Vector3 dir=transform.InverseTransformDirection(direction*Vector3.right);
				thisT.Translate (dir * panSpeed * deltaT * Input.GetAxisRaw("Horizontal"));
			}

			if(Input.GetButton("Vertical")) {
				Vector3 dir=transform.InverseTransformDirection(direction*Vector3.forward);
				thisT.Translate (dir * panSpeed * deltaT * Input.GetAxisRaw("Vertical"));
			}
		}
		
		
		if(enableMousePanning){
			Vector3 mousePos=Input.mousePosition;
			Vector3 dirHor=transform.InverseTransformDirection(direction*Vector3.right);
			if(mousePos.x<=0){
				thisT.Translate(dirHor * panSpeed * deltaT * -3);
			}
			else if(mousePos.x<=mousePanningZoneWidth){
				thisT.Translate(dirHor * panSpeed * deltaT * -1);
			}
			else if(mousePos.x>=Screen.width){
				thisT.Translate(dirHor * panSpeed * deltaT * 3);
			}
			else if(mousePos.x>Screen.width-mousePanningZoneWidth){
				thisT.Translate(dirHor * panSpeed * deltaT * 1);
			}
			
			Vector3 dirVer=transform.InverseTransformDirection(direction*Vector3.forward);
			if(mousePos.y<=0){
				thisT.Translate(dirVer * panSpeed * deltaT * -3);
			}
			else if(mousePos.y<=mousePanningZoneWidth){
				thisT.Translate(dirVer * panSpeed * deltaT * -1);
			}
			else if(mousePos.y>=Screen.height){
				thisT.Translate(dirVer * panSpeed * deltaT * 3);
			}
			else if(mousePos.y>Screen.height-mousePanningZoneWidth){
				thisT.Translate(dirVer * panSpeed * deltaT * 1);
			}
		}
		
		
		if(enableKeyRotate){
			if(Input.GetKey(KeyCode.Q)){
				thisT.Rotate(Vector3.up*Time.deltaTime*10*rotateSpeed, Space.World);
			}
			if(Input.GetKey(KeyCode.E)){
				thisT.Rotate(-Vector3.up*Time.deltaTime*10*rotateSpeed, Space.World);
			}
		}
		
		
		if(enableMouseWheelZoom){
			if(Input.GetAxis("Mouse ScrollWheel")<0){
				camT.Translate(Vector3.forward*zoomSpeed*Input.GetAxis("Mouse ScrollWheel"));
			}
			else if(Input.GetAxis("Mouse ScrollWheel")>0){
				camT.Translate(Vector3.forward*zoomSpeed*Input.GetAxis("Mouse ScrollWheel"));
			}
			
			float camZ=Mathf.Clamp(camT.localPosition.z, -maxRadius, -minRadius);
			camT.localPosition=new Vector3(camT.localPosition.x, camT.localPosition.y, camZ);
		}
		
		
		float x=Mathf.Clamp(thisT.position.x, minPosX, maxPosX);
		float z=Mathf.Clamp(thisT.position.z, minPosZ, maxPosZ);
		
		thisT.position=new Vector3(x, thisT.position.y, z);
	}
	
	
	
	
	public static Camera GetActiveCamera(){
		if(instance.mainCam.enabled) return instance.mainCam;
		return instance.actionCam;
	}
	
	
	public static void Enable(){ if(instance!=null) instance.enabled=true; }
	public static void Disable(){ if(instance!=null) instance.enabled=false; }
	
	
	public bool showGizmo=true;
	void OnDrawGizmos(){
		if(showGizmo){
			Vector3 p1=new Vector3(minPosX, transform.position.y, maxPosZ);
			Vector3 p2=new Vector3(maxPosX, transform.position.y, maxPosZ);
			Vector3 p3=new Vector3(maxPosX, transform.position.y, minPosZ);
			Vector3 p4=new Vector3(minPosX, transform.position.y, minPosZ);
			
			Gizmos.color=Color.green;
			Gizmos.DrawLine(p1, p2);
			Gizmos.DrawLine(p2, p3);
			Gizmos.DrawLine(p3, p4);
			Gizmos.DrawLine(p4, p1);
		}
	}
}
