using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatManager : MonoBehaviour
{
    public static StatManager instance;

    private string                  clickSound = "ClickSound";
    private string                  unequipSound = "UnequipSound";
    private string                  cautionSound = "CautionSound";

    public GameObject               cautionUI = null;               //경고 창
    public GameObject               selectedSlotFlicker = null;     //선택한 아이템 표시 이미지
    public Text                     atkText;
    public Text                     defText;
    public Text                     currentHpText;
    public Text                     maxHpText;
    public StatSlot[]               equipSlot;                      //슬롯
    public Button                   unequipButton;
    public Image                    equipPortionButton;             //물약 단축키 이미지
    public Text                     equipPortionText;

    public Item                     selectedItem = null;            //현재 선택된 아이템
    
    //필요한 컴포넌트
    private GameObject              player;
    private LivingEntity            livingEntity;
    private InventoryManager        inventoryManager;
    private AudioManager            audioManager;

    private const int HELMET = 0, ARMOR = 1, GLOVE = 2, BOOTS = 3, WEAPON = 4, PORTION = 5;     //아이템 종류
    public int added_atk, added_def;                                                            //스탯에 얼마나 추가되었는지 저장

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

    void Start()
    {
        player = FindObjectOfType<HeroKnight>().gameObject;
        livingEntity = player.GetComponent<LivingEntity>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        audioManager = FindObjectOfType<AudioManager>();

        StatSlotSelect(null);
    }

    //아이템 장착
    public void EquipItem(Item _item, int _count = 1)
    {
        string temp = _item.itemID.ToString();
        temp = temp.Substring(0, 3);    //앞에서 3자리까지 자름
        switch (temp)
        {
            case "200":         //투구
                EquipItemCheck(HELMET, _item);
                break;
            case "201":         //옷
                EquipItemCheck(ARMOR, _item);
                break;
            case "202":         //장갑
                EquipItemCheck(GLOVE, _item);
                break;
            case "203":         //신발
                EquipItemCheck(BOOTS, _item);
                break;
            case "204":         //무기
                EquipItemCheck(WEAPON, _item);
                break;
            case "100":         //물약
                //키패드 옆 물약 슬롯으로 이동
                EquipItemCheck(PORTION, _item, _count);
                break;
        }
    }
    
    //장착하고 있는 아이템 체크
    public void EquipItemCheck(int _count, Item _item, int itemCount = 1)
    {
        if (equipSlot[_count].slotItem.itemID == 0)                                         //장착한 아이템 없으면
        {
            equipSlot[_count].slotItem = _item;                                             //슬롯에 아이템 정보 저장
            equipSlot[_count].slotCount = itemCount;
        }
        else
        {
            if (_item.itemType == Item.ItemType.Use && _item == equipSlot[_count].slotItem) //물약 & 같은 아이템
            {
                equipSlot[_count].slotCount += itemCount;                                   //개수 추가
            }
            else                                                                            //장비 or 다른 물약
            {
                if (inventoryManager.CheckInventoryEmptySlot(equipSlot[_count].slotItem.itemID))  //인벤토리 빈 슬롯 있으면
                {
                    inventoryManager.EquipItemToInven(equipSlot[_count].slotItem, equipSlot[_count].slotCount);  //인벤토리로 아이템 넣기
                    TakeOffEffect(equipSlot[_count].slotItem);                              //해제시 스탯 정보 업데이트
                    equipSlot[_count].slotItem = _item;
                    equipSlot[_count].slotCount = itemCount;
                }
                else                                                                        //인벤토리 빈 슬롯 없으면
                {
                    cautionUI.SetActive(true);                                              //아이템 장착 실패
                    return;
                }
            }
        }

        if (_item.itemType == Item.ItemType.Use)                                            //물약 단축키 등록
        {
            equipPortionButton.sprite = equipSlot[_count].slotItem.itemIcon;
            equipPortionText.text = equipSlot[_count].slotCount.ToString();
        }

        TakeOnEffect(_item);                                                                //장착시 스탯 정보 업데이트
        EquipItemImage();                                                                   //아이템 이미지 활성화
    }

    //장착한 아이템 아이콘 업데이트
    public void EquipItemImage()
    {
        for (int i = 0; i < equipSlot.Length; i++)
        {
            equipSlot[i].EquipItemImageUpdeate();
        }
    }

    //스탯 text 업데이트
    public void UpdateStatText()
    {
        atkText.text = livingEntity.atkDamage.ToString() + " ( + " + added_atk + " )";
        defText.text = livingEntity.defense.ToString() + " ( + " + added_def + " )";
        currentHpText.text = livingEntity.currentHp.ToString();
        maxHpText.text = " / " + livingEntity.maxHp.ToString();
    }
    
    //아이템 장착하면 스탯 변경
    private void TakeOnEffect(Item _item)
    {
        livingEntity.atkDamage += _item.atk;
        livingEntity.defense += _item.def;

        added_atk += _item.atk;
        added_def += _item.def;

        UpdateStatText();
    }
    
    //아이템 장착 해제하면 스탯 변경
    private void TakeOffEffect(Item _item)
    {
        livingEntity.atkDamage -= _item.atk;
        livingEntity.defense -= _item.def;

        added_atk -= _item.atk;
        added_def -= _item.def;

        UpdateStatText();
    }
    
    //스탯 슬롯 클릭하면 선택 아이템 설정
    public void StatSlotSelect(Transform pos = null, Item item = null)
    {
        selectedItem = item;

        if (selectedItem != null)
        {
            unequipButton.enabled = true;

            selectedSlotFlicker.SetActive(true);                                //선택한 아이템 표시 이미지
            selectedSlotFlicker.transform.position = pos.position;              //선택한 아이템 위치로 
        }
        else
        {
            unequipButton.enabled = false;
            selectedSlotFlicker.SetActive(false);
        }
    }
    
    //장착 해제 버튼
    public void UnequipItemButton()
    {
        audioManager.Play(clickSound);                                          //클릭 사운드
        if (!inventoryManager.CheckInventoryEmptySlot(selectedItem.itemID))     //빈 슬롯 없으면 경고창
        {
            cautionUI.SetActive(true);
            audioManager.Play(cautionSound);                                    //경고 사운드
            return;
        }

        if (selectedItem != null)
        {
            if (selectedItem.itemType == Item.ItemType.Use)
            {
                inventoryManager.EquipItemToInven(selectedItem, equipSlot[PORTION].slotCount);  //해제한 아이템 인벤토리로
            }
            else
            {
                inventoryManager.EquipItemToInven(selectedItem);                                //해제한 아이템 인벤토리로
            }
            TakeOffEffect(selectedItem);                                        //스탯 변경
            audioManager.Play(unequipSound);                                    //장착 해제 사운드

            for (int i = 0; i < equipSlot.Length; i++)
            {
                if (equipSlot[i].slotItem == selectedItem)
                {
                    if (selectedItem.itemType == Item.ItemType.Use)
                    {
                        equipSlot[i].slotItem = new Item(0, "", "", Item.ItemType.Use);
                        equipSlot[i].slotCount = 0;
                        equipPortionButton.sprite = null;           //물약 단축키 이미지 초기화
                        equipPortionText.text = "";
                    }
                    else if (selectedItem.itemType == Item.ItemType.Equip)
                    {
                        equipSlot[i].slotItem = new Item(0, "", "", Item.ItemType.Equip);
                    }
                }
            }
            StatSlotSelect(null);                                                //선택된 아이템 초기화
            EquipItemImage();
        }
    }
    
    //물약 단축키 버튼
    public void UsePortion()
    {
        if (equipSlot[PORTION].slotCount > 0)
        {
            livingEntity.RecoverHp(equipSlot[PORTION].slotItem.recover_hp);     //체력 회복
            equipSlot[PORTION].slotCount--;                                     //카운트 --
            equipPortionText.text = equipSlot[PORTION].slotCount.ToString();

            if (equipSlot[PORTION].slotCount <= 0)                              //물약 다 쓰면
            {
                equipSlot[PORTION].slotItem = new Item(0, "", "", Item.ItemType.Use);
                equipPortionButton.sprite = null;
                equipPortionText.text = "";
            }
        }
        EquipItemImage();
    }

    //닫기 버튼
    public void CloseButton()
    {
        cautionUI.SetActive(false);
        audioManager.Play(clickSound);                                          //클릭 사운드
    }
}
