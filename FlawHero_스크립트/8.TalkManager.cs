using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkManager : MonoBehaviour
{
    private int                 talkIndex = 0;                      //몇번째 대화가 이어지는지 저장
    private int                 npcID = 0;                          //대화 가능한 npc id
    private string              talk;                               //출력될 대화 내용
    private string              clickSound = "ClickSound";

    private Vector2             vec = Vector2.zero;                 //대화창 위치를 담을 변수
    public Text                 talkText;                           //대화창 텍스트
    public Camera               cam = null;                         //메인 카메라
    public RectTransform        canvas = null;                      //대화창 캔버스
    public GameObject           dialogueUI = null;                  //대화창 UI
    public GameObject           dialogueBackground = null;          //대화창

    //필요한 컴포넌트
    private GameManager         gameManager = null;
    private AudioManager        audioManager = null;

    private Dictionary<int, string[]> talkData;                             //대화에 관련한 정보 저장

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
        talkData = new Dictionary<int, string[]>();
        MakeData();
    }

    //대화 내용
    void MakeData()
    {
        talkData.Add(1000, new string[] { "안녕?", "이 곳에 처음 왔구나", "성 안쪽에는 괴물이 있어", "가까이 가지 않는게 좋을거야" });
        talkData.Add(2000, new string[] { "처음 보는 얼굴인데", "소문 들었어?", "성에 보물이 있다는데.." });
        talkData.Add(3000, new string[] { "열쇠가 어딨지", "가방에 넣어뒀는데" });
        talkData.Add(4000, new string[] { "안녕하세요", "성에서 매일 이상한 소리가 들리는데", "보물을 지키는 괴물의 소리일까요?" });
    }

    //npc근처에 가면 활성화
    public void ActiveDialogueCanvas(Transform pos, int id)
    {
        npcID = id;
        dialogueUI.SetActive(true);

        StartCoroutine(CanvasPosition(pos));
    }

    //대화창 ui 위치 변경
    IEnumerator CanvasPosition(Transform pos)
    {
        while(true)
        {
            Vector2 _vec = new Vector2(pos.position.x, pos.position.y + 2.0f);

            var screenPos = RectTransformUtility.WorldToScreenPoint(cam, _vec);                         //월드 좌표로 RectTransform의 좌표를 반환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPos, cam, out vec);   //화면 공간 점을 RectTransform의 로컬 공간 위치로 변환
            dialogueUI.transform.localPosition = vec;
            yield return null;
        }
    }

    //npc trigger exit 하면 ui 비활성화
    public void UnActiveDialogueCanvas()
    {
        npcID = 0;
        StopAllCoroutines();
        dialogueUI.SetActive(false);                    //대화 캔버스 닫기
    }

    //대화창 버튼
    public void OnTalk()
    {
        audioManager.Play(clickSound);                  //클릭 사운드
        talk = GetTalk(npcID, talkIndex);

        if (talk == null)
        {
            talkIndex = 0;
            dialogueBackground.SetActive(false);        //대화 창 닫기
            gameManager.UnHideUI();                     //전체 UI 보이게
            return;
        }

        talkText.text = talk;
        talkIndex++;
    }

    //대화 내용 반환
    private string GetTalk(int id, int talkIndex)
    {
        if (talkIndex == talkData[id].Length)       //대화가 끝나면 null반환
            return null;
        else                                        //대화 내용 반환
            return talkData[id][talkIndex];
    }

    //npc 대화 가능 표시 버튼
    public void OnClickNpcButton()
    {
        dialogueBackground.SetActive(true);
        OnTalk();                                   //첫 대사 출력
        gameManager.HideUI();                       //전체 UI 숨김
    }
}
