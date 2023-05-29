using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public bool                isSelling = false;
    private string              clickSound = "ClickSound";
    private string              cautionSound = "CautionSound";

    public GameObject           selectedSloFlicker = null;          //선택한 슬롯 표시
    public GameObject           openShop = null;                    //상점 오픈 
    public GameObject           questionUI = null;                  //확인 창
    public GameObject           cautionUI = null;                   //경고 창
    public GameObject           slotPrefab = null;                  //상점 슬롯 프리팹
    public Transform            parent;                             //슬롯 생성할 부모

    public Button               shopButton;                         //상점 탭 버튼
    public Button               invenButton;                        //인벤토리 탭 버튼
    public Button               buyButton;                          //구입 버튼
    public Button               sellButton;                         //판매 버튼
    public Text                 descriptionText = null;             //아이템 설명창
    public Text                 coin = null;                        //소지한 코인
    public Text                 questionText = null;                //확인 창 텍스트
    public Text                 cautionText = null;                 //경고 창 텍스트
    public Item                selectedItem;                        //선택한 아이템

    public List<ShopSlot>       slots = new List<ShopSlot>();

    //필요한 컴포넌트
    private DatabaseManager     databaseManager = null;
    private InventoryManager    inventoryManager = null;
    private AudioManager        audioManager = null;
    private ShopTrigger         shopTrigger = null;

    void Start()
    {
        databaseManager = FindObjectOfType<DatabaseManager>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        audioManager = FindObjectOfType<AudioManager>();
        shopTrigger = FindObjectOfType<ShopTrigger>();
        Init();
    }

    //슬롯 생성
    private void Init()
    {
        for (int i = 0; i < databaseManager.itemList.Count; i++)
        {
            GameObject clone = Instantiate(slotPrefab, parent);
            ShopSlot slot = clone.GetComponent<ShopSlot>();
            slots.Add(slot);
        }
        coin.text = inventoryManager.coin.ToString();               //코인 텍스트 업데이트   
        ChangeShopOrInvenTab("Shop");
    }

    //상점 가방 탭
    private void ChangeShopOrInvenTab(string type)
    {
        if (type == "Shop")          
            isSelling = false;                                      //상점 탭을 누르면 아이템 판매 x / 구입 o
        else
            isSelling = true;

        ChangeTab("Equip");
    }

    //장비 소비 탭
    private void ChangeTab(string type)
    {
        if (!isSelling)          //상점 탭일 때
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (databaseManager.itemList[i].itemType.ToString() == type)
                {
                    slots[i].item = databaseManager.itemList[i];
                    slots[i].gameObject.SetActive(true);
                }
                else
                {
                    slots[i].gameObject.SetActive(false);
                }
                slots[i].Init();
                slots[i].ChangeCountText(isSelling);
            }
        }
        else                    //인벤토리 탭일 때
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (type == Item.ItemType.Equip.ToString())
                {
                    if (inventoryManager.equipSlotList[i].itemCount > 0)
                    {
                        slots[i].item = inventoryManager.equipSlotList[i].slotItem;
                        slots[i].gameObject.SetActive(true);
                    }
                    else
                        slots[i].gameObject.SetActive(false);
                }
                else if (type == Item.ItemType.Use.ToString())
                {
                    if (inventoryManager.useSlotList[i].itemCount > 0)
                    {
                        slots[i].item = inventoryManager.useSlotList[i].slotItem;
                        slots[i].slotCount = inventoryManager.useSlotList[i].itemCount;
                        slots[i].gameObject.SetActive(true);
                    }
                    else
                        slots[i].gameObject.SetActive(false);
                }
                slots[i].Init();
                slots[i].ChangeCountText(isSelling);                         //인벤토리 아이템 개수 업데이트
            }

        }
        ChangeDesText(null);                                                //설명창 초기화
    }

    //슬롯 버튼 누르면 아이템 설명 텍스트 업데이트
    public void ChangeDesText(Transform pos = null, Item item = null)
    {
        if(item == null)
        {
            descriptionText.text = "";
            selectedSloFlicker.SetActive(false);
            selectedItem = null;
            return;
        }
        if(item.itemType == Item.ItemType.Equip)
        {
            descriptionText.text = item.itemName + "\n" + item.itemDescription + "\n" +
            "공격력 : " + item.atk + "   " + "방어력 : " + item.def + "\n" +
            "가격 : " + item.price;
        }
        else
        {
            descriptionText.text = item.itemName + "\n" + item.itemDescription + "\n" + "가격 : " + item.price;
        }
        selectedItem = item;
        selectedSloFlicker.SetActive(true);
        selectedSloFlicker.transform.position = pos.position;               //선택한 아이템 슬롯 위치로

        if (inventoryManager.coin < selectedItem.price)
        {
            buyButton.enabled = false;
        }
        else
        {
            buyButton.enabled = true;
        }
    }

    //===========Button====================
    //상점 가방 탭 버튼
    public void ClickShopNInvenButton(string type)
    {
        ChangeShopOrInvenTab(type);
    }
    //탭 버튼 
    public void ClickTabButton(string type)
    {
        audioManager.Play(clickSound);                                  //클릭 사운드
        ChangeTab(type);
    }
    //구매버튼
    public void ClickBuyButton()
    {
        if (selectedItem == null || isSelling) return;
        questionUI.SetActive(true);
        questionText.text = "아이템을 구입 하시겠습니까?";
        audioManager.Play(clickSound);                                  //클릭 사운드
    }
    //판매버튼
    public void ClickSellButton()
    {
        if (selectedItem == null || !isSelling) return;
        questionUI.SetActive(true);
        questionText.text = "아이템을 판매 하시겠습니까?";
        audioManager.Play(clickSound);                                  //클릭 사운드
    }
    //확인버튼
    public void OkButton()
    {
        audioManager.Play(clickSound);                                              //클릭 사운드

        if (isSelling)              //선택한 아이템 판매 - 인벤토리
        {
            if(selectedItem.itemType == Item.ItemType.Equip)                        //장비 아이템
            {
                for (int i = 0; i < inventoryManager.equipSlotList.Count; i++)
                {
                    if (inventoryManager.equipSlotList[i].slotItem == selectedItem)
                    {
                        inventoryManager.coin += selectedItem.price;                //코인 +
                        coin.text = inventoryManager.coin.ToString();
                        selectedItem = null;
                        inventoryManager.equipSlotList[i].RemoveItem();
                        slots[i].gameObject.SetActive(false);
                        break;
                    }
                }
            }
            else if(selectedItem.itemType == Item.ItemType.Use)                     //소비 아이템
            {
                for (int i = 0; i < inventoryManager.useSlotList.Count; i++)
                {
                    if (inventoryManager.useSlotList[i].slotItem == selectedItem)
                    {
                        inventoryManager.coin += selectedItem.price;                //코인 +
                        coin.text = inventoryManager.coin.ToString();
                        if (slots[i].item == selectedItem)
                        {
                            slots[i].slotCount--;
                            inventoryManager.useSlotList[i].itemCount--;
                            if (slots[i].slotCount <= 0)
                            {
                                selectedItem = null;                                //선택한 아이템 초기화
                                inventoryManager.useSlotList[i].RemoveItem();       //인벤토리에서 아이템 삭제
                                slots[i].gameObject.SetActive(false);               //아이템이 0개면 슬롯 비활성화
                            }
                            else
                                slots[i].ChangeCountText(isSelling);                //아이템 개수가 1개이상이면 개수만
                        }
                    }
                }
            }

            ChangeDesText(null);                                                            //설명창 초기화
        }
        else                        //선택한 아이템 구매 - 상점
        {
            if (inventoryManager.coin >= selectedItem.price)
            {
                if (inventoryManager.CheckInventoryEmptySlot(selectedItem.itemID))          //인벤토리에 빈 슬롯 확인
                {
                    inventoryManager.GetItem(selectedItem.itemID);                          //인벤토리에 추가
                    inventoryManager.coin -= selectedItem.price;                            //코인 -
                    coin.text = inventoryManager.coin.ToString();                           //코인 텍스트 업데이트
                }
                else
                {
                    cautionUI.SetActive(true);                                              //경고창
                    cautionText.text = "가방이 꽉 찼습니다";                                //경고창 텍스트
                }
            }
            else
            {
                cautionUI.SetActive(true);
                cautionText.text = "코인이 부족합니다";
                audioManager.Play(cautionSound);                                            //경고 사운드
            }
        }
        questionUI.SetActive(false);
    }
    //확인 창 & 경고 창 닫기
    public void CancelButton()
    {
        audioManager.Play(clickSound);                                  //클릭 사운드
        questionUI.SetActive(false);
        cautionUI.SetActive(false);
    }
    public void ClickOpenButton()
    {
        openShop.SetActive(true);
        coin.text = inventoryManager.coin.ToString();                   //소지한 코인 업데이트
        ChangeShopOrInvenTab("Shop");                                   //상점 열면 상점 탭부터 시작
        audioManager.Play(clickSound);                                  //클릭 사운드
        shopTrigger.ShopTriggerOff();                                   //상점 클릭 컴포넌트
        GameManager.instance.HideUI();
    }
    public void ClickCloseButton()
    {
        openShop.SetActive(false);
        audioManager.Play(clickSound);                                  //클릭 사운드
        shopTrigger.ShopTriggerOn();                                    //상점 클릭 컴포넌트
        GameManager.instance.UnHideUI();
    }
}
