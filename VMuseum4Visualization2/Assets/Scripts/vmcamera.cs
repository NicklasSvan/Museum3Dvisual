using UnityEngine;
using System.Collections;
using System;

public class vmcamera : MonoBehaviour {

	private Camera	vmcam;
	private FirstPersonController m_FirstPersonController;
	GameObject m_FPSController;
	private GameObject m_eye;
	public float	posx, posy, posz;
	public Quaternion rotqnion;
	public float	tvx, tvy, tvz, tvrx, tvry, tvrz, tvf;
	public static int	mode, premode;
	private float kx, ky, kz;
	private Quaternion	kq, kqlook, kqDirectionObject;
	private Vector3 CenterOfObject, PositionOfCamera;
	private float DistanceCameraObject;
	public float	speedposDefault;
	public float	speedrotDefault;
	public float	cameraInertia;
    bool flagmovebrowse=false;
    bool flagmouseclick = false;
    Vector3 mouseposition = new Vector3(0,0,0);
	bool help;


	void Start () {
		vmcam = Camera.main;
		vmcam.enabled = true;
		m_eye = GameObject.Find ("FPSController/headcenter/eye");
		m_FirstPersonController  = GameObject.Find("FPSController").GetComponent<FirstPersonController>();
        m_FPSController = GameObject.Find ("FPSController");
		mode = premode = 0; // walkthrough mode
		speedposDefault = 10.0f;
		speedrotDefault = 10.0f;		
        cameraInertia = 0.5f;
		help = true;
		Cursor.visible = true;

        posx = m_FirstPersonController.transform.position.x;
        posy = m_FirstPersonController.transform.position.y;
        posz = m_FirstPersonController.transform.position.z;
        rotqnion = m_eye.transform.rotation;
        
        vmcam.fieldOfView = 40.0f;
    }
	
    int skipframe = 0;

	GameObject selectedObject=null;

