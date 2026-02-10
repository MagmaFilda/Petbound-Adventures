using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Shopkeeper : MonoBehaviour
{
    public ShopTemplate template;

    public Transform mainUI;
    public Canvas openUI;
    public Transform shopkeeperUI;
    public Transform hitbox;

    public Transform cameraPoints;

    private Camera shopCamera;
    private Transform interactPanel;
    private int itemNum = 0;

    private List<Transform> attachObjects;

    private PlayerStats playerStats = PlayerStats.Instance;

    public void SetNormalCamera()
    {
        shopCamera.enabled = false;
    }

    private void Start()
    {
        shopCamera = cameraPoints.Find("ShopCamera").GetComponent<Camera>();
        interactPanel = shopkeeperUI.Find("InteractPanel");
        attachObjects = new List<Transform>();
    }
    private void Update()
    {
        MouseHover();

        if (openUI.enabled && Keyboard.current.eKey.wasPressedThisFrame)
        {
            MainUI uiScript = mainUI.GetComponent<MainUI>();
            uiScript.OpenPanel(shopkeeperUI);

            itemNum = 0;
            shopCamera.transform.position = cameraPoints.Find(itemNum.ToString()).position;
            shopCamera.transform.rotation = cameraPoints.Find(itemNum.ToString()).rotation;
            shopCamera.enabled = true;

            SetItemUI();
        }
    }

    private void SetItemUI()
    {
        ItemTemplate itemTemplate = template.purchasableItems[itemNum];

        interactPanel.Find("ItemName").GetComponent<Text>().text = itemTemplate.typeOfItem.ToString();
        interactPanel.Find("PriceText").GetComponent<Text>().text = itemTemplate.price.ToString();

        Button btn = interactPanel.Find("BuyBtn").GetComponent<Button>();
        Text btnTxt = btn.transform.Find("Text").GetComponent<Text>();
        btn.onClick.RemoveAllListeners();

        if (itemTemplate.typeOfItem == ItemType.Upgrade)
        {
            if (playerStats.BoughtUpgrades.ContainsKey(itemTemplate) && playerStats.BoughtUpgrades[itemTemplate] == itemTemplate.rebuyable)
            {
                btnTxt.text = "Maxed";
            }
            else
            {
                btnTxt.text = "BUY";
                btn.onClick.AddListener(() => BuyItem(itemTemplate));
            }
        }
        else
        {
            if (playerStats.EquippedItems.Contains(itemTemplate))
            {
                btnTxt.text = "Equipped";
                btn.onClick.AddListener(() => InteractItem(itemTemplate, false));
            }
            else if (playerStats.OwnedItems.Contains(itemTemplate))
            {
                btnTxt.text = "Owned";
                btn.onClick.AddListener(() => InteractItem(itemTemplate, true));
            }
            else
            {
                btnTxt.text = "BUY";
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
        Transform character = playerStats.transform.Find("Character");
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
                attachObjects.Add(character.Find("TorsoJoint").Find("Torso").Find("BackpackPoint"));
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
                Destroy(attachObject.GetChild(0).gameObject);
            }         
        }
        SetItemUI();
    }

    private void SetArrows(bool left, bool right)
    {
        Button leftArrow = interactPanel.Find("LeftArrow").GetComponent<Button>();
        Button rightArrow = interactPanel.Find("RightArrow").GetComponent<Button>();

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

        shopCamera.transform.position = cameraPoints.Find(itemNum.ToString()).position;
        shopCamera.transform.rotation = cameraPoints.Find(itemNum.ToString()).rotation;

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
    private void MouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject == hitbox.gameObject)
            {
                openUI.enabled = true;
            }
            else
            {
                openUI.enabled = false;
            }
        }
        else
        {
            openUI.enabled = false;
        }
    }
}