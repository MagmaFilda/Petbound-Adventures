using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetInInventory : MonoBehaviour
{
    public TextMeshProUGUI petNameTxt;
    public TextMeshProUGUI damageTxt;
    public Image petImgPanel;
    public Image rarityImg;
    public Transform UIEquipPetSlotPrefab;

    private PlayerStats playerStats = PlayerStats.Instance;
    private MainUI mainUI;

    private Transform UIEquipPanel;

    public PetTemplate petTemplate { get; private set; }
    public string petName { get; private set; }
    public Rarity rarity { get; private set; }
    public int damage { get; private set; }

    private void Start()
    {
        UIEquipPanel = GameObject.FindGameObjectWithTag("EquipPanel").transform;
    }

    public void NewPet(PetTemplate template)
    {
        mainUI = FindFirstObjectByType<MainUI>();

        petTemplate = template;

        petName = template.petName;
        rarity = template.rarity;
        damage = Random.Range(template.minDamage, template.maxDamage+1);

        playerStats.PetsInInventory.Add(this);

        SetSlotUI();

        SortInventory();
    }
    public void UnEquipPet(PetTemplate template, int dmg)
    {
        mainUI = FindFirstObjectByType<MainUI>();

        petTemplate = template;

        petName = template.petName;
        rarity = template.rarity;
        damage = dmg;

        SetSlotUI();

        playerStats.PetsInInventory.Add(this);

        SortInventory();
    }

    public void Equip()
    {
        if (playerStats.deleteMode)
        {
            DeletePet();
            return;
        }

        if (playerStats.EquippedPets.Count < playerStats.maxEquippedPets)
        {
            if (!UIEquipPanel)
            {
                UIEquipPanel = GameObject.Find("MainCanvas").transform.Find("Inventory").Find("Equip");
            }

            Transform newPetModel = Instantiate(petTemplate.petPrefab, playerStats.transform.position, playerStats.transform.rotation);
            newPetModel.GetComponent<Pet>().SetDamage(damage);
            playerStats.EquippedPets.Add(newPetModel);
           
            EquipUISlot newUIPetSlot = Instantiate(UIEquipPetSlotPrefab, UIEquipPanel).GetComponent<EquipUISlot>();
            newUIPetSlot.SetValues(petName, damage.ToString(), petTemplate, transform.parent, newPetModel);

            playerStats.PetsInInventory.Remove(this);
            SortInventory();

            Destroy(gameObject);
        }
    }

    public void SetLayoutOrder(int orderNum)
    {
        transform.SetSiblingIndex(orderNum);
    }

    private void DeletePet()
    {
        if (playerStats.PetsInInventory.Count + playerStats.EquippedPets.Count > 1)
        {
            playerStats.PetsInInventory.Remove(this);
            SortInventory();
            Destroy(gameObject);
        }      
    }

    private void SortInventory()
    {
        mainUI.SortInventoryByDamage();

        mainUI.transform.Find("Inventory").Find("MaxPets").GetComponent<TextMeshProUGUI>().text = (playerStats.PetsInInventory.Count+playerStats.EquippedPets.Count) + "/" + playerStats.maxPets;
    }

    private void SetSlotUI()
    {
        petNameTxt.text = petName;
        damageTxt.text = damage.ToString();

        string path = "PetIcons/" + petName;
        mainUI.SetImage(petImgPanel, path);

        rarityImg.color = mainUI.SetRarityColor(rarity);
    }
}
