using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PetInInventory : MonoBehaviour
{
    public Text petNameTxt;
    public Text damageTxt;
    public Transform UIEquipPetSlotPrefab;

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

        PlayerStats.PetsInInventory.Add(this);

        SetSlotUI();
    }
    public void UnEquipPet(PetTemplate template, int dmg)
    {
        petTemplate = template;

        petName = template.petName;
        rarity = template.rarity;
        damage = dmg;

        SetSlotUI();
    }

    public void Equip()
    {
        if (PlayerStats.EquippedPets.Count < PlayerStats.maxEquippedPets)
        {
            Transform newPetModel = Instantiate(petTemplate.petPrefab);
            newPetModel.GetComponent<Pet>().SetDamage(damage);
            PlayerStats.EquippedPets.Add(newPetModel);

            Transform UIEquipPanel = GameObject.FindGameObjectWithTag("EquipPanel").transform;
            EquipUISlot newUIPetSlot = Instantiate(UIEquipPetSlotPrefab, UIEquipPanel).GetComponent<EquipUISlot>();
            newUIPetSlot.SetValues(petName, damage.ToString(), petTemplate, transform.parent, newPetModel);

            Destroy(gameObject);
        }
    }

    private void SetSlotUI()
    {
        petNameTxt.text = petName;
        damageTxt.text = damage.ToString();
    }
}
