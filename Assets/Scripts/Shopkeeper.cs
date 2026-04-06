using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Shopkeeper : MonoBehaviour
{
    public ShopTemplate template;

    public Transform openUI;
    public Transform shopkeeperUI;
    public TextMeshProUGUI bonusPartTemplate;

    public Transform cameraPoints;

    private Transform interactPanel;
    private int itemNum = 0;

    private List<Transform> attachObjects;

    private PlayerStats playerStats;
    private GameManager gameManager;
    private MainUI mainUI;

    private Transform character;

    private Button leftArrow;
    private Button rightArrow;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        gameManager = GameManager.Instance;
        mainUI = FindFirstObjectByType<MainUI>();

        character = playerStats.transform.Find("Character");

        interactPanel = shopkeeperUI.Find("InteractPanel");
        attachObjects = new List<Transform>();

        leftArrow = interactPanel.Find("LeftArrow").GetComponent<Button>();
        rightArrow = interactPanel.Find("RightArrow").GetComponent<Button>();
    }
    private void Update()
    {
        if (openUI.gameObject.activeSelf && Keyboard.current.eKey.wasPressedThisFrame && playerStats.canShowInteract)
        {
            
            mainUI.OpenPanel(shopkeeperUI);
            openUI.gameObject.SetActive(false);

            itemNum = 0;
            gameManager.SetCamera(cameraPoints.Find(itemNum.ToString()), true);

            SetItemUI();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(true);
            transform.Find("NpcIcon").gameObject.SetActive(false);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(false);
            transform.Find("NpcIcon").gameObject.SetActive(true);
        }
    }

    public void SetNormalCamera() //u exit buttonu v shopkeeper UI
    {
        gameManager.SetCamera(cameraPoints, false);
    }

    public void SetItemAfterLoad(ItemTemplate item)
    {
        InteractItem(item, false);
        InteractItem(item, true);
    }

    private void SetItemUI()
    {
        ItemTemplate itemTemplate = template.purchasableItems[itemNum];

        interactPanel.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemTemplate.itemName;
        interactPanel.Find("PriceText").GetComponent<TextMeshProUGUI>().text = itemTemplate.price.ToString();

        Button btn = interactPanel.Find("BuyBtn").GetComponent<Button>();
        TextMeshProUGUI btnTxt = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        btn.onClick.RemoveAllListeners();

        Transform bonusPanel = shopkeeperUI.Find("BonusStatsPanel");
        mainUI.ClearAllChilds(bonusPanel);
        for (int i = 0; i < itemTemplate.upgrades.Length; i++)
        {
            TextMeshProUGUI bonusStat = Instantiate(bonusPartTemplate, bonusPanel);
            bonusStat.text = itemTemplate.upgrades[i] + " +" + itemTemplate.upgradeValues[i];
        }

        if (itemTemplate.typeOfItem == ItemType.Upgrade)
        {
            if (playerStats.BoughtUpgrades.ContainsKey(itemTemplate) && playerStats.BoughtUpgrades[itemTemplate] == itemTemplate.rebuyable)
            {
                btnTxt.text = "Maximum";
            }
            else
            {
                btnTxt.text = "KOUPIT";
                btn.onClick.AddListener(() => BuyItem(itemTemplate));
            }
        }
        else
        {
            if (playerStats.EquippedItems.Contains(itemTemplate))
            {
                btnTxt.text = "Nasazeno";
                btn.onClick.AddListener(() => InteractItem(itemTemplate, false));
            }
            else if (playerStats.OwnedItems.Contains(itemTemplate))
            {
                btnTxt.text = "Vlastnìno";
                btn.onClick.AddListener(() => InteractItem(itemTemplate, true));
            }
            else if (itemTemplate.rebuyable == 0)
            {
                btnTxt.text = "Nelze koupit";
            }
            else
            {
                btnTxt.text = "KOUPIT";
                btn.onClick.AddListener(() => BuyItem(itemTemplate));
            }
        }
    
        bool leftArrow = false;
        bool rightArrow = false;
        if (itemNum != 0)
        {
            leftArrow = true;
        }
        if (itemNum+1 < template.purchasableItems.Length)
        {
            rightArrow = true;
        }

        SetArrows(leftArrow, rightArrow);
    }
    private void BuyItem(ItemTemplate item)
    {
        if (playerStats.coins >= item.price)
        {
            playerStats.coins -= item.price;          

            if (item.typeOfItem == ItemType.Upgrade)
            {
                if (!playerStats.BoughtUpgrades.ContainsKey(item))
                {
                    playerStats.BoughtUpgrades.Add(item, 1);
                    UpdateStat(item.upgrades[0], item.upgradeValues[0]);
                }
                else
                {
                    playerStats.BoughtUpgrades[item] += 1;
                    UpdateStat(item.upgrades[0], item.upgradeValues[0]);
                }
            }
            else
            {
                playerStats.OwnedItems.Add(item);
                InteractItem(item, true);
            }           

            SetItemUI();
        }
    }
    private void InteractItem(ItemTemplate item, bool equip)
    {
        attachObjects.Clear();
        
        switch (item.typeOfItem)
        {
            case ItemType.Boots:
                attachObjects.Add(character.Find("LeftLegJoint").Find("LeftFoot"));
                attachObjects.Add(character.Find("RightLegJoint").Find("RightFoot"));
                break;
            case ItemType.Helmet:
                attachObjects.Add(character.Find("TorsoJoint").Find("Head").Find("HelmetPoint"));
                break;
            case ItemType.Backpack:
                attachObjects.Add(character.Find("TorsoJoint").Find("BackpackPoint"));
                break;
        }

        if (equip)
        {
            if (!playerStats.EquippedItems.Contains(item))
            {
                playerStats.EquippedItems.Add(item);
                for (int i = 0; i < item.upgrades.Length; i++)
                {
                    UpdateStat(item.upgrades[i], item.upgradeValues[i]);
                }

                Transform newItemObject = Instantiate(item.item);
                foreach (Transform attachObject in attachObjects)
                {
                    Transform partOfItem = newItemObject;
                    if (attachObjects.Count > 1)
                    {
                        partOfItem = newItemObject.GetChild(0); //jak se ten item dá jinému parentu, tak tam vždy bude fungovat 0
                    }                 
                    partOfItem.SetParent(attachObject);
                    partOfItem.rotation = new Quaternion(0, 0, 0, 0);
                    partOfItem.position = partOfItem.parent.position;
                }                                          
            }
        }
        else
        {
            playerStats.EquippedItems.Remove(item);
            for (int i = 0; i < item.upgrades.Length; i++)
            {
                UpdateStat(item.upgrades[i], -item.upgradeValues[i]);
            }
            foreach (Transform attachObject in attachObjects)
            {
                if (attachObject.childCount > 0)
                {
                    Destroy(attachObject.GetChild(0).gameObject);
                }              
            }         
        }
        SetItemUI();
    }

    private void SetArrows(bool left, bool right)
    {
        leftArrow.interactable = left; 
        rightArrow.interactable = right;

        leftArrow.onClick.RemoveAllListeners();
        rightArrow.onClick.RemoveAllListeners();
        leftArrow.onClick.AddListener(() => ChangeItem(-1));
        rightArrow.onClick.AddListener(() => ChangeItem(1));
    }
    private void ChangeItem(int change)
    {
        itemNum += change;
        gameManager.SetCamera(cameraPoints.Find(itemNum.ToString()), true, 1);

        SetItemUI();
    }
    private void UpdateStat(UpgradeType upgrade, float value)
    {
        switch (upgrade)
        {
            case UpgradeType.ResourceCapacity:
                playerStats.resourceCapacity += Mathf.RoundToInt(value);
                break;
            case UpgradeType.PetStorage:
                playerStats.maxPets += Mathf.RoundToInt(value);
                break;
            case UpgradeType.MovementSpeed:
                playerStats.playerSpeed += value;
                break;
            case UpgradeType.JumpBoost:
                playerStats.playerJumpPower += value;
                break;
        }
    }
}