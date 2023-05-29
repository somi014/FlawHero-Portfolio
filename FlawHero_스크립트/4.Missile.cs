using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    private bool                m_IsStart = false;
    public float                m_Speed = 10;                       //미사일 속도
    public float                m_rotSpeed = 1;                     //유도탄 회전 시간
    public float                m_HeightArc = 1;                    //포물선 높이
    public float                guidedTime = 0;                     //유도탄 터지는 시간
    private float               time;                               //유도탄 날아가는 시간
    public int                  attack = 0;                         //공격력 (기본 공격력에서 +- 랜덤)     
    private string              missileType;                        //미사일 타입

    public GameObject           missile = null;                     //미사일 프리팹
    public GameObject           bombEffect = null;                  //폭발하는 이팩트
    private Transform           m_Target;                           
    private Vector3             m_arrivedPosition = Vector3.zero;   //날아가 도착할 위치
    private Vector3             m_StartPosition = Vector3.zero;     //발사 위치

    //필요한 컴포넌트
    private SpriteRenderer      spriteRenderer;

    void OnEnable()
    {
        m_Target = FindObjectOfType<HeroKnight>().gameObject.transform;
        spriteRenderer = missile.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (m_IsStart)      
        {
            if ("Palabola" == missileType)                  //포물선
            {
                float x0 = m_StartPosition.x;
                float x1 = m_arrivedPosition.x;
                float distance = x1 - x0;                   //도착 위치와 발사 위치 거리
                float nextX = Mathf.MoveTowards(transform.position.x, x1, m_Speed * Time.deltaTime);        //x값 이동
                float baseY = Mathf.Lerp(m_StartPosition.y, m_arrivedPosition.y, (nextX - x0) / distance);  //시작 위치와 도착 위치의 y값 선형보간
                float arc = m_HeightArc * (nextX - x0) * (nextX - x1) / (-0.25f * distance * distance);     //포물선의 높이
                Vector3 nextPosition = new Vector3(nextX, baseY + arc, transform.position.z);

                transform.rotation = LookAt2D(nextPosition - transform.position);
                transform.position = nextPosition;
                
                if (nextPosition == m_arrivedPosition)
                    Arrived();
            }
            else if ("Guided" == missileType)               //유도탄
            {
                time += Time.deltaTime;

                if (guidedTime <= time)
                {
                    Arrived();
                }

                Vector3 dir = (m_Target.position - transform.position).normalized;                                      //날아가야하는 방향
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;                                                //바라보는 방향
                Quaternion rotTarget = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotTarget, Time.deltaTime * m_rotSpeed);
                transform.position += new Vector3(dir.x * m_Speed * Time.deltaTime, dir.y * m_Speed * Time.deltaTime, 0);
            }
        }
    }

    Quaternion LookAt2D(Vector2 forward)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    }

    //미사일 값 초기화
    public void MissileInit(Transform start, Vector3 target,int atk, string type, bool flip)
    {
        missile.SetActive(true);

        this.transform.position = start.position;
        m_StartPosition = this.transform.position;          //미사일 발사 위치
        m_arrivedPosition = target;
        attack = atk;                                       //미사일 공격력
        missileType = type;                                 //미사일 종류

        m_IsStart = true;

        StartCoroutine(DisappearCoroutine());
    }

    //일정 시간 지나면 자동으로 터짐
    IEnumerator DisappearCoroutine()
    {
        float time = 0;
        while(time < 2.0f)
        {
            time += Time.deltaTime;
            yield return null;
        }
        BombEffectOn();
    }

    private void Arrived()
    {
        BombEffectOn();
    }
    
    //폭탄 터지는 효과
    public void BombEffectOn()
    {
        m_IsStart = false;
        missile.SetActive(false);                           //미사일 이미지 안 보이게
        bombEffect.SetActive(true);                         //터지는 이미지 실행
    }

    //터지는 효과 끝나면 호출 -> 미사일 오브젝트 풀에 다시 넣음
    public void InsertPool()
    {
        time = 0f;
        bombEffect.SetActive(false);                        //터지는 이미지 안 보이게
        MissilePool.instance.InsertQueue(this.gameObject);  //오브젝트 풀에 미사일 넣음
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 9)
        {
            BombEffectOn();                             //폭탄 터지는 효과
            collision.gameObject.GetComponent<LivingEntity>().Damaged(attack);
        }
    }
}