	void Update () {
		float diff;
		Ray	ray;
		RaycastHit hit;

		if(Input.GetKeyDown(KeyCode.Escape))	{	// quit
			Application.Quit();
		}
		if (Input.GetKeyDown ("f1")){
			if(help)   help=false;
			else    help=true;
        }
		if( Input.GetKeyDown(KeyCode.Alpha1) )	{ 
            moveToHome();
            skipframe = 1;
		}
        
		if( Input.GetKeyDown(KeyCode.F11) )	{ 
            Screen.fullScreen = !Screen.fullScreen;
		}
        

        // ����Ȃ��Ƃ͂��܂肵�����Ȃ����ACharacterController�̈ʒu�������ŕς��Ă��ACharacterController.Move()���R�[��
        // �����ƁA���̈ʒu�ɖ߂��Ă��܂����Ƃ�����i����Home�{�^�����j�A�����ʒu�ς��̌�A2�t���[���قǉ񂷂�OK�ɂȂ�
        // �Ȃ̂ŁA��������Ă������A�����ƃX�}�[�g�ȕ��@������͂��B���̂Ƃ��땪���炸�B
        if(skipframe != 0)   {
            skipframe++;
            if(skipframe == 4)  skipframe=0;
            else return;
        }

        
        float distpic=1;
        if(mode == 1) { 
            if(selectedObject != null)
            { 
                // �ȉ��̓e���|�����B�{���͍�i�ł͂Ȃ��A�����Ă���ǂ�T�����āA���̋����𑪂�ׂ��B�ȉ��̕��@�͂����΂߂̕ǂ��o�Ă�����A�E�g�B
                if(Mathf.Abs(selectedObject.transform.forward.normalized.z) == 1.0f)
                    distpic = Mathf.Abs(selectedObject.transform.localPosition.z - vmcam.transform.localPosition.z);
                else if(Mathf.Abs(selectedObject.transform.forward.normalized.x) == 1.0f)
                    distpic = Mathf.Abs(selectedObject.transform.localPosition.x - vmcam.transform.localPosition.x);
                else
                    distpic = 1.0f;
            }
        }

        // CharacterController�𓮂���
        m_FirstPersonController.FPSUpdate(mode, distpic);
 
        // �ȉ���mode�`�F���W�ł́ACharacterController�𓮂������ザ��Ȃ���FPSController��position���߂��Ⴍ����Ő����������Ȃ��B
        // �Ђ���Ƃ���ƁAVideo�t���[���ƁARigidbody�̃t���[���̍���������������Ȃ����A���̂Ƃ���悭������Ȃ��B
        if (Input.GetMouseButtonDown(0))  {
            flagmouseclick = true;
            mouseposition = Input.mousePosition;
        }
        if(flagmouseclick)  {
            if(Input.GetMouseButtonUp(0))  {
                float dist = (mouseposition - Input.mousePosition).magnitude;
                if(dist < 10f)  {
                    flagmovebrowse=true;
                    flagmouseclick = false;
                }
            }
        }
        if(flagmovebrowse || Input.GetKeyDown(KeyCode.Space))
        {
            flagmovebrowse=false;

            ray = vmcam.ScreenPointToRay(Input.mousePosition); //�}�E�X�̃|�W�V�������擾����Ray�ɑ��
			if(mode == 0)	{	
                if(Physics.Raycast(ray,out hit))  //�}�E�X�̃|�W�V��������Ray�𓊂��ĉ����ɓ���������hit�ɓ����
                {
				    selectedObject = hit.collider.gameObject;
				    string objtype = selectedObject.tag;
				    if(objtype == "2dobject")	{
                        mode=1;
                    }
				    if(objtype == "3dobject")	{
                        mode=2;
                    }
                }			
            }
            else  {
                mode=0;
            }
        }
        
     

       if((mode==1)&&(premode==0)) {
            m_FirstPersonController.FPSMoveToTarget(selectedObject);
            skipframe = 1;
        }
       if((mode==2)&&(premode==0)) {
            m_FirstPersonController.FPSMoveToTarget3DViewing(selectedObject);
            skipframe = 1;
        }
       if(((premode==1)||(premode==2))&&(mode==0)) {
            m_FirstPersonController.FPSMoveToWalkthrough();
            skipframe = 1;
        }
        premode = mode;

	    // �}�E�X�œ������Ă���CharacterController�̐���ۂ̖ڂ̈ʒu�ƕ������Q�b�g����        
    	kx = m_eye.transform.position.x;
		ky = m_eye.transform.position.y;
		kz = m_eye.transform.position.z;
		kq = m_eye.transform.rotation;

        float speedpos = speedposDefault * cameraInertia;
        float speedrot = speedrotDefault * cameraInertia;
				
		diff = kx - posx;
		posx = posx + speedpos * diff * Time.deltaTime;
		diff = ky - posy;
		posy = posy + speedpos * diff * Time.deltaTime;
		diff = kz - posz;
		posz = posz + speedpos * diff * Time.deltaTime;
		rotqnion = Quaternion.Slerp (kq, rotqnion, 1.0f - speedrot * Time.deltaTime);

		vmcam.transform.localPosition = new Vector3(posx, posy, posz);
		vmcam.transform.rotation = rotqnion;
		//vmcam.fieldOfView = 40.0f;
	}



    public void moveToHome()
    {
		float rstx, rsty, rstz;
        float rstrx, rstry;
		rstx = 0f;  
        rsty = 2.4f;  
        rstz = 0f; 
        rstrx = 0;
        rstry = 180;

		m_FirstPersonController.ResetFPSCamera(rstx, rsty, rstz, rstrx, rstry);
        skipframe = 1;
    }

	void OnGUI() {
		if(help){
			GUI.Label(new Rect(10, 15, 200, 20), "F1 : Help on/off");
			GUI.Label(new Rect(10, 30, 400, 20), "Mouse with holding left button: View direction");
			GUI.Label(new Rect(10, 45, 300, 20), "Mouse wheel: Move camera back & forth");
			GUI.Label(new Rect(10, 60, 300, 20), "W, S: Move camera back & forth");
			GUI.Label(new Rect(10, 75, 300, 20), "A, D: Move camera left & right");
			GUI.Label(new Rect(10, 90, 400, 20), "Mouse click on an artwork for Detail view");	
			GUI.Label(new Rect(10, 105, 400, 20), "Alt+Mouse: view direction & zoom in detail view");	
			GUI.Label(new Rect(10, 120, 400, 20), "Mouse click or Space key : Walkthrough / Detail view");	
			GUI.Label(new Rect(10, 135, 300, 20), "1 : Go to home position");	
			GUI.Label(new Rect(10, 150, 100, 20), "ESC: Quit");
		}
	}
    

}

