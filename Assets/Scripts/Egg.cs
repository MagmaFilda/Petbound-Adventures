using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Egg : MonoBehaviour
{
    public Canvas openUI;
    public Transform petUISlotTemplate;
    public Transform petInventory;

    public EggTemplate template;
    public PetTemplate[] petTemplates { get; private set; }
    public float[] chance { get; private set; }
    public int price { get; private set; }

    private bool playerLook;

    private void Awake()
    {
        petTemplates = template.petTemplates;
        chance = template.chance;
        price = template.price;
    }

    private void Start()
    {
        float testingChance = 0;
        foreach (float c in chance)
        {
            testingChance += c;
        }
        if (Mathf.Abs(testingChance - 100f) > 0.01f)
        {
            Debug.Log(template.name + "nemá správnì %");
        }
    }
    private void Update()
    {
        MouseHover();

        if (playerLook && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenEgg();
        }
    }

    public void OpenEgg()
    {
        if (PlayerStats.coins >= price && PlayerStats.PetsInInventory.Count < PlayerStats.maxPets)
        {
            PlayerStats.coins -= price;
            PetTemplate newPet = GetPet();

            Transform newPetUI = Instantiate(petUISlotTemplate, petInventory);
            newPetUI.GetComponent<PetInInventory>().NewPet(newPet);
        }
    }

    private PetTemplate GetPet()
    {
        int roll = Random.Range(0, 101);
        float actualChance = 0f;
        PetTemplate returningPet = petTemplates[0];

        for (int i = 0; i < petTemplates.Length; i++)
        {
            actualChance += chance[i];
            if (roll <= actualChance)
            {
                returningPet = petTemplates[i];
                break;
            }
        }
        return returningPet;
    }

    private void MouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject == gameObject)
            {
                openUI.enabled = true;
                playerLook = true;
            }
            else
            {
                openUI.enabled = false;
                playerLook = false;
            }
        }
        else
        {
            openUI.enabled = false;
            playerLook = false;
        }
    }
}
