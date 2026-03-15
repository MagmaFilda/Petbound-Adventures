using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Egg : MonoBehaviour
{
    public Transform openUI;
    public Transform cameraPoint;
    public Transform playerLeavePoint;
    public Transform petUISlotTemplate;
    public Transform petInventory;

    public EggTemplate template;
    public PetTemplate[] petTemplates { get; private set; }
    public float[] chance { get; private set; }
    public int price { get; private set; }

    private PlayerStats playerStats;
    private GameManager gameManager;
    private MainUI mainUI;

    private bool canOpen = true;

    private void Awake()
    {
        petTemplates = template.petTemplates;
        chance = template.chance;
        price = template.price;
    }

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        gameManager = GameManager.Instance;
        mainUI = FindFirstObjectByType<MainUI>();

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
        //MouseHover();

        if (openUI.gameObject.activeSelf && Keyboard.current.eKey.wasPressedThisFrame && playerStats.canShowInteract)
        {
            OpenEgg();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(false);
        }
    }

    public void OpenEgg()
    {
        if (playerStats.coins >= price && playerStats.PetsInInventory.Count + playerStats.EquippedPets.Count < playerStats.maxPets)
        {
            if (canOpen)
            {
                StartCoroutine(EggCooldown());
                if (!QuestManager.Instance.eggTemplatesDetection.ContainsKey(template))
                {
                    QuestManager.Instance.eggTemplatesDetection.Add(template, 1);
                }
                else
                {
                    QuestManager.Instance.eggTemplatesDetection[template] += 1;
                }

                playerStats.coins -= price;
                playerStats.totalOpenEggs++;

                PetTemplate newPet = GetPet();

                Transform newPetUI = Instantiate(petUISlotTemplate, petInventory);
                newPetUI.GetComponent<PetInInventory>().NewPet(newPet);
            }           
        }
        else { mainUI.ShowWarning("Nedostatek penìz"); }
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

    private IEnumerator EggCooldown()
    {
        canOpen = false;
        playerStats.canMove = false;
        openUI.gameObject.SetActive(false);
        playerStats.transform.position = playerLeavePoint.position;

        StartCoroutine(gameManager.SetCamera(cameraPoint, false, 2, false, 0));
        transform.GetComponent<Animator>().SetBool("opening", true);

        yield return new WaitForSeconds(2);
        playerStats.canMove = true;
        transform.GetComponent<Animator>().SetBool("opening", false);
               
        canOpen = true;
    }

    //private void MouseHover()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    //    if (Physics.Raycast(ray, out RaycastHit hit, 100f))
    //    {
    //        if (hit.collider.gameObject == gameObject)
    //        {
    //            openUI.enabled = true;
    //        }
    //        else
    //        {
    //            openUI.enabled = false;
    //        }
    //    }
    //    else
    //    {
    //        openUI.enabled = false;
    //    }
    //}
}
