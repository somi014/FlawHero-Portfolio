using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public string                       currentMapName;                         //현재 맵 이름

    public Image                        miniMap;                                //미니맵
    public Image                        img_player;                             //플레이어 이미지
    public List<Image>                  img_npc = new List<Image>();            //npc 이미지
    public List<Image>                  img_monster = new List<Image>();        //몬스터 이미지

    public Transform                    right, left, top, bottom;               //현재 맵의 사이즈를 구할 좌표

    public GameObject                   moveArrow;                              //이동 방향 화살표
    public Transform                    player;                                 //플레이어 위치
    private List<Transform>             mapSize = new List<Transform>();        //현재 맵의 사이즈 좌표를 넣을 List
    private List<Transform>             npcPos = new List<Transform>();         //NPC의 위치를 넣을 List
    private List<Transform>             monsterPos = new List<Transform>();     //현재 맵의 몬스터의 위치를 넣을 List
    public List<string>                 mapName;                                //전체 맵 이름 List

    private IEnumerator                 clearCoroutine = null;                  //던전 클리어 했는지 확인하는 Coroutine
    private IEnumerator                 miniMapCoroutine = null;                //미니맵 플레이어 위치 업데이트 Coroutine
    private IEnumerator                 miniMapNpcCoroutine = null;             //미니맵 npc 위치 업데이트 Coroutine
    private IEnumerator                 miniMapMonsterCoroutine = null;         //미니맵 몬스터 위치 없데이트 Coroutine

    //필요한 컴포넌트
    private GameManager                 gameManager = null;
    private CameraManager               cameraManager = null;
    private FadeManager                 fadeManager = null;

    //미니맵 사이즈 지정
    private void SetMinimapSize()
    {
        miniMap.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 650);
        miniMap.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 250);
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        cameraManager = FindObjectOfType<CameraManager>();
        fadeManager = FindObjectOfType<FadeManager>();

        SetMinimapSize();                                   //미니맵 사이즈 지정
        currentMapName = "Town";                            //현재 맵 = town

        clearCoroutine = ClearCoroutine();

        miniMapNpcCoroutine = MiniMapNpcCoroutine();
        NpcImage(currentMapName);                           //npc 위치 업데이트(마을에서 시작하기 때문에)

        miniMapMonsterCoroutine = MiniMapMonsterCoroutine();

        miniMapCoroutine = MiniMapCoroutine();
        StartCoroutine(miniMapCoroutine);                   //미니맵 플레이어 위치 업데이트
    }

    //미니맵에 플레이어 위치 업데이트
    IEnumerator MiniMapCoroutine()
    {
        while(true)
        {
            Vector2 mapArea = new Vector2(Vector3.Distance(left.position, right.position), Vector3.Distance(bottom.position, top.position));    //맵 가로 세로 사이즈
            Vector2 charPos = new Vector2(Vector3.Distance(new Vector3(left.position.x, 0f, 0f), new Vector3(player.position.x, 0f, 0f)),
                Vector3.Distance((new Vector3(0f, bottom.position.y, 0f)), new Vector3(0f, player.position.y, 0f)));                            //맵과 플레이어 위치 사이 거리
            Vector2 normalPos = new Vector2(charPos.x / mapArea.x, charPos.y / mapArea.y);                                  //맵과 플레이어 위치 비율로 계산

            img_player.rectTransform.anchoredPosition =
                new Vector2(miniMap.rectTransform.sizeDelta.x * normalPos.x, miniMap.rectTransform.sizeDelta.y * normalPos.y);
            yield return null;
        }
    }

    //던전 클리어 확인
    public bool CheckClearDungoen()
    {
        if (currentMapName.Contains("Dungeon"))
        {
            for (int i = 0; i < 8; i++)
            {
                if (GameObject.Find(currentMapName).gameObject.transform.Find("CharacterObject").
                    gameObject.transform.GetChild(i).gameObject.activeSelf == true)
                    return false;
            }

            for (int i = 0; i < mapName.Count; i++)
            {
                if (mapName[i] == currentMapName)           //현재 맵과 이름이 같은 맵
                {
                    mapName.RemoveAt(i);
                    break;
                }
            }
        }
        else if (currentMapName.Contains("Boss"))
        {
            return false;
        }
        return true;
    }

    IEnumerator ClearCoroutine()
    {
        while(true)
        {
            if (CheckClearDungoen())
                moveArrow.SetActive(true);
            yield return null;
        }
    }

    //TransferMap trigger enter
    public void TriggerTransferMap()
    {
        moveArrow.SetActive(false);                                         //맵 이동하면 화살표 안보이게

        GameObject.Find(currentMapName).gameObject.transform.GetChild(1).
           gameObject.GetComponent<BoxCollider2D>().enabled = false;        //마지막 맵 바운드 비활성화

        ChangeMap(mapName[0]);                                              //첫번째 맵으로 이동 -> 던전 클리어하면 해당 맵은 지워짐

        Transform pos = GameObject.Find(currentMapName).gameObject.transform.GetChild(5).gameObject.transform;
        player.transform.position = pos.position;                           //플레이어 위치 다음 맵 지정된 위치로 이동

        BoxCollider2D col = GameObject.Find(currentMapName).gameObject.transform.GetChild(1).
            gameObject.GetComponent<BoxCollider2D>();
        col.enabled = true;                                                 //현재 맵 바운드 활성화

        cameraManager.SetBound(col);                                        //카메라 바운드 변경
    }

    IEnumerator FadeCoroutine()
    {
        fadeManager.FadeOut();
        yield return new WaitForSeconds(1.5f);
    }

    //맵 사이즈 새로 업데이트 -> TransferMap trigger에서 호출
    public void ChangeMap(string mapName)
    {
        gameManager.HideUI();                               //ui 안보이게
        StartCoroutine(FadeCoroutine());                    //fadeOut -> setBound에서 fadeIn

        currentMapName = mapName;
        miniMap.sprite = Resources.Load<Sprite>("Sprite/UI/map/" + currentMapName); //미니맵 변경
        StopCoroutine(miniMapCoroutine);                    //맵 사이즈 변경 전 코루틴 멈춤
        StopCoroutine(clearCoroutine);                      //클리어 확인 코루틴 멈춤
        mapSize.Clear();

        for (int i = 0; i < 4; i++)
        {
            mapSize.Add(GameObject.Find(currentMapName).gameObject.transform.Find("MapSize").
                gameObject.transform.GetChild(i).GetComponent<Transform>());
        }
        right = mapSize[0];
        left = mapSize[1];
        top = mapSize[2];
        bottom = mapSize[3];

        StartCoroutine(miniMapCoroutine);                   //미니맵 플레이어 위치 코루틴 실행
        StartCoroutine(clearCoroutine);                     //던전 클리어 확인 코루틴 실행

        MonsterImage(currentMapName);
        NpcImage(currentMapName);                           //변경된 맵에 npc있는지 확인 미니맵 업데이트

        if(currentMapName.Contains("Boss"))                 //보스 맵에서는 미니맵 안 보이게
            miniMap.gameObject.SetActive(false);
        else
            miniMap.gameObject.SetActive(true);
    }

    //현재 맵 이름 받아와서 npc 몬스터 받아오기
    public void MonsterImage(string mapName)
    {
        monsterPos.Clear();

        for (int i = 0; i < 8; i++)
        {
            if (mapName.Contains("Dungeon"))
            {
                GameObject monster = GameObject.Find(currentMapName).gameObject.transform.Find("CharacterObject").
                gameObject.transform.GetChild(i).gameObject;

                monster.SetActive(true);        
                monster.GetComponent<LivingEntity>().Init();            //몬스터 체력 초기화

                monsterPos.Add(monster.GetComponent<Transform>());

                img_monster[i].gameObject.SetActive(true);              //미니맵 몬스터 표시 활성화
            }
            else
            {
                img_monster[i].gameObject.SetActive(false);
            }
        }
        if (mapName.Contains("Dungeon"))
        {
            StartCoroutine(miniMapMonsterCoroutine);
        }                           //던전이면 몬스터 미니맵 코루틴 활성화
        else
        {
            StopCoroutine(miniMapMonsterCoroutine);
        }
    }

    //moster 위치 미니맵에 업데이트
    IEnumerator MiniMapMonsterCoroutine()
    {
        Vector2 mapArea = new Vector2(Vector3.Distance(left.position, right.position), Vector3.Distance(bottom.position, top.position));    //맵 가로 세로 사이즈
        Vector2 charPos = Vector2.zero;
        while (true)
        {
            for (int i = 0; i < monsterPos.Count; i++)
            {
                charPos = new Vector2(Vector3.Distance(new Vector3(left.position.x, 0f, 0f), new Vector3(monsterPos[i].position.x, 0f, 0f)),
                   Vector3.Distance((new Vector3(0f, bottom.position.y, 0f)), new Vector3(0f, monsterPos[i].position.y, 0f)));               //맵과 몬스터 위치 사이 거리

                Vector2 normalPos = new Vector2(charPos.x / mapArea.x, charPos.y / mapArea.y);

                img_monster[i].rectTransform.anchoredPosition =
                    new Vector2(miniMap.rectTransform.sizeDelta.x * normalPos.x, miniMap.rectTransform.sizeDelta.y * normalPos.y);

                if(monsterPos[i].gameObject.activeSelf == false)            //몬스터 죽었으면 이미지도 비활성화
                {
                    img_monster[i].gameObject.SetActive(false);
                }
            }
            yield return null;
        }
    }

    public void NpcImage(string mapName)
    {
        for (int i = 0; i < 4; i++)
        {
            if (mapName == "Town")
            {
                npcPos.Add(GameObject.Find(currentMapName).gameObject.transform.Find("CharacterObject").
              gameObject.transform.GetChild(i).GetComponent<Transform>());
                img_npc[i].gameObject.SetActive(true);
            }
            else
            {
                npcPos.Clear();
                img_npc[i].gameObject.SetActive(false);
            }
        }
        if (mapName == "Town")
        {
            StartCoroutine(miniMapNpcCoroutine);
        }
        else
        {
            StopCoroutine(miniMapNpcCoroutine);
        }
    }

    //npc 위치 미니맵에 업데이트
    IEnumerator MiniMapNpcCoroutine()
    {
        Vector2 mapArea = new Vector2(Vector3.Distance(left.position, right.position), Vector3.Distance(bottom.position, top.position));    //맵 가로 세로 사이즈
        Vector2 charPos = Vector2.zero;
        while (true)
        {
            for (int i = 0; i < npcPos.Count; i++)
            {
                charPos = new Vector2(Vector3.Distance(new Vector3(left.position.x, 0f, 0f), new Vector3(npcPos[i].position.x, 0f, 0f)),
                   Vector3.Distance((new Vector3(0f, bottom.position.y, 0f)), new Vector3(0f, npcPos[i].position.y, 0f)));                  //맵과 npc 위치 사이 거리

                Vector2 normalPos = new Vector2(charPos.x / mapArea.x, charPos.y / mapArea.y);

                img_npc[i].rectTransform.anchoredPosition =
                    new Vector2(miniMap.rectTransform.sizeDelta.x * normalPos.x, miniMap.rectTransform.sizeDelta.y * normalPos.y);
            }
            yield return null;
        }
    }
}
