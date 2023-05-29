using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class HeroKnight : MonoBehaviour
{
    [SerializeField] int                jumpCount = 2;                  //점프 가능 횟수
    [SerializeField] float              m_speed = 4.0f;                 //이동 속도
    [SerializeField] float              m_jumpForce = 7.5f;             //점프 파워
    [SerializeField] float              m_rollForce = 6.0f;             //구르기 파워
    [SerializeField] GameObject         m_slideDust;                    //벽타기 이팩트

    //필요한 컴포넌트
    private Animator                    m_animator;
    private Rigidbody2D                 m_body2d;
    private Sensor_HeroKnight           m_groundSensor;
    private Sensor_HeroKnight           m_wallSensorR1;
    private Sensor_HeroKnight           m_wallSensorR2;
    private Sensor_HeroKnight           m_wallSensorL1;
    private Sensor_HeroKnight           m_wallSensorL2;
    private AttackTrigger               attack_trigger;
    private LivingEntity                livingEntity;
    private AudioManager                audioManager;
    private GameManager                 gameManager;

    private bool                        m_grounded = false;             //바닥 체크
    private bool                        m_rolling = false;              //구르기 체크
    private bool                        m_defense = false;              //방어 체크
    private bool                        trapOn = false;                 //가시 밟았는지 체크
    private int                         m_facingDirection = 1;          //바라보는 방향 = flip.x
    private int                         m_currentAttack = 0;            //현재 공격 패턴
    private float                       m_timeSinceAttack = 0.0f;       //마지막 공격 시간
    private float                       m_delayToIdle = 0.0f;           //Idle 모션 천천히 변환 시간
    public float                        keypadTime = 0.0f;              //방향키 입력 값(시간)
    public string                       attackSound1;                   //공격 소리1
    public string                       attackSound2;                   //공격 소리2
    public string                       attackSound3;                   //공격 소리3

    private IEnumerator                 moveCoroutine;                  //방향키 값 Coroutine    

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        livingEntity = GetComponent<LivingEntity>();
        audioManager = FindObjectOfType<AudioManager>();
        gameManager = FindObjectOfType<GameManager>();

        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        attack_trigger = transform.Find("AttackTrigger").GetComponent<AttackTrigger>();
    }

    void Update()
    {
        if (livingEntity.currentHp <= 0) return;

        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
            jumpCount = 2;
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // -- Handle Animations --
        //Wall Slide
        m_animator.SetBool("WallSlide", (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State()));
              
#if UNITY_ANDROID

        //Movement
        if (!m_rolling && !trapOn)
        {
            m_body2d.velocity = new Vector2(keypadTime * m_facingDirection * m_speed, m_body2d.velocity.y);
        }

        //Animations
        //Run
        if (Mathf.Abs(keypadTime) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }

#else
        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
            attack_trigger.transform.localPosition = new Vector2(0.4f, attack_trigger.transform.localPosition.y);
        }

        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
            attack_trigger.transform.localPosition = new Vector2(-1.4f, attack_trigger.transform.localPosition.y);
        }

        // Move
        if (!m_rolling)
        {
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
        }

        // -- Handle Animations --
       
        //Death
        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }

        //Hurt
        else if (Input.GetKeyDown("q") && !m_rolling)
            m_animator.SetTrigger("Hurt");

        //Attack
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }

        else if (Input.GetMouseButtonUp(1))
            m_animator.SetBool("IdleBlock", false);

        // Roll
        else if (Input.GetKeyDown("left shift") && !m_rolling)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }

        //Jump
        else if (Input.GetKeyDown("space") && jumpCount > 0 && !m_rolling && m_grounded)
        {
            if (m_grounded)
            {
                m_animator.SetTrigger("Jump");
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
                m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                m_groundSensor.Disable(0.2f);
            }
            else if (m_body2d.velocity.y > 0)
            {
                m_animator.SetTrigger("Jump");
                m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                m_groundSensor.Disable(0.2f);
                //Double Jump effect instantiate
            }

            jumpCount--;
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
#endif
    }

    IEnumerator MoveKeypadCount()
    {
        while (!gameManager.notMove)            //ui 숨겨지면 notMove = true
        {
            if (keypadTime <= 1.01f)
                keypadTime += 0.1f;
            yield return null;
        }
        MoveButtonUp();
    }

    public void LeftMoveButton()
    {
        GetComponent<SpriteRenderer>().flipX = true;
        m_facingDirection = -1;
        attack_trigger.transform.localPosition = new Vector2(-1.4f, attack_trigger.transform.localPosition.y);          //공격 트리거 위치 변경

        if (moveCoroutine == null)
        {
            moveCoroutine = MoveKeypadCount();
            StartCoroutine(moveCoroutine);
        }
    }

    public void RightMoveButton()
    {
        GetComponent<SpriteRenderer>().flipX = false;
        m_facingDirection = 1;
        attack_trigger.transform.localPosition = new Vector2(0.4f, attack_trigger.transform.localPosition.y);       //공격 트리거 위치 변경

        if (moveCoroutine == null)
        {
            moveCoroutine = MoveKeypadCount();
            StartCoroutine(moveCoroutine);
        }
    }

    //방향키 터치가 끝났을 때
    public void MoveButtonUp()
    {
        StopAllCoroutines();
        moveCoroutine = null;
        keypadTime = 0;
    }

    public void JumpButton()
    {
        if (jumpCount > 0 && !m_rolling)
        {
            if (m_grounded)                             //바닥일 때
            {
                m_animator.SetTrigger("Jump");
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
                m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                m_groundSensor.Disable(0.2f);
            }
            else if (m_body2d.velocity.y > 0)           //공중 일 때 한번 더 
            {
                m_animator.SetTrigger("Jump");
                m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
                m_groundSensor.Disable(0.2f);
            }

            jumpCount--;
        }
    }

    public void AttackButton()
    {
        if (m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);
            audioManager.Play("AttackSound" + m_currentAttack);                             //사운드

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }
    }

    public void RollButton()
    {
        if (!m_rolling && m_grounded)
        {
            livingEntity.isRoll = true;
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }
    }

    public void DefenseButton()
    {
        if (!m_rolling && !m_defense)
        {
            livingEntity.isDefense = true;
            m_defense = true;
            m_animator.SetTrigger("Block");
        }
    }

    // Animation Events
    // Called in end of roll animation.
    void AE_ResetRoll()
    {
        m_rolling = false;
        livingEntity.isRoll = false;
    }

    void AE_ResetDefense()
    {
        m_defense = false;
        livingEntity.isDefense = false;
    }

    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)             //오른쪽 바라보면
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    //가시 밟았을 때 or 보스 돌진
    public void StepOnTrap(Vector3 pos)
    {
        int dircX = transform.position.x - pos.x > 0 ? 1 : -1;   //팅겨져 나갈 방향
        int dircY = transform.position.y - pos.y > 0 ? 1 : -1;   //팅겨져 나갈 방향

        StartCoroutine(OnTrapCoroutine(dircX, dircY));
        if (!m_rolling && !livingEntity.isDead)
            m_animator.SetTrigger("Hurt");                      //공격 받는 애니메이션
    }

    IEnumerator OnTrapCoroutine(int dircX, int dircY)
    {
        float time = 0;
        while(time <= 0.3f)
        {
            time += Time.deltaTime;

            m_body2d.velocity = new Vector2(dircX * 2, dircY *2);   //해당 방향으로 팅겨져 나감
            yield return null;
        }
    }

    //게임 오버에서 마을로 돌아가면 호출
    public void Restart()
    {
        StopAllCoroutines();
        moveCoroutine = null;
        livingEntity.currentHp = livingEntity.maxHp;            //플레이어 체력 초기화
        livingEntity.isDead = false;
        gameObject.SetActive(true);                             //플레이어 활성화
        trapOn = false;                                         //가시 밟은 상태 초기화
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))            //가시 밟으면
        {
            if (livingEntity.currentHp <= 0 || m_rolling) return;

            trapOn = true;
            if (trapOn)
            {
                StepOnTrap(collision.gameObject.transform.position);

                int temp = (livingEntity.defense + 1);
                livingEntity.Damaged(temp);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))            //가시 밟으면
        {
            trapOn = false;
        }
    }
}
