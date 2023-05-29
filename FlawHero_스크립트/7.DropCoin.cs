using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCoin : MonoBehaviour
{
    private Rigidbody2D         rid;                            
    private BoxCollider2D       trigger;                        //일정 시간 뒤 trigger 활성화
    private Vector2             dropPow;                        //팅겨져 나갈 방향

    void OnEnable()
    {
        StartCoroutine(DropCoinTime());

        Physics2D.IgnoreLayerCollision(11, 11, true);           //코인끼리 충돌 무시

        int upDown = Random.Range(1, 3);
        int leftRight = Random.Range(-2, 3);

        rid = gameObject.GetComponent<Rigidbody2D>();
        trigger = gameObject.GetComponent<BoxCollider2D>();

        dropPow = new Vector2(leftRight, upDown);
        rid.AddForce(dropPow, ForceMode2D.Impulse);
    }

    IEnumerator DropCoinTime()
    {
        yield return new WaitForSeconds(0.8f);
        this.trigger.enabled = true;                //isTrigger = true
        rid.velocity = Vector2.zero;                //초기화
        yield return null;

        StartCoroutine(Disappear());
    }

    IEnumerator Disappear()
    {
        float time = 0;
        while(time < 30.0f)                                         //30초 뒤에 사라지도록
        {
            time += Time.deltaTime;
            yield return null;
        }

        StopAllCoroutines();
        DropManager.instance.InsertQueue(this.gameObject);          //큐에 다시 넣음
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))              //플레이어에 닿으면
        {
            AudioManager audio = FindObjectOfType<AudioManager>();
            audio.Play("ItemSound");                                //사운드
            StopAllCoroutines();
            InventoryManager inven = FindObjectOfType<InventoryManager>();
            inven.coin += 1;
            DropManager.instance.InsertQueue(this.gameObject);      //큐에 다시 넣음
        }
    }
}
