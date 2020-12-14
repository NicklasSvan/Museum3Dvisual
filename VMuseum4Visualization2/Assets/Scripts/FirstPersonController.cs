using System;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    float m_StickToGroundForce;
    float m_GravityMultiplier;

    public GameObject m_FPSController;
    CharacterController m_CharacterController;
    GameObject headcenter;
    GameObject m_eye;
    vmcamera vmcam;

    float controlX;
    float controlY;
    float controlW;
    float controlVertical;
    float controlHorizontal;
    float controlAltX;
    float controlAltY;

    CollisionFlags CollisionFlag2DBrowse;

    public float sensXWalkthrough;
    public float sensYWalkthrough;
    public float sensWWalkthrough;
    public float sensXBrowse;
    public float sensYBrowse;
    public float sensWBrowse;
    public float keySensitivity;
    public float controlSensitivity;

    public static float preyRot = 0;
    public static float prexRot = 0;

    float eyelevel;

    float posyHeadcenter;
    Vector3 CenterOfObject;
    float DistanceCameraObject;
	private float mousew, mousex, mousey;
    int flagbrowsecolforth=0;
    int flagbrowsecolback=0;

    int premode;


    // Use this for initialization
    private void Awake()
    {
		m_FPSController = GameObject.Find ("FPSController");
		m_CharacterController = m_FPSController.GetComponent<CharacterController>();
		headcenter = GameObject.Find ("FPSController/headcenter");
		m_eye = GameObject.Find ("FPSController/headcenter/eye");
        vmcam = GameObject.Find ("Camera").GetComponent<vmcamera>();

        premode = 0;

        m_StickToGroundForce = 10;
        m_GravityMultiplier = 30;

        sensXWalkthrough=1.55f;
        sensYWalkthrough=1.5f;
        sensWWalkthrough=30f; //6
        sensXBrowse=0.1f;
        sensYBrowse=0.1f;
        sensWBrowse=50f;
        keySensitivity = 0.03f;
        controlSensitivity = 1.0f;

        eyelevel = 1.7f;    // 170cm

        posyHeadcenter = headcenter.transform.localPosition.y;

        m_CharacterController.height = eyelevel;
    }

    private void Update()
    {
        GetInput();
    }

    public void FPSMoveToTarget(GameObject target)
    {
        CenterOfObject =  target.transform.position;
        BoxCollider bc;
        MeshFilter  mf;
        float size;
        if(bc = target.GetComponent<BoxCollider>())  size = bc.size.y * target.transform.localScale.y;  // Box collider必須
        else if(mf = target.GetComponent<MeshFilter>())  size = mf.mesh.bounds.size.y * target.transform.localScale.y;
        else  size = 1f; // 何も見つからなかったら縦が1mとみなす
        float vangle = 40f; // いまのところ40度固定
        float dist = size*0.5f/Mathf.Tan(vangle * 0.5f * 3.14f / 180f) + GameObject.Find ("FPSController/headcenter/eye").transform.localPosition.z; // eyeは少しｚ方向にずれてる
        Vector3 tmp2 = m_FPSController.transform.localPosition;
        tmp2.x = (CenterOfObject + dist * target.transform.forward).x;
        tmp2.y += 0.01f; // なぜかyをちょっと上げてやらないと、落っこちてしまうことがある
        tmp2.z = (CenterOfObject + dist * target.transform.forward).z;
        m_FPSController.transform.localPosition = tmp2;
		transform.localRotation = Quaternion.LookRotation(-target.transform.forward, target.transform.up); //.rotation; //Quaternion.Inverse(target.transform.rotation);

        headcenter.transform.localRotation = Quaternion.Euler (0, 0, 0); // チルトをゼロにする。チルトはheadcenterを回転。
        posyHeadcenter = CenterOfObject.y - m_FPSController.transform.localPosition.y;
        // 注意。この後にすぐにCharacterController.Move()を呼んでしまうとCharacterControllerが移動しない。ワンフレーム回すこと。
    }

    public void FPSMoveToTarget3DViewing(GameObject target)
    {
        CenterOfObject =  target.GetComponent<Collider>().bounds.center;
		DistanceCameraObject = (CenterOfObject - m_FPSController.transform.localPosition).magnitude;

        headcenter.transform.localRotation = Quaternion.Euler (0, 0, 0); // チルトをゼロにする。チルトはheadcenterを回転。
        posyHeadcenter = CenterOfObject.y - m_FPSController.transform.localPosition.y;
        // 注意。この後にすぐにCharacterController.Move()を呼んでしまうとCharacterControllerが移動しない。ワンフレーム回すこと。
    }

    public void FPSMoveToWalkthrough()
    {
        headcenter.transform.localRotation = Quaternion.Euler (0, 0, 0); // チルトをゼロにする。チルトはheadcenterを回転。
        Camera.main.fieldOfView = 40;
        //posyHeadcenter = CenterOfObject.y - m_FPSController.transform.localPosition.y;
        // 注意。この後にすぐにCharacterController.Move()を呼んでしまうとCharacterControllerが移動しない。ワンフレーム回すこと。
    }


    // CharacterControllerを動かす
    public void FPSUpdate(int mode, float dist)
    {
        Vector2 inputMove;
        Vector3 moveDir = Vector3.zero;
        Vector3 desiredMove;
        CollisionFlags m_CollisionFlags;


 		if(mode == 0) {   // Walkthroughのとき
            this.transform.localRotation *= Quaternion.Euler (0f, controlX * sensXWalkthrough * Time.deltaTime, 0f);  // pan the body
            headcenter.transform.localRotation *= Quaternion.Euler (-controlY * sensYWalkthrough * Time.deltaTime, 0f, 0f);    // tilt the head

            inputMove = new Vector2(controlHorizontal, controlW + controlVertical);

            desiredMove = transform.right * inputMove.x + transform.forward * inputMove.y;

            // 以下はCharacterControllerのFPSウォークスルーのコードそのまま
            // get a normal for the surface that is being touched to move along it
            
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo, m_CharacterController.height/2f);
//            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized; // これが元のなのだが、マウス感度がこれで消えてしまうことが分かった
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal);
            

            moveDir.x = desiredMove.x;
            moveDir.z = desiredMove.z;

            if (m_CharacterController.isGrounded)
            {
                moveDir.y = -m_StickToGroundForce;
            }
            else
            {
                moveDir += Physics.gravity * 2 * Time.fixedDeltaTime;
//                moveDir = new Vector3(0,-1,0); //Physics.gravity;
            }           

            m_CollisionFlags = m_CharacterController.Move(moveDir * sensWWalkthrough * Time.deltaTime);

            // walkthroughのときheadcenterは0.8m上方に固定。初期設定はされているが、Browseから戻ってきた時、ちゃんとまた0.8mになるように                
            Vector3 tmp = headcenter.transform.localPosition;
            tmp.y = eyelevel/2; 
            headcenter.transform.localPosition = tmp;

            float yyyyy =m_FPSController.transform.position.y;
            float yyyyy2 =m_FPSController.transform.localPosition.y;

        }
        else if(mode == 1)  // 2D Browseのとき
        { 
            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) { 
                controlX *= 0.5f;
                controlY *= 0.5f;
                controlW *= 5f;
                this.transform.localRotation *= Quaternion.Euler (0f, controlX * sensXWalkthrough * Time.deltaTime, 0f);  // pan the body
                headcenter.transform.localRotation *= Quaternion.Euler (-controlY * sensYWalkthrough * Time.deltaTime, 0f, 0f);    // tilt the head
                Camera.main.fieldOfView += -controlW * Camera.main.fieldOfView/40f;
                if(Camera.main.fieldOfView < 0.1f) Camera.main.fieldOfView=0.1f;
                if(Camera.main.fieldOfView > 100.0f) Camera.main.fieldOfView=100.0f;
                return;
            }

            controlX *= sensXBrowse;
            controlY *= sensYBrowse;
            controlW *= sensWBrowse;

            float distpic = 0.5f * dist;  // 距離に応じて動きをスローダウンする。0.5は調整値

            Vector3 vforward = this.transform.forward;
            Vector3 vside = Vector3.Cross(vforward, this.transform.up);    // 絵の横方向のベクトルを外積で求める
            vside.Normalize();
            Vector3 vec = (controlW + controlVertical) * vforward * distpic + (controlX + controlHorizontal) * vside * distpic;

            moveDir.x = vec.x;
            moveDir.z = vec.z;

            if (m_CharacterController.isGrounded)
            {
                moveDir.y = -m_StickToGroundForce;
            }
            else
            {
                moveDir += Physics.gravity * m_GravityMultiplier * Time.deltaTime;
            }           


            CollisionFlag2DBrowse = m_CharacterController.Move(moveDir * Time.deltaTime);


            // 上下移動はheadcenterを上下して行う。CharacterControllerのheightは今のところ２ｍ。
            // walkthrough時の目の高さは1.8m（ただし、なぜかheightを２ｍにすると、CharacterController高さが1.0686mになる）
            posyHeadcenter -= controlY * Time.deltaTime * distpic;
            Vector3 tmp = headcenter.transform.localPosition;
            tmp.x=0;
            tmp.y = posyHeadcenter;   // 前はm_CharacterController.height/2fを使っていたが、床の高さがゼロでないのにひかかった。。。
            tmp.z=0;
            headcenter.transform.localPosition = tmp;
               
        }
        else if(mode == 2)  // 3D Browseのとき
        { 
            float pxx, pzz;
            Quaternion pqq;

            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) { 
                controlX *= 0.5f;
                controlY *= 0.5f;
                controlW *= 5f;
                this.transform.localRotation *= Quaternion.Euler (0f, controlX * sensXWalkthrough * Time.deltaTime, 0f);  // pan the body
                headcenter.transform.localRotation *= Quaternion.Euler (-controlY * sensYWalkthrough * Time.deltaTime, 0f, 0f);    // tilt the head
                Camera.main.fieldOfView += -controlW * Camera.main.fieldOfView/40f;
                if(Camera.main.fieldOfView < 0.1f) Camera.main.fieldOfView=0.1f;
                if(Camera.main.fieldOfView > 100.0f) Camera.main.fieldOfView=100.0f;
                return;
            }


           
