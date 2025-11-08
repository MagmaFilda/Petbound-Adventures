using System.Collections.Generic;
using UnityEngine;

public class Pet : MonoBehaviour
{
    public PetTemplate template;
    public string petName { get; private set; }
    public Rarity rarity { get; private set; }
    public int damage { get; private set; }

    public float speed = 0.003f;
    public string mode = "Follow";

    private bool canDamage = true;
    private Transform breakableTarget;

    private Transform Player;
    private Transform PetPositions;
    private Transform[] positions;
    private Transform petEquipSlot;

    private List<Transform> pets;

    private void Awake()
    {
        petName = template.petName;
        rarity = template.rarity;
        damage = Random.Range(template.minDamage, template.maxDamage);
    }  
    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        PetPositions = Player.Find("EquippedPetSlots");

        PlayerStats.EquippedPets.Add(transform);

        positions = PetPositions.GetComponentsInChildren<Transform>();
        
        FindFreeSlot();
    }
    private void Update()
    {
        if (mode == "Follow")
        {
            FollowPlayer();
        }
        else
        {
            AttackBreakable(transform);
        }
    }

    public void ChangeMode(string change, Transform b)
    {
        mode = change;
        breakableTarget = b;
    }

    private void FollowPlayer()
    {
        pets = PlayerStats.EquippedPets;
        GetToPositon(transform, petEquipSlot.position);
    }
    private void AttackBreakable(Transform pet)
    {
        if (breakableTarget != null)
        {
            Breakable breakable = breakableTarget.GetComponent<Breakable>();
            if (pet.position != breakableTarget.position)
            {
                GetToPositon(pet, breakableTarget.position);
            }
            else
            {
                if (canDamage)
                {
                    breakable.TakeDamage(damage);
                    canDamage = false;

                    StartCoroutine(waitToDamage());
                }             
            }
        }
        else
        {
            ChangeMode("Follow", breakableTarget);
        }
    }

    private void FindFreeSlot()
    {
        foreach (Transform slot in positions)
        {
            if (slot == PetPositions) continue;
            if (!slot.CompareTag("Equipped"))
            {
                petEquipSlot = slot;
                slot.tag = "Equipped";
                break;
            }
        }
    }
    private void GetToPositon(Transform pet, Vector3 followingPosition)
    {
        pet.position = Vector3.MoveTowards(pet.position, followingPosition, speed);
    }

    private IEnumerator<WaitForSeconds> waitToDamage()
    {
        yield return new WaitForSeconds(1);
        canDamage = true;
    }
}
