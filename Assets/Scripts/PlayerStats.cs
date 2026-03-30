using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    [Header("Save/Load")]
    public Transform petInventory;
    public Transform petUISlotTemplate;
    public Shopkeeper[] shops;
    public QuestNPC[] questNpcs;

    [HideInInspector]
    public int coins;

    [HideInInspector]
    public float playerSpeed;
    [HideInInspector]
    public float playerJumpPower;
    [HideInInspector]
    public int maxPets;
    [HideInInspector]
    public int maxEquippedPets;
    [HideInInspector]
    public int resourceCapacity;
    [HideInInspector]
    public int storageCapacity;

    [Header("Resources")]
    public Dictionary<Resource, int> PlayerResources;
    public Dictionary<Resource, int> StorageResources;

    [Header("Pets")]
    public List<Transform> EquippedPets;
    public List<PetInInventory> PetsInInventory;

    [Header("Quests")]
    public List<ActiveQuest> ActiveQuests;
    [Header("Items")]
    public List<ItemTemplate> OwnedItems;
    public List<ItemTemplate> EquippedItems;
    public Dictionary<ItemTemplate, int> BoughtUpgrades;

    [Header("Stats")]
    public int totalOpenEggs;
    public int totalBreakables;

    [HideInInspector]
    public bool canMove = false;
    [HideInInspector]
    public bool canRotateCamera = false;
    [HideInInspector]
    public bool canShowInteract = true;
    [HideInInspector]
    public bool deleteMode = false;

    private PlayerInput playerInput;
    private InputAction clickAction;

    private float clicked;
    private bool canClick = true;
    private int coinsFrameBefore;

    private MainUI mainUI;
    private Transform targetBreakable;

    private string path;
    private PetTemplate[] petTemplates;
    private ItemTemplate[] itemTemplates;

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;

        if (Instance != null && Instance != this)
        {
            Debug.Log("Duplicate PlayerStats destroyed");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        coins = 0;
        coinsFrameBefore = coins;

        path = Application.persistentDataPath + "/data.json";
    }
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions.FindAction("Click");

        PlayerResources = new Dictionary<Resource, int>();
        StorageResources = new Dictionary<Resource, int>();

        EquippedPets = new List<Transform>();
        PetsInInventory = new List<PetInInventory>();

        ActiveQuests = new List<ActiveQuest>();
        OwnedItems = new List<ItemTemplate>();
        EquippedItems = new List<ItemTemplate>();
        BoughtUpgrades = new Dictionary<ItemTemplate, int>();

        foreach (Resource res in Enum.GetValues(typeof(Resource)))
        {
            PlayerResources.Add(res, 0);
            StorageResources.Add(res, 0);
        }
       
        mainUI = FindFirstObjectByType<MainUI>();

        petTemplates = Resources.LoadAll<PetTemplate>("Templates/PetTemplates/DirtCave");
        itemTemplates = Resources.LoadAll<ItemTemplate>("Templates/ItemTemplates/DirtCave");

        LoadData();
    }

    private void Update()
    {
        CheckingClick();
        ChangeCoinsDetection();
    }

    public void SaveData()
    {
        SaveData savingData = new SaveData();

        savingData.coins = coins;
        savingData.playerSpeed = playerSpeed;
        savingData.playerJumpPower = playerJumpPower;

        savingData.maxPets = maxPets;
        savingData.maxEquippedPets = maxEquippedPets;
        savingData.resourceCapacity = resourceCapacity;
        savingData.storageCapacity = storageCapacity;

        foreach (var res in PlayerResources)
        {
            if (res.Value > 0)
            {
                savingData.playerResNames.Add(res.Key.ToString());
                savingData.playerResValues.Add(res.Value);
            }
        }
        foreach (var res in StorageResources)
        {
            if (res.Value > 0)
            {
                savingData.storageResNames.Add(res.Key.ToString());
                savingData.storageResValues.Add(res.Value);
            }
        }

        foreach (var pet in EquippedPets)
        {
            savingData.equippedPetsName.Add(pet.GetComponent<Pet>().template.id);
            savingData.equippedPetsDmg.Add(pet.GetComponent<Pet>().damage);
        }
        foreach (var pet in PetsInInventory)
        {
            savingData.petsInInventoryName.Add(pet.petTemplate.id);
            savingData.petsInInventoryDmg.Add(pet.damage);
        }

        foreach (var npc in questNpcs)
        {
            savingData.npcName.Add(npc.transform.name);
            savingData.npcActivated.Add(npc.activatedQuest);
            savingData.npcQuestNum.Add(npc.actualQuestNum);
        }
        foreach (var quest in ActiveQuests)
        {
            savingData.quests.Add(quest.template.id);
            savingData.questProgress.Add(quest.progress);
        }

        foreach (var item in OwnedItems)
        {
            savingData.ownedItems.Add(item.id);
        }
        foreach (var item in EquippedItems)
        {
            savingData.equippedItems.Add(item.id);
        }
        foreach(var upgrade in BoughtUpgrades)
        {
            savingData.boughtUpgradesNames.Add(upgrade.Key.id);
            savingData.boughtUpgradesValues.Add(upgrade.Value);
        }

        savingData.totalOpenEggs = totalOpenEggs;
        savingData.totalBreakables = totalBreakables;

        Debug.Log("SAVING DATA");
        string json = JsonUtility.ToJson(savingData, true);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        string encoded = System.Convert.ToBase64String(bytes);
        File.WriteAllText(path, encoded);
    }
    private void LoadData()
    {
        if (File.Exists(path))
        {
            Debug.Log("LoadingData");

            string encoded = File.ReadAllText(path);
            byte[] bytes = System.Convert.FromBase64String(encoded);
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);

            coins = loadedData.coins;
            playerSpeed = loadedData.playerSpeed;
            playerJumpPower = loadedData.playerJumpPower;

            maxPets = loadedData.maxPets;
            maxEquippedPets = loadedData.maxEquippedPets;
            resourceCapacity = loadedData.resourceCapacity;
            storageCapacity = loadedData.storageCapacity;

            for (int i = 0; i < loadedData.playerResNames.Count; i++)
            {
                Resource findingRes = mainUI.GetResourceFromString(loadedData.playerResNames[i], PlayerResources);
                PlayerResources[findingRes] = loadedData.playerResValues[i];
            }
            for (int i = 0; i < loadedData.storageResNames.Count; i++)
            {
                Resource findingRes = mainUI.GetResourceFromString(loadedData.storageResNames[i], StorageResources);
                StorageResources[findingRes] = loadedData.storageResValues[i];
            }

            for (int i = 0; i < loadedData.petsInInventoryName.Count; i++)
            {
                Transform newPetUI = Instantiate(petUISlotTemplate, petInventory);
                PetInInventory newPetStats = newPetUI.GetComponent<PetInInventory>();
                foreach (PetTemplate petTemp in petTemplates)
                {
                    if (petTemp.id == loadedData.petsInInventoryName[i])
                    {
                        newPetStats.UnEquipPet(petTemp, loadedData.petsInInventoryDmg[i]);
                        break;
                    }
                }
            }
            for (int i = 0; i < loadedData.equippedPetsName.Count; i++)
            {
                Transform newPetUI = Instantiate(petUISlotTemplate, petInventory);
                PetInInventory loadedPet = newPetUI.GetComponent<PetInInventory>();
                foreach (PetTemplate petTemp in petTemplates)
                {
                    if (petTemp.id == loadedData.equippedPetsName[i])
                    {
                        loadedPet.UnEquipPet(petTemp, loadedData.equippedPetsDmg[i]);
                        loadedPet.Equip();
                        break;
                    }
                }
            }

            StartCoroutine(LoadQuests(loadedData));

            foreach (var itemID in loadedData.ownedItems)
            {
                foreach (ItemTemplate itemTemp in itemTemplates)
                {
                    if (itemTemp.id == itemID)
                    {
                        OwnedItems.Add(itemTemp);
                        if (itemTemp.typeOfItem == ItemType.Key) { GameObject.Find("VillageGate").GetComponent<BoxCollider>().enabled = false; }
                        break;
                    }
                }
            }
            foreach (var itemID in loadedData.equippedItems)
            {
                foreach (ItemTemplate itemTemp in itemTemplates)
                {
                    if (itemTemp.id == itemID)
                    {
                        EquippedItems.Add(itemTemp);
                        bool foundItem = false;
                        foreach (var shop in shops)
                        {
                            foreach (ItemTemplate item in shop.template.purchasableItems)
                            {
                                if (item == itemTemp)
                                {
                                    shop.GetComponent<Shopkeeper>().SetItemAfterLoad(itemTemp);
                                    foundItem = true;
                                    break;
                                }
                            }
                            if (foundItem)
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            for (int i = 0; i < loadedData.boughtUpgradesNames.Count; i++)
            {
                foreach (ItemTemplate itemTemp in itemTemplates)
                {
                    if (itemTemp.id == loadedData.boughtUpgradesNames[i])
                    {
                        BoughtUpgrades[itemTemp] = loadedData.boughtUpgradesValues[i];
                        break;
                    }
                }
            }
        }
    }
    private void ChangeCoinsDetection()
    {
        if (coins != coinsFrameBefore)
        {
            mainUI.UpdateCoins();
        }
        coinsFrameBefore = coins;
    }
    private void CheckingClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Breakable"))
            {
                if (targetBreakable != hit.transform)
                {
                    targetBreakable = hit.transform;
                }
            }
            else
            {
                if (targetBreakable != null)
                {
                    targetBreakable = null;
                }
            }
        }

        if (targetBreakable != null)
        {
            clicked = clickAction.ReadValue<float>();
            if (clicked > 0 && canClick)
            {
                foreach (Transform p in EquippedPets)
                {
                    Pet pet = p.GetComponent<Pet>();
                    if (pet.mode == "Follow")
                    {
                        pet.ChangeMode("Attack", targetBreakable);
                        break;
                    }
                }

                canClick = false;
                StartCoroutine(ResetClick());
            }
        }
    }

    private IEnumerator LoadQuests(SaveData loadedData)
    {
        yield return new WaitForSeconds(5);

        for (int i = 0; i < loadedData.npcName.Count; i++)
        {
            QuestNPC npc = GameObject.Find(loadedData.npcName[i]).GetComponent<QuestNPC>();
            npc.actualQuestNum = loadedData.npcQuestNum[i];

            if (loadedData.npcActivated[i])
            {
                npc.activatedQuest = true;
                npc.LoadQuest();
            }
        }
        for (int i = 0; i < loadedData.quests.Count; i++)
        {
            foreach (var activeQuest in ActiveQuests)
            {
                if (activeQuest.template.id == loadedData.quests[i])
                {
                    activeQuest.progress = loadedData.questProgress[i];
                    break;
                }
            }
        }
    }
    private IEnumerator ResetClick()
    {
        while (clicked > 0)
        {
            yield return null;
        }        
        canClick = true;
    }
}
