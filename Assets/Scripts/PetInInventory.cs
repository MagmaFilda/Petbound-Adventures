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

    private PetTemplate petTemplate;
    public string petName { get; private set; }
    public Rarity rarity { get; private set; }
    public int damage { get; private set; }

    public void NewPet(PetTemplate template)
    {
        petTemplate = template;

        petName = template.petName;
        rarity = template.rarity;
        damage = Random.Range(template.minDamage, template.maxDamage);

        playerStats.PetsInInventory.Add(this);

        SetSlotUI();

        SortInventory();
    }
    public void UnEquipPet(PetTemplate template, int dmg)
    {
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
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            Transform newPetModel = Instantiate(petTemplate.petPrefab, player.position, player.rotation);
            newPetModel.GetComponent<Pet>().SetDamage(damage);
            playerStats.EquippedPets.Add(newPetModel);

            Transform UIEquipPanel = GameObject.FindGameObjectWithTag("EquipPanel").transform;
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
        playerStats.PetsInInventory.Remove(this);
        SortInventory();
        Destroy(gameObject);
    }

    private void SortInventory()
    {
        Transform mainUI = transform.parent.parent.parent.parent.parent;
        mainUI.GetComponent<MainUI>().SortInventoryByDamage();

        mainUI.Find("Inventory").Find("MaxPets").GetComponent<TextMeshProUGUI>().text = (playerStats.PetsInInventory.Count+playerStats.EquippedPets.Count) + "/" + playerStats.maxPets;
    }

    private void SetSlotUI()
    {
        petNameTxt.text = petName;
        damageTxt.text = damage.ToString();

        mainUI = FindFirstObjectByType<MainUI>();
        string path = "PetIcons/" + petName;
        mainUI.SetImage(petImgPanel, path);

        rarityImg.color = mainUI.SetRarityColor(rarity);
    }
}