//          if(!MuseumManager.inverseCameraMove // 3Dブラウズは固定。もしこれをするなら設定メニューに追加
//          {
                controlX = -controlX;
                controlY = -controlY;
//          }

            controlX *= sensXBrowse;
            controlY *= sensYBrowse;
            controlW *= sensWBrowse;

            float Sensitivity3dobjectMx=-1.0f;
		    float Sensitivity3dobjectMw=-0.01f;

		    mousex = controlX;
		    mousey = controlY;
		    mousew = controlW;
		
            if (flagbrowsecolforth == 0) {
                if(((CollisionFlag2DBrowse & CollisionFlags.CollidedSides) != 0)&&(mousew>0)) {
                    mousew =0;
                    flagbrowsecolforth = 1;
                    flagbrowsecolback = -1;
                }
            }
            else if (flagbrowsecolforth == 1) {
                if(((CollisionFlag2DBrowse & CollisionFlags.CollidedSides) == 0) && (mousew<0)) {
                    flagbrowsecolforth = 0;
                    flagbrowsecolback = 0;
                }
                else if(mousew>=0)    mousew=0;
            }
        
            if (flagbrowsecolback == 0) {
                if(((CollisionFlag2DBrowse & CollisionFlags.CollidedSides) != 0)&&(mousew<0)) {
                    mousew =0;
                    flagbrowsecolback = 1;
                    flagbrowsecolforth = -1;
                }
            }
            if (flagbrowsecolback == 1) {
                if(((CollisionFlag2DBrowse & CollisionFlags.CollidedSides) == 0) && (mousew>0)) {
                    flagbrowsecolback = 0;
                    flagbrowsecolforth = 0;
                }
                else if(mousew<=0)    mousew=0;
            }

            /*
            float floorlevel = MuseumManager.exhibition.rooms[iroom].floorlevel;
            float ceilinglevel = floorlevel + MuseumManager.exhibition.rooms[iroom].ceilingheight;
            if((pyy<floorlevel+0.2f)&&(mousey<0))    mousey=0; // 20cmのマージン
            if((pyy>(ceilinglevel-0.2f))&&(mousey>0))    mousey=0;
            */

            // オブジェクト周りの回転移動を計算
            Vector3 VectorCenterToCamera;
		    Vector3 CenterNoHeight;
            Vector3 PositionOfCamera;
            PositionOfCamera = m_CharacterController.transform.position;
		    CenterNoHeight = CenterOfObject;
		    CenterNoHeight.y = PositionOfCamera.y;
		    VectorCenterToCamera = PositionOfCamera - CenterNoHeight;
		    VectorCenterToCamera = Quaternion.AngleAxis(Sensitivity3dobjectMx * mousex, new Vector3(0,1,0)) * VectorCenterToCamera;
		    VectorCenterToCamera.Normalize();
		    DistanceCameraObject = DistanceCameraObject + Sensitivity3dobjectMw * mousew;
		    PositionOfCamera = CenterNoHeight + DistanceCameraObject * VectorCenterToCamera;

		    pxx = PositionOfCamera.x;
            pzz = PositionOfCamera.z; 
            pqq = transform.localRotation;
		    pqq.SetLookRotation(CenterNoHeight - PositionOfCamera);
        
            // それに従ってCharacterControllerのｘとｚを動かす。ｙは動かさず、常に地面に接地。
            moveDir.x = pxx - m_CharacterController.transform.position.x;
            moveDir.z = pzz - m_CharacterController.transform.position.z;
            if (m_CharacterController.isGrounded)
            {
                moveDir.y = -10f;
            }
            else
            {
                moveDir += Physics.gravity*2*Time.fixedDeltaTime;
            }

            CollisionFlag2DBrowse = m_CharacterController.Move(moveDir);

            // オブジェクト周りの回転移動カメラの視線方向のQuatanionをゲット
            transform.localRotation = pqq;
                
            // 上下移動はheadcenterを上下して行う
            posyHeadcenter += controlY * Time.deltaTime * 0.3f;
            Vector3 tmp = headcenter.transform.localPosition;
            tmp.x=0;
            tmp.y = posyHeadcenter;
            tmp.z=0;
            headcenter.transform.localPosition = tmp;
                
        }

        premode = mode;

    }


    // マウス、キーボード、ジョイスティックなどの入力を受け付ける
    private void GetInput()
    {

        float diffx=0;
        float diffy=0;

        if(Input.GetMouseButtonDown(0))
        {   
            prexRot = Input.mousePosition.x;
            preyRot = Input.mousePosition.y;
        }
        if(Input.GetMouseButton(0)) // while pressed
        {   
            diffx = - prexRot + Input.mousePosition.x;
            diffy = - preyRot + Input.mousePosition.y;
        }
        prexRot = Input.mousePosition.x;
        preyRot = Input.mousePosition.y;

        controlX = diffx;
        controlY = diffy;
        controlW = Input.GetAxis("Mouse ScrollWheel");
        controlVertical = Input.GetAxis("Vertical") * keySensitivity;    // キーボード
		controlHorizontal = Input.GetAxis("Horizontal") * keySensitivity;    // キーボード

        float ms = 0;
        if(controlSensitivity < 0.5f)   ms = 1.6f * controlSensitivity + 0.2f;
        else   ms = 8f * controlSensitivity - 3f;
        controlX *= ms;
        controlY *= ms;
        controlW *= ms;
        controlVertical *= ms;
        controlHorizontal *= ms;
    }

    public void ResetFPSCamera(float x, float y, float z, float rx, float ry)
    {
        this.transform.localPosition = new Vector3(x, y, z);
        m_FPSController.transform.localRotation = Quaternion.Euler(0f, ry, 0f);
		headcenter.transform.localRotation = Quaternion.Euler(rx, 0f, 0f);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Coin")
        {
            GetComponent<AudioSource>().Play();
        }
    }
    
    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Coin")
        {
            GetComponent<AudioSource>().Pause();
        }
    }

    /*
    public static bool GetButton(string name)
    {
        bool ret;

        if(name == "mouseleftbutton")
            ret = Input.GetMouseButtonDown(0);
        else if(name == "mouserightbutton")
            ret = Input.GetMouseButtonDown(1);
        else if(name == "mousemiddlebutton")
            ret = Input.GetMouseButtonDown(2);
        else
            ret = Input.GetKeyDown(name);

        return ret;
    }
      */

}

