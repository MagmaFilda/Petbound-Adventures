using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PetInInventory : MonoBehaviour
{
    public Text petNameTxt;
    public Text damageTxt;
    public Transform UIEquipPetSlotPrefab;

    private PlayerStats playerStats = PlayerStats.Instance;

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
        if (playerStats.EquippedPets.Count < playerStats.maxEquippedPets)
        {
            Transform newPetModel = Instantiate(petTemplate.petPrefab);
            newPetModel.GetComponent<Pet>().SetDamage(damage);
            playerStats.EquippedPets.Add(newPetModel);

            Transform UIEquipPanel = GameObject.FindGameObjectWithTag("EquipPanel").transform;
            EquipUISlot newUIPetSlot = Instantiate(UIEquipPetSlotPrefab, UIEquipPanel).GetComponent<EquipUISlot>();
            newUIPetSlot.SetValues(petName, damage.ToString(), petTemplate, transform.parent, newPetModel);

            SortInventory();

            playerStats.PetsInInventory.Remove(this);
            Destroy(gameObject);
        }
    }

    public void SetLayoutOrder(int orderNum)
    {
        transform.SetSiblingIndex(orderNum);
    }

    private void SortInventory()
    {
        MainUI inv = transform.parent.parent.parent.parent.parent.GetComponent<MainUI>();
        inv.SortInventoryByDamage();
    }

    private void SetSlotUI()
    {
        petNameTxt.text = petName;
        damageTxt.text = damage.ToString();
    }
}
