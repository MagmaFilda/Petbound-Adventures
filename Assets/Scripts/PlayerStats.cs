using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int coins = 100;
    public float playerSpeed = 5f;
    public float playerJumpPower = 2f;
    public int maxPets = 20;
    public int maxEquippedPets = 3;
    public int resourceCapacity = 100;

    public List<Transform> EquippedPets;
    public List<PetInInventory> PetsInInventory;
    public List<ActiveQuest> ActiveQuests;
    public List<ItemTemplate> OwnedItems;
    public List<ItemTemplate> EquippedItems;
    public Dictionary<Resource, int> PlayerResources;

    public int totalOpenEggs = 0;
    public int totalBreakables = 0;

    public bool deleteMode = false;

    private PlayerInput playerInput;
    private InputAction clickAction;

    private float clicked;
    private bool canClick = true;
    private int coinsFrameBefore;

    private MainUI mainUI;
    private Transform targetBreakable;

    private void Awake()
    {
        Instance = this;
        mainUI = GameObject.Find("MainCanvas").transform.GetComponent<MainUI>();
        coinsFrameBefore = coins;
    }
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions.FindAction("Click");
        EquippedPets = new List<Transform>();
        PetsInInventory = new List<PetInInventory>();
        ActiveQuests = new List<ActiveQuest>();
        OwnedItems = new List<ItemTemplate>();
        EquippedItems = new List<ItemTemplate>();
        PlayerResources = new Dictionary<Resource, int>();

        foreach (Resource res in Enum.GetValues(typeof(Resource)))
        {
            PlayerResources.Add(res, 0);
        }

        GameObject.Find("MainCanvas").transform.GetComponent<MainUI>().UpdateCoins();
    }

    private void Update()
    {
        CheckingClick();
        ChangeCoinsDetection();
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
