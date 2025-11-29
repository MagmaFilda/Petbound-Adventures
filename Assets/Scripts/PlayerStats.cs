using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int coins = 100;
    public int maxPets = 20;
    public int maxEquippedPets = 3;
    public List<Transform> EquippedPets;
    public List<PetInInventory> PetsInInventory;
    public List<QuestTemplate> ActiveQuests;

    public int totalOpenEggs = 0;
    public int totalBreakables = 0;

    private PlayerInput playerInput;
    private InputAction clickAction;
    private QuestNPC[] allQuestNPCs;

    private float clicked;
    private bool canClick = true;
    private int coinsFrameBefore;
    private int eggsFrameBefore;
    private int breakablesFrameBefore;

    private Transform targetBreakable;

    private void Awake()
    {
        Instance = this;
        coinsFrameBefore = coins;
        eggsFrameBefore = totalOpenEggs;
    }
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions.FindAction("Click");
        EquippedPets = new List<Transform>();
        PetsInInventory = new List<PetInInventory>();
        ActiveQuests = new List<QuestTemplate>();

        allQuestNPCs = GameObject.FindGameObjectsWithTag("QuestNPC").Select(npc => npc.GetComponent<QuestNPC>()).ToArray();

        GameObject.Find("MainCanvas").transform.GetComponent<MainUI>().UpdateCoins();
    }

    private void Update()
    {
        CheckingClick();
        ChangeCoinsDetection();
        EggsDetection();
        BreakablesDetection();
    }

    private void ChangeCoinsDetection()
    {
        if (coins != coinsFrameBefore)
        {
            GameObject.Find("MainCanvas").transform.GetComponent<MainUI>().UpdateCoins();
            if (coins > coinsFrameBefore)
            {
                SendDataToQuestNPC(coins, coinsFrameBefore,"Coins");
            }
        }
        coinsFrameBefore = coins;
    }
    private void EggsDetection()
    {
        if (totalOpenEggs != eggsFrameBefore)
        {
            SendDataToQuestNPC(totalOpenEggs, eggsFrameBefore, "Eggs"); 
        }
        eggsFrameBefore = totalOpenEggs;
    }
    private void BreakablesDetection()
    {
        if (totalBreakables != breakablesFrameBefore)
        {
            SendDataToQuestNPC(totalBreakables, breakablesFrameBefore, "Breakables");
        }
        breakablesFrameBefore = totalBreakables;
    }
    private void SendDataToQuestNPC(int resources, int resourcesBefore, string resourceName)
    {
        foreach (QuestNPC npc in allQuestNPCs)
        {
            npc.AddToQuest(resourceName, resources - resourcesBefore);
        }
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
        }
        else
        {
            if (targetBreakable != null)
            {
                targetBreakable = null;
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

    private IEnumerator ResetClick()
    {
        while (clicked > 0)
        {
            yield return null;
        }        
        canClick = true;
    }
}
