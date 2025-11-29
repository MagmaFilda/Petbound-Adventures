using UnityEngine;
using UnityEngine.UI;

public class EquipUISlot : MonoBehaviour
{
    public Text petNameTxt;
    public Text petDamageTxt;
    public Transform petUISlotTemplate;

    private PlayerStats playerStats = PlayerStats.Instance;

    private PetTemplate petTemplate;
    private Transform petTransform;
    private Transform Inventory;

    public void SetValues(string petName, string petDmg, PetTemplate template, Transform inv, Transform petT)
    {
        petNameTxt.text = petName;
        petDamageTxt.text = petDmg.ToString();
        petTemplate = template;
        Inventory = inv;
        petTransform = petT;
    }

    public void UnEquip()
    {
        Transform newPetUI = Instantiate(petUISlotTemplate, Inventory);
        newPetUI.GetComponent<PetInInventory>().UnEquipPet(petTemplate, int.Parse(petDamageTxt.text));

        playerStats.EquippedPets.Remove(petTransform);
        petTransform.GetComponent<Pet>().petEquipSlot.tag = "Untagged";
        Destroy(petTransform.gameObject);
        Destroy(gameObject);
    }
}
