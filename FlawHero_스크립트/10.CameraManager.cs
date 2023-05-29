using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    
    private float               halfWidth;                  //카메라의 반너비
    private float               halfHeight;                 //카메라의 반높이 
    private float               shakeTime = 0;              //카메라 흔들리는 시간
    private float               shakeAmount = 0.05f;        //흔들리는 세기
    public float                moveSpeed;                  //카메라가 얼마나 빠른 속도로

    public GameObject           target;                     //카메라가 따라갈 대상 = 플레이어
    private Vector3             targetPosition;             //대상의 현재 위치값
    public BoxCollider2D        bound;                      //현재 바운드 설정
    private Vector3             minBound;                   //박스 콜라이더 영역의 최소 최대 xyz 값을 지님
    private Vector3             maxBound;

    //필요한 컴포넌트
    private GameManager         gameManager = null;
    private Camera              theCamera = null;
    private FadeManager         fadeManager = null;

    private WaitForSeconds      wait = new WaitForSeconds(1.0f);

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        theCamera = GetComponent<Camera>();
        fadeManager = FindObjectOfType<FadeManager>();

        minBound = bound.bounds.min;                                
        maxBound = bound.bounds.max;
        halfHeight = theCamera.orthographicSize;                                  
        halfWidth = halfHeight * Screen.width / Screen.height;      
    }

    void Update()
    {
        if (target.gameObject != null)
        {
            targetPosition.Set(target.transform.position.x, target.transform.position.y, this.transform.position.z);        // z축은 카메라

            this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, moveSpeed * Time.deltaTime);    // 1초에 무브 스피드만큼 이동

            //카메라 위치x , 최소값(바운드 최소 x + halfWidth(맵의 왼쪽 반)), 최대값(바운드 최대 x - halfWidth)
            float clampedX = Mathf.Clamp(this.transform.position.x, minBound.x + halfWidth, maxBound.x - halfWidth);
            float clampedY = Mathf.Clamp(this.transform.position.y, minBound.y + halfHeight, maxBound.y - halfHeight);

            this.transform.position = new Vector3(clampedX, clampedY, this.transform.position.z);
        }
    }

    //현재 맵의 바운드 설정
    public void SetBound(BoxCollider2D newBound)
    {
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine(newBound));
    }

    //바운드 교체 & fade 효과 Coroutine
    IEnumerator FadeCoroutine(BoxCollider2D newBound)
    {
        yield return wait;

        bound = newBound;
        minBound = bound.bounds.min;
        maxBound = bound.bounds.max;
        yield return wait;

        fadeManager.FadeIn();                           //mapManeger.changeMap fadeOut -> fadeIn
        gameManager.UnHideUI();                         //ui 보이게
    }

    //카메라 흔들리는 효과
    public void ShakeCamera()
    {
        shakeTime = 1.5f;                               //흔들리는 시간 설정
        StartCoroutine(ShakeCoroutine());
    }

    IEnumerator ShakeCoroutine()
    {
        Vector3 vec = transform.position;               
        while(shakeTime > 0)
        {
            transform.position = Random.insideUnitSphere * shakeAmount + vec;
            shakeTime -= Time.deltaTime;
            yield return null;
        }
        shakeTime = 0;                                  //흔들리는 시간 초기화
    }
}
