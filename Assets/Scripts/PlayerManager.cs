using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination; //namespace : �Ҽ�(�̸��� �ߺ��� ���Ƽ�)
public class PlayerManager : MonoBehaviour
{
    
    List<int> listMultiValue = new List<int>();
    int dan = 2;
    int index = 1;
    int listIndex = 0;

    private float moveSpeed = 5.0f;
    public float mouseSensitivity = 100.0f;
    public Transform cameraTransform;
    public CharacterController characterController;
    public Transform playerHead; // �÷��̾� �Ӹ���ġ(1��Ī ��带 ���ؼ�)
    public float thirdPersonDistance = 3.0f;
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, .0f);//3��Ī ��忡�� ī�޶� ������
    public Transform playLookObj;//�÷��̾� �þ� ��ġ 

    public float zoomDistance = 1.0f;//ī�޶� Ȯ��ɶ��� �Ÿ�(3��Ī��)
    public float zoomSpeed = 5.0f;// Ȯ����� �ӵ�
    public float defaultFov = 60.0f;// �⺻ ī�޶� �þ߰� 
    public float zoomFov = 30.0f;//Ȯ�� �� ī�޶� �þ߰� (1��Ī��忡�� ���_

    private float currentDistance;//���� ī�޶���� �Ÿ�(3��Ī ���)
    private float targetDistance;// ��ǥ ī�޶���� �Ÿ�
    private float targetFov;//��ǥ Fov
    private bool isZoomed =false;//Ȯ�� ���� Ȯ��
    private Coroutine zoomCoroutine;// �ڷ�ƾ�� ����Ͽ� Ȯ�� ��� ó��
    private Camera mainCamera;//ī�޶� ������Ʈ 

    private float pitch = 0.0f;//�� �Ʒ� ȸ�� ��
    private float yaw = 0.0f;//�¿� ȸ�� ��
    private bool isFirstPerson = false;//1��Ī ��� ����
    private bool isRotateAroundPlayer = true;//ī�޶� �÷��̾� ������ ȸ�� �ϴ��� ���� 

    //�߷� ���� ���� 
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround;
 
    private Animator animator;
    private float horizontal;
    private float vertical;
    bool isRunning = false;

    public float walkSpeed =5.0f;
    public float runSpeed = 10.0f;
    public float currentSpeed = 1.0f;

    //��� 
    private bool isAim =false;
    private bool isFire = false;

    AudioSource audioSource;
    public AudioClip audioClipFire;
    public AudioClip audioClipWeaponChange;
    public GameObject RifleM4Obj;

    void Start()
    {
       
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
       

    }




    void Update()
    {
        //���콺 �Է��� �޾� ī�޶�� �÷��̾� ȸ��ó��
        float mouseX = Input.GetAxis("Mouse X")*mouseSensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        isGround = characterController.isGrounded;
        moveSpeed = isRunning ? runSpeed : walkSpeed;
        if (isGround&&velocity.y < 0)
        {

            velocity.y = -2f;

        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1��Ī���" : "3��Ī ���");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotateAroundPlayer= !isRotateAroundPlayer;
            Debug.Log(isRotateAroundPlayer ? "ī�޶� ������ ȸ���մϴ�." : "�÷��̾ �þ߿� ���� ȸ���մϴ�.");
        }
        
        if(isFirstPerson)
        {
            FirstPersonMovement();
        }

        else
        {
            ThirdPersonMovement();
        }
       
        if (Input.GetMouseButtonDown(1))
        {
                       
            isAim = true;
            animator.SetBool("IsAim", isAim);
            
            moveSpeed = 0;

            isFire = true;
            animator.SetBool("IsFire",isFire);
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine); //�̹� �ڷ�ƾ�� ����ǰ������� �ڷ�ƾ ���� �ߺ�����
            }

            if (isFirstPerson)//1��Ī ���� 
            {
               
                SetTargetFov(zoomFov);// ī�޶��� fov�� ���� 
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));

            }


            else
            {
                
                SetTargetDistance(zoomDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));

            }

        }

        if (Input.GetMouseButtonUp(1))
        {
            isAim = false;
            animator.SetBool("IsAim", isAim);

            isFire = false;
            animator.SetBool("IsFire", isFire);

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine); //�̹� �ڷ�ƾ�� ����ǰ������� �ڷ�ƾ ���� �ߺ�����
            }

            if (isFirstPerson)//1��Ī ���� 
            {
               
                SetTargetFov(defaultFov);// ī�޶��� fov�� ���� 
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));

            }

            else
            {
               
                SetTargetDistance(thirdPersonDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));

            }

        }

        if(Input.GetMouseButtonDown(0))
        {
            if (isAim)
            {
                isFire = true;
                animator.SetBool("IsFire", isFire);
                audioSource.PlayOneShot(audioClipFire);
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            if (isAim)
            {
                isFire = false;
                animator.SetBool("IsFire", isFire);
            }
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;   
        }
        else
        {
            isRunning =false;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioSource.PlayOneShot(audioClipWeaponChange);
            animator.SetTrigger("IsWeaponChange");
            RifleM4Obj.SetActive(true);
        }
        Debug.Log($"{horizontal}{vertical}");
        animator.SetFloat("Horizontal", horizontal);
        
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("IsRunning",isRunning);
        

    }
    void FirstPersonMovement()
    {
        if (!isAim)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
            moveDirection.y = 0;
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime); //1��Ī ���� �� ĳ���� ������
        }
        
        
        //
      

        cameraTransform.position = playerHead.transform.position;//1��Ī ī�޶��� ��ġ ����     
        cameraTransform.rotation = Quaternion.Euler(pitch,yaw,0);//���콺 ȸ�������� ī�޶� ȸ�� ����

        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0);//�÷��̾��� ȸ�� ��ġ ���� 

    }

    void ThirdPersonMovement()
    {

        if (!isAim)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            characterController.Move(move * moveSpeed * Time.deltaTime);
        }
        

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if(isRotateAroundPlayer)
        {
            //ī�޶� �÷��̾� �����ʿ��� ȸ���ϵ��� ����
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵� 
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ���� 
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }

        else
        {
            //�÷��̾ ���� ȸ���ϴ� ��� 
            transform.rotation =Quaternion.Euler(0,yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransform.position = playLookObj.position+thirdPersonOffset+Quaternion.Euler(pitch, yaw, 0)*direction;
            cameraTransform.LookAt(playLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
    }
    IEnumerator ZoomCamera(float targerDistance)
    {
        Debug.Log("ZCCCC");
        while(Math.Abs(currentDistance -targerDistance)>0.01f)
        {
            currentDistance =Mathf.Lerp(currentDistance,targerDistance,Time.deltaTime*zoomSpeed);
            yield return null;  
        }
        currentDistance = targerDistance;
    }
    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }

    public void SetTargetFov(float fov)
    {
        targetFov = fov;
    }



    IEnumerator ZoomFieldOfView(float targerDistance)
    {
        Debug.Log("FovScpe1");
        while (Math.Abs(mainCamera.fieldOfView - targerDistance) > 0.01f)
        {
            Debug.Log("FovScpe2");
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.fieldOfView =targetFov;
    }

    


}
