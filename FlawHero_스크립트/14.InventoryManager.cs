using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public int                          coin = 50;
    private int                         tabCount = 0;
    private string                      clickSound = "ClickSound";
    private string                      equipSound = "EquipSound";

    public GameObject                   go_inven = null;        //인벤토리 창
    public GameObject[]                 tab = null;             //탭
    public GameObject[]                 slotGrid = null;        //타입별 슬롯 그리드
    public GameObject                   questionUI = null;      //판매/장착 버튼 누르면 확인 창
    public GameObject                   selectedSlotFlicker;
    
    public Transform                    slotParent;             //슬롯 부모
    public Text                         descripitionText;       //아이템 설명
    public Text                         questionText;           //확인 창 텍스트
    public Text                         coinText;               //코인 텍스트
    public Button                       equipButton;            //장착 버튼
    public Button                       okButton;               //확인 버튼
    public Button                       cancelButton;           //취소 버튼

    public List<InventorySlot>          equipSlotList = new List<InventorySlot>();
    public List<InventorySlot>          useSlotList = new List<InventorySlot>();
    public List<InventorySlot>          etcSlotList = new List<InventorySlot>();     
    public Item                         selectedItem;           //선택한 아이템

    //필요한 컴포넌트
    private DatabaseManager             databaseManager;
    private StatManager                 statManager;
    private AudioManager                audioManager;

    void Start()
    {
        databaseManager = FindObjectOfType<DatabaseManager>();
        statManager = FindObjectOfType<StatManager>();
        audioManager = FindObjectOfType<AudioManager>();
    }
   
    //빈슬롯 있는지 체크
    public bool CheckInventoryEmptySlot(int id)
    {
        string temp = id.ToString();
        temp = temp.Substring(0, 1);                        //아이템 아이디 앞자리로 구분
        switch (temp)
        {
            case "1":
                foreach (var slot in useSlotList)
                {
                    if (slot.itemCount <= 0)                 //하나라도 빈슬롯 있음
                        return true;
                }
                break;
            case "2":
                foreach (var slot in equipSlotList)
                {
                    if (slot.itemCount <= 0)
                        return true;
                }
                break;
            case "3":
                foreach (var slot in etcSlotList)
                {
                    if (slot.itemCount <= 0)
                        return true;
                }
                break;
        }
        Debug.Log("빈슬롯 없음");
        return false;
    }

    //아이템 획득
    public void GetItem(int _itemID, int _count = 1)
    {
        for (int i = 0; i < databaseManager.itemList.Count; i++)
        {
            if (_itemID == databaseManager.itemList[i].itemID)                      //아이템 데이터와 비교 같으면
            {
                if(databaseManager.itemList[i].itemType == Item.ItemType.Equip)     //아이템 타입이 같다면
                {
                    for (int j = 0; j < equipSlotList.Count; j++)
                    {                       
                        if (equipSlotList[j].itemCount == 0)
                        {
                            equipSlotList[j].slotItem = databaseManager.itemList[i];
                            equipSlotList[j].Additem(equipSlotList[j].slotItem);
                            break;
                        }
                    }
                }
                else if(databaseManager.itemList[i].itemType == Item.ItemType.Use)
                {
                    for (int j = 0; j < useSlotList.Count; j++)
                    {
                        if (useSlotList[j].itemCount == 0)                              //아이템 없는 슬롯에 
                        {
                            useSlotList[j].slotItem = databaseManager.itemList[i];
                            useSlotList[j].Additem(useSlotList[j].slotItem);
                            break;
                        }
                        else
                        {
                            if (useSlotList[j].slotItem.itemID == _itemID)              //같은 아이템 있으면 합치기
                            {
                                useSlotList[j].itemCount += _count;
                                useSlotList[j].SlotItemCountUpdate();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < etcSlotList.Count; j++)
                    {
                        if (etcSlotList[j].itemCount == 0)
                        {
                            etcSlotList[j].slotItem = databaseManager.itemList[i];
                            etcSlotList[j].Additem(etcSlotList[j].slotItem);
                            break;
                        }
                    }
                }
            }
        }
        ShowItem(tabCount);
    }

    //아이템 활성화(inventoryTabList에 조건에 맞는 아이템들만 넣어주고 인벤토리 슬롯에 출력)
    public void ShowItem(int index)
    {
        tabCount = index;
        
        for (int i = 0; i < slotGrid.Length; i++)
        {
            slotGrid[i].SetActive(false);
        }
        slotGrid[tabCount].SetActive(true);             //선택된 아이템 타입의 슬롯만 보여줌
        if(tabCount == 1)
        {
            for (int i = 0; i < useSlotList.Count; i++)
            {
                useSlotList[i].SlotItemCountUpdate();
            }
        }

        //새 탭 누르면 선택한 아이템 리셋
        ItemSelect();
    }

    //아이템 클릭 (선택된 아이템)
    public void ItemSelect(Transform pos = null, Item item = null)
    {
        selectedItem = item;            //클릭한 아이템 선택된 아이템으로

        if (selectedItem != null)
        {
            ItemDescriptonTextUpdate(selectedItem);
            equipButton.enabled = true;

            selectedSlotFlicker.SetActive(true);                            //선택한 아이템 표시 이미지
            selectedSlotFlicker.transform.position = pos.position;          //선택한 아이템 슬롯 표시
        }
        else
        {
            descripitionText.text = "";
            equipButton.enabled = false;

            selectedSlotFlicker.SetActive(false);
        }
    }

    //아이템 설명 텍스트 업데이트
    public void ItemDescriptonTextUpdate(Item item)
    {
        if (Item.ItemType.Equip == item.itemType)
        {
            descripitionText.text = item.itemDescription + "\n" +
            "공격력 : " + item.atk + "   " + "방아력 : " + item.def + "\n" +
            "가격 : " + item.price;
        }
        else
        {
            descripitionText.text = item.itemDescription + "\n" + "가격 : " + item.price;
        }
    }

    //코인 개수 업데이트
    public void CoinUpdate()
    {
        coinText.text = coin.ToString();
    }

    //장착 해제한 아이템 인벤토리에 추가
    public void EquipItemToInven(Item item, int count = 1)
    {
        if(item.itemType == Item.ItemType.Equip)
        {
            for (int i = 0; i < equipSlotList.Count; i++)
            {
                if(equipSlotList[i].itemCount <= 0)
                {
                    equipSlotList[i].slotItem = item;
                    equipSlotList[i].Additem(item);
                    break;
                }
            }
        }
        else if (item.itemType == Item.ItemType.Use)
        {
            for (int i = 0; i < useSlotList.Count; i++)
            {
                if (useSlotList[i].itemCount <= 0)
                {
                    useSlotList[i].slotItem = item;
                    useSlotList[i].Additem(item, count);
                    break;
                }
                else
                {
                    if(useSlotList[i].slotItem == item)
                    {
                        useSlotList[i].itemCount += count;
                        useSlotList[i].SlotItemCountUpdate();
                        break;
                    }
                }
            }
        }
        ShowItem(tabCount);
    }

    // 아이템 개수 확인
    public int GetItemCount(int id)
    {
        for (int i = 0; i < databaseManager.itemList.Count; i++)
        {
            if(id == databaseManager.itemList[i].itemID)
            {
                if(databaseManager.itemList[i].itemType == Item.ItemType.ETC)
                {
                    foreach (var _item in etcSlotList)
                    {
                        if (_item.slotItem.itemID == id)
                        {
                            Debug.Log("count" + _item.itemCount);
                            return _item.itemCount;
                        }
                    }
                }
            }
        }
        Debug.Log("0");
        return 0;
    }

    //================= Button=====================
    //장착하기 버튼
    public void EquipButton()
    {
        if (selectedItem == null || selectedItem.itemType == Item.ItemType.ETC) return;
        questionUI.SetActive(true);
        questionText.text = "아이템을 장착 하시겠습니까?";
        audioManager.Play(clickSound);                                  //클릭 사운드
    }

    //확인 버튼
    public void OkButton()
    {
        if (selectedItem != null)
        {
            if (selectedItem.itemType == Item.ItemType.Equip)
            {
                for (int i = 0; i < equipSlotList.Count; i++)
                {
                    if (equipSlotList[i].slotItem == selectedItem)
                    {
                        equipSlotList[i].RemoveItem();
                        statManager.EquipItem(selectedItem);
                        audioManager.Play(equipSound);
                        break;
                    }
                }
            }
            else if (selectedItem.itemType == Item.ItemType.Use)
            {
                for (int i = 0; i < useSlotList.Count; i++)
                {
                    if (useSlotList[i].slotItem == selectedItem)
                    {
                        int count = useSlotList[i].itemCount;
                        useSlotList[i].RemoveItem();
                        statManager.EquipItem(selectedItem, count);
                        audioManager.Play(equipSound);
                        break;
                    }
                }
            }
            selectedItem = null;
        }
        ShowItem(tabCount);
        statManager.UpdateStatText();                                   //스탯 텍스트 업데이트
        questionUI.SetActive(false);
    }

    //취소 버튼
    public void CancelButton()
    {
        questionUI.SetActive(false);
        audioManager.Play(clickSound);                                  //클릭 사운드
    }
    
    //창 열기 버튼
    public void OnClickInvenOpenButton()
    {
        go_inven.SetActive(true);
        ShowItem(0);
        CoinUpdate();
        statManager.UpdateStatText();
        audioManager.Play(clickSound);                                  //클릭 사운드
        GameManager.instance.HideUI();
    }
    
    //창 닫기 버튼
    public void OnClickExitButton()
    {
        ItemSelect(null);
        statManager.selectedItem = null;
        go_inven.SetActive(false);
        audioManager.Play(clickSound);                                  //클릭 사운드
        GameManager.instance.UnHideUI();
    }
}
