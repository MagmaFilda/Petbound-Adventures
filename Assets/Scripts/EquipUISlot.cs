using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipUISlot : MonoBehaviour
{
    public TextMeshProUGUI petNameTxt;
    public TextMeshProUGUI petDamageTxt;
    public Image petImgPanel;
    public Image rarityImg;
    public Transform petUISlotTemplate;

    private PlayerStats playerStats;
    private MainUI mainUI;

    private PetTemplate petTemplate;
    private Transform petTransform;
    private Transform Inventory;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
    }

    public void SetValues(string petName, string petDmg, PetTemplate template, Transform inv, Transform petT)
    {
        petNameTxt.text = petName;
        petDamageTxt.text = petDmg.ToString();

        mainUI = FindFirstObjectByType<MainUI>();
        string path = "PetIcons/" + petName;
        mainUI.SetImage(petImgPanel, path);

        rarityImg.color = mainUI.SetRarityColor(template.rarity);

        petTemplate = template;
        Inventory = inv;
        petTransform = petT;
    }

    public void UnEquip()
    {
        Transform newPetUI = Instantiate(petUISlotTemplate, Inventory);
        playerStats.EquippedPets.Remove(petTransform);
        newPetUI.GetComponent<PetInInventory>().UnEquipPet(petTemplate, int.Parse(petDamageTxt.text));

        petTransform.GetComponent<Pet>().petEquipSlot.tag = "Untagged";
        Destroy(petTransform.gameObject);
        Destroy(gameObject);
    }
}
