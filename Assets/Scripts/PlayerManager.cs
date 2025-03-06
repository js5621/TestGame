using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination; //namespace : 소속(이름이 중복이 많아서)
public class PlayerManager : MonoBehaviour
{
    
    List<int> listMultiValue = new List<int>();
    int dan = 2;
    int index = 1;
    int listIndex = 0;

    public float moveSpeed = 5.0f;
    public float mouseSensitivity = 100.0f;
    public Transform cameraTransform;
    public CharacterController characterController;
    public Transform playerHead; // 플레이어 머리위치(1인칭 모드를 위해서)
    public float thirdPersonDistance = 3.0f;
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, .0f);//3인칭 모드에서 카메라 오프셋
    public Transform playLookObj;//플레이어 시야 위치 

    public float zoomDistance = 1.0f;//카메라가 확대될때의 거리(3인칭몯)
    public float zoomSpeed = 5.0f;// 확대축소 속도
    public float defaultFov = 60.0f;// 기본 카메라 시야각 
    public float zoomFov = 30.0f;//확대 시 카메라 시야각 (1인칭모드에서 사용_

    private float currentDistance;//현재 카메라와의 거리(3인칭 모드)
    private float targerDistance;// 목표 카메라와의 거리
    private float targetFov;//목표 Fov
    private bool isZoomed =false;//확대 여부 확인
    private Coroutine zoomCoroutine;// 코루틴을 사용하여 확대 축소 처리
    private Camera mainCamera;//카메라 컴포넌트 

    private float pitch = 0.0f;//위 아래 회전 값
    private float yaw = 0.0f;//좌우 회전 값
    private bool isFirstPerson = false;//1인칭 모드 여부
    private bool isRotateAroundPlayer = true;//카메라가 플레이어 주위를 회전 하는지 여부 

    //중력 관련 변수 
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround;
 


    void Start()
    {
        //for (int i = 2; i <=9;i++)
        //{
        //    for (int j = 1;j<=9;j++)
        //    {
        //        listMultiValue.Add(i*j);
        //    }
        //}

        //for (int i=0;i<listMultiValue.Count;i++)
        //{
        //    Debug.Log(i.ToString()+":"+listMultiValue[i]);
        //}

        //listMultiValue.Clear();
        /*
        while (dan < 10)
        {
            while (index < 10)
            {
                listMultiValue.Add(dan * index);
                index++;
            }
            index = 1;
            dan++;
        }
        */
        /*
        while (listIndex<listMultiValue.Count)
        {
            Debug.Log(listIndex.ToString() + ":" + listMultiValue[listIndex]);
            
        }
        */

        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targerDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;

    }

    

    
    void Update()
    {
        //마우스 입력을 받아 카메라와 플레이어 회전처리
        float mouseX = Input.GetAxis("Mouse X")*mouseSensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        isGround = characterController.isGrounded;

        if(isGround&&velocity.y < 0)
        {

            velocity.y = -2f;

        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭모드" : "3인칭 모드");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotateAroundPlayer= !isRotateAroundPlayer;
            Debug.Log(isRotateAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어가 시야에 따라서 회전합니다.");
        }

        if(isFirstPerson)
        {
            FirstPersonMovement();
        }

        else
        {
            ThirdPersonMovement();
        }
       

    }
    void FirstPersonMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //
        Vector3 moveDirection = cameraTransform.forward*vertical+cameraTransform.right*horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection*moveSpeed*Time.deltaTime); //1인칭 설정 시 캐릭터 움직임

        cameraTransform.position = playerHead.transform.position;//1인칭 카메라의 위치 설정     
        cameraTransform.rotation = Quaternion.Euler(pitch,yaw,0);//마우스 회전값으로 카메라 회전 조정

        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0);//플레이어의 회전 위치 설정 

    }

    void ThirdPersonMovement()
    {

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if(isRotateAroundPlayer)
        {
            //카메라가 플레이어 오른쪽에서 회전하도록 설정
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동 
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정 
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }

        else
        {
            //플레이어가 직접 회전하는 모드 
            transform.rotation =Quaternion.Euler(0,yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransform.position = playLookObj.position+thirdPersonOffset+Quaternion.Euler(pitch, yaw, 0)*direction;
            cameraTransform.LookAt(playLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
    }
}
