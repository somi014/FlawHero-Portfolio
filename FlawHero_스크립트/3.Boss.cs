using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("View config")]
    public bool m_bDebugMode = false;

    public List<Collider2D> hitedTargetContainer = new List<Collider2D>();

    [Range(0f, 360f)]
    public float                    m_horizontalViewAngle = 0f;         //시야각
    public float                    m_viewRadius = 1f;                  //시야 범위
    public float                    m_atkRadius = 2f;                   //공격 범위
    private float                   m_horizontalViewHalfAngle = 0f;     //시야각 반
    [Range(-180f, 180f)]
    public float                    m_viewRotateZ = 0f;                 //시야각 회전
    private float                   atkTime = 0.0f;                     //마지막 공격 시간
    public float                    atkCool = 2.0f;                     //공격 쿨
    public float                    currentSpeed = 1f;
    public float                    rushSpeed = 5.0f;
    private int                     nextMove = 0;
    private int                     facingDirection = -1;               //바라보는 방향
    private int                     random = 0;                         //공격 랜덤

    public bool                     canAttack = false;                  //공격 가능한지 -> think 끝나면 true
    public bool                     isAttack = false;                   //공격 모션 중인지
    public bool                     canMelee = false;                   //근접 공격 가능 거리면 true
    public bool                     isTurn = false;                     //바닥 체크해서 바닥이 아니면 반대로 이동

    private GameObject              player = null;
    private GameObject              attackTrigger = null;
    public Transform                missilePos;                         //미사일 나가는 위치
    private Vector3                 originPos;                          //보스 시작 위치

    //필요한 컴포넌트
    private Animator                anim = null;
    private SpriteRenderer          spriteRenderer = null;
    private LivingEntity            livingEntity = null;
    private MonsterGroundSensor     sensorRight = null;
    private MonsterGroundSensor     sensorLeft = null;
    private GameManager             gameManager = null;
    private MissilePool             missilePool = null;
    private BossStage               bossStage = null;

    public LayerMask                m_viewTargetMask;

    private IEnumerator             thinkCoroutine = null;
    private IEnumerator             attackCoroutine = null;

    void Start()
    {      
        player = GameObject.Find("Player");
        attackTrigger = transform.GetChild(0).gameObject;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        livingEntity = GetComponent<LivingEntity>();
        sensorRight = transform.Find("GroundSensorRight").GetComponent<MonsterGroundSensor>();
        sensorLeft = transform.Find("GroundSensorLeft").GetComponent<MonsterGroundSensor>();
        gameManager = FindObjectOfType<GameManager>();
        missilePool = FindObjectOfType<MissilePool>();
        bossStage = FindObjectOfType<BossStage>();

        originPos = transform.localPosition;                             //시작 위치 저장

        attackCoroutine = Attack1Coroutine();

        thinkCoroutine = Think();
        StartCoroutine(thinkCoroutine);
    }

    public void Init()
    {
        transform.localPosition = originPos;                //처음 위치로 이동
        StopAllCoroutines();
        isAttack = false;                                   //공격 모션 끊겼을 때
    }

    void Update()
    {
        //죽었거나 UI가 켜지면 움직임 멈춤
        if (livingEntity.isDead || gameManager.notMove || !bossStage.stageOn)
        {
            isAttack = false;       //공격 모션 끝
            StopAllCoroutines();
            thinkCoroutine = null;
            return;
        }

        if (canAttack && !isAttack)     //공격 가능 && 공격 중 아닐 때
        {
            if (canMelee)
            {
                atkTime += Time.deltaTime;

                if (atkTime >= atkCool && !isAttack)
                {
                    random = Random.Range(1, 3);                //1,2 근접 공격
                    anim.SetTrigger("Attack" + random);
                    isAttack = true;
                    atkTime = 0.0f;
                    if (random == 1)                            //돌진 
                    {
                        attackCoroutine = Attack1Coroutine();
                        StartCoroutine(attackCoroutine);
                    }
                }
            }
            else
            {
                atkTime += Time.deltaTime;

                if (atkTime >= atkCool && !isAttack)
                {
                    random = Random.Range(3, 5);                //3,4 원거리 공격
                    anim.SetTrigger("Attack" + random);
                    isAttack = true;
                    atkTime = 0.0f;
                }
            }

            random = 0;
            transform.position += new Vector3(nextMove * facingDirection * currentSpeed * Time.deltaTime, 0, 0);
        }
        else                                    //think 중
        {
            if(thinkCoroutine == null)
            {
                thinkCoroutine = Think();
                StartCoroutine(thinkCoroutine);
            }
            transform.position += new Vector3(nextMove * facingDirection * currentSpeed * Time.deltaTime, 0, 0);
            anim.SetBool("Move", nextMove == 1);
        }

        if (!sensorRight.State() || !sensorLeft.State())                //바닥 체크
        {
            if (!isTurn && nextMove != 0)
            {
                int index = facingDirection * -1;                      //반대 방향으로 움직이게
                SetFlip(index);
                CheckNextGround();
                isTurn = true;
            }
        }
    }

    IEnumerator Think()
    {
        canAttack = false;
        int random = Random.Range(0, 2);
        nextMove = random;

        yield return new WaitForSeconds(2.0f);          

        if (player.transform.position.x > transform.position.x)         //플레이어를 바라보도록
            facingDirection = 1;
        else
            facingDirection = -1;
        SetFlip(facingDirection);

        FindViewTargets();
        yield return new WaitForSeconds(0.01f);          
        canAttack = true;
    }

    //돌진 공격
    IEnumerator Attack1Coroutine()
    {
        float time = 0;
        nextMove = 1;                           //공격 1일 때만  

        while (time < 1.5f)
        {
            time += Time.deltaTime;
            currentSpeed = rushSpeed;           //이동속도 돌진 속도
            yield return null;
        }

        currentSpeed = 1.0f;                    //기본 속도
        anim.SetTrigger("Attack1End");          //공격 1 모션 끝나도록
        EndAttack();
    }

    //공격 모션 끝날 때
    public void EndAttack()
    {
        isAttack = false;       //공격 모션 끝
        thinkCoroutine = Think();
        StartCoroutine(thinkCoroutine);
    }

    //센서가 바닥에 닿았는지 체크
    private void CheckNextGround()
    {
        if (sensorRight.State() && sensorLeft.State())          //두 센서 모두 닿으면
            isTurn = false;
        else                                                    //한쪽이라도 안 닿으면 다시 재귀
            Invoke("CheckNextGround", 1.0f);
    }

    //좌우 변경
    private void SetFlip(int index)
    {
        facingDirection = index;
        spriteRenderer.flipX = facingDirection == 1;

        if (facingDirection == 1)
            attackTrigger.transform.localPosition = new Vector3(1.1f, -0.62f, 0);       //공격 트리거 위치 조정
        else
            attackTrigger.transform.localPosition = new Vector3(-0.8f, -0.62f, 0);
    }

    private Vector3 AngleToDirZ(float angleInDegree)
    {
        float radian = (angleInDegree - transform.eulerAngles.z) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0f);
    }

    //탐색 범위 기지모
    private void OnDrawGizmos()
    {
        if (m_bDebugMode)
        {
            m_horizontalViewHalfAngle = m_horizontalViewAngle * 0.5f;

            Vector3 originPos = transform.position;

            Gizmos.DrawWireSphere(originPos, m_viewRadius);

            Vector3 horizontalRightDir = AngleToDirZ(-m_horizontalViewHalfAngle + (m_viewRotateZ * facingDirection));
            Vector3 horizontalLeftDir = AngleToDirZ(m_horizontalViewHalfAngle + (m_viewRotateZ * facingDirection));
            Vector3 lookDir = AngleToDirZ(m_viewRotateZ * facingDirection);

            Debug.DrawRay(originPos, horizontalLeftDir * m_viewRadius, Color.cyan);
            Debug.DrawRay(originPos, lookDir * m_viewRadius, Color.green);
            Debug.DrawRay(originPos, horizontalRightDir * m_viewRadius, Color.cyan);
        }
    }

    //플레이어 찾기
    public void FindViewTargets()
    {
        hitedTargetContainer.Clear();

        Vector2 originPos = transform.position;
        Collider2D[] hitedTargets = Physics2D.OverlapCircleAll(originPos, m_viewRadius, m_viewTargetMask);          //플레이어 감지

        foreach (Collider2D hitedtarget in hitedTargets)
        {
            Vector2 targetPos = hitedtarget.transform.position;
            Vector2 dir = (targetPos - originPos).normalized;                               
            Vector2 lookDir = AngleToDirZ(m_viewRotateZ * facingDirection);                 

            float dot = Vector2.Dot(lookDir, dir);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;                          //플레이어 바라보는 시야각

            m_horizontalViewHalfAngle = m_horizontalViewAngle * 0.5f;

            if (angle <= m_horizontalViewHalfAngle)
            {
                RaycastHit2D rayHitedTarget = Physics2D.Raycast(originPos, dir, m_viewRadius, m_viewTargetMask);           //시야 범위내에 플레이어쪽으로 레이 쏨
                if (rayHitedTarget)     
                {
                    hitedTargetContainer.Add(hitedtarget);
                    if (m_bDebugMode)
                        Debug.DrawLine(originPos, targetPos, Color.red);
                }
            }
        }
        nextMove = 0;                                       //공격시 이동하지 않도록
        atkTime = 0;

        if (hitedTargetContainer.Count > 0)                 //범위내 플레이어 있으면 근접 공격
            canMelee = true;
        else                                                //없으면 원거리 공격
            canMelee = false;
    }

    //공격 3 발사
    public void ParabolaMissile()
    {
        int index = Random.Range(-1, 2);                    //데미지 추가 랜덤 값
        int atk = livingEntity.atkDamage + index;
        Vector3 pos = player.transform.position;
        missilePool.FireMissile(missilePos, pos, atk, "Palabola", facingDirection == 1);    //오브젝트 풀 호출
    }

    //공격 4 유도탄 발사
    public void GuidedMissile()
    {
        int index2 = Random.Range(-1, 3);                   //데미지 추가 랜덤 값
        int atk2 = livingEntity.atkDamage + index2;
        Vector3 pos2 = player.transform.position;
        missilePool.FireMissile(missilePos, pos2, atk2, "Guided", facingDirection == 1);    //오브젝트 풀 호출
    }

}
