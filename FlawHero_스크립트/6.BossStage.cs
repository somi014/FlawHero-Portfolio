using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossStage : MonoBehaviour
{
    public bool                     stageOn = false;
    private bool                    clear = false;

    public GameObject               arrow = null;
    public GameObject               miniMap = null;                 
    public GameObject               hpBar = null;                   //보스 체력바
    public GameObject               boss = null;                    //보스
    public GameObject               wall = null;                    //보스전 시작하면 벽으로 막음
    public GameObject               chest = null;                   //보스 죽으면 상자 오픈

    //필요한 컴포넌트
    public LivingEntity             livingEntity = null;            //보스
    private LivingEntity            player = null;                  //플레이어
    private CameraManager           cameraManager = null;

    private WaitForSeconds          wait = new WaitForSeconds(0.5f);

    void Start()
    {
        player = FindObjectOfType<HeroKnight>().gameObject.GetComponent<LivingEntity>();
        cameraManager = FindObjectOfType<CameraManager>();
    }

    void Update()
    {
        if (!clear && stageOn)                                  //보스 스테이지 클리어 x, 스테이지 시작
        {
            if (livingEntity.currentHp <= 0)                    //보스 죽으면
            {
                clear = true;
                hpBar.SetActive(false);
                chest.SetActive(true);                          //보물 상자 오픈

            }
        }
    }

    //마을로 돌아가기 버튼
    public void ActiveFasle()
    {
        if (!stageOn) return;                                       //보스 스테이지가 아니면 리턴 -> 옵션에 마을 돌아가는 버튼 누르면 호출되기 때문에

        boss.GetComponent<Boss>().Init();                           //위치 초기화
        livingEntity.Init();                                        //체력 초기화

        stageOn = false;

        miniMap.SetActive(true);                                    //미니맵 보이게
        hpBar.SetActive(false);                                     //hpbar 안 보이게
        boss.SetActive(false);                                      //보스 안 보이게

        wall.transform.localPosition = new Vector2(0, -220);        //벽 안 보이는 위치로 이동
    }

    private void ActiveTrue()
    {
        boss.SetActive(true);                           //보스 보이게
        hpBar.SetActive(true);                          //hpbar
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && !stageOn)
        {
            arrow.SetActive(false);                         //화살표 표시 안보이게
            miniMap.SetActive(false);                       //미니맵 안 보이게

            Invoke("ActiveTrue", 0.5f);
            StartCoroutine(WallCoroutine());
            cameraManager.ShakeCamera();
        }
    }

    //벽 올라오는 효과
    IEnumerator WallCoroutine()
    {
        wall.SetActive(true);

        float y = wall.transform.localPosition.y;
        while (y < 0)                                   //y값이 0이 될 때 벽이 다 올라감
        {
            y += 1f;
            wall.transform.localPosition = new Vector2(0, y);
            yield return null;
        }

        yield return wait;
        stageOn = true;                                 //벽이 다 올라오면 스테이지 시작
    }
}
