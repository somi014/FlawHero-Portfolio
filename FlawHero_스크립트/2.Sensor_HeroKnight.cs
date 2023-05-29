using UnityEngine;
using System.Collections;

public class Sensor_HeroKnight : MonoBehaviour {

    private int m_ColCount = 0;                 //상태 카운트
    private float m_DisableTimer;               //상태 체크 딜레이용

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    public bool State()
    {
        if (m_DisableTimer > 0)                 //점프 일 때 
            return false;
        return m_ColCount > 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != 0)
            m_ColCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != 0)
            m_ColCount--;
    }

    void Update()
    {
        m_DisableTimer -= Time.deltaTime;
    }

    public void Disable(float duration)
    {
        m_DisableTimer = duration;          //점프 키 눌렀을 때 호출 0.2
    }
}
