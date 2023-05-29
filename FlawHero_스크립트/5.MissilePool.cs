using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissilePool : MonoBehaviour
{
    public static MissilePool instance;

    public int                      count = 20;                                 //미사일 개수
    public GameObject               missilePrefab = null;                       //미사일 프리팹
    private Queue<GameObject>       missileList = new Queue<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);         //씬 이동시 삭제 안되도록
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject missile = Instantiate(missilePrefab, this.transform);
            missileList.Enqueue(missile);
            missile.SetActive(false);
        }
    }

    //도착 위치 or 일정 시간 지나면 큐에 넣기
    public void InsertQueue(GameObject _object)
    {
        missileList.Enqueue(_object);
        _object.SetActive(false);
    }

    //큐에서 꺼내기
    public GameObject GetQueue()
    {
        GameObject _missile = missileList.Dequeue();
        _missile.SetActive(true);

        return _missile;
    }

    //미사일 발사
    public void FireMissile(Transform start, Vector3 target, int atk, string type, bool flip)
    {
        GameObject _missile = GetQueue();               //큐에서 미사일 꺼냄
        _missile.GetComponent<Missile>().MissileInit(start, target, atk, type, flip);
    }
}
